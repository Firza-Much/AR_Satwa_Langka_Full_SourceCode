using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using SatwaLangka.Data;
using SatwaLangka.AR;

namespace SatwaLangka.UI
{
    public class ScanPlaneController : MonoBehaviour
    {
        [Header("AR Tracking Engine")]
        [SerializeField] private ARModelTrackingManager trackingManager;

        [Header("Top Navigation & Header")]
        [SerializeField] private Button btnKembali;
        [SerializeField] private TextMeshProUGUI txtScanningPrompt;
        [SerializeField] private Image imgTrackingPill;

        [Header("Live AR Debug Overlay (Editor & Development)")]
        [SerializeField] private GameObject debugOverlayPanel;
        [SerializeField] private TextMeshProUGUI txtDebugInfo;
        [SerializeField] private Button btnToggleDebug;

        [Header("Bottom Info Panel - Header")]
        [SerializeField] private GameObject bottomInfoCard;
        [SerializeField] private TextMeshProUGUI txtCommonName;
        [SerializeField] private TextMeshProUGUI txtLatinName;
        [SerializeField] private TextMeshProUGUI txtStatusBadge;
        [SerializeField] private Image imgStatusBadge;
        [SerializeField] private Button btnVoiceover;
        [SerializeField] private TextMeshProUGUI txtVoiceoverBtn;

        [Header("Bottom Info Panel - 4 Tabs")]
        [SerializeField] private Button tabDeskripsi;
        [SerializeField] private Button tabHabitat;
        [SerializeField] private Button tabMitigasi;
        [SerializeField] private Button tabFakta;
        [SerializeField] private Image imgTabDeskripsi;
        [SerializeField] private Image imgTabHabitat;
        [SerializeField] private Image imgTabMitigasi;
        [SerializeField] private Image imgTabFakta;
        [SerializeField] private TextMeshProUGUI txtTabDeskripsi;
        [SerializeField] private TextMeshProUGUI txtTabHabitat;
        [SerializeField] private TextMeshProUGUI txtTabMitigasi;
        [SerializeField] private TextMeshProUGUI txtTabFakta;

        [Header("Bottom Info Panel - Content")]
        [SerializeField] private TextMeshProUGUI txtDetailContent;
        [SerializeField] private ScrollRect scrollContent;

        [Header("Data")]
        [SerializeField] private AnimalDataSO currentAnimal;
        [SerializeField] private AnimalDataSO[] allAnimals;

        public AnimalDataSO[] AllAnimals => allAnimals;
        public AnimalDataSO CurrentAnimal => currentAnimal;

        private AudioSource audioSource;
        private int currentTabIndex = 0;
        private bool showDebugOverlay = false;

        private static readonly Color TabActiveColor   = new Color(0.02f, 0.40f, 0.28f, 1f);
        private static readonly Color TabInactiveColor = new Color(0.90f, 0.96f, 0.93f, 1f);
        private static readonly Color TabTextActive    = Color.white;
        private static readonly Color TabTextInactive  = new Color(0.04f, 0.25f, 0.18f, 1f);

        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;

            if (trackingManager == null)
            {
                trackingManager = FindAnyObjectByType<ARModelTrackingManager>();
                if (trackingManager == null)
                {
                    GameObject tmObj = new GameObject("ARModelTrackingManager");
                    trackingManager = tmObj.AddComponent<ARModelTrackingManager>();
                }
            }
        }

        private void Start()
        {
            if (btnKembali != null) btnKembali.onClick.AddListener(OnKembaliClicked);
            if (btnVoiceover != null) btnVoiceover.onClick.AddListener(OnVoiceoverClicked);
            if (btnToggleDebug != null) btnToggleDebug.onClick.AddListener(ToggleDebugOverlay);

            if (tabDeskripsi != null) tabDeskripsi.onClick.AddListener(() => SetTab(0));
            if (tabHabitat != null) tabHabitat.onClick.AddListener(() => SetTab(1));
            if (tabMitigasi != null) tabMitigasi.onClick.AddListener(() => SetTab(2));
            if (tabFakta != null) tabFakta.onClick.AddListener(() => SetTab(3));

            if (trackingManager != null)
            {
                trackingManager.OnStateChanged += HandleARStateChanged;
            }

            LoadSelectedAnimal();
            UpdateUI();

            // Sync UI ke state AR — bottomInfoCard tampil setelah model placed
            bool alreadyPlaced = trackingManager != null && trackingManager.HasPlacedModel;
            if (bottomInfoCard != null) bottomInfoCard.SetActive(alreadyPlaced);

            if (trackingManager != null && currentAnimal != null)
            {
                trackingManager.SetCurrentAnimal(currentAnimal);
            }

            SetTab(0);

#if UNITY_EDITOR
            // In Editor, show debug info button by default
            if (btnToggleDebug != null) btnToggleDebug.gameObject.SetActive(true);
#endif
        }

        private void OnDestroy()
        {
            if (trackingManager != null)
            {
                trackingManager.OnStateChanged -= HandleARStateChanged;
            }
        }

        private void Update()
        {
            UpdateLiveDebugInfo();
        }

        private void LoadSelectedAnimal()
        {
            string selectedCode = PlayerPrefs.GetString("SelectedSatwaCode", "SATWA01");
            Debug.Log($"[ANIMAL] SelectedSatwaCode = {selectedCode}");

            if (allAnimals != null && allAnimals.Length > 0)
            {
                foreach (var animal in allAnimals)
                {
                    if (animal != null && animal.animalCode == selectedCode)
                    {
                        currentAnimal = animal;
                        Debug.Log($"[ANIMAL] Controller.CurrentAnimal = {currentAnimal.animalCode} ({currentAnimal.commonName})");
                        return;
                    }
                }
            }

            // Fallback from Resources
            if (currentAnimal == null)
            {
                var resList = Resources.LoadAll<AnimalDataSO>("Data/Animals");
                if (resList != null && resList.Length > 0)
                {
                    allAnimals = resList;
                    foreach (var animal in resList)
                    {
                        if (animal != null && animal.animalCode == selectedCode)
                        {
                            currentAnimal = animal;
                            Debug.Log($"[ANIMAL] Controller.CurrentAnimal from Resources = {currentAnimal.animalCode} ({currentAnimal.commonName})");
                            return;
                        }
                    }
                    if (currentAnimal == null) currentAnimal = resList[0];
                }
            }

#if UNITY_EDITOR
            if (currentAnimal == null || currentAnimal.animalCode != selectedCode)
            {
                string[] guids = UnityEditor.AssetDatabase.FindAssets(selectedCode, new[] { "Assets/Resources/Data/Animals", "Assets/Data/Animals" });
                if (guids.Length > 0)
                {
                    string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[0]);
                    currentAnimal = UnityEditor.AssetDatabase.LoadAssetAtPath<AnimalDataSO>(path);
                }
            }
#endif
            if (currentAnimal != null)
            {
                Debug.Log($"[ANIMAL] Controller.CurrentAnimal = {currentAnimal.animalCode} ({currentAnimal.commonName})");
            }
            else
            {
                Debug.LogError($"[ANIMAL] Failed to resolve animal data for '{selectedCode}'!");
            }
        }

        public void SetAnimalData(AnimalDataSO data)
        {
            if (data == null) return;
            currentAnimal = data;
            PlayerPrefs.SetString("SelectedSatwaCode", data.animalCode);
            PlayerPrefs.Save();

            Debug.Log($"[ANIMAL] SetAnimalData called: {data.animalCode} ({data.commonName})");
            UpdateUI();
            if (trackingManager != null)
            {
                trackingManager.SetCurrentAnimal(data);
            }
            SetTab(0);
        }

        private void HandleARStateChanged(ARPlacementState state, string message)
        {
            bool isTrackingOk = (state == ARPlacementState.Tracking || state == ARPlacementState.SurfaceDetected);
            SetScanningPrompt(message, isTrackingOk);

            if (state == ARPlacementState.SearchingForSurface)
            {
                if (bottomInfoCard != null) bottomInfoCard.SetActive(false);
            }
            else if (state == ARPlacementState.Tracking)
            {
                if (bottomInfoCard != null) bottomInfoCard.SetActive(true);
            }
        }

        public void SetScanningPrompt(string message, bool isTrackingOk = true)
        {
            if (txtScanningPrompt != null) txtScanningPrompt.text = $"<b><color=#FFFFFF>{message}</color></b>";
            if (imgTrackingPill != null)
            {
                imgTrackingPill.color = new Color(0.08f, 0.12f, 0.14f, 0.85f); // Frosted dark glass
            }
        }

        public void ToggleDebugOverlay()
        {
            showDebugOverlay = !showDebugOverlay;
            if (debugOverlayPanel != null) debugOverlayPanel.SetActive(showDebugOverlay);
        }

        private void UpdateLiveDebugInfo()
        {
            if (txtDebugInfo == null || (!showDebugOverlay && !Application.isEditor)) return;

            string animalName = currentAnimal != null ? $"{currentAnimal.animalCode} - {currentAnimal.commonName}" : "None";
            string prefabName = (currentAnimal != null && currentAnimal.modelPrefab != null) ? currentAnimal.modelPrefab.name : "None";
            bool prefabLoaded = trackingManager != null && trackingManager.SpawnedModel != null;
            string planeId = trackingManager != null ? trackingManager.CurrentPlaneId : "None";
            string trackStatus = trackingManager != null ? trackingManager.TrackingStatus : "Searching";
            string stateStr = trackingManager != null ? trackingManager.State.ToString() : "Unknown";

            Vector3 curScale = trackingManager != null && trackingManager.SpawnedModel != null ? trackingManager.SpawnedModel.transform.localScale : Vector3.zero;
            Vector3 initScale = trackingManager != null ? trackingManager.InitialScale : Vector3.zero;
            Vector3 pos = trackingManager != null && trackingManager.SpawnedModel != null ? trackingManager.SpawnedModel.transform.position : Vector3.zero;
            Bounds bounds = trackingManager != null ? trackingManager.ModelBounds : new Bounds();

            txtDebugInfo.text = $"<b>[AR MODEL LIVE TRACKING HUD]</b>\n" +
                               $"• <b>Satwa:</b> {animalName}\n" +
                               $"• <b>Prefab:</b> {prefabName} (Loaded: {(prefabLoaded ? "<color=#10B981>YES</color>" : "<color=#EF4444>NO</color>")})\n" +
                               $"• <b>State:</b> <color=#0284C7>{stateStr}</color>\n" +
                               $"• <b>Plane ID:</b> {planeId}\n" +
                               $"• <b>Tracking:</b> {trackStatus}\n" +
                               $"• <b>Renderers:</b> {(trackingManager != null ? trackingManager.RendererCount : 0)} active meshes\n" +
                               $"• <b>Bounds:</b> W:{bounds.size.x:F2}m H:{bounds.size.y:F2}m D:{bounds.size.z:F2}m\n" +
                               $"• <b>Initial Scale:</b> {initScale.x:F3} | <b>Cur Scale:</b> {curScale.x:F3}\n" +
                               $"• <b>World Pos:</b> ({pos.x:F2}, {pos.y:F2}, {pos.z:F2})\n" +
                               $"• <i>Controls: [WASD+RMB] Move Cam | [P/O] Sim Tracking Lost/Recover</i>";
        }

        private void UpdateUI()
        {
            if (currentAnimal == null) return;

            if (txtCommonName != null) txtCommonName.text = $"<b>{currentAnimal.commonName}</b>";
            if (txtLatinName != null) txtLatinName.text = $"<i>{currentAnimal.latinName}</i>";

            if (txtStatusBadge != null)
            {
                txtStatusBadge.text = $"<b>● {GetStatusLabel(currentAnimal.iucnStatus)}</b>";
                txtStatusBadge.color = GetStatusTextColor(currentAnimal.iucnStatus);
            }

            if (imgStatusBadge != null)
            {
                imgStatusBadge.color = GetStatusBadgeColor(currentAnimal.iucnStatus);
            }
        }

        public void SetTab(int tabIndex)
        {
            currentTabIndex = tabIndex;
            if (txtDetailContent == null || currentAnimal == null) return;

            if (tabIndex == 0) txtDetailContent.text = BuildDeskripsiText();
            else if (tabIndex == 1) txtDetailContent.text = BuildHabitatText();
            else if (tabIndex == 2) txtDetailContent.text = BuildMitigasiText();
            else if (tabIndex == 3) txtDetailContent.text = BuildFaktaText();

            if (scrollContent != null) scrollContent.verticalNormalizedPosition = 1f;

            UpdateTabButtonStyles();
        }

        private string BuildDeskripsiText()
        {
            string s = "";
            if (!string.IsNullOrEmpty(currentAnimal.description))
                s += $"<b>Deskripsi Satwa:</b>\n{currentAnimal.description}\n\n";
            if (!string.IsNullOrEmpty(currentAnimal.diet))
                s += $"<b>Pola Makan / Diet:</b>\n{currentAnimal.diet}\n\n";
            if (!string.IsNullOrEmpty(currentAnimal.daerahAsal))
                s += $"<b>Daerah Asal:</b>\n{currentAnimal.daerahAsal}";
            return s.TrimEnd();
        }

        private string BuildHabitatText()
        {
            string s = "";
            if (!string.IsNullOrEmpty(currentAnimal.daerahAsal))
                s += $"<b>Sebaran Wilayah:</b>\n{currentAnimal.daerahAsal}\n\n";
            if (!string.IsNullOrEmpty(currentAnimal.habitat))
                s += $"<b>Tipe Ekosistem:</b>\n{currentAnimal.habitat}";
            return s.TrimEnd();
        }

        private string BuildMitigasiText()
        {
            string s = "";
            if (!string.IsNullOrEmpty(currentAnimal.tingkatBahaya))
                s += $"<b>Tingkat Bahaya:</b>\n{currentAnimal.tingkatBahaya}\n\n";
            if (!string.IsNullOrEmpty(currentAnimal.tindakanSaatBertemu))
                s += $"<b>Panduan Keselamatan & Mitigasi:</b>\n{currentAnimal.tindakanSaatBertemu}";
            return s.TrimEnd();
        }

        private string BuildFaktaText()
        {
            string s = "";
            if (!string.IsNullOrEmpty(currentAnimal.funFact))
            {
                s += $"<b>Fakta Unik Satwa:</b>\n{currentAnimal.funFact}";
            }
            return s.TrimEnd();
        }

        private void UpdateTabButtonStyles()
        {
            SetTabVisual(tabDeskripsi, imgTabDeskripsi, txtTabDeskripsi, currentTabIndex == 0);
            SetTabVisual(tabHabitat, imgTabHabitat, txtTabHabitat, currentTabIndex == 1);
            SetTabVisual(tabMitigasi, imgTabMitigasi, txtTabMitigasi, currentTabIndex == 2);
            SetTabVisual(tabFakta, imgTabFakta, txtTabFakta, currentTabIndex == 3);
        }

        private void SetTabVisual(Button btn, Image bg, TextMeshProUGUI txt, bool isActive)
        {
            if (bg != null) bg.color = isActive ? TabActiveColor : TabInactiveColor;
            if (txt != null) txt.color = isActive ? TabTextActive : TabTextInactive;
        }

        private void OnVoiceoverClicked()
        {
            if (currentAnimal == null || currentAnimal.animalSound == null)
            {
                Debug.LogWarning("<b>[AUDIO]</b> No animalSound clip assigned!");
                return;
            }

            if (audioSource != null)
            {
                if (audioSource.isPlaying)
                {
                    audioSource.Stop();
                    if (txtVoiceoverBtn != null) txtVoiceoverBtn.text = "<b><color=#064E3B>▶ Suara</color></b>";
                }
                else
                {
                    if (UIAudioManager.Instance != null)
                    {
                        UIAudioManager.Instance.PlayAnimalVoiceover(currentAnimal.animalSound);
                    }
                    else
                    {
                        audioSource.clip = currentAnimal.animalSound;
                        audioSource.Play();
                    }

                    if (txtVoiceoverBtn != null) txtVoiceoverBtn.text = "<b><color=#064E3B>■ Stop</color></b>";
                    StartCoroutine(ResetVoiceoverBtnRoutine(currentAnimal.animalSound.length));
                }
            }
        }

        private IEnumerator ResetVoiceoverBtnRoutine(float duration)
        {
            yield return new WaitForSeconds(duration + 0.2f);
            if (txtVoiceoverBtn != null) txtVoiceoverBtn.text = "<b><color=#064E3B>▶ Suara</color></b>";
        }

        private void OnKembaliClicked()
        {
            if (audioSource != null && audioSource.isPlaying) audioSource.Stop();
            SceneManager.LoadScene("PilihHewan");
        }

        private string GetStatusLabel(ConservationStatus status)
        {
            switch (status)
            {
                case ConservationStatus.CriticallyEndangered: return "Kritis (CR)";
                case ConservationStatus.Endangered: return "Terancam (EN)";
                case ConservationStatus.Vulnerable: return "Rentan (VU)";
                case ConservationStatus.LeastConcern: return "Risiko Rendah (LC)";
                default: return "Dilindungi";
            }
        }

        private Color GetStatusBadgeColor(ConservationStatus status)
        {
            switch (status)
            {
                case ConservationStatus.CriticallyEndangered: return new Color(0.99f, 0.88f, 0.88f, 1f); // #FEE2E2
                case ConservationStatus.Endangered: return new Color(1.0f, 0.92f, 0.82f, 1f);           // #FFEDD5
                case ConservationStatus.Vulnerable: return new Color(1.0f, 0.95f, 0.78f, 1f);           // #FEF3C7
                default: return new Color(0.86f, 0.98f, 0.90f, 1f);                              // #DCFCE7
            }
        }

        // =====================================================================

        private Color GetStatusTextColor(ConservationStatus status)
        {
            switch (status)
            {
                case ConservationStatus.CriticallyEndangered: return new Color(0.60f, 0.05f, 0.05f, 1f);
                case ConservationStatus.Endangered: return new Color(0.60f, 0.20f, 0.05f, 1f);
                case ConservationStatus.Vulnerable: return new Color(0.55f, 0.25f, 0.05f, 1f);
                default: return new Color(0.09f, 0.40f, 0.20f, 1f);
            }
        }
    }
}
