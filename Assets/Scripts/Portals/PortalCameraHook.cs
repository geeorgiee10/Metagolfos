using UnityEngine;

[RequireComponent(typeof(Camera))]
public class PortalCameraHook : MonoBehaviour {
    private Camera cam;
    private Portal[] allPortals;

    void Awake() {
        cam = GetComponent<Camera>();
    }

    // Este evento se dispara automáticamente por cada cámara en la escena
    void OnPreRender() {
        if (Portal.freezePortals) return;

        // Buscamos portales si no los tenemos
        if (allPortals == null || allPortals.Length == 0) 
            allPortals = Object.FindObjectsOfType<Portal>();

        // 1. Pre-render (Slicing)
        for (int i = 0; i < allPortals.Length; i++) {
            if (allPortals[i].linkedPortal != null)
                allPortals[i].PrePortalRender(cam);
        }

        // 2. Render real del portal para ESTA cámara
        for (int i = 0; i < allPortals.Length; i++) {
            if (allPortals[i].linkedPortal != null)
                allPortals[i].RenderPortal(cam);
        }
    }

    void OnPostRender() {
        if (allPortals == null) return;
        for (int i = 0; i < allPortals.Length; i++) {
            if (allPortals[i].linkedPortal != null)
                allPortals[i].PostPortalRender(cam);
        }
    }
}