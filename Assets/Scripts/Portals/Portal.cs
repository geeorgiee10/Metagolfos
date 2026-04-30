using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Portal : MonoBehaviour {
    public static bool freezePortals = false;

    [SerializeField] private Renderer portalRenderer;
    private Material portalMaterial;
    private Color baseColor;

    [Header("Main Settings")]
    public bool isActive = true;
    public Portal linkedPortal;
    public MeshRenderer screen;
    public int recursionLimit = 5;

    [Header("Advanced Settings")]
    public float nearClipOffset = 0.05f;
    public float nearClipLimit = 0.2f;

    RenderTexture viewTexture;
    Camera portalCam;
    List<PortalTraveller> trackedTravellers;
    MeshFilter screenMeshFilter;
    
    // Usaremos esto para que cada renderizado sea independiente
    MaterialPropertyBlock screenPropertyBlock;

    void Awake() {
        portalCam = GetComponentInChildren<Camera>();
        portalCam.enabled = false;
        
        // IMPORTANTE: La cámara del portal debe limpiar el fondo para evitar el bleeding
        portalCam.clearFlags = CameraClearFlags.Skybox; 

        trackedTravellers = new List<PortalTraveller>();
        screenMeshFilter = screen.GetComponent<MeshFilter>();
        
        portalMaterial = portalRenderer.material;
        baseColor = portalMaterial.color;
        screenPropertyBlock = new MaterialPropertyBlock();
    }

    void Update() {
        SetEmission();
    }

    void LateUpdate() {
        HandleTravellers();
    }

    private void SetEmission() {
        if (isActive) {
            portalMaterial.EnableKeyword("_EMISSION");
            portalMaterial.SetColor("_EmissionColor", baseColor);
        } else {
            portalMaterial.SetColor("_EmissionColor", Color.black);
            portalMaterial.DisableKeyword("_EMISSION");
        }
    }

    void HandleTravellers() {
        for (int i = 0; i < trackedTravellers.Count; i++) {
            PortalTraveller traveller = trackedTravellers[i];
            Transform travellerT = traveller.transform;
            var m = linkedPortal.transform.localToWorldMatrix * transform.worldToLocalMatrix * travellerT.localToWorldMatrix;

            Vector3 offsetFromPortal = travellerT.position - transform.position;
            int portalSide = System.Math.Sign(Vector3.Dot(offsetFromPortal, transform.forward));
            int portalSideOld = System.Math.Sign(Vector3.Dot(traveller.previousOffsetFromPortal, transform.forward));
            
            if (portalSide != portalSideOld) {
                var positionOld = travellerT.position;
                var rotOld = travellerT.rotation;
                traveller.Teleport(transform, linkedPortal.transform, m.GetColumn(3), m.rotation);
                traveller.graphicsClone.transform.SetPositionAndRotation(positionOld, rotOld);
                linkedPortal.OnTravellerEnterPortal(traveller);
                trackedTravellers.RemoveAt(i);
                i--;
            } else {
                traveller.graphicsClone.transform.SetPositionAndRotation(m.GetColumn(3), m.rotation);
                traveller.previousOffsetFromPortal = offsetFromPortal;
            }
        }
    }

    public void PrePortalRender(Camera viewingCamera) {
        foreach (var traveller in trackedTravellers) {
            UpdateSliceParams(traveller, viewingCamera);
        }
    }

    public void RenderPortal(Camera viewingCamera) {
        if (!isActive || !CameraUtility.VisibleFromCamera(linkedPortal.screen, viewingCamera)) {
            return;
        }

        CreateViewTexture();

        var localToWorldMatrix = viewingCamera.transform.localToWorldMatrix;
        var renderPositions = new Vector3[recursionLimit];
        var renderRotations = new Quaternion[recursionLimit];

        portalCam.projectionMatrix = viewingCamera.projectionMatrix;
        int startIndex = 0;

        for (int i = 0; i < recursionLimit; i++) {
            if (i > 0) {
                if (!CameraUtility.BoundsOverlap(screenMeshFilter, linkedPortal.screenMeshFilter, portalCam)) {
                    break;
                }
            }
            localToWorldMatrix = transform.localToWorldMatrix * linkedPortal.transform.worldToLocalMatrix * localToWorldMatrix;
            int renderOrderIndex = recursionLimit - i - 1;
            renderPositions[renderOrderIndex] = localToWorldMatrix.GetColumn(3);
            renderRotations[renderOrderIndex] = localToWorldMatrix.rotation;

            portalCam.transform.SetPositionAndRotation(renderPositions[renderOrderIndex], renderRotations[renderOrderIndex]);
            startIndex = renderOrderIndex;
        }

        // 1. Ocultar la pantalla del portal actual para que la cámara vea a través
        screen.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
        
        // Usar PropertyBlock para ocultar la máscara solo en este renderizado
        screenPropertyBlock.SetInt("displayMask", 0);
        linkedPortal.screen.SetPropertyBlock(screenPropertyBlock);

        for (int i = startIndex; i < recursionLimit; i++) {
            portalCam.transform.SetPositionAndRotation(renderPositions[i], renderRotations[i]);
            SetNearClipPlane(viewingCamera);
            HandleClipping();
            portalCam.Render();

            if (i == startIndex) {
                // Volver a mostrar la máscara
                screenPropertyBlock.SetInt("displayMask", 1);
                linkedPortal.screen.SetPropertyBlock(screenPropertyBlock);
            }
        }

        screen.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
        
        // 2. Asignar la textura final al portal de destino
        screenPropertyBlock.SetTexture("_MainTex", viewTexture);
        screenPropertyBlock.SetInt("displayMask", 1);
        linkedPortal.screen.SetPropertyBlock(screenPropertyBlock);
    }

    void HandleClipping() {
        const float hideDst = -1000;
        const float showDst = 1000;
        float screenThickness = linkedPortal.ProtectScreenFromClipping(portalCam.transform.position, portalCam);

        foreach (var traveller in trackedTravellers) {
            if (SameSideOfPortal(traveller.transform.position, portalCamPos)) {
                traveller.SetSliceOffsetDst(hideDst, false);
            } else {
                traveller.SetSliceOffsetDst(showDst, false);
            }
            int cloneSideOfLinkedPortal = -SideOfPortal(traveller.transform.position);
            bool camSameSideAsClone = linkedPortal.SideOfPortal(portalCamPos) == cloneSideOfLinkedPortal;
            traveller.SetSliceOffsetDst(camSameSideAsClone ? screenThickness : -screenThickness, true);
        }

        foreach (var linkedTraveller in linkedPortal.trackedTravellers) {
            var travellerPos = linkedTraveller.graphicsObject.transform.position;
            bool cloneOnSameSideAsCam = linkedPortal.SideOfPortal(travellerPos) != SideOfPortal(portalCamPos);
            linkedTraveller.SetSliceOffsetDst(cloneOnSameSideAsCam ? hideDst : showDst, true);
            bool camSameSideAsTraveller = linkedPortal.SameSideOfPortal(linkedTraveller.transform.position, portalCamPos);
            linkedTraveller.SetSliceOffsetDst(camSameSideAsTraveller ? screenThickness : -screenThickness, false);
        }
    }

    public void PostPortalRender(Camera viewingCamera) {
        foreach (var traveller in trackedTravellers) {
            UpdateSliceParams(traveller, viewingCamera);
        }
        ProtectScreenFromClipping(viewingCamera.transform.position, viewingCamera);
    }

    void CreateViewTexture() {
        // CORRECCIÓN: Se añade '24' como buffer de profundidad para evitar bleeding/negros
        if (viewTexture == null || viewTexture.width != Screen.width || viewTexture.height != Screen.height) {
            if (viewTexture != null) viewTexture.Release();
            viewTexture = new RenderTexture(Screen.width, Screen.height, 24); 
            portalCam.targetTexture = viewTexture;
        }
    }

    float ProtectScreenFromClipping(Vector3 viewPoint, Camera viewingCamera) {
        float halfHeight = viewingCamera.nearClipPlane * Mathf.Tan(viewingCamera.fieldOfView * 0.5f * Mathf.Deg2Rad);
        float halfWidth = halfHeight * viewingCamera.aspect;
        float dstToNearClipPlaneCorner = new Vector3(halfWidth, halfHeight, viewingCamera.nearClipPlane).magnitude;
        float screenThickness = dstToNearClipPlaneCorner;

        Transform screenT = screen.transform;
        bool camFacingSameDirAsPortal = Vector3.Dot(transform.forward, transform.position - viewPoint) > 0;
        screenT.localScale = new Vector3(screenT.localScale.x, screenT.localScale.y, screenThickness);
        screenT.localPosition = Vector3.forward * screenThickness * ((camFacingSameDirAsPortal) ? 0.5f : -0.5f);
        return screenThickness;
    }

    void UpdateSliceParams(PortalTraveller traveller, Camera viewingCamera) {
        int side = SideOfPortal(traveller.transform.position);
        Vector3 sliceNormal = transform.forward * -side;
        Vector3 cloneSliceNormal = linkedPortal.transform.forward * side;
        Vector3 slicePos = transform.position;
        Vector3 cloneSlicePos = linkedPortal.transform.position;
        float screenThickness = screen.transform.localScale.z;

        bool playerSameSideAsTraveller = SameSideOfPortal(viewingCamera.transform.position, traveller.transform.position);
        float sliceOffsetDst = playerSameSideAsTraveller ? 0 : -screenThickness;

        bool playerSameSideAsCloneAppearing = side != linkedPortal.SideOfPortal(viewingCamera.transform.position);
        float cloneSliceOffsetDst = playerSameSideAsCloneAppearing ? 0 : -screenThickness;

        for (int i = 0; i < traveller.originalMaterials.Length; i++) {
            traveller.originalMaterials[i].SetVector("sliceCentre", slicePos);
            traveller.originalMaterials[i].SetVector("sliceNormal", sliceNormal);
            traveller.originalMaterials[i].SetFloat("sliceOffsetDst", sliceOffsetDst);
            traveller.cloneMaterials[i].SetVector("sliceCentre", cloneSlicePos);
            traveller.cloneMaterials[i].SetVector("sliceNormal", cloneSliceNormal);
            traveller.cloneMaterials[i].SetFloat("sliceOffsetDst", cloneSliceOffsetDst);
        }
    }

    void SetNearClipPlane(Camera viewingCamera) {
        Transform clipPlane = transform;
        int dot = System.Math.Sign(Vector3.Dot(clipPlane.forward, transform.position - portalCam.transform.position));
        Vector3 camSpacePos = portalCam.worldToCameraMatrix.MultiplyPoint(clipPlane.position);
        Vector3 camSpaceNormal = portalCam.worldToCameraMatrix.MultiplyVector(clipPlane.forward) * dot;
        float camSpaceDst = -Vector3.Dot(camSpacePos, camSpaceNormal) + nearClipOffset;

        if (Mathf.Abs(camSpaceDst) > nearClipLimit) {
            Vector4 clipPlaneCameraSpace = new Vector4(camSpaceNormal.x, camSpaceNormal.y, camSpaceNormal.z, camSpaceDst);
            portalCam.projectionMatrix = viewingCamera.CalculateObliqueMatrix(clipPlaneCameraSpace);
        } else {
            portalCam.projectionMatrix = viewingCamera.projectionMatrix;
        }
    }

    void OnTravellerEnterPortal(PortalTraveller traveller) {
        if (!trackedTravellers.Contains(traveller)) {
            traveller.EnterPortalThreshold();
            traveller.previousOffsetFromPortal = traveller.transform.position - transform.position;
            trackedTravellers.Add(traveller);
        }
    }

    void OnTriggerEnter(Collider other) {
        var traveller = other.GetComponent<PortalTraveller>();
        if (traveller) OnTravellerEnterPortal(traveller);
    }

    void OnTriggerExit(Collider other) {
        var traveller = other.GetComponent<PortalTraveller>();
        if (traveller && trackedTravellers.Contains(traveller)) {
            traveller.ExitPortalThreshold();
            trackedTravellers.Remove(traveller);
        }
    }

    int SideOfPortal(Vector3 pos) => System.Math.Sign(Vector3.Dot(pos - transform.position, transform.forward));
    bool SameSideOfPortal(Vector3 posA, Vector3 posB) => SideOfPortal(posA) == SideOfPortal(posB);
    Vector3 portalCamPos => portalCam.transform.position;

    void OnValidate() {
        if (linkedPortal != null) {
            linkedPortal.linkedPortal = this;
            linkedPortal.isActive = isActive;
        }
    }
}