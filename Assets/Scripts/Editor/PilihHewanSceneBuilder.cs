using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using TMPro;
using SatwaLangka.Data;
using SatwaLangka.UI;

namespace SatwaLangka.EditorScripts
{
    public static class PilihHewanSceneBuilder
    {
        private static TMP_FontAsset appFont => AppTheme.Font;

        [MenuItem("Satwa Langka/Create & Open PilihHewan Scene (Vertical Portrait Cards)")]
        public static void BuildPilihHewanScene()
        {
            ConfigureImporters();

            string scenePath = "Assets/Scenes/PilihHewan.unity";
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            Camera cam = SetupCameraAndLighting();
            GameObject cardPrefab = BuildCardPrefab();
            BuildPilihHewanCanvas(cardPrefab, cam);

            EditorSceneManager.SaveScene(scene, scenePath);
            UpdateBuildSettings(scenePath);

            Debug.Log("<b>[SATWA AR]</b> PilihHewan scene built and saved at " + scenePath);
        }

        public static void ConfigureImporters()
        {
            AssetDatabase.Refresh();

            for (int i = 1; i <= 12; i++)
            {
                string path = $"Assets/Sprites/Animals/Thumb_SATWA{i:02d}.png";
                TextureImporter ti = AssetImporter.GetAtPath(path) as TextureImporter;
                if (ti != null)
                {
                    ti.textureType = TextureImporterType.Sprite;
                    ti.spriteImportMode = SpriteImportMode.Single;
                    ti.alphaIsTransparency = true;
                    ti.mipmapEnabled = false;
                    ti.filterMode = FilterMode.Bilinear;
                    ti.textureCompression = TextureImporterCompression.Uncompressed;
                    ti.SaveAndReimport();
                }
            }

            string[] uiSprites = {
                "Assets/Sprites/UI/Card_Exact.png",
                "Assets/Sprites/UI/Card_2Col_Premium.png",
                "Assets/Sprites/UI/Badge_CR_Kritis.png",
                "Assets/Sprites/UI/Badge_EN_Terancam.png",
                "Assets/Sprites/UI/Badge_VU_Rentan.png",
                "Assets/Sprites/UI/Badge_LC_Aman.png",
                "Assets/Sprites/UI/Btn_Action_AR.png",
                "Assets/Sprites/UI/Card_Cohesive_White.png",
                "Assets/Sprites/UI/Palette_Daylight_Soft.png",
                "Assets/Sprites/UI/Pill_Hero_Perfect.png",
                "Assets/Sprites/UI/Pill_White_Perfect.png"
            };

            foreach (var path in uiSprites)
            {
                TextureImporter ti = AssetImporter.GetAtPath(path) as TextureImporter;
                if (ti != null)
                {
                    ti.textureType = TextureImporterType.Sprite;
                    ti.spriteImportMode = SpriteImportMode.Single;
                    ti.alphaIsTransparency = true;
                    ti.mipmapEnabled = false;
                    ti.filterMode = FilterMode.Bilinear;
                    if (path.Contains("Card"))
                    {
                        ti.spriteBorder = new Vector4(28, 28, 28, 28);
                    }
                    else if (path.Contains("Badge_") || path.Contains("Btn_Action_AR") || path.Contains("Pill"))
                    {
                        ti.spriteBorder = new Vector4(24, 0, 24, 0);
                    }
                    ti.SaveAndReimport();
                }
            }
        }

        private static Camera SetupCameraAndLighting()
        {
            GameObject camObj = new GameObject("Main Camera");
            Camera cam = camObj.AddComponent<Camera>();
            camObj.AddComponent<AudioListener>();
            camObj.tag = "MainCamera";
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.94f, 0.96f, 0.95f, 1.0f);
            cam.transform.position = new Vector3(0, 0, -10);

            GameObject lightObj = new GameObject("Directional Light");
            Light l = lightObj.AddComponent<Light>();
            l.type = LightType.Directional;
            l.color = Color.white;
            l.intensity = 1.0f;
            lightObj.transform.rotation = Quaternion.Euler(50, -30, 0);

            GameObject esObj = new GameObject("EventSystem");
            esObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
            esObj.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
            return cam;
        }

        private static void UpdateBuildSettings(string scenePath)
        {
            var currentScenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            if (!currentScenes.Exists(s => s.path == scenePath))
            {
                currentScenes.Add(new EditorBuildSettingsScene(scenePath, true));
                EditorBuildSettings.scenes = currentScenes.ToArray();
            }
        }

        public static GameObject BuildCardPrefab()
        {
            string prefabDir = "Assets/Prefabs/UI";
            if (!Directory.Exists(prefabDir)) Directory.CreateDirectory(prefabDir);
            string prefabPath = $"{prefabDir}/Animal_Select_Card.prefab";
            if (File.Exists(prefabPath)) AssetDatabase.DeleteAsset(prefabPath);

            string resDir = "Assets/Resources/UI";
            if (!Directory.Exists(resDir)) Directory.CreateDirectory(resDir);
            string resPath = $"{resDir}/Animal_Select_Card.prefab";

            Sprite cardBg = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Card_Exact.png");
            if (cardBg == null) cardBg = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Card_2Col_Premium.png");
            if (cardBg == null) cardBg = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Card_Cohesive_White.png");

            Sprite pillHero = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Pill_Hero_Perfect.png");
            if (pillHero == null) pillHero = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Playful/Btn_Hero_Playful.png");

            Sprite pillWhite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Pill_White_Perfect.png");

            // 1. Root Card Object (484 x 660 - Large Vertical Portrait Card with clear hierarchy)
            GameObject cardRoot = new GameObject("Animal_Select_Card", typeof(RectTransform));
            RectTransform cr = cardRoot.GetComponent<RectTransform>();
            cr.sizeDelta = new Vector2(484, 660);

            Image img = cardRoot.AddComponent<Image>();
            if (cardBg != null) { img.sprite = cardBg; img.type = Image.Type.Sliced; }
            img.color = Color.white;

            Button btn = cardRoot.AddComponent<Button>();
            ColorBlock cb = btn.colors;
            cb.normalColor = Color.white;
            cb.highlightedColor = new Color(0.95f, 0.99f, 0.96f, 1.0f);
            cb.pressedColor = new Color(0.88f, 0.95f, 0.91f, 1.0f);
            cb.selectedColor = Color.white;
            btn.colors = cb;

            // 2. Top Showcase Container (444 x 260, soft halo background)
            GameObject showcaseObj = CreateUI("ThumbContainer", cardRoot.transform);
            RectTransform scr = showcaseObj.GetComponent<RectTransform>();
            scr.anchorMin = new Vector2(0.5f, 1); scr.anchorMax = new Vector2(0.5f, 1);
            scr.pivot = new Vector2(0.5f, 1);
            scr.sizeDelta = new Vector2(444, 260);
            scr.anchoredPosition = new Vector2(0, -20);

            Image scBg = showcaseObj.AddComponent<Image>();
            if (cardBg != null) { scBg.sprite = cardBg; scBg.type = Image.Type.Sliced; }
            scBg.color = Color.white;

            // 2.1 Cartoon Animal Avatar (250 x 250, centered, preserve aspect)
            GameObject thumbImgObj = CreateUI("Image", showcaseObj.transform);
            RectTransform tir = thumbImgObj.GetComponent<RectTransform>();
            tir.anchorMin = new Vector2(0.5f, 0.5f); tir.anchorMax = new Vector2(0.5f, 0.5f);
            tir.pivot = new Vector2(0.5f, 0.5f);
            tir.sizeDelta = new Vector2(250, 250);
            tir.anchoredPosition = Vector2.zero;

            Image ti = thumbImgObj.AddComponent<Image>();
            ti.preserveAspect = true;
            ti.raycastTarget = false;

            // 3. Animal Common Name (Main Title - Font size 34pt Bold)
            GameObject nameObj = CreateUI("Txt_Name", cardRoot.transform);
            RectTransform nr = nameObj.GetComponent<RectTransform>();
            nr.anchorMin = new Vector2(0.5f, 1); nr.anchorMax = new Vector2(0.5f, 1);
            nr.pivot = new Vector2(0.5f, 1);
            nr.sizeDelta = new Vector2(440, 52);
            nr.anchoredPosition = new Vector2(0, -300);

            TextMeshProUGUI nt = nameObj.AddComponent<TextMeshProUGUI>();
            if (appFont != null) nt.font = appFont;
            nt.text = "<b>Gajah Sumatra</b>";
            nt.fontSize = 34f;
            nt.enableAutoSizing = true;
            nt.fontSizeMin = 26f;
            nt.fontSizeMax = 34f;
            nt.color = new Color(0.04f, 0.22f, 0.16f);
            nt.alignment = TextAlignmentOptions.Center;
            nt.raycastTarget = false;

            // 4. Latin Scientific Name (Font size 22pt Italic)
            GameObject latinObj = CreateUI("Txt_Latin", cardRoot.transform);
            RectTransform lr = latinObj.GetComponent<RectTransform>();
            lr.anchorMin = new Vector2(0.5f, 1); lr.anchorMax = new Vector2(0.5f, 1);
            lr.pivot = new Vector2(0.5f, 1);
            lr.sizeDelta = new Vector2(440, 36);
            lr.anchoredPosition = new Vector2(0, -356);

            TextMeshProUGUI lt = latinObj.AddComponent<TextMeshProUGUI>();
            if (appFont != null) lt.font = appFont;
            lt.text = "<i>Elephas maximus</i>";
            lt.fontSize = 22f;
            lt.color = new Color(0.35f, 0.45f, 0.40f);
            lt.alignment = TextAlignmentOptions.Center;
            lt.overflowMode = TextOverflowModes.Ellipsis;
            lt.raycastTarget = false;

            // 5. IUCN Status Badge (Centered Pill: 260 x 48)
            GameObject badgeObj = CreateUI("Badge_Status", cardRoot.transform);
            RectTransform br = badgeObj.GetComponent<RectTransform>();
            br.anchorMin = new Vector2(0.5f, 1); br.anchorMax = new Vector2(0.5f, 1);
            br.pivot = new Vector2(0.5f, 1);
            br.sizeDelta = new Vector2(260, 48);
            br.anchoredPosition = new Vector2(0, -406);

            Image badgeImg = badgeObj.AddComponent<Image>();
            Sprite badgePill = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Badge_CR_Kritis.png");
            if (badgePill != null) { badgeImg.sprite = badgePill; badgeImg.type = Image.Type.Sliced; }
            else if (pillHero != null) { badgeImg.sprite = pillHero; badgeImg.type = Image.Type.Sliced; }
            badgeImg.color = new Color(0.99f, 0.88f, 0.88f, 1f);
            badgeImg.raycastTarget = false;

            GameObject badgeTxtObj = CreateUI("Text", badgeObj.transform);
            TextMeshProUGUI bt = badgeTxtObj.AddComponent<TextMeshProUGUI>();
            if (appFont != null) bt.font = appFont;
            bt.text = "<b>● Kritis (CR)</b>";
            bt.fontSize = 20f;
            bt.alignment = TextAlignmentOptions.Center;
            bt.color = new Color(0.75f, 0.1f, 0.1f, 1f);
            bt.raycastTarget = false;
            FillParent(badgeTxtObj.GetComponent<RectTransform>());

            // 6. Bottom Hero Action Button [ ▶ Eksplorasi AR ] (444 x 82)
            GameObject actionBtnObj = CreateUI("Btn_Action_AR", cardRoot.transform);
            RectTransform abr = actionBtnObj.GetComponent<RectTransform>();
            abr.anchorMin = new Vector2(0.5f, 0); abr.anchorMax = new Vector2(0.5f, 0);
            abr.pivot = new Vector2(0.5f, 0);
            abr.sizeDelta = new Vector2(444, 82);
            abr.anchoredPosition = new Vector2(0, 22);

            Image actionImg = actionBtnObj.AddComponent<Image>();
            if (pillHero != null) { actionImg.sprite = pillHero; actionImg.type = Image.Type.Sliced; }
            actionImg.color = new Color(0.04f, 0.55f, 0.38f, 1.0f);
            actionImg.raycastTarget = false;

            GameObject actionTxtObj = CreateUI("Text", actionBtnObj.transform);
            TextMeshProUGUI at = actionTxtObj.AddComponent<TextMeshProUGUI>();
            if (appFont != null) at.font = appFont;
            at.text = "<b>▶ Eksplorasi AR</b>";
            at.fontSize = 26f;
            at.alignment = TextAlignmentOptions.Center;
            at.color = Color.white;
            at.raycastTarget = false;
            FillParent(actionTxtObj.GetComponent<RectTransform>());

            GameObject savedPrefab = PrefabUtility.SaveAsPrefabAsset(cardRoot, prefabPath);
            PrefabUtility.SaveAsPrefabAsset(cardRoot, resPath);
            Object.DestroyImmediate(cardRoot);
            return savedPrefab;
        }

        private static void BuildPilihHewanCanvas(GameObject cardPrefab, Camera cam)
        {
            GameObject canvasObj = new GameObject("Canvas_PilihHewan");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0.0f;

            canvasObj.AddComponent<GraphicRaycaster>();
            
            PilihHewanController controller = canvasObj.AddComponent<PilihHewanController>();

            // Sprites
            Sprite bgBackdrop = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Playful/Backdrop_Nature_Playful.png");
            if (bgBackdrop == null) bgBackdrop = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Palette_Daylight_Soft.png");
            Sprite btnSubtle = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Playful/Pill_Badge_Tag.png");
            if (btnSubtle == null) btnSubtle = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Pill_White_Perfect.png");

            // 0. BACKGROUND
            GameObject bgObj = CreateUI("Background", canvasObj.transform);
            FillParent(bgObj.GetComponent<RectTransform>());
            Image bgImg = bgObj.AddComponent<Image>();
            if (bgBackdrop != null) bgImg.sprite = bgBackdrop;
            bgImg.color = Color.white;
            bgImg.raycastTarget = false;

            // 1. SAFE AREA ROOT
            GameObject safeArea = CreateUI("SafeArea", canvasObj.transform);
            FillParent(safeArea.GetComponent<RectTransform>());
            safeArea.AddComponent<SafeAreaFitter>();

            // 1.1 TOP BAR (Height 210, generous top margin for camera notch)
            GameObject topBar = CreateUI("TopBar", safeArea.transform);
            RectTransform tbRect = topBar.GetComponent<RectTransform>();
            tbRect.anchorMin = new Vector2(0, 1); tbRect.anchorMax = new Vector2(1, 1);
            tbRect.pivot = new Vector2(0.5f, 1);
            tbRect.sizeDelta = new Vector2(0, 210);
            tbRect.anchoredPosition = new Vector2(0, -24);

            // Kembali Button (164 x 80)
            GameObject backBtn = CreateUI("Btn_Kembali", topBar.transform);
            RectTransform bbr = backBtn.GetComponent<RectTransform>();
            bbr.anchorMin = new Vector2(0, 0.5f); bbr.anchorMax = new Vector2(0, 0.5f);
            bbr.pivot = new Vector2(0, 0.5f);
            bbr.sizeDelta = new Vector2(164, 80);
            bbr.anchoredPosition = new Vector2(36, 0);

            Image backImg = backBtn.AddComponent<Image>();
            if (btnSubtle != null) { backImg.sprite = btnSubtle; backImg.type = Image.Type.Sliced; }
            Button bKembali = backBtn.AddComponent<Button>();

            GameObject backTxt = CreateUI("Text", backBtn.transform);
            TextMeshProUGUI bkt = backTxt.AddComponent<TextMeshProUGUI>();
            if (appFont != null) bkt.font = appFont;
            bkt.text = "<b><color=#064E3B>‹ Menu</color></b>";
            bkt.fontSize = 28f;
            bkt.alignment = TextAlignmentOptions.Center;
            bkt.raycastTarget = false;
            FillParent(backTxt.GetComponent<RectTransform>());

            // Title & Subtitle Header (Large, bold, child-friendly)
            GameObject titleObj = CreateUI("HeaderTitle", topBar.transform);
            RectTransform tr = titleObj.GetComponent<RectTransform>();
            tr.anchorMin = new Vector2(0.5f, 0.5f); tr.anchorMax = new Vector2(0.5f, 0.5f);
            tr.pivot = new Vector2(0.5f, 0.5f);
            tr.sizeDelta = new Vector2(760, 160);
            tr.anchoredPosition = new Vector2(30, 0);

            TextMeshProUGUI tt = titleObj.AddComponent<TextMeshProUGUI>();
            if (appFont != null) tt.font = appFont;
            tt.text = "<b><color=#064E3B>PILIH SATWA LANGKA</color></b>\n<size=24><color=#047857>12 Spesies Dilindungi • Sentuh Kartu untuk AR</color></size>";
            tt.fontSize = 42f;
            tt.lineSpacing = 1.18f;
            tt.alignment = TextAlignmentOptions.Center;
            tt.raycastTarget = false;

            // 1.2 MOBILE SCROLL RECT CONTAINER (Anchored below TopBar, fills entire screen)
            GameObject scrollRoot = CreateUI("ScrollView_Satwa", safeArea.transform);
            RectTransform srr = scrollRoot.GetComponent<RectTransform>();
            srr.anchorMin = new Vector2(0, 0); srr.anchorMax = new Vector2(1, 1);
            srr.offsetMin = new Vector2(28, 20);
            srr.offsetMax = new Vector2(-28, -244);

            ScrollRect scrollRect = scrollRoot.AddComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Elastic;
            scrollRect.elasticity = 0.1f;
            scrollRect.inertia = true;
            scrollRect.decelerationRate = 0.135f;
            scrollRect.scrollSensitivity = 45f;

            // Viewport
            GameObject viewport = CreateUI("Viewport", scrollRoot.transform);
            FillParent(viewport.GetComponent<RectTransform>());
            viewport.AddComponent<RectMask2D>();

            // Content
            GameObject content = CreateUI("Content", viewport.transform);
            RectTransform cr = content.GetComponent<RectTransform>();
            cr.anchorMin = new Vector2(0, 1); cr.anchorMax = new Vector2(1, 1);
            cr.pivot = new Vector2(0.5f, 1);
            cr.offsetMin = Vector2.zero;
            cr.offsetMax = Vector2.zero;
            cr.anchoredPosition = Vector2.zero;

            scrollRect.viewport = viewport.GetComponent<RectTransform>();
            scrollRect.content = cr;

            // GridLayoutGroup (2-Column Grid: 484x640 cellSize, fills screen perfectly)
            GridLayoutGroup grid = content.AddComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(484, 640);
            grid.spacing = new Vector2(24, 36);
            grid.startCorner = GridLayoutGroup.Corner.UpperLeft;
            grid.startAxis = GridLayoutGroup.Axis.Horizontal;
            grid.childAlignment = TextAnchor.UpperCenter;
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 2;
            grid.padding = new RectOffset(16, 16, 20, 140);

            // ContentSizeFitter to dynamically adjust height for 6 rows
            ContentSizeFitter fitter = content.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // Load all 12 AnimalDataSO
            string[] guids = AssetDatabase.FindAssets("t:AnimalDataSO", new[] { "Assets/Resources/Data/Animals", "Assets/Data/Animals" });
            List<AnimalDataSO> dataList = new List<AnimalDataSO>();
            foreach (var g in guids)
            {
                string p = AssetDatabase.GUIDToAssetPath(g);
                var data = AssetDatabase.LoadAssetAtPath<AnimalDataSO>(p);
                if (data != null) dataList.Add(data);
            }
            dataList.Sort((a, b) => string.Compare(a.animalCode, b.animalCode, System.StringComparison.Ordinal));

            foreach (var data in dataList)
            {
                data.thumbnail = AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/Sprites/Animals/Thumb_{data.animalCode}.png");
                EditorUtility.SetDirty(data);
            }
            AssetDatabase.SaveAssets();

            // Wire Controller Properties
            controller.AllAnimals = new List<AnimalDataSO>(dataList);
            SerializedObject so = new SerializedObject(controller);
            so.FindProperty("btnKembali").objectReferenceValue = bKembali;
            so.FindProperty("txtHeaderTitle").objectReferenceValue = tt;
            so.FindProperty("gridContentParent").objectReferenceValue = content.transform;
            so.FindProperty("animalCardPrefab").objectReferenceValue = cardPrefab;
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(controller);

            // Populate preview cards in Editor
            for (int i = content.transform.childCount - 1; i >= 0; i--)
            {
                Object.DestroyImmediate(content.transform.GetChild(i).gameObject);
            }

            foreach (var data in dataList)
            {
                GameObject card = (GameObject)PrefabUtility.InstantiatePrefab(cardPrefab, content.transform);
                card.name = $"Card_{data.animalCode}_{data.commonName}";

                var txtName = card.transform.Find("Txt_Name")?.GetComponent<TextMeshProUGUI>();
                var txtLatin = card.transform.Find("Txt_Latin")?.GetComponent<TextMeshProUGUI>();
                var txtStatus = card.transform.Find("Badge_Status/Text")?.GetComponent<TextMeshProUGUI>();
                var imgBadge = card.transform.Find("Badge_Status")?.GetComponent<Image>();
                var imgThumb = card.transform.Find("ThumbContainer/Image")?.GetComponent<Image>();

                if (txtName != null) txtName.text = $"<b>{data.commonName}</b>";
                if (txtLatin != null)
                {
                    txtLatin.text = $"<i>{data.latinName}</i>";
                }
                if (txtStatus != null)
                {
                    string stLabel = data.iucnStatus == ConservationStatus.CriticallyEndangered ? "● Kritis (CR)" :
                                     data.iucnStatus == ConservationStatus.Endangered ? "● Terancam (EN)" :
                                     data.iucnStatus == ConservationStatus.Vulnerable ? "● Rentan (VU)" : "● Risiko Rendah (LC)";
                    Color stColor = data.iucnStatus == ConservationStatus.CriticallyEndangered ? new Color(0.6f, 0.1f, 0.1f, 1f) :
                                    data.iucnStatus == ConservationStatus.Endangered ? new Color(0.57f, 0.25f, 0.05f, 1f) :
                                    data.iucnStatus == ConservationStatus.Vulnerable ? new Color(0.52f, 0.3f, 0.05f, 1f) : new Color(0.08f, 0.4f, 0.2f, 1f);
                    Color bgCol = data.iucnStatus == ConservationStatus.CriticallyEndangered ? new Color(0.99f, 0.88f, 0.88f, 1f) :
                                  data.iucnStatus == ConservationStatus.Endangered ? new Color(0.99f, 0.95f, 0.78f, 1f) :
                                  data.iucnStatus == ConservationStatus.Vulnerable ? new Color(0.99f, 0.97f, 0.76f, 1f) : new Color(0.86f, 0.98f, 0.9f, 1f);
                    txtStatus.text = stLabel;
                    txtStatus.color = stColor;
                    if (imgBadge != null) imgBadge.color = bgCol;
                }
                if (imgThumb != null && data.thumbnail != null) imgThumb.sprite = data.thumbnail;
            }
        }

        private static GameObject CreateUI(string name, Transform parent)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return go;
        }

        private static void FillParent(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }
    }
}
