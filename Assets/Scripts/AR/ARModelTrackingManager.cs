using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using Unity.XR.CoreUtils;
using SatwaLangka.Data;
using SatwaLangka.UI;

namespace SatwaLangka.AR
{
    public enum ARPlacementState
    {
        SearchingForSurface,
        SurfaceDetected,
        Tracking
    }

    /// <summary>
    /// Standar Resmi Unity AR Foundation: Simple Plane Placement & Tracking.
    /// Berdasarkan template standar resmi Unity Technologies (arfoundation-samples: PlaceOnPlane).
    /// </summary>
    public class ARModelTrackingManager : MonoBehaviour
    {
        [Header("AR Foundation References")]
        [SerializeField] private ARSession arSession;
        [SerializeField] private XROrigin xrOrigin;
        [SerializeField] private Camera arCamera;
        [SerializeField] private ARPlaneManager planeManager;
        [SerializeField] private ARRaycastManager raycastManager;
        [SerializeField] private ARAnchorManager anchorManager;

        [Header("Placement Reticle")]
        [SerializeField] private GameObject placementIndicator;

        [Header("Stage & World Root")]
        [SerializeField] private Transform worldStageRoot;

        [Header("Animal Data")]
        [SerializeField] private AnimalDataSO currentAnimal;
        [SerializeField] private AnimalDataSO[] allAnimals;

        [Header("Runtime State")]
        [SerializeField] private ARPlacementState state = ARPlacementState.SearchingForSurface;

        // Runtime Tracking Data
        private GameObject spawnedModel;
        private ARPlane currentTrackedPlane;
        private ARAnchor currentAnchor;
        private Vector3 initialModelScale = Vector3.one;
        private Bounds modelBounds;
        private int rendererCount = 0;
        private bool hasPlacedModel = false;
        private Pose lastValidHitPose;
        private bool hasValidHit = false;
        private float surfaceDetectedDuration = 0f;
        private const float AutoPlaceDelay = 0.35f;

        // Out-of-view rescan timer
        private float modelOutOfViewTimer = 0f;
        private const float RescanTimeout = 5f;
        // Min AR placement distance from camera (prevent model too large/close)
        private const float MinPlacementDistance = 0.5f;

        public System.Action<ARPlacementState, string> OnStateChanged;
        public System.Action<string> OnDebugInfoUpdated;

        public ARPlacementState State => state;
        public GameObject SpawnedModel => spawnedModel;
        public AnimalDataSO CurrentAnimal => currentAnimal;
        public Vector3 InitialScale => initialModelScale;
        public Bounds ModelBounds => modelBounds;
        public int RendererCount => rendererCount;
        public bool HasPlacedModel => hasPlacedModel;
        public ARAnchor CurrentAnchor => currentAnchor;
        public Camera ARCamera => arCamera;
        public string CurrentPlaneId => currentTrackedPlane != null ? currentTrackedPlane.trackableId.ToString() : "None";
        public string TrackingStatus => hasPlacedModel ? "Tracking" : state.ToString();

        public void SetPlacementIndicator(GameObject reticle)
        {
            placementIndicator = reticle;
        }

        private static readonly List<ARRaycastHit> s_Hits = new List<ARRaycastHit>();
        private bool isPlaneSubscribed = false;

        private void OnEnable()
        {
            SubscribePlaneEvents();
        }

        private void OnDisable()
        {
            UnsubscribePlaneEvents();
        }

        private void SubscribePlaneEvents()
        {
            if (planeManager == null) planeManager = GetComponent<ARPlaneManager>() ?? FindAnyObjectByType<ARPlaneManager>();
            if (planeManager != null && !isPlaneSubscribed)
            {
                planeManager.planesChanged += OnPlanesChanged;
                isPlaneSubscribed = true;
            }
        }

        private void UnsubscribePlaneEvents()
        {
            if (planeManager != null && isPlaneSubscribed)
            {
                planeManager.planesChanged -= OnPlanesChanged;
                isPlaneSubscribed = false;
            }
        }

        private void Awake()
        {
            if (arSession == null) arSession = FindAnyObjectByType<ARSession>();
            if (xrOrigin == null) xrOrigin = FindAnyObjectByType<XROrigin>();
            if (arCamera == null && xrOrigin != null) arCamera = xrOrigin.Camera;
            if (arCamera == null) arCamera = Camera.main;

            if (planeManager == null) planeManager = GetComponent<ARPlaneManager>() ?? FindAnyObjectByType<ARPlaneManager>();
            if (raycastManager == null) raycastManager = GetComponent<ARRaycastManager>() ?? FindAnyObjectByType<ARRaycastManager>();
            if (anchorManager == null) anchorManager = GetComponent<ARAnchorManager>() ?? FindAnyObjectByType<ARAnchorManager>();

            if (worldStageRoot == null) worldStageRoot = transform;

            CreatePlacementIndicatorIfMissing();
            SubscribePlaneEvents();
        }

        private void Start()
        {
            ResolveSelectedAnimal();

            // Sembunyikan visualisasi plane hijau — material + disable renderer
            HideAllPlaneVisuals();

            // Subscribe untuk hide plane baru yang spawn
            if (planeManager != null)
            {
                planeManager.planesChanged += OnPlanesChangedHideVisuals;
            }

            state = ARPlacementState.SearchingForSurface;
            OnStateChanged?.Invoke(state, "Mendeteksi permukaan datar...");
            Debug.Log("[AR Engine] Standard Unity AR Foundation Plane Placement Initialized.");
        }

        /// <summary>
        /// Sembunyikan semua plane visual secara total — tidak ada alas hijau/hitam sama sekali.
        /// ARCore tetap tracking bidang datar (plane detection masih aktif),
        /// hanya visualnya yang dimatikan.
        /// </summary>
        private void HideAllPlaneVisuals()
        {
            if (planeManager == null) return;

            // JANGAN set planePrefab = null — itu merusak ARPlane tracking & raycast
            // Cukup disable visual renderer saja

            // Disable semua plane yang sudah spawn
            foreach (var plane in planeManager.trackables)
            {
                HidePlaneVisual(plane);
            }
        }

        /// <summary>
        /// Disable semua visual component pada 1 plane
        /// </summary>
        private void HidePlaneVisual(ARPlane plane)
        {
            if (plane == null) return;

            // Disable ARPlaneMeshVisualizer — ini yang generate mesh hijau/hitam
            var visualizer = plane.GetComponent<UnityEngine.XR.ARFoundation.ARPlaneMeshVisualizer>();
            if (visualizer != null) visualizer.enabled = false;

            // Disable MeshRenderer
            var rends = plane.GetComponentsInChildren<MeshRenderer>(true);
            foreach (var r in rends) r.enabled = false;

            // Disable LineRenderer (border outline)
            var line = plane.GetComponent<LineRenderer>();
            if (line != null) line.enabled = false;

            // Disable MeshFilter agar tidak ada geometry sama sekali
            var filters = plane.GetComponentsInChildren<MeshFilter>(true);
            foreach (var f in filters)
            {
                if (f.sharedMesh != null) f.sharedMesh = null;
            }
        }

        /// <summary>
        /// Hide visual setiap kali plane baru terdeteksi/update
        /// </summary>
        private void OnPlanesChangedHideVisuals(ARPlanesChangedEventArgs args)
        {
            if (args.added != null)
                foreach (var plane in args.added) HidePlaneVisual(plane);
            if (args.updated != null)
                foreach (var plane in args.updated) HidePlaneVisual(plane);
        }

        public void CreatePlacementIndicatorIfMissing()
        {
            if (placementIndicator == null || !placementIndicator)
            {
                Transform existing = transform.Find("PlacementReticle_FloorRing");
                if (existing != null)
                {
                    placementIndicator = existing.gameObject;
                    return;
                }

                GameObject prefab = Resources.Load<GameObject>("Prefabs/PlacementReticle_FloorRing");
                if (prefab != null)
                {
                    placementIndicator = Instantiate(prefab, transform);
                    placementIndicator.name = "PlacementReticle_FloorRing";
                    placementIndicator.SetActive(false);
                    return;
                }

                placementIndicator = new GameObject("PlacementReticle_FloorRing");
                placementIndicator.transform.SetParent(transform, false);

                GameObject disc = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                disc.name = "Disc";
                disc.transform.SetParent(placementIndicator.transform, false);
                disc.transform.localScale = new Vector3(0.50f, 0.003f, 0.50f);
                disc.transform.localPosition = Vector3.zero;

                var col = disc.GetComponent<Collider>();
                if (col != null) Destroy(col);

                var mr = disc.GetComponent<MeshRenderer>();
                if (mr != null)
                {
                    Shader s = Shader.Find("Universal Render Pipeline/Unlit");
                    if (s == null) s = Shader.Find("Unlit/Color");
                    Material mat = new Material(s);
                    mat.color = new Color(0.05f, 0.85f, 0.65f, 0.65f);
                    mr.sharedMaterial = mat;
                }

                placementIndicator.SetActive(false);
            }
        }

        private void Update()
        {
            if (arCamera == null) arCamera = Camera.main;

            if (!hasPlacedModel)
            {
                modelOutOfViewTimer = 0f;
                UpdatePlacementPose();
            }
            else
            {
                if (placementIndicator != null && placementIndicator.activeSelf)
                    placementIndicator.SetActive(false);

                // Cek apakah model masih terlihat kamera
                if (spawnedModel != null && arCamera != null)
                {
                    bool isVisible = IsModelInCameraFrustum();
                    if (!isVisible)
                    {
                        modelOutOfViewTimer += Time.deltaTime;
                        if (modelOutOfViewTimer >= RescanTimeout)
                        {
                            Debug.Log("[AR Engine] Model keluar dari kamera selama 5 detik. Reset scan.");
                            ResetPlacement();
                        }
                    }
                    else
                    {
                        modelOutOfViewTimer = 0f;
                    }
                }
            }

            HandleTapToPlace();
        }

        /// <summary>
        /// Cek apakah bounding box model masih dalam camera frustum (terlihat kamera)
        /// </summary>
        private bool IsModelInCameraFrustum()
        {
            if (spawnedModel == null || arCamera == null) return false;

            // Update modelBounds realtime
            Renderer[] rends = spawnedModel.GetComponentsInChildren<Renderer>(true);
            if (rends == null || rends.Length == 0) return true; // Jika tidak ada renderer, anggap masih terlihat

            Bounds b = rends[0].bounds;
            for (int i = 1; i < rends.Length; i++) b.Encapsulate(rends[i].bounds);
            modelBounds = b;

            Plane[] frustumPlanes = GeometryUtility.CalculateFrustumPlanes(arCamera);
            return GeometryUtility.TestPlanesAABB(frustumPlanes, modelBounds);
        }

        /// <summary>
        /// Standar Unity AR Foundation: Memindai bidang datar fisik via ARRaycastManager di tengah layar
        /// </summary>
        private void UpdatePlacementPose()
        {
            if (raycastManager == null) return;

            Vector2 screenCenter = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
            hasValidHit = raycastManager.Raycast(screenCenter, s_Hits,
                TrackableType.PlaneWithinPolygon | TrackableType.PlaneWithinBounds | TrackableType.PlaneEstimated);

            if (hasValidHit && s_Hits.Count > 0)
            {
                // Pastikan tidak terlalu dekat ke kamera (model jadi terlalu besar)
                Vector3 hitPos = s_Hits[0].pose.position;
                float distToCamera = arCamera != null ? Vector3.Distance(arCamera.transform.position, hitPos) : 1f;
                if (distToCamera < MinPlacementDistance)
                {
                    hasValidHit = false;
                    return;
                }

                lastValidHitPose = s_Hits[0].pose;
                currentTrackedPlane = planeManager != null ? planeManager.GetPlane(s_Hits[0].trackableId) : null;

                if (placementIndicator != null)
                {
                    placementIndicator.SetActive(true);
                    placementIndicator.transform.SetPositionAndRotation(lastValidHitPose.position, lastValidHitPose.rotation);
                }

                surfaceDetectedDuration += Time.deltaTime;

                if (state != ARPlacementState.SurfaceDetected)
                {
                    state = ARPlacementState.SurfaceDetected;
                    OnStateChanged?.Invoke(state, "Permukaan terdeteksi! Menempatkan satwa...");
                }

                // Otomatis menempatkan satwa setelah bidang terdeteksi
                if (surfaceDetectedDuration >= AutoPlaceDelay && !hasPlacedModel)
                {
                    Quaternion rot = GetLookRotationTowardsCamera(lastValidHitPose.position);
                    PlaceObject(lastValidHitPose.position, rot);
                }
            }
            else if (!hasPlacedModel && planeManager != null && planeManager.trackables.count > 0)
            {
                // Fallback otomatis ke bidang pertama yang sedang terlacak
                foreach (var plane in planeManager.trackables)
                {
                    if (plane != null && plane.trackingState == TrackingState.Tracking)
                    {
                        currentTrackedPlane = plane;
                        Vector3 worldPos = plane.transform.TransformPoint(plane.center);
                        Quaternion rot = GetLookRotationTowardsCamera(worldPos);
                        PlaceObject(worldPos, rot);
                        return;
                    }
                }
            }
            else
            {
                surfaceDetectedDuration = 0f;

                if (placementIndicator != null && placementIndicator.activeSelf)
                    placementIndicator.SetActive(false);

                if (state != ARPlacementState.SearchingForSurface)
                {
                    state = ARPlacementState.SearchingForSurface;
                    OnStateChanged?.Invoke(state, "Arahkan kamera ke lantai / permukaan datar...");
                }
            }
        }

        /// <summary>
        /// Standar Unity AR Foundation: Event saat ARPlaneManager mendeteksi bidang lantai/dinding baru
        /// </summary>
        private void OnPlanesChanged(ARPlanesChangedEventArgs args)
        {
            if (hasPlacedModel) return;

            var list = (args.added != null && args.added.Count > 0)
                ? args.added
                : (args.updated != null && args.updated.Count > 0 ? args.updated : null);
            if (list == null) return;

            foreach (var plane in list)
            {
                if (plane != null)
                {
                    currentTrackedPlane = plane;
                    Vector3 worldPos = plane.transform.TransformPoint(plane.center);
                    Quaternion rot = GetLookRotationTowardsCamera(worldPos);
                    PlaceObject(worldPos, rot);
                    return;
                }
            }
        }

        /// <summary>
        /// Tap to Place saja — model hanya bisa ditempatkan sekali.
        /// Setelah placed, tap tidak memindahkan model.
        /// Rotasi: 1-jari swipe (TouchManipulator). Scale: 2-jari pinch (TouchManipulator).
        /// </summary>
        private void HandleTapToPlace()
        {
            // Setelah model sudah placed, tap tidak melakukan apa-apa di sini
            // (gesture rotasi & scale dihandle oleh TouchManipulator di spawned model)
            if (hasPlacedModel) return;

            bool tapPressed = false;
            Vector2 tapPos = Vector2.zero;

            if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
            {
                tapPressed = true;
                tapPos = Touchscreen.current.primaryTouch.position.ReadValue();
            }
            else if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                tapPressed = true;
                tapPos = Mouse.current.position.ReadValue();
            }

            if (!tapPressed) return;

            // Abaikan klik pada UI Button
            if (UnityEngine.EventSystems.EventSystem.current != null && UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }

            // AR Raycast — hanya untuk placement pertama kali
            if (raycastManager != null && raycastManager.Raycast(tapPos, s_Hits, TrackableType.All))
            {
                Pose hitPose = s_Hits[0].pose;
                currentTrackedPlane = planeManager != null ? planeManager.GetPlane(s_Hits[0].trackableId) : null;
                Quaternion rot = GetLookRotationTowardsCamera(hitPose.position);
                PlaceObject(hitPose.position, rot);
            }
        }

        /// <summary>
        /// Menempatkan objek 3D pada koordinat bidang lantai fisik nyata
        /// </summary>
        public void PlaceObject(Vector3 worldPos, Quaternion rotation)
        {
            ResolveSelectedAnimal();

            GameObject prefab = currentAnimal != null ? currentAnimal.modelPrefab : null;
            if (prefab == null)
            {
                Debug.LogError($"[AR Engine] Prefab model {(currentAnimal != null ? currentAnimal.animalCode : "NULL")} tidak ditemukan!");
                return;
            }

            ClearSpawnedModel();

            Transform parent = worldStageRoot != null ? worldStageRoot : transform;
            // Hanya unparent jika masih punya parent (cegah unparent berulang)
            if (parent.parent != null) parent.SetParent(null, true);
            parent.position = worldPos;
            parent.rotation = Quaternion.identity;

            spawnedModel = Instantiate(prefab, parent);
            spawnedModel.name = $"Spawned_{(currentAnimal != null ? currentAnimal.animalCode : "Animal")}";
            spawnedModel.SetActive(true);
            spawnedModel.transform.localPosition = Vector3.zero;
            spawnedModel.transform.rotation = rotation;

            // Terapkan ukuran proporsional
            Vector3 baseScale = currentAnimal.defaultScale != Vector3.zero ? currentAnimal.defaultScale : Vector3.one * 0.45f;
            spawnedModel.transform.localScale = baseScale;
            initialModelScale = baseScale;

            AutoFitBoundingBox(1.2f);
            RegroundModel();

            // Update debug tracking info
            Renderer[] rends = spawnedModel.GetComponentsInChildren<Renderer>(true);
            rendererCount = rends != null ? rends.Length : 0;
            if (rends != null && rends.Length > 0)
            {
                modelBounds = rends[0].bounds;
                for (int i = 1; i < rends.Length; i++) modelBounds.Encapsulate(rends[i].bounds);
            }

            // Pasang gesture sentuhan (Rotasi 360 1-jari, Zoom 2-jari)
            if (spawnedModel.GetComponent<TouchManipulator>() == null)
                spawnedModel.AddComponent<TouchManipulator>();

            hasPlacedModel = true;
            state = ARPlacementState.Tracking;
            OnStateChanged?.Invoke(state, $"Satwa {currentAnimal.commonName} berhasil ditempatkan!");

            if (placementIndicator != null)
                placementIndicator.SetActive(false);

            Debug.Log($"[AR Engine] Objek {spawnedModel.name} berhasil diletakkan pada koordinat: {worldPos}");
        }

        public void RepositionObject(Vector3 newWorldPos)
        {
            if (spawnedModel == null) return;
            Transform parent = worldStageRoot != null ? worldStageRoot : transform;
            parent.SetParent(null, true);
            parent.position = newWorldPos;
            parent.rotation = Quaternion.identity;
            spawnedModel.transform.localPosition = Vector3.zero;
            RegroundModel();
            Debug.Log($"[AR Engine] Objek dipindahkan ke koordinat: {newWorldPos}");
        }

        public void ResolveSelectedAnimal()
        {
            string targetCode = PlayerPrefs.GetString("SelectedSatwaCode", "SATWA01");
            if (string.IsNullOrEmpty(targetCode)) targetCode = "SATWA01";

            // 1. Check local array
            if (allAnimals != null && allAnimals.Length > 0)
            {
                foreach (var a in allAnimals)
                {
                    if (a != null && a.animalCode == targetCode)
                    {
                        currentAnimal = a;
                        break;
                    }
                }
            }

            // 2. Check ScanPlaneController in scene
            if (currentAnimal == null)
            {
                var ctrl = FindAnyObjectByType<ScanPlaneController>();
                if (ctrl != null && ctrl.AllAnimals != null && ctrl.AllAnimals.Length > 0)
                {
                    allAnimals = ctrl.AllAnimals;
                    foreach (var a in allAnimals)
                    {
                        if (a != null && a.animalCode == targetCode)
                        {
                            currentAnimal = a;
                            break;
                        }
                    }
                }
            }

            // 3. Check Resources
            if (currentAnimal == null)
            {
                var resAnimals = Resources.LoadAll<AnimalDataSO>("Data/Animals");
                if (resAnimals == null || resAnimals.Length == 0)
                    resAnimals = Resources.LoadAll<AnimalDataSO>("");

                if (resAnimals != null && resAnimals.Length > 0)
                {
                    allAnimals = resAnimals;
                    foreach (var a in resAnimals)
                    {
                        if (a != null && a.animalCode == targetCode)
                        {
                            currentAnimal = a;
                            break;
                        }
                    }
                    if (currentAnimal == null) currentAnimal = resAnimals[0];
                }
            }

#if UNITY_EDITOR
            // 4. Editor Fallback
            if (currentAnimal == null)
            {
                string[] guids = UnityEditor.AssetDatabase.FindAssets("t:AnimalDataSO");
                foreach (var g in guids)
                {
                    var a = UnityEditor.AssetDatabase.LoadAssetAtPath<AnimalDataSO>(UnityEditor.AssetDatabase.GUIDToAssetPath(g));
                    if (a != null && a.animalCode == targetCode)
                    {
                        currentAnimal = a;
                        break;
                    }
                }
            }
#endif

            if (currentAnimal != null)
            {
                Debug.Log($"[ANIMAL] Resolved: {currentAnimal.animalCode} ({currentAnimal.commonName}), Prefab: {(currentAnimal.modelPrefab != null ? currentAnimal.modelPrefab.name : "NULL")}");
            }
            else
            {
                Debug.LogError($"[ANIMAL] FAILED TO RESOLVE ANIMAL FOR CODE: {targetCode}!");
            }
        }

        private void AutoFitBoundingBox(float targetMaxMeters)
        {
            if (spawnedModel == null) return;
            Renderer[] renderers = spawnedModel.GetComponentsInChildren<Renderer>(true);
            if (renderers == null || renderers.Length == 0) return;

            Bounds b = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++) b.Encapsulate(renderers[i].bounds);

            float maxDim = Mathf.Max(b.size.x, b.size.y, b.size.z);
            if (maxDim > 0.001f)
            {
                float factor = targetMaxMeters / maxDim;
                spawnedModel.transform.localScale *= factor;
                initialModelScale = spawnedModel.transform.localScale;
            }
        }

        public void RegroundModel()
        {
            if (spawnedModel == null) return;
            Renderer[] renderers = spawnedModel.GetComponentsInChildren<Renderer>(true);
            if (renderers == null || renderers.Length == 0) return;

            Bounds b = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++) b.Encapsulate(renderers[i].bounds);

            Transform parent = worldStageRoot != null ? worldStageRoot : transform;
            float floorY = parent.position.y;
            float feetY = b.min.y;
            float diff = floorY - feetY;

            spawnedModel.transform.position += new Vector3(0f, diff, 0f);
        }

        public Quaternion GetLookRotationTowardsCamera(Vector3 targetPos)
        {
            if (arCamera == null) arCamera = Camera.main;
            Vector3 camPos = arCamera != null ? arCamera.transform.position : Vector3.zero;
            Vector3 lookDir = camPos - targetPos;
            lookDir.y = 0f;

            if (lookDir.sqrMagnitude > 0.001f)
                return Quaternion.LookRotation(lookDir.normalized, Vector3.up);

            return Quaternion.identity;
        }

        public void ClearSpawnedModel()
        {
            if (spawnedModel != null)
            {
                Destroy(spawnedModel);
                spawnedModel = null;
            }
            if (currentAnchor != null)
            {
                Destroy(currentAnchor.gameObject);
                currentAnchor = null;
            }
        }

        public void SetCurrentAnimal(AnimalDataSO animal)
        {
            currentAnimal = animal;
            if (hasPlacedModel)
            {
                Transform parent = worldStageRoot != null ? worldStageRoot : transform;
                Quaternion rot = GetLookRotationTowardsCamera(parent.position);
                PlaceObject(parent.position, rot);
            }
        }

        public void ResetPlacement()
        {
            ClearSpawnedModel();
            hasPlacedModel = false;
            surfaceDetectedDuration = 0f;
            modelOutOfViewTimer = 0f;
            state = ARPlacementState.SearchingForSurface;
            OnStateChanged?.Invoke(state, "Mendeteksi permukaan datar...");
            if (placementIndicator != null) placementIndicator.SetActive(false);
        }
    }
}
