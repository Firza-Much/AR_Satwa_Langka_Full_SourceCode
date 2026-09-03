using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using SatwaLangka.Data;

namespace SatwaLangka.UI
{
    public class PilihHewanController : MonoBehaviour
    {
        [Header("Top Navigation")]
        [SerializeField] private Button btnKembali;
        [SerializeField] private TextMeshProUGUI txtHeaderTitle;
        [SerializeField] private TextMeshProUGUI txtHeaderSubtitle;

        [Header("Category Filter Buttons (Optional)")]
        [SerializeField] private Button btnFilterAll;
        [SerializeField] private Button btnFilterKulitTebal;
        [SerializeField] private Button btnFilterCangkang;
        [SerializeField] private Button btnFilterMamalia;

        [Header("Animal Data & Grid")]
        [SerializeField] private List<AnimalDataSO> allAnimalData = new List<AnimalDataSO>();
        public List<AnimalDataSO> AllAnimals { get => allAnimalData; set => allAnimalData = value; }
        [SerializeField] private Transform gridContentParent;
        [SerializeField] private GameObject animalCardPrefab;

        private List<GameObject> spawnedCards = new List<GameObject>();
        private SatwaCategory? activeCategoryFilter = null;
        private int lastScreenWidth = -1;
        private int lastScreenHeight = -1;

        private void Start()
        {
            if (btnKembali != null) btnKembali.onClick.AddListener(OnKembaliClicked);

            UpdateOrientationLayout(true);
            SetupFilterListeners();
            PopulateAnimalGrid();
        }

        private void Update()
        {
            if (Screen.width != lastScreenWidth || Screen.height != lastScreenHeight)
            {
                UpdateOrientationLayout(false);
            }
        }

        private void UpdateOrientationLayout(bool force)
        {
            lastScreenWidth = Screen.width;
            lastScreenHeight = Screen.height;

            if (gridContentParent == null) return;
            var grid = gridContentParent.GetComponent<GridLayoutGroup>();
            if (grid == null) return;

            // 2-Column Vertical Portrait Cards (484x640) - Fills entire screen with large, readable content
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 2;
            grid.cellSize = new Vector2(484, 640);
            grid.spacing = new Vector2(24, 36);
        }

        private void SetupFilterListeners()
        {
            if (btnFilterAll != null) btnFilterAll.onClick.AddListener(() => SetFilter(null));
            if (btnFilterKulitTebal != null) btnFilterKulitTebal.onClick.AddListener(() => SetFilter(SatwaCategory.KulitTebal));
            if (btnFilterCangkang != null) btnFilterCangkang.onClick.AddListener(() => SetFilter(SatwaCategory.BercangkangDanBersisik));
            if (btnFilterMamalia != null) btnFilterMamalia.onClick.AddListener(() => SetFilter(SatwaCategory.MamaliaBerbuluPendek));
        }

        public void SetFilter(SatwaCategory? category)
        {
            activeCategoryFilter = category;
            PopulateAnimalGrid();
        }

        public void PopulateAnimalGrid()
        {
            // Ensure allAnimalData is populated with multi-tier fallback
            if (allAnimalData == null || allAnimalData.Count == 0)
            {
                var resList = Resources.LoadAll<AnimalDataSO>("Data/Animals");
                if (resList == null || resList.Length == 0)
                {
                    resList = Resources.LoadAll<AnimalDataSO>("");
                }
                if (resList != null && resList.Length > 0)
                {
                    allAnimalData = new List<AnimalDataSO>(resList);
                }
#if UNITY_EDITOR
                if (allAnimalData == null || allAnimalData.Count == 0)
                {
                    string[] guids = UnityEditor.AssetDatabase.FindAssets("t:AnimalDataSO", new[] { "Assets/Resources/Data/Animals", "Assets/Data/Animals" });
                    List<AnimalDataSO> editorList = new List<AnimalDataSO>();
                    foreach (var g in guids)
                    {
                        string p = UnityEditor.AssetDatabase.GUIDToAssetPath(g);
                        var data = UnityEditor.AssetDatabase.LoadAssetAtPath<AnimalDataSO>(p);
                        if (data != null) editorList.Add(data);
                    }
                    if (editorList.Count > 0) allAnimalData = editorList;
                }
#endif
                if (allAnimalData != null && allAnimalData.Count > 0)
                {
                    allAnimalData.Sort((a, b) => string.Compare(a.animalCode, b.animalCode, System.StringComparison.Ordinal));
                }
            }

            // Try Inspector reference first
            // Fallback 1: Resources.Load
            if (animalCardPrefab == null)
                animalCardPrefab = Resources.Load<GameObject>("UI/Animal_Select_Card");

            // Fallback 2: Check in Resources prefabs folder
            if (animalCardPrefab == null)
            {
                animalCardPrefab = Resources.Load<GameObject>("Prefabs/Animal_Select_Card");
            }

            // Fallback 3: Create minimal card prefab from code if nothing else works
            if (animalCardPrefab == null)
                animalCardPrefab = CreateFallbackCardPrefab();

            Debug.Log("[PILIH] PopulateAnimalGrid: prefab=" + (animalCardPrefab?.name ?? "NULL") + " grid=" + (gridContentParent?.name ?? "NULL") + " data=" + (allAnimalData?.Count ?? 0));

            if (gridContentParent == null || animalCardPrefab == null) return;

            // Clear previous cards cleanly
            for (int i = gridContentParent.childCount - 1; i >= 0; i--)
            {
                var child = gridContentParent.GetChild(i);
                if (child != null)
                {
                    if (Application.isPlaying) Destroy(child.gameObject);
                    else DestroyImmediate(child.gameObject);
                }
            }
            spawnedCards.Clear();

            foreach (var data in allAnimalData)
            {
                if (data == null) continue;

                // Apply category filter
                if (activeCategoryFilter.HasValue && data.category != activeCategoryFilter.Value)
                {
                    continue;
                }

                GameObject cardObj = Instantiate(animalCardPrefab, gridContentParent);
                spawnedCards.Add(cardObj);

                // Disable raycast target only on child texts and non-interactive decorative images
                foreach (var txt in cardObj.GetComponentsInChildren<TextMeshProUGUI>(true))
                {
                    txt.raycastTarget = false;
                }
                foreach (var img in cardObj.GetComponentsInChildren<Image>(true))
                {
                    // Keep raycastTarget true if this image belongs to a Button (e.g. root card or action button)
                    if (img.GetComponent<Button>() == null && img.transform.parent?.GetComponent<Button>() == null)
                    {
                        if (img.gameObject != cardObj) img.raycastTarget = false;
                    }
                }

                // Populate Card UI Elements with robust recursive lookup
                var txtName = cardObj.transform.Find("Txt_Name")?.GetComponent<TextMeshProUGUI>() 
                    ?? cardObj.transform.Find("InfoArea/Txt_Name")?.GetComponent<TextMeshProUGUI>();
                var txtLatin = cardObj.transform.Find("Txt_Latin")?.GetComponent<TextMeshProUGUI>() 
                    ?? cardObj.transform.Find("InfoArea/Txt_Latin")?.GetComponent<TextMeshProUGUI>();
                var txtCategory = cardObj.transform.Find("Txt_Category")?.GetComponent<TextMeshProUGUI>()
                    ?? cardObj.transform.Find("InfoArea/Txt_Category")?.GetComponent<TextMeshProUGUI>();
                var txtHabitat = cardObj.transform.Find("Txt_Habitat")?.GetComponent<TextMeshProUGUI>()
                    ?? cardObj.transform.Find("InfoArea/Txt_Habitat")?.GetComponent<TextMeshProUGUI>();
                var txtStatus = cardObj.transform.Find("Badge_Status/Text")?.GetComponent<TextMeshProUGUI>()
                    ?? cardObj.transform.Find("InfoArea/Badge_Status/Text")?.GetComponent<TextMeshProUGUI>()
                    ?? cardObj.transform.Find("Badge_Status")?.GetComponentInChildren<TextMeshProUGUI>();
                var imgBadge = cardObj.transform.Find("Badge_Status")?.GetComponent<Image>()
                    ?? cardObj.transform.Find("InfoArea/Badge_Status")?.GetComponent<Image>();
                var imgThumb = cardObj.transform.Find("ThumbContainer/Image")?.GetComponent<Image>()
                    ?? cardObj.transform.Find("ThumbContainer")?.GetComponentInChildren<Image>();
                var imgAccent = cardObj.transform.Find("Accent_Bar")?.GetComponent<Image>();

                // Thumbnail
                if (imgThumb != null && data.thumbnail != null)
                {
                    imgThumb.sprite = data.thumbnail;
                    imgThumb.color = Color.white;
                }

                // Texts
                if (txtName != null) txtName.text = $"<b>{data.commonName}</b>";
                if (txtLatin != null) txtLatin.text = $"<i>{data.latinName}</i>";
                
                if (txtCategory != null)
                {
                    txtCategory.text = $"<color={GetCategoryHex(data.category)}><b>{GetCategoryLabel(data.category)}</b></color>";
                }

                if (txtHabitat != null)
                {
                    string shortHab = !string.IsNullOrEmpty(data.habitat) ? data.habitat.Split(',')[0].Trim() : "Indonesia";
                    if (shortHab.Length > 16) shortHab = shortHab.Substring(0, 14) + "..";
                    txtHabitat.text = $"<color=#047857>• {shortHab}</color>";
                }

                // Status Badge & Accent Color
                if (txtStatus != null)
                {
                    txtStatus.text = GetStatusBadgeLabel(data.iucnStatus);
                    txtStatus.color = GetStatusTextColor(data.iucnStatus);
                }

                if (imgBadge != null)
                {
                    imgBadge.color = GetStatusBadgeBgColor(data.iucnStatus);
                }

                if (imgAccent != null)
                {
                    imgAccent.color = GetStatusAccentColor(data.iucnStatus);
                }

                // Interaction: Attach click listener to root card and ALL child buttons
                AnimalDataSO capturedData = data;
                var allButtons = cardObj.GetComponentsInChildren<Button>(true);
                if (allButtons != null && allButtons.Length > 0)
                {
                    foreach (var btn in allButtons)
                    {
                        btn.onClick.RemoveAllListeners();
                        btn.onClick.AddListener(() => OnAnimalSelected(capturedData));
                    }
                }
                else
                {
                    var btnCard = cardObj.AddComponent<Button>();
                    btnCard.onClick.AddListener(() => OnAnimalSelected(capturedData));
                }
            }
        }

        public void OnAnimalSelected(AnimalDataSO selectedData)
        {
            UIAudioManager.Instance?.PlayClick();

            Debug.Log($"[ANIMAL] SelectedSatwaCode = {selectedData.animalCode} ({selectedData.commonName})");
            PlayerPrefs.SetString("SelectedSatwaCode", selectedData.animalCode);
            PlayerPrefs.Save();

            // Load Stage 3: ScanPlaneDetection (or SampleScene fallback)
            if (Application.CanStreamedLevelBeLoaded("ScanPlaneDetection"))
            {
                SceneManager.LoadScene("ScanPlaneDetection");
            }
            else if (Application.CanStreamedLevelBeLoaded("SampleScene"))
            {
                SceneManager.LoadScene("SampleScene");
            }
        }

        public void OnKembaliClicked()
        {
            UIAudioManager.Instance?.PlayBack();

            Debug.Log("<b>[PILIH HEWAN]</b> Returning to MainMenu...");
            if (Application.CanStreamedLevelBeLoaded("MainMenu"))
            {
                SceneManager.LoadScene("MainMenu");
            }
        }

        private GameObject CreateFallbackCardPrefab()
        {
            var forest = new Color(0.055f, 0.478f, 0.353f, 1f);
            var dark   = new Color(0.13f, 0.13f, 0.15f, 1f);
            var gray   = new Color(0.5f,  0.5f,  0.5f,  1f);

            // Root card GO
            var card = new GameObject("Animal_Select_Card");
            var rt   = card.AddComponent<RectTransform>();
            var img  = card.AddComponent<UnityEngine.UI.Image>();
            img.color = new Color(1f, 1f, 1f, 0.97f);
            card.AddComponent<UnityEngine.UI.Button>();

            void MakeText(string goName, string defaultTxt, float ancMinX, float ancMinY,
                          float ancMaxX, float ancMaxY, float fontSize, bool bold,
                          Color color, TextAnchor align = TextAnchor.MiddleLeft)
            {
                var go = new GameObject(goName);
                go.transform.SetParent(card.transform, false);
                var t = go.AddComponent<TMPro.TextMeshProUGUI>();
                t.text = defaultTxt; t.fontSize = fontSize; t.color = color;
                t.fontStyle = bold ? TMPro.FontStyles.Bold : TMPro.FontStyles.Normal;
                t.overflowMode = TMPro.TextOverflowModes.Ellipsis;
                var r = go.GetComponent<RectTransform>();
                r.anchorMin = new Vector2(ancMinX, ancMinY);
                r.anchorMax = new Vector2(ancMaxX, ancMaxY);
                r.offsetMin = new Vector2(12, 2); r.offsetMax = new Vector2(-8, -2);
            }

            // Accent bar (left edge)
            var accentGO = new GameObject("Accent_Bar");
            accentGO.transform.SetParent(card.transform, false);
            var accentImg = accentGO.AddComponent<UnityEngine.UI.Image>();
            accentImg.color = forest;
            var accentRT = accentGO.GetComponent<RectTransform>();
            accentRT.anchorMin = Vector2.zero; accentRT.anchorMax = new Vector2(0.03f, 1f);
            accentRT.offsetMin = accentRT.offsetMax = Vector2.zero;

            // Animal name (largest, top 50%)
            MakeText("Txt_Name", "Nama Satwa", 0.04f, 0.5f, 1f, 1f, 30, true, dark);

            // Latin name (italic, 35-55%)
            MakeText("Txt_Latin", "Latin name", 0.04f, 0.28f, 1f, 0.52f, 21, false,
                     new Color(0.18f, 0.35f, 0.28f, 1f));

            // Badge container (bottom 30%)
            var badgeGO = new GameObject("Badge_Status");
            badgeGO.transform.SetParent(card.transform, false);
            var badgeImg = badgeGO.AddComponent<UnityEngine.UI.Image>();
            badgeImg.color = new Color(0.86f, 0.98f, 0.9f, 1f);
            var bRT = badgeGO.GetComponent<RectTransform>();
            bRT.anchorMin = new Vector2(0.04f, 0.02f);
            bRT.anchorMax = new Vector2(0.65f, 0.28f);
            bRT.offsetMin = bRT.offsetMax = Vector2.zero;

            var badgeTxtGO = new GameObject("Text");
            badgeTxtGO.transform.SetParent(badgeGO.transform, false);
            var badgeTxt = badgeTxtGO.AddComponent<TMPro.TextMeshProUGUI>();
            badgeTxt.text = "● LC"; badgeTxt.fontSize = 19;
            badgeTxt.fontStyle = TMPro.FontStyles.Bold;
            var bTRT = badgeTxtGO.GetComponent<RectTransform>();
            bTRT.anchorMin = Vector2.zero; bTRT.anchorMax = Vector2.one;
            bTRT.offsetMin = new Vector2(4,0); bTRT.offsetMax = new Vector2(-4,0);

            Debug.LogWarning("[PILIH] Using fallback card prefab (Inspector assignment missing)");
            return card;
        }

        private string GetCategoryLabel(SatwaCategory category)
        {
            switch (category)
            {
                case SatwaCategory.KulitTebal: return "SATWA KULIT TEBAL";
                case SatwaCategory.BercangkangDanBersisik: return "BERCANGKANG & BERSISIK";
                case SatwaCategory.MamaliaBerbuluPendek: return "MAMALIA BERBULU PENDEK";
                default: return "SATWA NUSANTARA";
            }
        }

        private string GetCategoryHex(SatwaCategory category)
        {
            switch (category)
            {
                case SatwaCategory.KulitTebal: return "#047857";
                case SatwaCategory.BercangkangDanBersisik: return "#0D9488";
                case SatwaCategory.MamaliaBerbuluPendek: return "#D97706";
                default: return "#059669";
            }
        }

        private string GetStatusBadgeLabel(ConservationStatus status)
        {
            switch (status)
            {
                case ConservationStatus.CriticallyEndangered: return "● Kritis (CR)";
                case ConservationStatus.Endangered: return "● Terancam (EN)";
                case ConservationStatus.Vulnerable: return "● Rentan (VU)";
                default: return "● Risiko Rendah (LC)";
            }
        }

        private Color GetStatusTextColor(ConservationStatus status)
        {
            switch (status)
            {
                case ConservationStatus.CriticallyEndangered: return new Color(0.6f, 0.1f, 0.1f, 1f);
                case ConservationStatus.Endangered: return new Color(0.57f, 0.25f, 0.05f, 1f);
                case ConservationStatus.Vulnerable: return new Color(0.52f, 0.3f, 0.05f, 1f);
                default: return new Color(0.08f, 0.4f, 0.2f, 1f);
            }
        }

        private Color GetStatusBadgeBgColor(ConservationStatus status)
        {
            switch (status)
            {
                case ConservationStatus.CriticallyEndangered: return new Color(0.99f, 0.88f, 0.88f, 1f); // Light pinkish-red
                case ConservationStatus.Endangered: return new Color(0.99f, 0.95f, 0.78f, 1f);           // Light amber
                case ConservationStatus.Vulnerable: return new Color(0.99f, 0.97f, 0.76f, 1f);           // Light yellow-gold
                default: return new Color(0.86f, 0.98f, 0.9f, 1f);                                       // Light emerald
            }
        }

        private Color GetStatusAccentColor(ConservationStatus status)
        {
            switch (status)
            {
                case ConservationStatus.CriticallyEndangered: return new Color(0.86f, 0.15f, 0.15f, 1f);
                case ConservationStatus.Endangered: return new Color(0.96f, 0.62f, 0.07f, 1f);
                case ConservationStatus.Vulnerable: return new Color(0.94f, 0.78f, 0.1f, 1f);
                default: return new Color(0.06f, 0.72f, 0.5f, 1f);
            }
        }
    }
}
