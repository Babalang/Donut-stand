using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShrinkAndGrow : MonoBehaviour
{
    public float scaleRate = 1f;
    public float minScale = 0.5f;
    public float maxScale = 2.0f;
 
    public void ApplyScaleRate()
    {
        transform.localScale += Vector3.one * scaleRate * Time.deltaTime;
    }

    void ScaleObject()
    {
        //if we exceed the defined range then correct the sign of scaleRate.
        if (transform.localScale.x < minScale)
        {
            scaleRate = Mathf.Abs(scaleRate);
        }
        else if (transform.localScale.x > maxScale)
        {
            scaleRate = -Mathf.Abs(scaleRate);
        }

        ApplyScaleRate();
    }

    void Update()
    {
        ScaleObject();
    }
}
