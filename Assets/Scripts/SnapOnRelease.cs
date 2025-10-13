using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections.Generic;
using UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.State;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;
using UnityEngine.InputSystem.Controls;



public class SnapOnRelease : MonoBehaviour
{
    public GridSnapper gridSnapper;

    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grabInteractable;
    private List<Collider> ignoredColliders = new List<Collider>();
    private Collider[] myColliders;
    private GameObject ghostInstance;
    private Material ghostMaterial;

    private bool isGrabbed = false;

    private Camera mainCamera;
    public Camera mouseCamera;
    public Color mouseRayColor = new Color(0.68f, 0.85f, 0.90f); // Bleu clair

    private int frameSkip = 2;
    private int frameCount = 0;
    public LayerMask plateauLayerMask;

    private Quaternion grabbedRotation = Quaternion.identity;
    private bool isRotated = false;



    void Awake()
    {
        grabInteractable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        if (grabInteractable != null)
        {
            grabInteractable.selectExited.AddListener(OnRelease);
            grabInteractable.selectEntered.AddListener(OnGrab);
        }
        myColliders = GetComponentsInChildren<Collider>();
        ghostMaterial = Resources.Load<Material>("GhostMaterial");
        mainCamera = mouseCamera != null ? mouseCamera : Camera.main;

    }

    void OnDestroy()
    {
        if (grabInteractable != null)
        {
            grabInteractable.selectExited.RemoveListener(OnRelease);
            grabInteractable.selectEntered.RemoveListener(OnGrab);
        }
    }

    void Update()
    {
        if (!UnityEngine.XR.XRSettings.isDeviceActive)
        {
            HandleMouseInteraction();
            if (isGrabbed && Mouse.current.delta.ReadValue() != Vector2.zero)
            {
                if(frameCount++ % frameSkip == 0)
                {
                    MoveObjectWithMouse();
                }
            }
            if(isGrabbed && Keyboard.current !=null && Keyboard.current.rKey.wasPressedThisFrame)
            {
                Debug.Log("Rotation demandée");
                RotateGrabbedObject();
            }
        }
        else
        {
            UnityEngine.InputSystem.XR.XRController rightController = InputSystem.GetDevice<UnityEngine.InputSystem.XR.XRController>();
            if (rightController != null && isGrabbed)
            {
                var primaryButton = rightController.TryGetChildControl<ButtonControl>("primaryButton");
                if (primaryButton != null && primaryButton.wasPressedThisFrame)
                {
                    RotateGrabbedObject();
                }
            }
        }
        if (!isGrabbed)
        {
            return;
        }
        UpdateGhostPosition();
    }

    // Fonction de gestion de la souris : 
    private void HandleMouseInteraction()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.collider.gameObject == gameObject)
                {
                    OnGrab(null);
                }
            }
        }
        if (Mouse.current.leftButton.wasReleasedThisFrame && isGrabbed)
        {
            OnRelease(null);
        }
    }

    private void MoveObjectWithMouse()
    {
        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, plateauLayerMask))
        {
            if (hit.collider.CompareTag("Plateau"))
            {
                Vector3 newPos = hit.point;
                float bottomOffset = transform.position.y - GetBottomY();
                newPos.y += bottomOffset;
                transform.position = newPos;
            }
        }
    }

    private float GetBottomY()
    {
        float minY = float.MaxValue;
        foreach (var col in myColliders)
            minY = Mathf.Min(minY, col.bounds.min.y);
        return minY;
    }






    // Fonctions pour la gestion de prise et relachement de l'objet
    private void OnGrab(SelectEnterEventArgs args)
    {
        isGrabbed = true;
        grabbedRotation = transform.rotation;
        isRotated = Mathf.Abs(Mathf.DeltaAngle(transform.rotation.eulerAngles.x, 90f)) < 1f;

        GameObject[] grabbables = GameObject.FindGameObjectsWithTag("Grabbable");
        ignoredColliders.Clear();
        foreach (GameObject go in grabbables)
        {
            if (go == this.gameObject) continue;

            foreach (Collider otherCol in go.GetComponentsInChildren<Collider>())
            {
                foreach (Collider myCol in myColliders)
                {
                    Physics.IgnoreCollision(myCol, otherCol, true);
                }
                ignoredColliders.Add(otherCol);
            }
        }
        if (ghostInstance == null && ghostMaterial != null)
        {
            ghostInstance = Instantiate(gameObject, transform.position, transform.rotation);
            DestroyImmediate(ghostInstance.GetComponent<SnapOnRelease>());
            DestroyImmediate(ghostInstance.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>());
            var lockRotation = ghostInstance.GetComponent<LockRotationOnGrab>();
            if (lockRotation != null)
                DestroyImmediate(lockRotation);

            foreach (var col in ghostInstance.GetComponentsInChildren<Collider>())
                col.enabled = false;
            foreach (var rend in ghostInstance.GetComponentsInChildren<MeshRenderer>())
                rend.material = ghostMaterial;
            foreach (var affordance in ghostInstance.GetComponentsInChildren<UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.State.BaseAffordanceStateProvider>())
                DestroyImmediate(affordance);
            foreach (var receiver in ghostInstance.GetComponentsInChildren<UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Receiver.BaseAffordanceStateReceiver<float>>(true))
                DestroyImmediate(receiver);
            foreach (var receiver in ghostInstance.GetComponentsInChildren<UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Receiver.BaseAffordanceStateReceiver<Color>>(true))
                DestroyImmediate(receiver);


        }

    }

    private void OnRelease(SelectExitEventArgs args)
    {
        isGrabbed = false;

        foreach (Collider otherCol in ignoredColliders)
        {
            foreach (Collider myCol in myColliders)
            {
                Physics.IgnoreCollision(myCol, otherCol, false);
            }
        }
        if (ghostInstance != null)
        {
            DestroyImmediate(ghostInstance);
            ghostInstance = null;
        }
        ignoredColliders.Clear();
        Rigidbody rb = GetComponent<Rigidbody>();
        bool hadRb = rb != null;
        if (hadRb)
        {
            rb.isKinematic = true;
        }
        if (gridSnapper != null)
        {
            Vector3 snappedPos = gridSnapper.GetSnappedPosition(transform.position);
            float cellHalf = gridSnapper.cellSize / 2f;
            float minY = float.MaxValue;
            float maxY = float.MinValue;
            foreach (var col in myColliders)
            {
                minY = Mathf.Min(minY, col.bounds.min.y);
                maxY = Mathf.Max(maxY, col.bounds.max.y);
            }
            float objectHeight = maxY - minY;

            float highestY = gridSnapper.gridOrigin.y;

            GameObject[] grabbables = GameObject.FindGameObjectsWithTag("Grabbable");
            foreach (GameObject go in grabbables)
            {
                if (go != this.gameObject)
                {
                    Vector3 otherPos = go.transform.position;
                    Vector2 otherXZ = new Vector2(otherPos.x, otherPos.z);
                    Vector2 snappedXZ = new Vector2(snappedPos.x, snappedPos.z);

                    if (Vector2.Distance(otherXZ, snappedXZ) <= cellHalf * 0.7f)
                    {
                        foreach (var col in go.GetComponentsInChildren<Collider>())
                        {
                            float topY = col.bounds.max.y;
                            if (topY > highestY)
                            {
                                highestY = topY;
                            }
                        }
                    }
                }
            }

            Vector3 finalPos = snappedPos;
            float currentBottom = minY;
            float bottomOffset = transform.position.y - currentBottom;
            finalPos.y = highestY + bottomOffset;


            transform.position = finalPos;
            transform.rotation = grabbedRotation;

            if (hadRb)
            {
                rb.isKinematic = false;
            }

            WorldStateManager worldManager = FindObjectOfType<WorldStateManager>();
            if (worldManager != null)
            {
                worldManager.UpdateObjectState(this.gameObject);
            }
        }
    }


    private void UpdateGhostPosition()
    {
        if (ghostInstance == null)
            return;
        Vector3 releasePos = grabInteractable.attachTransform != null ? grabInteractable.attachTransform.position :transform.position;
        Vector3 snappedPos = gridSnapper.GetSnappedPosition(releasePos);
        float checkRadius = 1.0f;
        float minY = float.MaxValue;
        float maxY = float.MinValue;
        foreach (var col in myColliders)
        {
            minY = Mathf.Min(minY, col.bounds.min.y);
            maxY = Mathf.Max(maxY, col.bounds.max.y);
        }
        float objectHeight = maxY - minY;

        Collider[] collidersAtPos = Physics.OverlapSphere(new Vector3(snappedPos.x, 0, snappedPos.z), checkRadius);
        maxY = 0;
        foreach (Collider col in collidersAtPos)
        {
            if (col.gameObject != this.gameObject && col.CompareTag("Grabbable"))
            {
                float topY = col.bounds.max.y;
                if (topY > maxY)
                {
                    maxY = topY;
                }
            }
        }
        Vector3 finalPos = snappedPos;
        float currentBottom = minY;
        float bottomOffset = transform.position.y - currentBottom;
        finalPos.y = maxY + bottomOffset;
        ghostInstance.transform.position = finalPos;
        ghostInstance.transform.rotation = grabbedRotation;
    }

    
    private void RotateGrabbedObject()
    {
        float targetAngle = isRotated ? 0f : 90f;
        Vector3 euler = transform.rotation.eulerAngles;
        euler.x = targetAngle;
        transform.rotation = Quaternion.Euler(euler);
        grabbedRotation = transform.rotation;
        isRotated = !isRotated;
        if (ghostInstance != null)
        {
            ghostInstance.transform.rotation = grabbedRotation;
        }
    }


    
}




