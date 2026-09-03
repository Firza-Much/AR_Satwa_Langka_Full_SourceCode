using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using TMPro;
using SatwaLangka.UI;

namespace SatwaLangka.EditorScripts
{
    public static class PanduanSceneBuilder
    {
        private static TMP_FontAsset appFont => AppTheme.Font;

        [MenuItem("Satwa Langka/Create & Open Panduan Scene (Modern Onboarding)")]
        public static void BuildPanduanScene()
        {
            string scenePath = "Assets/Scenes/Panduan.unity";
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            Camera cam = SetupCameraAndLighting();
            BuildPanduanCanvas(cam);

            EditorSceneManager.SaveScene(scene, scenePath);
            UpdateBuildSettings(scenePath);

            Debug.Log("<b>[SATWA AR]</b> Panduan scene successfully rebuilt with modern onboarding layout at " + scenePath);
        }

        private static Camera SetupCameraAndLighting()
        {
            GameObject camObj = new GameObject("Main Camera");
            Camera cam = camObj.AddComponent<Camera>();
            camObj.tag = "MainCamera";
            camObj.AddComponent<AudioListener>();

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

        private static void BuildPanduanCanvas(Camera cam)
        {
            GameObject canvasObj = new GameObject("Canvas_Panduan");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0.0f;

            canvasObj.AddComponent<GraphicRaycaster>();
            PanduanController controller = canvasObj.AddComponent<PanduanController>();

            // Fonts & UI Assets
            
            Sprite bgBackdrop = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Playful/Backdrop_Nature_Playful.png");
            if (bgBackdrop == null) bgBackdrop = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Palette_Daylight_Soft.png");
            Sprite cardSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Playful/Card_Cohesive_Playful.png");
            if (cardSprite == null) cardSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Card_Exact.png");

            Sprite pillHero = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Pill_Hero_Perfect.png");
            if (pillHero == null) pillHero = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Pill_Button.png");

            Sprite btnWhite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Pill_White_Perfect.png");
            if (btnWhite == null) btnWhite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Btn_Subtle_Card.png");

            // 0. BACKGROUND
            GameObject bgObj = CreateUI("Background", canvasObj.transform);
            FillParent(bgObj.GetComponent<RectTransform>());
            Image bgImg = bgObj.AddComponent<Image>();
            if (bgBackdrop != null) bgImg.sprite = bgBackdrop;
            bgImg.color = Color.white;
            bgImg.raycastTarget = false;

            // 1. SAFE AREA CONTAINER (Full Screen with Auto Notch Insets)
            GameObject safeArea = CreateUI("SafeArea", canvasObj.transform);
            FillParent(safeArea.GetComponent<RectTransform>());
            safeArea.AddComponent<SafeAreaFitter>();

            // ==================== 2. PROMINENT FIXED HEADER ====================
            GameObject headerObj = CreateUI("HeaderContainer", safeArea.transform);
            RectTransform hr = headerObj.GetComponent<RectTransform>();
            hr.anchorMin = new Vector2(0, 1); hr.anchorMax = new Vector2(1, 1);
            hr.pivot = new Vector2(0.5f, 1);
            hr.sizeDelta = new Vector2(0, 250);
            hr.anchoredPosition = new Vector2(0, -20);

            // 2.1 Back Button (Top-Left, Large & Clear)
            GameObject backBtn = CreateUI("Btn_Kembali", headerObj.transform);
            RectTransform bbr = backBtn.GetComponent<RectTransform>();
            bbr.anchorMin = new Vector2(0, 1); bbr.anchorMax = new Vector2(0, 1);
            bbr.pivot = new Vector2(0, 1);
            bbr.sizeDelta = new Vector2(210, 68);
            bbr.anchoredPosition = new Vector2(32, -10);

            Image backImg = backBtn.AddComponent<Image>();
            if (btnWhite != null) { backImg.sprite = btnWhite; backImg.type = Image.Type.Sliced; }
            Button bKembali = backBtn.AddComponent<Button>();

            GameObject backTxt = CreateUI("Text", backBtn.transform);
            TextMeshProUGUI bkt = backTxt.AddComponent<TextMeshProUGUI>();
            if (appFont != null) bkt.font = appFont;
            bkt.text = "<b><color=#064E3B>‹ Kembali</color></b>";
            bkt.fontSize = 26;
            bkt.alignment = TextAlignmentOptions.Center;
            bkt.raycastTarget = false;
            FillParent(backTxt.GetComponent<RectTransform>());

            // 2.2 Main Title (Centered, Large & Bold)
            GameObject titleObj = CreateUI("HeaderTitle", headerObj.transform);
            RectTransform tr = titleObj.GetComponent<RectTransform>();
            tr.anchorMin = new Vector2(0.5f, 1); tr.anchorMax = new Vector2(0.5f, 1);
            tr.pivot = new Vector2(0.5f, 1);
            tr.sizeDelta = new Vector2(900, 60);
            tr.anchoredPosition = new Vector2(0, -90);

            TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
            if (appFont != null) titleText.font = appFont;
            titleText.text = "<b><color=#064E3B>Panduan Aplikasi AR</color></b>";
            titleText.fontSize = 38;
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.raycastTarget = false;

            // 2.3 Sub-Badge Pill ("4 Langkah Mudah AR")
            GameObject badgeObj = CreateUI("Badge_4Langkah", headerObj.transform);
            RectTransform bgr = badgeObj.GetComponent<RectTransform>();
            bgr.anchorMin = new Vector2(0.5f, 1); bgr.anchorMax = new Vector2(0.5f, 1);
            bgr.pivot = new Vector2(0.5f, 1);
            bgr.sizeDelta = new Vector2(440, 56);
            bgr.anchoredPosition = new Vector2(0, -165);

            Image badgeImg = badgeObj.AddComponent<Image>();
            if (pillHero != null) { badgeImg.sprite = pillHero; badgeImg.type = Image.Type.Sliced; }
            badgeImg.color = new Color(0.04f, 0.48f, 0.35f, 1f);

            GameObject badgeTxtObj = CreateUI("Text", badgeObj.transform);
            TextMeshProUGUI bgt = badgeTxtObj.AddComponent<TextMeshProUGUI>();
            if (appFont != null) bgt.font = appFont;
            bgt.text = "<b>4 Langkah Mudah Eksplorasi AR</b>";
            bgt.fontSize = 22;
            bgt.alignment = TextAlignmentOptions.Center;
            bgt.color = Color.white;
            FillParent(badgeTxtObj.GetComponent<RectTransform>());

            // ==================== 3. FULL-WIDTH SCROLLVIEW FOR STEP CARDS ====================
            GameObject scrollObj = CreateUI("ScrollView_Panduan", safeArea.transform);
            RectTransform srr = scrollObj.GetComponent<RectTransform>();
            srr.anchorMin = new Vector2(0, 0); srr.anchorMax = new Vector2(1, 1);
            srr.offsetMin = new Vector2(28, 20);      // 28px responsive side margin
            srr.offsetMax = new Vector2(-28, -265);   // Positioned right under Header!

            ScrollRect sr = scrollObj.AddComponent<ScrollRect>();
            sr.horizontal = false;
            sr.vertical = true;
            sr.movementType = ScrollRect.MovementType.Elastic;
            sr.scrollSensitivity = 38f;

            GameObject viewport = CreateUI("Viewport", scrollObj.transform);
            FillParent(viewport.GetComponent<RectTransform>());
            viewport.AddComponent<RectMask2D>();

            GameObject content = CreateUI("Content", viewport.transform);
            RectTransform cr = content.GetComponent<RectTransform>();
            cr.anchorMin = new Vector2(0, 1); cr.anchorMax = new Vector2(1, 1);
            cr.pivot = new Vector2(0.5f, 1);
            cr.offsetMin = Vector2.zero; cr.offsetMax = Vector2.zero;

            VerticalLayoutGroup vlg = content.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 28;
            vlg.padding = new RectOffset(4, 4, 15, 60); // Ample 60px bottom padding!
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            ContentSizeFitter csf = content.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            sr.viewport = viewport.GetComponent<RectTransform>();
            sr.content = cr;

            // ==================== 4. THE 4 MODERN ONBOARDING STEP CARDS ====================

            // Step 1: Pilih Satwa Langka
            CreateModernStepCard(content.transform, "1",
                "Pilih Satwa Langka",
                "Eksplorasi Galeri 12 Satwa Endemik",
                "Buka menu <b><color=#047857>Pilih Hewan</color></b> untuk menjelajahi 12 fauna langka Indonesia. Sentuh kartu satwa pilihan Anda untuk langsung memunculkan proyeksi visual 3D di dunia nyata.",
                "💡 Info: Tersedia 12 fauna unik dari Sumatra hingga Papua",
                new Color(0.93f, 0.98f, 0.95f, 1f), new Color(0.04f, 0.65f, 0.45f, 1f), cardSprite, pillHero, appFont);

            // Step 2: Scan Permukaan Datar (AR)
            CreateModernStepCard(content.transform, "2",
                "Scan Permukaan Datar (AR)",
                "Deteksi Lantai atau Meja Otomatis",
                "Arahkan kamera HP ke <b><color=#047857>lantai atau meja</color></b> bertekstur dengan pencahayaan memadai. Satwa 3D akan otomatis muncul tegak dan berdiri kokoh di atas panggung pendaratan.",
                "💡 Tips: Markerless AR otomatis tanpa perlu kartu penanda kertas",
                new Color(0.99f, 0.98f, 0.92f, 1f), new Color(0.85f, 0.55f, 0.05f, 1f), cardSprite, pillHero, appFont);

            // Step 3: Putar, Zoom & Suara Satwa
            CreateModernStepCard(content.transform, "3",
                "Putar, Zoom & Suara Satwa",
                "Rotasi Horizontal, Pinch Zoom & Audio",
                "• <b>Geser Jari Kiri / Kanan:</b> Memutar satwa 360° secara horizontal.\n• <b>Cubit 2 Jari:</b> Memperbesar atau memperkecil ukuran model 3D.\n• <b>Tombol Suara Satwa:</b> Sentuh untuk mendengarkan rekaman suara khas asli satwa.",
                "🎮 Kontrol: Gestur sentuh intuitif & responsif multi-touch",
                new Color(0.93f, 0.97f, 1.0f, 1f), new Color(0.1f, 0.55f, 0.85f, 1f), cardSprite, pillHero, appFont);

            // Step 4: Pelajari Fakta & Kerjakan Kuis
            CreateModernStepCard(content.transform, "4",
                "Pelajari Fakta & Kerjakan Kuis",
                "Taksonomi, Habitat, Mitigasi & Kuis",
                "Buka panel info untuk membaca taksonomi ilmiah, sebaran habitat, mitigasi bahaya, dan fakta unik melalui 4 Tab terpadu. Uji pemahaman Anda melalui <b><color=#047857>Kuis Evaluasi 20 Soal</color></b>.",
                "📚 Referensi: Raih skor 100 dan jadilah Duta Konservasi Satwa!",
                new Color(0.97f, 0.94f, 1.0f, 1f), new Color(0.55f, 0.25f, 0.85f, 1f), cardSprite, pillHero, appFont);

            // Wire Controller Fields
            SerializedObject so = new SerializedObject(controller);
            so.FindProperty("btnKembali").objectReferenceValue = bKembali;
            so.ApplyModifiedProperties();
        }

        private static void CreateModernStepCard(Transform parent, string stepNum, string title, string subtitle, string description, string tagText, Color bgColor, Color themeColor, Sprite cardSprite, Sprite pillSprite, TMP_FontAsset font)
        {
            GameObject cardObj = CreateUI($"Card_Step_{stepNum}", parent);
            RectTransform cr = cardObj.GetComponent<RectTransform>();

            Image ci = cardObj.AddComponent<Image>();
            if (cardSprite != null) { ci.sprite = cardSprite; ci.type = Image.Type.Sliced; }
            ci.color = bgColor;

            VerticalLayoutGroup cardVlg = cardObj.AddComponent<VerticalLayoutGroup>();
            cardVlg.padding = new RectOffset(32, 32, 28, 28);
            cardVlg.spacing = 18;
            cardVlg.childControlWidth = true;
            cardVlg.childControlHeight = true;
            cardVlg.childForceExpandWidth = true;
            cardVlg.childForceExpandHeight = false;

            ContentSizeFitter cardCsf = cardObj.AddComponent<ContentSizeFitter>();
            cardCsf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // 1. Header Row (Circle Number Badge + Title/Subtitle Column)
            GameObject headerRow = CreateUI("HeaderRow", cardObj.transform);
            HorizontalLayoutGroup hlg = headerRow.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 20;
            hlg.childControlWidth = false;
            hlg.childControlHeight = false;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;

            LayoutElement hle = headerRow.AddComponent<LayoutElement>();
            hle.minHeight = 78;

            // Number Badge Circle
            GameObject numBadge = CreateUI("NumBadge", headerRow.transform);
            RectTransform nbr = numBadge.GetComponent<RectTransform>();
            nbr.sizeDelta = new Vector2(76, 76);

            Image nbi = numBadge.AddComponent<Image>();
            if (pillSprite != null) { nbi.sprite = pillSprite; nbi.type = Image.Type.Sliced; }
            nbi.color = themeColor;

            GameObject numTxtObj = CreateUI("Text", numBadge.transform);
            TextMeshProUGUI numTxt = numTxtObj.AddComponent<TextMeshProUGUI>();
            if (font != null) numTxt.font = font;
            numTxt.text = $"<b>{stepNum}</b>";
            numTxt.fontSize = 36;
            numTxt.alignment = TextAlignmentOptions.Center;
            numTxt.color = Color.white;
            FillParent(numTxtObj.GetComponent<RectTransform>());

            // Title Column (Title + Subtitle)
            GameObject titleCol = CreateUI("TitleCol", headerRow.transform);
            RectTransform tcr = titleCol.GetComponent<RectTransform>();
            tcr.sizeDelta = new Vector2(820, 78);

            VerticalLayoutGroup tvlg = titleCol.AddComponent<VerticalLayoutGroup>();
            tvlg.spacing = 4;
            tvlg.childControlWidth = true;
            tvlg.childControlHeight = true;
            tvlg.childForceExpandWidth = true;
            tvlg.childForceExpandHeight = false;

            GameObject titleTextObj = CreateUI("TitleText", titleCol.transform);
            TextMeshProUGUI tt = titleTextObj.AddComponent<TextMeshProUGUI>();
            if (font != null) tt.font = font;
            tt.text = $"<b>{title}</b>";
            tt.fontSize = 28;
            tt.color = new Color(0.04f, 0.22f, 0.16f);

            GameObject subTextObj = CreateUI("SubText", titleCol.transform);
            TextMeshProUGUI st = subTextObj.AddComponent<TextMeshProUGUI>();
            if (font != null) st.font = font;
            st.text = $"<b>{subtitle}</b>";
            st.fontSize = 20;
            st.color = themeColor;

            // 2. Body Description Text (Dynamic Height, Large & Readable)
            GameObject bodyObj = CreateUI("BodyText", cardObj.transform);
            TextMeshProUGUI bt = bodyObj.AddComponent<TextMeshProUGUI>();
            if (font != null) bt.font = font;
            bt.text = description;
            bt.fontSize = 24;
            bt.lineSpacing = 1.38f;
            bt.color = new Color(0.08f, 0.15f, 0.18f);

            // 3. Bottom Micro-Tag Pill
            GameObject tagPill = CreateUI("TagPill", cardObj.transform);
            LayoutElement tle = tagPill.AddComponent<LayoutElement>();
            tle.minHeight = 56;

            Image tpi = tagPill.AddComponent<Image>();
            if (pillSprite != null) { tpi.sprite = pillSprite; tpi.type = Image.Type.Sliced; }
            tpi.color = new Color(themeColor.r, themeColor.g, themeColor.b, 0.16f);

            GameObject tagTxtObj = CreateUI("Text", tagPill.transform);
            TextMeshProUGUI tagTxt = tagTxtObj.AddComponent<TextMeshProUGUI>();
            if (font != null) tagTxt.font = font;
            tagTxt.text = $"<b><color=#{ColorUtility.ToHtmlStringRGB(themeColor)}>{tagText}</color></b>";
            tagTxt.fontSize = 20;
            tagTxt.alignment = TextAlignmentOptions.Center;
            FillParent(tagTxtObj.GetComponent<RectTransform>());
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
