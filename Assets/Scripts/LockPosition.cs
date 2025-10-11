using UnityEngine;

public class LockPosition : MonoBehaviour
{
    private Vector3 fixedPosition;

    void Start()
    {
        fixedPosition = transform.position;
    }

    void LateUpdate()
    {
        transform.position = fixedPosition;
    }
}