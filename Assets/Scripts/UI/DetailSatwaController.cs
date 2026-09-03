using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using SatwaLangka.Data;

namespace SatwaLangka.UI
{
    public class DetailSatwaController : MonoBehaviour
    {
        [Header("Top Navigation")]
        [SerializeField] private Button btnKembali;
        [SerializeField] private Button btnMenu;

        [Header("3D Animal Stage")]
        [SerializeField] private Transform animalSpawnParent;
        [SerializeField] private float autoRotateSpeed = 12f;

        [Header("Info Panel Header")]
        [SerializeField] private TextMeshProUGUI txtCommonName;
        [SerializeField] private TextMeshProUGUI txtLatinName;
        [SerializeField] private TextMeshProUGUI txtStatusBadge;
        [SerializeField] private Image imgStatusBadge;
        [SerializeField] private TextMeshProUGUI txtDangerBadge;
        [SerializeField] private Image imgDangerBadge;
        [SerializeField] private Button btnPlayAudio;

        [Header("Tabs & Dynamic Content")]
        [SerializeField] private Button tabDeskripsi;
        [SerializeField] private Button tabHabitat;
        [SerializeField] private Button tabMitigasi;
        [SerializeField] private Button tabFakta;

        [SerializeField] private Image imgTabDeskripsi;
        [SerializeField] private Image imgTabHabitat;
        [SerializeField] private Image imgTabMitigasi;
        [SerializeField] private Image imgTabFakta;

        [SerializeField] private TextMeshProUGUI txtContentBody;

        [Header("Data")]
        [SerializeField] private AnimalDataSO currentAnimal;
        [SerializeField] private AnimalDataSO[] allAnimals;

        private GameObject spawnedModel;
        private int currentTabIndex = 0;

        private void Start()
        {
            if (btnKembali != null) btnKembali.onClick.AddListener(OnKembaliClicked);
            if (btnMenu != null) btnMenu.onClick.AddListener(OnMenuClicked);
            if (btnPlayAudio != null) btnPlayAudio.onClick.AddListener(OnPlayAudioClicked);

            SetupTabs();
            LoadSelectedAnimal();
            UpdateUI();
            ShowTabContent(0); // Default to Deskripsi
        }

        public void SetAnimalData(AnimalDataSO data)
        {
            currentAnimal = data;
            SpawnAnimalModel();
            UpdateUI();
            ShowTabContent(0);
        }

        private void Update()
        {
            if (spawnedModel != null)
            {
                spawnedModel.transform.Rotate(Vector3.up, autoRotateSpeed * Time.deltaTime, Space.World);
            }
        }

        private void LoadSelectedAnimal()
        {
            string selectedCode = PlayerPrefs.GetString("SelectedSatwaCode", "SATWA01");

            // 1. Search in allAnimals array
            if (allAnimals != null && allAnimals.Length > 0)
            {
                foreach (var animal in allAnimals)
                {
                    if (animal != null && animal.animalCode == selectedCode)
                    {
                        currentAnimal = animal;
                        SpawnAnimalModel();
                        return;
                    }
                }
            }

            // 2. Fallback in Resources
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
                            SpawnAnimalModel();
                            return;
                        }
                    }
                    if (currentAnimal == null) currentAnimal = resList[0];
                }
            }

            // 3. Fallback in Editor
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
            SpawnAnimalModel();
        }

        private void SpawnAnimalModel()
        {
            if (animalSpawnParent != null)
            {
                for (int i = animalSpawnParent.childCount - 1; i >= 0; i--)
                {
                    var child = animalSpawnParent.GetChild(i);
                    if (child != null)
                    {
                        if (Application.isPlaying) Destroy(child.gameObject);
                        else DestroyImmediate(child.gameObject);
                    }
                }
            }

            if (currentAnimal == null || currentAnimal.modelPrefab == null || animalSpawnParent == null) return;

            spawnedModel = Instantiate(currentAnimal.modelPrefab, animalSpawnParent);
            spawnedModel.name = $"DetailStage_{currentAnimal.animalCode}_{currentAnimal.commonName}";
            float baseAngle = GetCalibratedBaseAngle(currentAnimal != null ? currentAnimal.animalCode : "");
            spawnedModel.transform.localPosition = new Vector3(0f, 0.015f, 0f);
            spawnedModel.transform.localRotation = Quaternion.Euler(0f, baseAngle, 0f);
            
            // Auto-scale to prominent size so it fills the showcase frame clearly
            Renderer[] renderers = spawnedModel.GetComponentsInChildren<Renderer>(true);
            if (renderers != null && renderers.Length > 0)
            {
                Bounds rawB = renderers[0].bounds;
                for (int i = 1; i < renderers.Length; i++) rawB.Encapsulate(renderers[i].bounds);
                float maxDim = Mathf.Max(rawB.size.x, rawB.size.y, rawB.size.z);
                if (maxDim > 0.001f)
                {
                    float targetShowcaseSize = 1.65f; // Prominent, besar, jelas dan megah di frame
                    spawnedModel.transform.localScale = Vector3.one * (targetShowcaseSize / maxDim);
                }

                Physics.SyncTransforms();
                Bounds b = renderers[0].bounds;
                for (int i = 1; i < renderers.Length; i++) b.Encapsulate(renderers[i].bounds);
                float pedTopY = animalSpawnParent.position.y + 0.015f;
                float diffY = pedTopY - b.min.y;
                float diffX = animalSpawnParent.position.x - b.center.x;
                float diffZ = animalSpawnParent.position.z - b.center.z;
                spawnedModel.transform.position += new Vector3(diffX, diffY, diffZ);
            }

            // Ensure TouchManipulator attached
            if (spawnedModel.GetComponent<SatwaLangka.AR.TouchManipulator>() == null)
            {
                spawnedModel.AddComponent<SatwaLangka.AR.TouchManipulator>();
            }
        }

        private float GetCalibratedBaseAngle(string code)
        {
            switch (code)
            {
                case "SATWA01": return 135f; // Gajah Sumatra
                case "SATWA02": return 270f; // Banteng Jawa
                case "SATWA03": return 0f;   // Anoa
                case "SATWA04": return 180f; // Babirusa
                case "SATWA05": return 270f; // Sanca Batik
                case "SATWA06": return 270f; // Kura Moncong Babi
                case "SATWA07": return 270f; // Kura Leher Ular
                case "SATWA08": return 0f;   // Trenggiling
                case "SATWA09": return 0f;   // Macan Tutul Jawa
                case "SATWA10": return 45f;  // Rusa Timor
                case "SATWA11": return 0f;   // Sigung
                case "SATWA12": return 315f; // Bekantan
                default:        return 0f;
            }
        }

        private void SetupTabs()
        {
            if (tabDeskripsi != null) tabDeskripsi.onClick.AddListener(() => ShowTabContent(0));
            if (tabHabitat != null) tabHabitat.onClick.AddListener(() => ShowTabContent(1));
            if (tabMitigasi != null) tabMitigasi.onClick.AddListener(() => ShowTabContent(2));
            if (tabFakta != null) tabFakta.onClick.AddListener(() => ShowTabContent(3));
        }

        public void ShowTabContent(int tabIndex)
        {
            if (currentTabIndex != tabIndex) UIAudioManager.Instance?.PlayClick();
            currentTabIndex = tabIndex;
            HighlightTab(tabIndex);

            if (currentAnimal == null || txtContentBody == null) return;

            switch (tabIndex)
            {
                case 0: // Deskripsi & Ciri Fisik
                    txtContentBody.text = $"<color=#047857><b>■ PERAN EKOLOGIS & KARAKTERISTIK FISIK:</b></color>\n{currentAnimal.description}\n\n<color=#047857><b>■ POLA MAKAN / DIET:</b></color>\n{currentAnimal.diet}";
                    break;
                case 1: // Habitat & Daerah Asal
                    txtContentBody.text = $"<color=#047857><b>■ DAERAH ASAL PERSEBARAN:</b></color>\n{currentAnimal.daerahAsal}\n\n<color=#047857><b>■ HABITAT ASLI & BIOMA:</b></color>\n{currentAnimal.habitat}\n\n<i>Sumber data: Rujukan Resmi BRIN & KLHK RI.</i>";
                    break;
                case 2: // Status & Tindakan Saat Bertemu (Mitigasi)
                    string bahayaColor = GetDangerColorHex(currentAnimal.tingkatBahaya);
                    txtContentBody.text = $"<color={bahayaColor}><b>■ TINGKAT BAHAYA: {currentAnimal.tingkatBahaya.ToUpper()}</b></color>\nStatus Konservasi: <b>{GetStatusLabel(currentAnimal.iucnStatus)}</b>\n\n<color=#D97706><b>■ TINDAKAN SAAT BERTEMU DI ALAM LIAR:</b></color>\n{currentAnimal.tindakanSaatBertemu}";
                    break;
                case 3: // Fakta Unik
                    txtContentBody.text = $"<color=#047857><b>■ FAKTA MENARIK & KEUNIKAN SATWA:</b></color>\n• {currentAnimal.funFact}\n\n<color=#047857><b>■ STATUS PERLINDUNGAN:</b></color>\nDilindungi penuh oleh Undang-Undang Republik Indonesia Nomor 5 Tahun 1990 tentang Konservasi Sumber Daya Alam Hayati.";
                    break;
            }
        }

        private void HighlightTab(int activeIndex)
        {
            Color activeColor = new Color(0.04f, 0.47f, 0.34f, 1f); // #047857 Forest Emerald
            Color inactiveColor = new Color(0.92f, 0.96f, 0.94f, 1f);

            if (imgTabDeskripsi != null) imgTabDeskripsi.color = (activeIndex == 0) ? activeColor : inactiveColor;
            if (imgTabHabitat != null) imgTabHabitat.color = (activeIndex == 1) ? activeColor : inactiveColor;
            if (imgTabMitigasi != null) imgTabMitigasi.color = (activeIndex == 2) ? activeColor : inactiveColor;
            if (imgTabFakta != null) imgTabFakta.color = (activeIndex == 3) ? activeColor : inactiveColor;

            SetTabLabelColor(tabDeskripsi, activeIndex == 0);
            SetTabLabelColor(tabHabitat, activeIndex == 1);
            SetTabLabelColor(tabMitigasi, activeIndex == 2);
            SetTabLabelColor(tabFakta, activeIndex == 3);
        }

        private void SetTabLabelColor(Button tab, bool active)
        {
            if (tab == null) return;
            var label = tab.GetComponentInChildren<TextMeshProUGUI>();
            if (label != null) label.color = active ? Color.white : new Color(0.06f, 0.28f, 0.22f, 1f);
        }

        private void UpdateUI()
        {
            if (currentAnimal == null) return;

            if (txtCommonName != null) txtCommonName.text = $"<b>{currentAnimal.commonName}</b>";
            if (txtLatinName != null) txtLatinName.text = $"<i>{currentAnimal.latinName}</i>";

            if (txtStatusBadge != null)
            {
                txtStatusBadge.text = $"● {GetStatusLabel(currentAnimal.iucnStatus)}";
            }
            if (imgStatusBadge != null)
            {
                imgStatusBadge.color = GetStatusColor(currentAnimal.iucnStatus);
            }

            if (txtDangerBadge != null && !string.IsNullOrEmpty(currentAnimal.tingkatBahaya))
            {
                txtDangerBadge.text = $"• {currentAnimal.tingkatBahaya}";
            }
        }

        public void OnKembaliClicked()
        {
            UIAudioManager.Instance?.PlayClick();
            Debug.Log("<b>[DETAIL SATWA]</b> Returning to previous screen...");
            if (Application.CanStreamedLevelBeLoaded("ScanPlaneDetection"))
            {
                SceneManager.LoadScene("ScanPlaneDetection");
            }
            else if (Application.CanStreamedLevelBeLoaded("PilihHewan"))
            {
                SceneManager.LoadScene("PilihHewan");
            }
            else
            {
                SceneManager.LoadScene("MainMenu");
            }
        }

        public void OnMenuClicked()
        {
            UIAudioManager.Instance?.PlayClick();
            Debug.Log("<b>[DETAIL SATWA]</b> Returning to MainMenu...");
            if (Application.CanStreamedLevelBeLoaded("MainMenu"))
            {
                SceneManager.LoadScene("MainMenu");
            }
        }

        public void OnPlayAudioClicked()
        {
            if (currentAnimal != null && currentAnimal.animalSound != null)
            {
                if (UIAudioManager.Instance != null)
                {
                    UIAudioManager.Instance.PlayAnimalSound(currentAnimal.animalSound);
                }
                else
                {
                    AudioSource audio = GetComponent<AudioSource>();
                    if (audio == null) audio = gameObject.AddComponent<AudioSource>();
                    audio.spatialBlend = 0f;
                    audio.PlayOneShot(currentAnimal.animalSound);
                }
                Debug.Log($"<b>[AUDIO]</b> Playing sound for {currentAnimal.commonName}");
            }
            else
            {
                Debug.Log($"<b>[AUDIO]</b> Suara simulasi karakteristik {currentAnimal?.commonName} aktif!");
            }
        }

        private string GetStatusLabel(ConservationStatus status)
        {
            switch (status)
            {
                case ConservationStatus.CriticallyEndangered: return "Kritis (CR)";
                case ConservationStatus.Endangered: return "Terancam (EN)";
                case ConservationStatus.Vulnerable: return "Rentan (VU)";
                default: return "Risiko Rendah (LC)";
            }
        }

        private Color GetStatusColor(ConservationStatus status)
        {
            switch (status)
            {
                case ConservationStatus.CriticallyEndangered: return new Color(0.99f, 0.90f, 0.90f, 1.0f);
                case ConservationStatus.Endangered: return new Color(0.99f, 0.93f, 0.85f, 1.0f);
                case ConservationStatus.Vulnerable: return new Color(0.99f, 0.96f, 0.85f, 1.0f);
                default: return new Color(0.88f, 0.96f, 0.92f, 1.0f);
            }
        }

        private string GetDangerColorHex(string danger)
        {
            if (string.IsNullOrEmpty(danger)) return "#047857";
            if (danger.Contains("Sangat") || danger.Contains("Kritis")) return "#DC2626";
            if (danger.Contains("Sedang") || danger.Contains("Cukup") || danger.Contains("Tinggi")) return "#EA580C";
            return "#047857";
        }
    }
}
