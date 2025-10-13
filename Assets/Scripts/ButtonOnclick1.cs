using UnityEngine;

public class ButtonOnclick1 : MonoBehaviour
{
    public void OnButtonClicked()
    {
        Debug.Log("Bouton cliqué !");
        WorldStateManager worldManager = FindObjectOfType<WorldStateManager>();
        if (worldManager != null)
            worldManager.PlaySolution();
    }

    public void SaveProjectBase()
    {
        Debug.Log("Bouton cliqué !");
        WorldStateManager worldManager = FindObjectOfType<WorldStateManager>();
        if (worldManager != null)
            worldManager.SaveWorldState("base.json");
    }

    public void SaveProjectFinal()
    {
        Debug.Log("Bouton cliqué !");
        WorldStateManager worldManager = FindObjectOfType<WorldStateManager>();
        if (worldManager != null)
            worldManager.SaveWorldState("final.json");
    }
}
