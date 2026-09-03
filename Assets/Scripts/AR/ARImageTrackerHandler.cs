using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using SatwaLangka.Data;
using SatwaLangka.UI;

namespace SatwaLangka.AR
{
    public class ARImageTrackerHandler : MonoBehaviour
    {
        [Header("AR References")]
        [SerializeField] private ARTrackedImageManager trackedImageManager;

        [Header("Satwa Data Database")]
        [SerializeField] private List<AnimalDataSO> allAnimalData = new List<AnimalDataSO>();

        [Header("UI Presenter")]
        [SerializeField] private ModernSatwaPresenter uiPresenter;
        [SerializeField] private GameObject scanningReticlePanel;
        [SerializeField] private GameObject infoCardPanel;

        [Header("Settings")]
        [SerializeField] private float modelScaleFactor = 0.45f;

        // Runtime Instances dictionary (Marker Name -> Spawned GameObject)
        private Dictionary<string, GameObject> spawnedPrefabs = new Dictionary<string, GameObject>();
        private string currentTrackedAnimalCode = "";

        private void Awake()
        {
            if (trackedImageManager == null)
            {
                trackedImageManager = GetComponent<ARTrackedImageManager>();
            }
        }

        private void OnEnable()
        {
            if (trackedImageManager != null)
            {
                trackedImageManager.trackablesChanged.AddListener(OnTrackablesChanged);
            }
        }

        private void OnDisable()
        {
            if (trackedImageManager != null)
            {
                trackedImageManager.trackablesChanged.RemoveListener(OnTrackablesChanged);
            }
        }

        private void Start()
        {
            // Initial State: Scanning
            SetScanningState(true);
        }

        private void OnTrackablesChanged(ARTrackablesChangedEventArgs<ARTrackedImage> eventArgs)
        {
            // Handle newly added or updated tracked images
            foreach (var trackedImage in eventArgs.added)
            {
                UpdateTrackedAnimal(trackedImage);
            }

            foreach (var trackedImage in eventArgs.updated)
            {
                if (trackedImage.trackingState == TrackingState.Tracking)
                {
                    UpdateTrackedAnimal(trackedImage);
                }
            }
        }

        private void UpdateTrackedAnimal(ARTrackedImage trackedImage)
        {
            string markerName = trackedImage.referenceImage.name; // e.g. SATWA01
            
            // Find Animal Data
            AnimalDataSO animalData = allAnimalData.Find(a => a != null && a.animalCode == markerName);
            if (animalData == null) return;

            // Spawn or reposition model on the physical QR marker
            if (!spawnedPrefabs.ContainsKey(markerName))
            {
                if (animalData.modelPrefab != null)
                {
                    GameObject model = Instantiate(animalData.modelPrefab, trackedImage.transform);
                    model.transform.localPosition = Vector3.zero;
                    model.transform.localRotation = Quaternion.identity;
                    model.transform.localScale = Vector3.one * modelScaleFactor;
                    spawnedPrefabs.Add(markerName, model);
                }
            }
            else
            {
                GameObject model = spawnedPrefabs[markerName];
                if (model != null)
                {
                    model.SetActive(true);
                    model.transform.position = trackedImage.transform.position;
                    model.transform.rotation = trackedImage.transform.rotation;
                }
            }

            if (currentTrackedAnimalCode != markerName)
            {
                currentTrackedAnimalCode = markerName;
                if (uiPresenter != null)
                {
                    uiPresenter.DisplayAnimal(animalData);
                }
                SetScanningState(false);
            }
        }

        public void SetScanningState(bool isScanning)
        {
            if (scanningReticlePanel != null)
            {
                scanningReticlePanel.SetActive(isScanning);
            }

            if (infoCardPanel != null)
            {
                infoCardPanel.SetActive(!isScanning);
            }
        }

        public void ResetScan()
        {
            currentTrackedAnimalCode = "";
            foreach (var kvp in spawnedPrefabs)
            {
                if (kvp.Value != null) kvp.Value.SetActive(false);
            }
            SetScanningState(true);
        }

        // ==================== EDITOR SIMULATION ====================
        public void SimulateScan(string satwaCode)
        {
            AnimalDataSO animalData = allAnimalData.Find(a => a != null && a.animalCode == satwaCode);
            if (animalData != null && uiPresenter != null)
            {
                currentTrackedAnimalCode = satwaCode;
                uiPresenter.DisplayAnimal(animalData);
                SetScanningState(false);
                Debug.Log($"<b>[AR SIMULATION]</b> Simulated Scan QR: {satwaCode} ({animalData.commonName})");
            }
        }
    }
}
