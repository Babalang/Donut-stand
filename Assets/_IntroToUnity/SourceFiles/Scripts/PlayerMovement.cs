using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private float turnSpeed = 100f;

    Animator m_Animator;
    Rigidbody m_Rigidbody;
    Vector3 m_Movement;
    Vector3 m_MovementForward;

    float rotateSpeed = 10f;

    Quaternion m_Rotation = Quaternion.identity;

    float horizontal;
    float vertical;


    void Start ()
    {
        m_Animator = GetComponent<Animator> ();
        m_Rigidbody = GetComponent<Rigidbody> ();
    }

    void FixedUpdate ()
    {
        horizontal = Input.GetAxis ("Horizontal");
        vertical = Input.GetAxis ("Vertical");
        
        m_Movement.Set(horizontal, 0f, vertical);
        m_Movement.Normalize ();
        m_MovementForward.Set(0f, 0f, vertical);

        //bool hasHorizontalInput = !Mathf.Approximately (horizontal, 0f);
        bool isWalking = /*hasHorizontalInput ||*/ vertical > 0.1f;
        m_Animator.SetBool ("IsWalking", isWalking);
        

        //Vector3 desiredForward = Vector3.RotateTowards (transform.forward, m_Movement, turnSpeed * Time.deltaTime, 0f);
        //m_Rotation = Quaternion.LookRotation (desiredForward);
        //transform.Rotate(Vector3.up, horizontal * Time.deltaTime * rotateSpeed, Space.Self);

        Quaternion deltaRotation = Quaternion.Euler(new Vector3(0, horizontal * turnSpeed * Time.deltaTime, 0));
        m_Rigidbody.MoveRotation(transform.rotation * deltaRotation);
    }

    void OnAnimatorMove ()
    {
        //m_Rigidbody.MovePosition (m_Rigidbody.position + m_Movement * m_Animator.deltaPosition.magnitude);

        m_Rigidbody.MovePosition (m_Rigidbody.position + transform.forward * m_Animator.deltaPosition.magnitude);

    }
}