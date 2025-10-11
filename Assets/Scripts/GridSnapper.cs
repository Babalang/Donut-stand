using UnityEngine;

public class GridSnapper : MonoBehaviour
{
    public Vector3 gridOrigin = Vector3.zero;
    public float cellSize = 3.0f;
    public int gridSize = 3;

    public Vector3 GetSnappedPosition(Vector3 worldPosition)
    {
        Vector3 relative = worldPosition - gridOrigin;
        int x = Mathf.Clamp(Mathf.FloorToInt(relative.x / cellSize), 0, gridSize - 1);
        int z = Mathf.Clamp(Mathf.FloorToInt(relative.z / cellSize), 0, gridSize - 1);
        Vector3 snapped = gridOrigin + new Vector3((x + 0.5f) * cellSize, 0, (z + 0.5f) * cellSize);
        return snapped;
    }
}
