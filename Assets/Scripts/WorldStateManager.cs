using UnityEngine;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;


public class WorldStateManager : MonoBehaviour
{

    [Header("Paramètres de grille")]
    public float gridCellSize = 1.0f;
    public GridSnapper gridSnapper;

    [Header("Fichier JSON (chemin complet ou relatif)")]
    private string jsonFilePath = "";
    private string jsonSolPath = "";
    private Dictionary<string, GameObject> allObjects;
    private Dictionary<string, FormeData> allData;


    void Awake()
    {
        if (string.IsNullOrEmpty(jsonFilePath))
            jsonFilePath = Path.Combine(Application.dataPath, "../Projet Donut Stand/base.json");
        if (string.IsNullOrEmpty(jsonSolPath))
            jsonSolPath = Path.Combine(Application.dataPath, "../Projet Donut Stand/route.json");
        Debug.Log("Chemin complet JSON : " + Path.GetFullPath(jsonFilePath));

    }

    private void PlaceObjectRecursively(string name, HashSet<string> placedObjects)
    {
        if (placedObjects.Contains(name)) return;
        FormeData f = allData[name];
        GameObject obj = allObjects[name];
        if (!string.IsNullOrEmpty(f.on_Top_of) && f.on_Top_of != "table" && f.on_Top_of != "nothing")
        {
            PlaceObjectRecursively(f.on_Top_of, placedObjects);
        }
        float y = gridSnapper.gridOrigin.y;
        if (!string.IsNullOrEmpty(f.on_Top_of) && f.on_Top_of != "table" && f.on_Top_of != "nothing")
        {
            GameObject support = allObjects[f.on_Top_of];
            Collider[] cols = support.GetComponentsInChildren<Collider>();
            float maxY = float.MinValue;
            foreach (var col in cols)
                maxY = Mathf.Max(maxY, col.bounds.max.y);
            y = maxY;
        }
        Collider[] myCols = obj.GetComponentsInChildren<Collider>();
        float minY = float.MaxValue;
        foreach (var col in myCols)
            minY = Mathf.Min(minY, col.bounds.min.y);
        float offset = obj.transform.position.y - minY;

        Vector3 finalPos = obj.transform.position;
        finalPos.y = y + offset;
        obj.transform.position = finalPos;

        placedObjects.Add(name);
    }


    public void LoadWorldState()
    {
        if (!File.Exists(jsonFilePath))
        {
            Debug.LogError($"Fichier introuvable : {jsonFilePath}");
            return;
        }
        string json = File.ReadAllText(jsonFilePath);
        RootData data = JsonConvert.DeserializeObject<RootData>(json);
        allObjects = new Dictionary<string, GameObject>();
        allData = new Dictionary<string, FormeData>();
        foreach (var kvp in data.forme)
        {
            string name = kvp.Key;
            GameObject obj = GameObject.Find(name);
            if (obj != null && obj.CompareTag("Grabbable"))
                allObjects[name] = obj;
            allData[name] = kvp.Value;
        }
        foreach (var kvp in allObjects)
        {
            GameObject obj = kvp.Value;
            FormeData f = allData[obj.name];
            Rigidbody rb = obj.GetComponent<Rigidbody>();
            if (rb != null) rb.isKinematic = true;

            float x = f.position_occupe[0][0] * gridCellSize;
            float z = f.position_occupe[0][1] * gridCellSize;
            Vector3 pos = new Vector3(x, gridSnapper.gridOrigin.y, z);
            obj.transform.position = gridSnapper.GetSnappedPosition(pos);

            if (f.orientation != null && f.orientation.Count == 3)
            {
                Vector3 orient = new Vector3(f.orientation[0], f.orientation[1], f.orientation[2]);

                 if (orient == Vector3.forward)
                {
                    obj.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
                }
            }
            if (rb != null) rb.isKinematic = false;
        }
        HashSet<string> placedObjects = new HashSet<string>();
        foreach (var kvp in allObjects)
        {
            PlaceObjectRecursively(kvp.Key, placedObjects);
        }

        Debug.Log($"Monde chargé depuis : {jsonFilePath}");
    }

    public void SaveWorldState(string JsonFile)
    {
        if (string.IsNullOrEmpty(JsonFile))
        {
            Debug.LogError("Nom de fichier JSON invalide !");
            return;
        }
        string jsonPath = Path.Combine(Application.dataPath, "../Projet Donut Stand/" + JsonFile);
        if (allData == null)
        {
            Debug.LogError("allData est null, impossible de sauvegarder !");
            return;
        }
        RootData data = new RootData();
        data.forme = allData;
        string json = JsonConvert.SerializeObject(data, Formatting.Indented);
        File.WriteAllText(jsonPath, json);

        Debug.Log($"Monde sauvegardé dans : {jsonPath}");
    }




    public void UpdateObjectState(GameObject obj)
    {
        if (!allData.ContainsKey(obj.name)) return;
        FormeData f = allData[obj.name];
        int gx = Mathf.RoundToInt(obj.transform.position.x / gridCellSize);
        int gz = Mathf.RoundToInt(obj.transform.position.z / gridCellSize);
        f.position_occupe = new List<List<float>> { new List<float> { gx, gz } };
        Vector3 up = obj.transform.up.normalized;
        if (up == Vector3.up)
        {
            f.orientation = new List<float> { 0, 1, 0 };
        }
        else
        {
            Debug.Log("couché");
            f.orientation = new List<float> { 0, 0, 1 };
        }

        f.on_Top_of = DetectSupport(obj);
        f.under = DetectUnder(obj);
    }


    private string DetectSupport(GameObject obj)
    {
        Ray ray = new Ray(obj.transform.position + Vector3.up * 0.1f, Vector3.down);
        if (Physics.Raycast(ray, out RaycastHit hit, 2f))
            if (hit.collider.gameObject.CompareTag("Grabbable"))
            {
                allData[hit.collider.gameObject.name].under = obj.name;
                Debug.Log(hit.collider.gameObject.name);
                Debug.Log(allData[hit.collider.gameObject.name].under);
                return hit.collider.gameObject.name;
            }
        return "table";
    }

    private string DetectUnder(GameObject obj)
    {
        Ray ray = new Ray(obj.transform.position + Vector3.down * 0.1f, Vector3.up);
        if (Physics.Raycast(ray, out RaycastHit hit, 2f))
            if (hit.collider.gameObject.CompareTag("Grabbable"))
            {
                allData[hit.collider.gameObject.name].on_Top_of = obj.name;
                return hit.collider.gameObject.name;
            }
        return "nothing";
    }

    public void PlaySolution()
    {
        StartCoroutine(PlaySolutionCoroutine());
    }

    private IEnumerator PlaySolutionCoroutine()
    {
        if (!File.Exists(jsonSolPath))
        {
            Debug.LogError($"Fichier solution introuvable : {jsonSolPath}");
            yield break;
        }
        string json = File.ReadAllText(jsonSolPath);
        List<SolutionStep> solutionSteps = JsonConvert.DeserializeObject<List<SolutionStep>>(json);
        float pauseDuration = 1.0f;

        foreach (var step in solutionSteps)
        {
            if(step.id != 0)
            {
                ApplyWorldState(step.state);
                yield return new WaitForSeconds(pauseDuration);
            }
           
        }
        Debug.Log("Solution terminée !");
    }

    private void ApplyWorldState(StateData state)
    {
        HashSet<string> placedObjects = new HashSet<string>();

        void PlaceRecursively(string name)
        {
            if (placedObjects.Contains(name)) return;
            if (!allObjects.ContainsKey(name)) return;
            FormeData f = state.forme[name];
            GameObject obj = allObjects[name];
            if (!string.IsNullOrEmpty(f.on_Top_of) && f.on_Top_of != "table" && f.on_Top_of != "nothing")
            {
                PlaceRecursively(f.on_Top_of);
            }
            float y = gridSnapper.gridOrigin.y;
            if (!string.IsNullOrEmpty(f.on_Top_of) && f.on_Top_of != "table" && f.on_Top_of != "nothing")
            {
                GameObject support = allObjects[f.on_Top_of];
                Collider[] cols = support.GetComponentsInChildren<Collider>();
                float maxY = float.MinValue;
                foreach (var col in cols)
                    maxY = Mathf.Max(maxY, col.bounds.max.y);
                y = maxY;
            }
            float x = f.position_occupe[0][0] * gridCellSize;
            float z = f.position_occupe[0][1] * gridCellSize;
            Vector3 pos = new Vector3(x, y, z);
            Collider[] myCols = obj.GetComponentsInChildren<Collider>();
            float minY = float.MaxValue;
            foreach (var col in myCols)
                minY = Mathf.Min(minY, col.bounds.min.y);
            float bottomOffset = obj.transform.position.y - minY;
            pos = gridSnapper.GetSnappedPosition(pos);
            pos.y = y + bottomOffset;
            obj.transform.position = pos;
            if (f.orientation != null && f.orientation.Count == 3)
            {
                Vector3 orient = new Vector3(f.orientation[0], f.orientation[1], f.orientation[2]);
                if (f.orientation[2] == 1)
                {
                    obj.transform.rotation = Quaternion.Euler(90f, 0f, 0f);

                }
                else
                {
                    obj.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
                }
            }
            placedObjects.Add(name);
        }
        foreach (var kvp in state.forme)
            PlaceRecursively(kvp.Key);
    }

    public static bool AreFormeEqual(Dictionary<string, FormeData> a, Dictionary<string, FormeData> b)
    {
        if (a.Count != b.Count) return false;

        foreach (var kvp in a)
        {
            if (!b.ContainsKey(kvp.Key)) return false;
            if (!AreFormeDataEqual(kvp.Value, b[kvp.Key])) return false;
        }
        return true;
    }

    public static bool AreFormeDataEqual(FormeData f1, FormeData f2)
    {
        if (f1.on_Top_of != f2.on_Top_of) return false;
        if (f1.under != f2.under) return false;
        if (f1.position_occupe.Count != f2.position_occupe.Count) return false;
        for (int i = 0; i < f1.position_occupe.Count; i++)
        {
            if (f1.position_occupe[i].Count != f2.position_occupe[i].Count) return false;
            for (int j = 0; j < f1.position_occupe[i].Count; j++)
            {
                if (Mathf.Abs(f1.position_occupe[i][j] - f2.position_occupe[i][j]) > 0.001f) return false;
            }
        }
        if (f1.orientation.Count != f2.orientation.Count) return false;
        for (int i = 0; i < f1.orientation.Count; i++)
        {
            if (Mathf.Abs(f1.orientation[i] - f2.orientation[i]) > 0.001f) return false;
        }
        return true;
    }

    public void ChercheSolution()
    {
        string jsonFileArbre = Path.Combine(Application.dataPath, "../Projet Donut Stand/arbre.json");
        string jsonArbre = File.ReadAllText(jsonFileArbre);
        List<SolutionStep> solutionSteps = JsonConvert.DeserializeObject<List<SolutionStep>>(jsonArbre);
        string jsonBase = File.ReadAllText(jsonFilePath);
        RootData data = JsonConvert.DeserializeObject<RootData>(jsonBase);
        bool b=false,f=false;
        int idBase=0,idFinal=0;
        foreach(var step in solutionSteps){
            StateData state=step.state;
            if(AreFormeEqual(state.forme,data.forme) && !state.arm.holding){
                b=true;
                idBase=step.id;
                Debug.Log("Base trouvé "+idBase);
                break;
            }
        }
        if(b==false){
            Debug.Log("Base non trouve");
            return;
        }
        string jsonFileFinal = Path.Combine(Application.dataPath, "../Projet Donut Stand/final.json");
        string jsonFinal = File.ReadAllText(jsonFileFinal);
        RootData data2 = JsonConvert.DeserializeObject<RootData>(jsonFinal);
        foreach(var step in solutionSteps){
            StateData state=step.state;
            if(AreFormeEqual(state.forme,data2.forme)){
                f=true;
                idFinal=step.id;
                Debug.Log("Final trouvé et "+idFinal);
                break;
            }
        }
        if(f==false){
            Debug.Log("Final non trouve");
            return;
        }
        int? nb;
        List<int?> chemin=new List<int?>();
        if(idBase==0){
            nb=idFinal;
            chemin.Add(nb);
            while(nb!=idBase){
                foreach(var step in solutionSteps){
                    if(step.id==nb){
                        nb=step.parent;
                        chemin.Add(nb);
                        break;
                    }
                }
            }
            chemin.Reverse();
        }
        else if(idFinal==0){
            nb=idBase;
            chemin.Add(nb);
            while(nb!=idFinal){
                foreach(var step in solutionSteps){
                    if(step.id==nb){
                        nb=step.parent;
                        chemin.Add(nb);
                        break;
                    }
                }
            }
        }else{
            List<int?> chemin1=new List<int?>();
            nb=idBase;
            chemin1.Add(nb);
            while(nb!=0){
                foreach(var step in solutionSteps){
                    if(step.id==nb){
                        nb=step.parent;
                        chemin1.Add(nb);
                        break;
                    }
                }
            }

            foreach (var step in solutionSteps)
            {
                if (step.id == nb)
                {
                    nb = step.parent;
                    chemin1.Add(nb);
                    break;
                }
            }
            List<int?> chemin2=new List<int?>();
            nb=idFinal;
            chemin2.Add(nb);
            while(nb!=0){
                foreach(var step in solutionSteps){
                    if(step.id==nb){
                        nb=step.parent;
                        chemin2.Add(nb);
                        break;
                    }
                }
            }
            chemin2.Reverse();
            chemin.AddRange(chemin1);
            chemin.AddRange(chemin2);
        }
        foreach(var n in chemin){
            Debug.Log(n);
        }
        List<SolutionStep> solu = new List<SolutionStep>();
        foreach(var n in chemin){
            foreach(var step in solutionSteps){
                if(step.id==n){
                    solu.Add(step);
                    break;
                }
            }
        }
        string jsonSolu = JsonConvert.SerializeObject(solu, Formatting.Indented);
        File.WriteAllText(jsonSolPath, jsonSolu);
        Debug.Log("solution trouvé");
        return;
    }


}
