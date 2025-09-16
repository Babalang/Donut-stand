using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PushOnClick : MonoBehaviour
{
    public float clickForce = 10.0f;

    private void OnMouseOver()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mouse = Input.mousePosition;
            Ray castPoint = Camera.main.ScreenPointToRay(mouse);
            RaycastHit hit;
            if (Physics.Raycast(castPoint, out hit, Mathf.Infinity))
            {
                Rigidbody rb = GetComponentInChildren<Rigidbody>();
                Vector3 shootDirection = castPoint.direction.normalized;
                rb.AddForce(shootDirection * clickForce, ForceMode.Impulse);
            }

        }

    }
}
