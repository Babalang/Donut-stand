using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootUpAfterDelay : MonoBehaviour
{
    public float delay = 3.0f;
    public float upwardForce = 10.0f;


    void Start()
    {
        StartCoroutine(ExampleCoroutine());
    }

    IEnumerator ExampleCoroutine()
    {
        //yield on a new YieldInstruction that waits for 5 seconds.
        yield return new WaitForSeconds(delay);

        Rigidbody rb = GetComponentInChildren<Rigidbody>();
        rb.AddForce(Vector3.up * upwardForce, ForceMode.Impulse);

    }
}
