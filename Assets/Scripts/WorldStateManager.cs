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
            jsonSolPath = Path.Combine(Application.dataPath, "../Projet Donut Stand/tmp.json");
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
        Vector3 forward = obj.transform.forward.normalized;
        if (up == Vector3.up)
        {
            f.orientation = new List<float> { 0, 1, 0 };
        }
        else
        {
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
                return hit.collider.gameObject.name;
        return "table";
    }

    private string DetectUnder(GameObject obj)
    {
        Ray ray = new Ray(obj.transform.position + Vector3.down * 0.1f, Vector3.up);
        if (Physics.Raycast(ray, out RaycastHit hit, 2f))
            if (hit.collider.gameObject.CompareTag("Grabbable"))
                return hit.collider.gameObject.name;
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
            ApplyWorldState(step.state);
            yield return new WaitForSeconds(pauseDuration);
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
            obj.transform.position = gridSnapper.GetSnappedPosition(pos);
            if (f.orientation != null && f.orientation.Count == 3)
            {
                Vector3 orient = new Vector3(f.orientation[0], f.orientation[1], f.orientation[2]);
                if (orient == Vector3.forward)
                    obj.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            }
            placedObjects.Add(name);
        }
        foreach (var kvp in state.forme)
            PlaceRecursively(kvp.Key);
    }


}
