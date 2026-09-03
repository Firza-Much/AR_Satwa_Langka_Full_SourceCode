using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using SatwaLangka.Data;

namespace SatwaLangka.AR
{
    /// <summary>
    /// Pure Markerless AR Placement dengan Deteksi Bidang Datar Fisik.
    /// Objek satwa HANYA muncul secara otomatis begitu bidang lantai fisik terdeteksi oleh kamera.
    /// </summary>
    public class MarkerlessARPlacementController : MonoBehaviour
    {
        [Header("AR References")]
        [SerializeField] private Camera arCamera;
        [SerializeField] private ARPlaneManager arPlaneManager;
        [SerializeField] private ARRaycastManager arRaycastManager;

        [Header("Model Settings")]
        [SerializeField] private float targetModelSize = 0.60f;

        [Header("Placement Reticle")]
        [SerializeField] private GameObject placementIndicatorPrefab;

        [Header("Data")]
        [SerializeField] private AnimalDataSO currentAnimal;
        [SerializeField] private AnimalDataSO[] allAnimals;

        [Header("UI")]
        [SerializeField] private GameObject scanningPromptUI;

        private GameObject spawnedModel;
        private GameObject placementIndicator;
        private Transform cameraTransform;
        private bool modelPlaced = false;
        private static readonly List<ARRaycastHit> hits = new List<ARRaycastHit>();

        private void OnEnable()
        {
            if (arPlaneManager == null)
                arPlaneManager = GetComponent<ARPlaneManager>() ?? FindAnyObjectByType<ARPlaneManager>();

            if (arPlaneManager != null)
                arPlaneManager.planesChanged += OnPlanesChanged;
        }

        private void OnDisable()
        {
            if (arPlaneManager != null)
                arPlaneManager.planesChanged -= OnPlanesChanged;
        }

        private void Start()
        {
            LoadSelectedAnimal();

            var infoPanel = Object.FindAnyObjectByType<SatwaLangka.AR.ARInfoPanelController>();
            if (infoPanel != null && currentAnimal != null)
                infoPanel.Populate(currentAnimal);

            if (arCamera == null) arCamera = Camera.main;
            cameraTransform = arCamera != null ? arCamera.transform : (Camera.main != null ? Camera.main.transform : transform);

            if (arRaycastManager == null)
                arRaycastManager = GetComponent<ARRaycastManager>() ?? FindAnyObjectByType<ARRaycastManager>();

            if (placementIndicatorPrefab != null && placementIndicator == null)
            {
                placementIndicator = Instantiate(placementIndicatorPrefab);
                placementIndicator.SetActive(false);
            }

            ShowUI(scanningPromptUI, true);
        }

        private void Update()
        {
            if (!modelPlaced)
            {
                UpdatePlacementIndicatorAndAutoPlace();
            }
            else
            {
                if (placementIndicator != null && placementIndicator.activeSelf)
                    placementIndicator.SetActive(false);
            }

            HandleTapReposition();
        }

        private void OnPlanesChanged(ARPlanesChangedEventArgs args)
        {
            if (modelPlaced) return;

            if (args.added != null && args.added.Count > 0)
            {
                foreach (var plane in args.added)
                {
                    if (plane != null && (plane.alignment == PlaneAlignment.HorizontalUp || plane.alignment == PlaneAlignment.HorizontalDown || plane.alignment == PlaneAlignment.NotAxisAligned))
                    {
                        Vector3 worldPos = plane.transform.TransformPoint(plane.center);
                        PlaceModelAt(worldPos, Quaternion.identity);
                        return;
                    }
                }
            }
        }

        private void UpdatePlacementIndicatorAndAutoPlace()
        {
            if (arRaycastManager == null) return;

            Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
            bool hit = arRaycastManager.Raycast(screenCenter, hits, TrackableType.PlaneWithinPolygon | TrackableType.PlaneEstimated);

            if (hit && hits.Count > 0)
            {
                var hitPose = hits[0].pose;

                if (placementIndicator != null)
                {
                    placementIndicator.SetActive(true);
                    placementIndicator.transform.position = hitPose.position;
                    placementIndicator.transform.rotation = hitPose.rotation;
                }

                if (!modelPlaced)
                {
                    PlaceModelAt(hitPose.position, hitPose.rotation);
                }
            }
            else
            {
                if (placementIndicator != null)
                    placementIndicator.SetActive(false);
            }
        }

        private void HandleTapReposition()
        {
            if (!modelPlaced || arRaycastManager == null) return;

            if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
            {
                Touch touch = Input.GetTouch(0);
                if (UnityEngine.EventSystems.EventSystem.current != null &&
                    UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject(touch.fingerId))
                {
                    return;
                }

                if (arRaycastManager.Raycast(touch.position, hits, TrackableType.PlaneWithinPolygon | TrackableType.PlaneEstimated))
                {
                    PlaceModelAt(hits[0].pose.position, hits[0].pose.rotation);
                }
            }
        }

        private void PlaceModelAt(Vector3 position, Quaternion rotation)
        {
            if (currentAnimal == null || currentAnimal.modelPrefab == null)
            {
                LoadSelectedAnimal();
                if (currentAnimal == null || currentAnimal.modelPrefab == null) return;
            }

            if (spawnedModel != null)
                Destroy(spawnedModel);

            Quaternion targetRot = rotation;
            if (cameraTransform != null)
            {
                Vector3 lookDir = cameraTransform.position - position;
                lookDir.y = 0f;
                if (lookDir != Vector3.zero)
                    targetRot = Quaternion.LookRotation(lookDir);
            }

            spawnedModel = Instantiate(currentAnimal.modelPrefab, position, targetRot);
            spawnedModel.name = $"AR_{currentAnimal.animalCode}";

            var scale = currentAnimal.defaultScale;
            if (scale == Vector3.zero) scale = Vector3.one * 0.45f;
            spawnedModel.transform.localScale = scale;

            AutoFitModelSize(targetModelSize);
            GroundModelOnPlane(position.y);

            if (spawnedModel.GetComponent<TouchManipulator>() == null)
                spawnedModel.AddComponent<TouchManipulator>();

            modelPlaced = true;

            if (placementIndicator != null) placementIndicator.SetActive(false);
            ShowUI(scanningPromptUI, false);

            Debug.Log($"[MarkerlessAR] Placed {currentAnimal.commonName} at floor position: {position}");
        }

        private void AutoFitModelSize(float targetSize)
        {
            if (spawnedModel == null) return;
            Renderer[] renderers = spawnedModel.GetComponentsInChildren<Renderer>(true);
            if (renderers == null || renderers.Length == 0) return;

            Bounds b = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++) b.Encapsulate(renderers[i].bounds);

            float maxDim = Mathf.Max(b.size.x, b.size.y, b.size.z);
            if (maxDim < 0.001f) return;

            float scaleFactor = targetSize / maxDim;
            spawnedModel.transform.localScale *= scaleFactor;
        }

        private void GroundModelOnPlane(float planeY)
        {
            if (spawnedModel == null) return;
            Renderer[] renderers = spawnedModel.GetComponentsInChildren<Renderer>(true);
            if (renderers == null || renderers.Length == 0) return;

            Bounds b = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++) b.Encapsulate(renderers[i].bounds);

            float diffY = planeY - b.min.y;
            spawnedModel.transform.position += new Vector3(0f, diffY, 0f);
        }

        private void LoadSelectedAnimal()
        {
            string code = PlayerPrefs.GetString("SelectedSatwaCode", "SATWA01");
            if (allAnimals == null || allAnimals.Length == 0)
            {
                allAnimals = Resources.LoadAll<AnimalDataSO>("Data/Animals");
                if (allAnimals == null || allAnimals.Length == 0)
                    allAnimals = Resources.LoadAll<AnimalDataSO>("");
            }
            if (allAnimals != null)
            {
                foreach (var a in allAnimals)
                    if (a != null && a.animalCode == code) { currentAnimal = a; return; }
                foreach (var a in allAnimals)
                    if (a != null) { currentAnimal = a; return; }
            }
        }

        private void ShowUI(GameObject ui, bool show)
        {
            if (ui != null) ui.SetActive(show);
        }

        public void ResetPlacement()
        {
            modelPlaced = false;
            if (spawnedModel != null) Destroy(spawnedModel);
            if (placementIndicator != null) placementIndicator.SetActive(false);
            ShowUI(scanningPromptUI, true);
        }
    }
}
