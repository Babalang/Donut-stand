using UnityEngine;

public class ButtonOnclick1 : MonoBehaviour
{
    public void OnButtonClicked()
    {
        Debug.Log("Bouton cliqu� !");
        WorldStateManager worldManager = FindObjectOfType<WorldStateManager>();
        if (worldManager != null)
            worldManager.ChercheSolution();
            worldManager.PlaySolution();
    }

    public void SaveProjectBase()
    {
        Debug.Log("Bouton cliqu� !");
        WorldStateManager worldManager = FindObjectOfType<WorldStateManager>();
        if (worldManager != null)
            worldManager.SaveWorldState("base.json");
    }

    public void SaveProjectFinal()
    {
        Debug.Log("Bouton cliqu� !");
        WorldStateManager worldManager = FindObjectOfType<WorldStateManager>();
        if (worldManager != null)
            worldManager.SaveWorldState("final.json");
    }
}
