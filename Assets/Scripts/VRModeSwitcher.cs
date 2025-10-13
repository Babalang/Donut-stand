using System.Collections;
using UnityEngine;
using UnityEngine.XR;
using TMPro;
using UnityEngine.EventSystems;
#if UNITY_XR_MANAGEMENT
using UnityEngine.XR.Management;
#endif

public class VRModeSwitcher : MonoBehaviour
{
    public GameObject vrCanvas;
    public GameObject nonVRCam;
    public GameObject loadingScreen;
    public TMP_Text statusText;
    public BaseInputModule mouseModule;
    public WorldStateManager worldStateManager;

    IEnumerator Start()
    {
        // Activer l'écran de chargement
        if (loadingScreen != null) loadingScreen.SetActive(true);

#if UNITY_XR_MANAGEMENT
        // Attendre que le XR loader soit prêt
        while (XRGeneralSettings.Instance.Manager.activeLoader == null)
        {
            yield return null;
        }
#endif

        yield return new WaitForSeconds(1f);

        if (XRSettings.isDeviceActive)
        {
            Debug.Log("VR Mode detected!");
            statusText.text = "Lancement en mode VR";
            ActiveVRMode();
        }
        else
        {
            Debug.Log("Non-VR Mode detected!");
            statusText.text = "Lancement en mode classique";
            ActiveNonVRMode();
        }
        worldStateManager.LoadWorldState();
        if (loadingScreen != null) loadingScreen.SetActive(false);
    }

    void ActiveVRMode()
    {
        if (vrCanvas != null) vrCanvas.SetActive(true);
        if (nonVRCam != null) nonVRCam.SetActive(false);
        if(mouseModule != null) mouseModule.enabled = false;
    }

    void ActiveNonVRMode()
    {
        if (nonVRCam != null) nonVRCam.SetActive(true);
        if (vrCanvas != null) vrCanvas.SetActive(false);
        if (mouseModule != null) mouseModule.enabled = true;
    }
}
