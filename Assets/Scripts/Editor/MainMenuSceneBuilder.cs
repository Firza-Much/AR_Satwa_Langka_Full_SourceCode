using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using TMPro;
using SatwaLangka.UI;

namespace SatwaLangka.EditorScripts
{
    public static class MainMenuSceneBuilder
    {
        private static TMP_FontAsset appFont => AppTheme.Font;

        [MenuItem("Satwa Langka/Create & Open MainMenu Scene (Playful Theme)")]
        public static void BuildMainMenuScene()
        {
            string scenePath = "Assets/Scenes/MainMenu.unity";
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            Camera cam = SetupCameraAndLighting();
            BuildMainMenuCanvas(cam);

            EditorSceneManager.SaveScene(scene, scenePath);
            UpdateBuildSettings(scenePath);

            Debug.Log("<b>[SATWA AR]</b> MainMenu scene rebuilt with Playful Educational Design System!");
        }

        private static Camera SetupCameraAndLighting()
        {
            GameObject camObj = new GameObject("Main Camera");
            Camera cam = camObj.AddComponent<Camera>();
            camObj.tag = "MainCamera";
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.96f, 0.98f, 0.97f, 1.0f);
            cam.transform.position = new Vector3(0, 0, -10);
            camObj.AddComponent<AudioListener>();

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

        private static void BuildMainMenuCanvas(Camera cam)
        {
            GameObject canvasObj = new GameObject("Canvas_MainMenu");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0.0f; // Match Width = 0 for perfect mobile portrait scaling!

            canvasObj.AddComponent<GraphicRaycaster>();
            MainMenuController controller = canvasObj.AddComponent<MainMenuController>();

            // UI Sprites
            Sprite bgBackdrop = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Playful/Backdrop_Nature_Playful.png");
            if (bgBackdrop == null) bgBackdrop = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Palette_Daylight_Soft.png");

            Sprite btnHero = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Playful/Btn_Hero_Playful.png");
            if (btnHero == null) btnHero = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Pill_Hero_Perfect.png");

            Sprite cardQuiz = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Playful/Card_Quiz_Yellow.png");
            Sprite cardPanduan = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Playful/Card_Panduan_Blue.png");
            Sprite cardTentang = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Playful/Card_Tentang_Purple.png");
            Sprite cardSettings = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Playful/Card_Settings_Teal.png");
            Sprite cardBase = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Playful/Card_Cohesive_Playful.png");

            Sprite pillTag = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Playful/Pill_Badge_Tag.png");
            if (pillTag == null) pillTag = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Pill_White_Perfect.png");

            // Icons
            Sprite iconMascot = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Logo_Mascot_Playful.png");
            if (iconMascot == null) iconMascot = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Icons/Playful/Icon_Logo_Mascot.png");
            if (iconMascot == null) iconMascot = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Logo_Emblem_Exact.png");

            Sprite iconMulaiAR = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Icons/Playful/Icon_Mulai_AR.png");
            Sprite iconQuiz = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Icons/Playful/Icon_Quiz.png");
            Sprite iconPanduan = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Icons/Playful/Icon_Panduan.png");
            Sprite iconTentang = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Icons/Playful/Icon_Tentang.png");
            Sprite iconSettings = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Icons/Playful/Icon_Pengaturan.png");
            Sprite iconKeluar = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Icons/Playful/Icon_Keluar.png");

            // 0. BACKGROUND
            GameObject bgObj = CreateUI("Background", canvasObj.transform);
            FillParent(bgObj.GetComponent<RectTransform>());
            Image bgImg = bgObj.AddComponent<Image>();
            if (bgBackdrop != null) bgImg.sprite = bgBackdrop;
            bgImg.color = Color.white;
            bgImg.raycastTarget = false;

            // 1. SAFE AREA CONTAINER
            GameObject safeArea = CreateUI("SafeArea", canvasObj.transform);
            FillParent(safeArea.GetComponent<RectTransform>());
            safeArea.AddComponent<SafeAreaFitter>();

            // ==================== 1. HERO HEADER AREA ====================
            GameObject heroContainer = CreateUI("HeroContainer", safeArea.transform);
            RectTransform hcr = heroContainer.GetComponent<RectTransform>();
            hcr.anchorMin = new Vector2(0.5f, 1); hcr.anchorMax = new Vector2(0.5f, 1);
            hcr.pivot = new Vector2(0.5f, 1);
            hcr.sizeDelta = new Vector2(980, 580);
            hcr.anchoredPosition = new Vector2(0, -30);

            // Mascot Logo (320x320)
            GameObject mascotObj = CreateUI("MascotLogo", heroContainer.transform);
            RectTransform mr = mascotObj.GetComponent<RectTransform>();
            mr.anchorMin = new Vector2(0.5f, 1); mr.anchorMax = new Vector2(0.5f, 1);
            mr.pivot = new Vector2(0.5f, 1);
            mr.sizeDelta = new Vector2(320, 320);
            mr.anchoredPosition = new Vector2(0, 0);

            Image mascotImg = mascotObj.AddComponent<Image>();
            if (iconMascot != null) mascotImg.sprite = iconMascot;
            mascotImg.preserveAspect = true;
            mascotImg.raycastTarget = false;

            // App Title
            GameObject titleObj = CreateUI("AppTitle", heroContainer.transform);
            RectTransform tr = titleObj.GetComponent<RectTransform>();
            tr.anchorMin = new Vector2(0.5f, 1); tr.anchorMax = new Vector2(0.5f, 1);
            tr.pivot = new Vector2(0.5f, 1);
            tr.sizeDelta = new Vector2(920, 75);
            tr.anchoredPosition = new Vector2(0, -330);

            TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
            if (appFont != null) titleText.font = appFont;
            titleText.text = "<b><color=#064E3B>SATWA LANGKA</color></b>";
            titleText.fontSize = 54;
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.raycastTarget = false;

            // Subtitle Badge Pill
            GameObject subPill = CreateUI("SubtitlePill", heroContainer.transform);
            RectTransform spr = subPill.GetComponent<RectTransform>();
            spr.anchorMin = new Vector2(0.5f, 1); spr.anchorMax = new Vector2(0.5f, 1);
            spr.pivot = new Vector2(0.5f, 1);
            spr.sizeDelta = new Vector2(500, 56);
            spr.anchoredPosition = new Vector2(0, -415);

            Image spImg = subPill.AddComponent<Image>();
            if (pillTag != null) { spImg.sprite = pillTag; spImg.type = Image.Type.Sliced; }
            spImg.color = new Color(0.93f, 0.98f, 0.95f, 1f);
            spImg.raycastTarget = false;

            GameObject subTxt = CreateUI("Text", subPill.transform);
            TextMeshProUGUI st = subTxt.AddComponent<TextMeshProUGUI>();
            if (appFont != null) st.font = appFont;
            st.text = "<b><color=#059669>Jelajah Fauna Nusantara</color></b>";
            st.fontSize = 26;
            st.alignment = TextAlignmentOptions.Center;
            st.raycastTarget = false;
            FillParent(subTxt.GetComponent<RectTransform>());

            // Tagline
            GameObject tagObj = CreateUI("Tagline", heroContainer.transform);
            RectTransform tagRt = tagObj.GetComponent<RectTransform>();
            tagRt.anchorMin = new Vector2(0.5f, 1); tagRt.anchorMax = new Vector2(0.5f, 1);
            tagRt.pivot = new Vector2(0.5f, 1);
            tagRt.sizeDelta = new Vector2(920, 50);
            tagRt.anchoredPosition = new Vector2(0, -485);

            TextMeshProUGUI tagText = tagObj.AddComponent<TextMeshProUGUI>();
            if (appFont != null) tagText.font = appFont;
            tagText.text = "<b><color=#475569>\"Ayo kenali dan lindungi satwa langka Indonesia!\"</color></b>";
            tagText.fontSize = 24;
            tagText.alignment = TextAlignmentOptions.Center;
            tagText.raycastTarget = false;

            // ==================== 2. PRIMARY ACTION: [ 🐾 MULAI EKSPLORASI AR  › ] ====================
            GameObject btnMulaiObj = CreateUI("Btn_Mulai", safeArea.transform);
            RectTransform bmr = btnMulaiObj.GetComponent<RectTransform>();
            bmr.anchorMin = new Vector2(0.5f, 1); bmr.anchorMax = new Vector2(0.5f, 1);
            bmr.pivot = new Vector2(0.5f, 1);
            bmr.sizeDelta = new Vector2(920, 140);
            bmr.anchoredPosition = new Vector2(0, -640);

            Image bmi = btnMulaiObj.AddComponent<Image>();
            if (btnHero != null) { bmi.sprite = btnHero; bmi.type = Image.Type.Sliced; }
            bmi.raycastTarget = true;
            Button bMulai = btnMulaiObj.AddComponent<Button>();

            // Left Icon
            GameObject bmiIcon = CreateUI("Icon", btnMulaiObj.transform);
            RectTransform biRt = bmiIcon.GetComponent<RectTransform>();
            biRt.anchorMin = new Vector2(0, 0.5f); biRt.anchorMax = new Vector2(0, 0.5f);
            biRt.pivot = new Vector2(0, 0.5f);
            biRt.sizeDelta = new Vector2(90, 90);
            biRt.anchoredPosition = new Vector2(35, 0);
            Image biImg = bmiIcon.AddComponent<Image>();
            if (iconMulaiAR != null) biImg.sprite = iconMulaiAR;
            biImg.raycastTarget = false;

            // Text Center
            GameObject bmiTxt = CreateUI("Text", btnMulaiObj.transform);
            RectTransform btRt = bmiTxt.GetComponent<RectTransform>();
            btRt.anchorMin = new Vector2(0, 0); btRt.anchorMax = new Vector2(1, 1);
            btRt.offsetMin = new Vector2(130, 0); btRt.offsetMax = new Vector2(-80, 0);
            TextMeshProUGUI bmt = bmiTxt.AddComponent<TextMeshProUGUI>();
            if (appFont != null) bmt.font = appFont;
            bmt.text = "<b><color=#FFFFFF>MULAI EKSPLORASI AR</color></b>";
            bmt.fontSize = 34;
            bmt.alignment = TextAlignmentOptions.Center;
            bmt.raycastTarget = false;

            // Right Chevron Arrow
            GameObject bmiArr = CreateUI("Arrow", btnMulaiObj.transform);
            RectTransform baRt = bmiArr.GetComponent<RectTransform>();
            baRt.anchorMin = new Vector2(1, 0.5f); baRt.anchorMax = new Vector2(1, 0.5f);
            baRt.pivot = new Vector2(1, 0.5f);
            baRt.sizeDelta = new Vector2(50, 50);
            baRt.anchoredPosition = new Vector2(-35, 0);
            TextMeshProUGUI bat = bmiArr.AddComponent<TextMeshProUGUI>();
            if (appFont != null) bat.font = appFont;
            bat.text = "<b><color=#FFFFFF>›</color></b>";
            bat.fontSize = 44;
            bat.alignment = TextAlignmentOptions.Center;
            bat.raycastTarget = false;

            // ==================== 3. 2x2 FEATURE MENU CARDS ====================
            // Row 1 (Y = -810)
            (Button bQuiz, GameObject cardQuizObj) = CreateMenuCard(safeArea.transform, "Btn_Quiz", -240, -810, 440, 260,
                cardQuiz != null ? cardQuiz : cardBase, iconQuiz, "Kuis Satwa", "20 Soal Menarik", "#D97706");

            (Button bPanduan, GameObject cardPanduanObj) = CreateMenuCard(safeArea.transform, "Btn_Panduan", 240, -810, 440, 260,
                cardPanduan != null ? cardPanduan : cardBase, iconPanduan, "Panduan AR", "4 Langkah Mudah", "#0284C7");

            // Row 2 (Y = -1100)
            (Button bTentang, GameObject cardTentangObj) = CreateMenuCard(safeArea.transform, "Btn_Tentang", -240, -1100, 440, 260,
                cardTentang != null ? cardTentang : cardBase, iconTentang, "Tentang Kami", "Info Konservasi", "#7C3AED");

            (Button bSettings, GameObject cardSettingsObj) = CreateMenuCard(safeArea.transform, "Btn_Pengaturan", 240, -1100, 440, 260,
                cardSettings != null ? cardSettings : cardBase, iconSettings, "Pengaturan", "Musik & Suara", "#0D9488");

            // ==================== 4. BOTTOM EXIT BUTTON ====================
            GameObject btnKeluarObj = CreateUI("Btn_Keluar", safeArea.transform);
            RectTransform bkr = btnKeluarObj.GetComponent<RectTransform>();
            bkr.anchorMin = new Vector2(0.5f, 0); bkr.anchorMax = new Vector2(0.5f, 0);
            bkr.pivot = new Vector2(0.5f, 0);
            bkr.sizeDelta = new Vector2(360, 72);
            bkr.anchoredPosition = new Vector2(0, 60);

            Image bki = btnKeluarObj.AddComponent<Image>();
            if (pillTag != null) { bki.sprite = pillTag; bki.type = Image.Type.Sliced; }
            bki.color = new Color(0.99f, 0.90f, 0.90f, 0.95f);
            bki.raycastTarget = true;
            Button bKeluar = btnKeluarObj.AddComponent<Button>();

            // Icon Keluar
            GameObject bkIcon = CreateUI("Icon", btnKeluarObj.transform);
            RectTransform bkiRt = bkIcon.GetComponent<RectTransform>();
            bkiRt.anchorMin = new Vector2(0, 0.5f); bkiRt.anchorMax = new Vector2(0, 0.5f);
            bkiRt.pivot = new Vector2(0, 0.5f);
            bkiRt.sizeDelta = new Vector2(40, 40);
            bkiRt.anchoredPosition = new Vector2(25, 0);
            Image bkiImg = bkIcon.AddComponent<Image>();
            if (iconKeluar != null) bkiImg.sprite = iconKeluar;
            bkiImg.raycastTarget = false;

            GameObject bkTxt = CreateUI("Text", btnKeluarObj.transform);
            RectTransform bktRt = bkTxt.GetComponent<RectTransform>();
            bktRt.anchorMin = new Vector2(0, 0); bktRt.anchorMax = new Vector2(1, 1);
            bktRt.offsetMin = new Vector2(60, 0); bktRt.offsetMax = new Vector2(-20, 0);
            TextMeshProUGUI bkt = bkTxt.AddComponent<TextMeshProUGUI>();
            if (appFont != null) bkt.font = appFont;
            bkt.text = "<b><color=#DC2626>Keluar Aplikasi</color></b>";
            bkt.fontSize = 24;
            bkt.alignment = TextAlignmentOptions.Center;
            bkt.raycastTarget = false;

            // ==================== 5. MODALS CONTAINER ====================
            GameObject modalOverlay = CreateUI("ModalOverlay", canvasObj.transform);
            FillParent(modalOverlay.GetComponent<RectTransform>());
            Image moImg = modalOverlay.AddComponent<Image>();
            moImg.color = new Color(0.04f, 0.12f, 0.08f, 0.65f); // Deep emerald tint backdrop
            modalOverlay.SetActive(false);

            // 5.1 Modal Pengaturan
            (GameObject panelPengaturan, Button btnClosePengaturan, Slider sBGM, Slider sSFX) =
                BuildPengaturanModal(modalOverlay.transform, cardBase, pillTag, btnHero);

            // 5.2 Modal Tentang Kami
            (GameObject panelTentang, Button btnCloseTentang) =
                BuildTentangModal(modalOverlay.transform, cardBase, pillTag, iconMascot);

            // 5.3 Modal Keluar
            (GameObject panelKeluar, Button btnConfirmKeluar, Button btnCancelKeluar) =
                BuildKeluarModal(modalOverlay.transform, cardBase, pillTag, btnHero);

            // ==================== 6. WIRE CONTROLLER ====================
            SerializedObject so = new SerializedObject(controller);
            so.FindProperty("btnMulai").objectReferenceValue = bMulai;
            so.FindProperty("btnQuiz").objectReferenceValue = bQuiz;
            so.FindProperty("btnPanduan").objectReferenceValue = bPanduan;
            so.FindProperty("btnTentang").objectReferenceValue = bTentang;
            so.FindProperty("btnKeluar").objectReferenceValue = bKeluar;
            so.FindProperty("btnPengaturan").objectReferenceValue = bSettings;

            so.FindProperty("modalOverlay").objectReferenceValue = modalOverlay;
            so.FindProperty("panelPengaturan").objectReferenceValue = panelPengaturan;
            so.FindProperty("panelTentang").objectReferenceValue = panelTentang;
            so.FindProperty("panelKeluar").objectReferenceValue = panelKeluar;

            so.FindProperty("sliderBGM").objectReferenceValue = sBGM;
            so.FindProperty("sliderSFX").objectReferenceValue = sSFX;
            so.FindProperty("btnClosePengaturan").objectReferenceValue = btnClosePengaturan;
            so.FindProperty("btnCloseTentang").objectReferenceValue = btnCloseTentang;
            so.FindProperty("btnConfirmKeluar").objectReferenceValue = btnConfirmKeluar;
            so.FindProperty("btnCancelKeluar").objectReferenceValue = btnCancelKeluar;

            so.ApplyModifiedProperties();
        }

        private static (Button, GameObject) CreateMenuCard(Transform parent, string name, float posX, float posY, float width, float height,
            Sprite cardSprite, Sprite iconSprite, string title, string subtitle, string accentColorHex)
        {
            GameObject cardObj = CreateUI(name, parent);
            RectTransform crt = cardObj.GetComponent<RectTransform>();
            crt.anchorMin = new Vector2(0.5f, 1); crt.anchorMax = new Vector2(0.5f, 1);
            crt.pivot = new Vector2(0.5f, 1);
            crt.sizeDelta = new Vector2(width, height);
            crt.anchoredPosition = new Vector2(posX, posY);

            Image ci = cardObj.AddComponent<Image>();
            if (cardSprite != null) { ci.sprite = cardSprite; ci.type = Image.Type.Sliced; }
            ci.raycastTarget = true;
            Button btn = cardObj.AddComponent<Button>();

            // Icon Center-Top (96x96)
            GameObject iconObj = CreateUI("Icon", cardObj.transform);
            RectTransform irt = iconObj.GetComponent<RectTransform>();
            irt.anchorMin = new Vector2(0.5f, 1); irt.anchorMax = new Vector2(0.5f, 1);
            irt.pivot = new Vector2(0.5f, 1);
            irt.sizeDelta = new Vector2(96, 96);
            irt.anchoredPosition = new Vector2(0, -22);

            Image iconImg = iconObj.AddComponent<Image>();
            if (iconSprite != null) iconImg.sprite = iconSprite;
            iconImg.preserveAspect = true;
            iconImg.raycastTarget = false;

            // Title (Centered below icon)
            GameObject titleObj = CreateUI("Title", cardObj.transform);
            RectTransform trt = titleObj.GetComponent<RectTransform>();
            trt.anchorMin = new Vector2(0.5f, 1); trt.anchorMax = new Vector2(0.5f, 1);
            trt.pivot = new Vector2(0.5f, 1);
            trt.sizeDelta = new Vector2(width - 30, 46);
            trt.anchoredPosition = new Vector2(0, -128);

            TextMeshProUGUI tt = titleObj.AddComponent<TextMeshProUGUI>();
            if (appFont != null) tt.font = appFont;
            tt.text = $"<b><color=#0F172A>{title}</color></b>";
            tt.fontSize = 28;
            tt.alignment = TextAlignmentOptions.Center;
            tt.raycastTarget = false;

            // Subtitle Badge (Centered near bottom)
            GameObject subObj = CreateUI("Subtitle", cardObj.transform);
            RectTransform srt = subObj.GetComponent<RectTransform>();
            srt.anchorMin = new Vector2(0.5f, 1); srt.anchorMax = new Vector2(0.5f, 1);
            srt.pivot = new Vector2(0.5f, 1);
            srt.sizeDelta = new Vector2(width - 40, 40);
            srt.anchoredPosition = new Vector2(0, -182);

            TextMeshProUGUI st = subObj.AddComponent<TextMeshProUGUI>();
            if (appFont != null) st.font = appFont;
            st.text = $"<b><color={accentColorHex}>{subtitle}</color></b>";
            st.fontSize = 20;
            st.alignment = TextAlignmentOptions.Center;
            st.raycastTarget = false;

            return (btn, cardObj);
        }

        private static (GameObject, Button, Slider, Slider) BuildPengaturanModal(Transform parent, Sprite cardSprite, Sprite pillSprite, Sprite btnSprite)
        {
            GameObject modal = CreateUI("Panel_Pengaturan", parent);
            RectTransform mr = modal.GetComponent<RectTransform>();
            mr.anchorMin = new Vector2(0.5f, 0.5f); mr.anchorMax = new Vector2(0.5f, 0.5f);
            mr.pivot = new Vector2(0.5f, 0.5f);
            mr.sizeDelta = new Vector2(920, 680);
            mr.anchoredPosition = Vector2.zero;

            Image mi = modal.AddComponent<Image>();
            if (cardSprite != null) { mi.sprite = cardSprite; mi.type = Image.Type.Sliced; }
            mi.color = Color.white;

            // Header Title
            GameObject titleObj = CreateUI("Title", modal.transform);
            RectTransform tr = titleObj.GetComponent<RectTransform>();
            tr.anchorMin = new Vector2(0.5f, 1); tr.anchorMax = new Vector2(0.5f, 1);
            tr.pivot = new Vector2(0.5f, 1);
            tr.sizeDelta = new Vector2(800, 70);
            tr.anchoredPosition = new Vector2(0, -35);

            TextMeshProUGUI tt = titleObj.AddComponent<TextMeshProUGUI>();
            if (appFont != null) tt.font = appFont;
            tt.text = "<b><color=#064E3B>Pengaturan Suara</color></b>";
            tt.fontSize = 38;
            tt.alignment = TextAlignmentOptions.Center;

            // BGM Slider Row
            (Slider sBGM, _) = CreateSliderRow(modal.transform, "Row_BGM", -140, "Musik Latar (BGM)");

            // SFX Slider Row
            (Slider sSFX, _) = CreateSliderRow(modal.transform, "Row_SFX", -290, "Efek Suara Satwa (SFX)");

            // Close Button
            GameObject closeBtn = CreateUI("Btn_Tutup", modal.transform);
            RectTransform cbr = closeBtn.GetComponent<RectTransform>();
            cbr.anchorMin = new Vector2(0.5f, 0); cbr.anchorMax = new Vector2(0.5f, 0);
            cbr.pivot = new Vector2(0.5f, 0);
            cbr.sizeDelta = new Vector2(480, 84);
            cbr.anchoredPosition = new Vector2(0, 45);

            Image cbi = closeBtn.AddComponent<Image>();
            if (btnSprite != null) { cbi.sprite = btnSprite; cbi.type = Image.Type.Sliced; }
            Button btnClose = closeBtn.AddComponent<Button>();

            GameObject closeTxt = CreateUI("Text", closeBtn.transform);
            TextMeshProUGUI ctt = closeTxt.AddComponent<TextMeshProUGUI>();
            if (appFont != null) ctt.font = appFont;
            ctt.text = "<b><color=#FFFFFF>Simpan & Tutup</color></b>";
            ctt.fontSize = 28;
            ctt.alignment = TextAlignmentOptions.Center;
            FillParent(closeTxt.GetComponent<RectTransform>());

            modal.SetActive(false);
            return (modal, btnClose, sBGM, sSFX);
        }

        private static (GameObject, Button) BuildTentangModal(Transform parent, Sprite cardSprite, Sprite pillSprite, Sprite iconMascot)
        {
            // Modal Root Box (920 x 830 - Snug, perfectly proportioned card with zero wasted space)
            GameObject modal = CreateUI("Panel_Tentang", parent);
            RectTransform mr = modal.GetComponent<RectTransform>();
            mr.anchorMin = new Vector2(0.5f, 0.5f); mr.anchorMax = new Vector2(0.5f, 0.5f);
            mr.pivot = new Vector2(0.5f, 0.5f);
            mr.sizeDelta = new Vector2(920, 830);
            mr.anchoredPosition = Vector2.zero;

            Image mi = modal.AddComponent<Image>();
            if (cardSprite != null) { mi.sprite = cardSprite; mi.type = Image.Type.Sliced; }
            mi.color = Color.white;

            // 1. Logo Mascot (104 x 104, centered top, elegant proportion)
            GameObject mascot = CreateUI("Mascot", modal.transform);
            RectTransform msr = mascot.GetComponent<RectTransform>();
            msr.anchorMin = new Vector2(0.5f, 1); msr.anchorMax = new Vector2(0.5f, 1);
            msr.pivot = new Vector2(0.5f, 1);
            msr.sizeDelta = new Vector2(104, 104);
            msr.anchoredPosition = new Vector2(0, -24);
            Image msi = mascot.AddComponent<Image>();
            if (iconMascot != null) msi.sprite = iconMascot;
            msi.preserveAspect = true;
            msi.raycastTarget = false;

            // 2. Title: Tentang Aplikasi
            GameObject titleObj = CreateUI("Title", modal.transform);
            RectTransform tr = titleObj.GetComponent<RectTransform>();
            tr.anchorMin = new Vector2(0.5f, 1); tr.anchorMax = new Vector2(0.5f, 1);
            tr.pivot = new Vector2(0.5f, 1);
            tr.sizeDelta = new Vector2(840, 44);
            tr.anchoredPosition = new Vector2(0, -136);

            TextMeshProUGUI tt = titleObj.AddComponent<TextMeshProUGUI>();
            if (appFont != null) tt.font = appFont;
            tt.text = "<b><color=#064E3B>Tentang Aplikasi</color></b>";
            tt.fontSize = 36;
            tt.alignment = TextAlignmentOptions.Center;
            tt.raycastTarget = false;

            // 3. App Name & Version Subtitle
            GameObject subObj = CreateUI("Subtitle", modal.transform);
            RectTransform sr = subObj.GetComponent<RectTransform>();
            sr.anchorMin = new Vector2(0.5f, 1); sr.anchorMax = new Vector2(0.5f, 1);
            sr.pivot = new Vector2(0.5f, 1);
            sr.sizeDelta = new Vector2(840, 52);
            sr.anchoredPosition = new Vector2(0, -184);

            TextMeshProUGUI st = subObj.AddComponent<TextMeshProUGUI>();
            if (appFont != null) st.font = appFont;
            st.text = "<b><color=#047857>SATWA LANGKA AR INDONESIA</color></b>\n<size=19><color=#64748B>Versi 1.0.0 • Edisi Edukasi Konservasi Hayati</color></size>";
            st.fontSize = 24;
            st.lineSpacing = 1.15f;
            st.alignment = TextAlignmentOptions.Center;
            st.raycastTarget = false;

            // 4. Description Box (Rounded soft container with clear padding)
            GameObject descBox = CreateUI("DescBox", modal.transform);
            RectTransform dbr = descBox.GetComponent<RectTransform>();
            dbr.anchorMin = new Vector2(0.5f, 1); dbr.anchorMax = new Vector2(0.5f, 1);
            dbr.pivot = new Vector2(0.5f, 1);
            dbr.sizeDelta = new Vector2(840, 136);
            dbr.anchoredPosition = new Vector2(0, -242);

            Image dbi = descBox.AddComponent<Image>();
            if (cardSprite != null) { dbi.sprite = cardSprite; dbi.type = Image.Type.Sliced; }
            dbi.color = new Color(0.95f, 0.98f, 0.96f, 1f);
            dbi.raycastTarget = false;

            GameObject descTextObj = CreateUI("Text", descBox.transform);
            RectTransform dtr = descTextObj.GetComponent<RectTransform>();
            dtr.anchorMin = Vector2.zero; dtr.anchorMax = Vector2.one;
            dtr.offsetMin = new Vector2(20, 8); dtr.offsetMax = new Vector2(-20, -8);

            TextMeshProUGUI dt = descTextObj.AddComponent<TextMeshProUGUI>();
            if (appFont != null) dt.font = appFont;
            dt.text = "<color=#1E293B>Media pembelajaran interaktif berbasis <b>Augmented Reality (AR)</b> tanpa marker untuk mengenalkan 12 satwa endemik Indonesia yang dilindungi secara mendalam, visual, dan menyenangkan.</color>";
            dt.fontSize = 20.5f;
            dt.lineSpacing = 1.25f;
            dt.alignment = TextAlignmentOptions.Center;
            dt.enableWordWrapping = true;
            dt.raycastTarget = false;

            // 5. Fitur Unggulan Container
            GameObject featBox = CreateUI("FeatureBox", modal.transform);
            RectTransform fbr = featBox.GetComponent<RectTransform>();
            fbr.anchorMin = new Vector2(0.5f, 1); fbr.anchorMax = new Vector2(0.5f, 1);
            fbr.pivot = new Vector2(0.5f, 1);
            fbr.sizeDelta = new Vector2(840, 276);
            fbr.anchoredPosition = new Vector2(0, -386);

            Image fbi = featBox.AddComponent<Image>();
            if (cardSprite != null) { fbi.sprite = cardSprite; fbi.type = Image.Type.Sliced; }
            fbi.color = new Color(0.91f, 0.97f, 0.93f, 1f);
            fbi.raycastTarget = false;

            // 5.1 Fitur Title
            GameObject featTitleObj = CreateUI("FeatTitle", featBox.transform);
            RectTransform ftr = featTitleObj.GetComponent<RectTransform>();
            ftr.anchorMin = new Vector2(0.5f, 1); ftr.anchorMax = new Vector2(0.5f, 1);
            ftr.pivot = new Vector2(0.5f, 1);
            ftr.sizeDelta = new Vector2(800, 32);
            ftr.anchoredPosition = new Vector2(0, -12);

            TextMeshProUGUI ftt = featTitleObj.AddComponent<TextMeshProUGUI>();
            if (appFont != null) ftt.font = appFont;
            ftt.text = "<b><color=#047857>★ FITUR UNGGULAN APLIKASI ★</color></b>";
            ftt.fontSize = 20;
            ftt.alignment = TextAlignmentOptions.Center;
            ftt.raycastTarget = false;

            // 5.2 Fitur Items List (Left-aligned within container for clean readability)
            GameObject featListObj = CreateUI("FeatList", featBox.transform);
            RectTransform flr = featListObj.GetComponent<RectTransform>();
            flr.anchorMin = new Vector2(0, 0); flr.anchorMax = new Vector2(1, 1);
            flr.offsetMin = new Vector2(24, 12); flr.offsetMax = new Vector2(-24, -46);

            TextMeshProUGUI flt = featListObj.AddComponent<TextMeshProUGUI>();
            if (appFont != null) flt.font = appFont;
            flt.text = "<color=#064E3B><b>• Visualisasi 3D Realistis:</b></color> <color=#334155>Model 3D satwa skala alami & interaktif</color>\n" +
                       "<color=#064E3B><b>• Audio Vokalisasi Asli:</b></color> <color=#334155>Suara karakteristik satwa di habitat aslinya</color>\n" +
                       "<color=#064E3B><b>• Ensiklopedia Konservasi:</b></color> <color=#334155>Data taksonomi standar IUCN, BRIN & KLHK</color>\n" +
                       "<color=#064E3B><b>• Kuis Interaktif:</b></color> <color=#334155>Uji pemahaman fauna & dapatkan sertifikat digital</color>";
            flt.fontSize = 19.5f;
            flt.lineSpacing = 1.25f;
            flt.alignment = TextAlignmentOptions.TopLeft;
            flt.enableWordWrapping = true;
            flt.raycastTarget = false;

            // 6. Footer Info / Research Note
            GameObject footerObj = CreateUI("FooterInfo", modal.transform);
            RectTransform fr = footerObj.GetComponent<RectTransform>();
            fr.anchorMin = new Vector2(0.5f, 1); fr.anchorMax = new Vector2(0.5f, 1);
            fr.pivot = new Vector2(0.5f, 1);
            fr.sizeDelta = new Vector2(840, 42);
            fr.anchoredPosition = new Vector2(0, -670);

            TextMeshProUGUI ft = footerObj.AddComponent<TextMeshProUGUI>();
            if (appFont != null) ft.font = appFont;
            ft.text = "<color=#64748B>Media Edukasi Konservasi Hayati Indonesia Berbasis Augmented Reality\n© 2026 Tim Pengembang Satwa Langka AR</color>";
            ft.fontSize = 16;
            ft.lineSpacing = 1.15f;
            ft.alignment = TextAlignmentOptions.Center;
            ft.raycastTarget = false;

            // 7. Primary Action Close Button ("Mengerti & Tutup")
            GameObject closeBtn = CreateUI("Btn_Tutup", modal.transform);
            RectTransform cbr = closeBtn.GetComponent<RectTransform>();
            cbr.anchorMin = new Vector2(0.5f, 0); cbr.anchorMax = new Vector2(0.5f, 0);
            cbr.pivot = new Vector2(0.5f, 0);
            cbr.sizeDelta = new Vector2(560, 88);
            cbr.anchoredPosition = new Vector2(0, 28);

            Image cbi = closeBtn.AddComponent<Image>();
            Sprite heroBtnSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Pill_Hero_Perfect.png");
            if (heroBtnSprite == null) heroBtnSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Playful/Btn_Hero_Playful.png");
            if (heroBtnSprite != null) { cbi.sprite = heroBtnSprite; cbi.type = Image.Type.Sliced; }
            cbi.color = new Color(0.04f, 0.55f, 0.38f, 1.0f); // Lush vibrant emerald
            Button btnClose = closeBtn.AddComponent<Button>();

            GameObject closeTxt = CreateUI("Text", closeBtn.transform);
            TextMeshProUGUI ctt = closeTxt.AddComponent<TextMeshProUGUI>();
            if (appFont != null) ctt.font = appFont;
            ctt.text = "<b><color=#FFFFFF>Mengerti & Tutup</color></b>";
            ctt.fontSize = 28;
            ctt.alignment = TextAlignmentOptions.Center;
            ctt.raycastTarget = false;
            FillParent(closeTxt.GetComponent<RectTransform>());

            modal.SetActive(false);
            return (modal, btnClose);
        }

        private static (GameObject, Button, Button) BuildKeluarModal(Transform parent, Sprite cardSprite, Sprite pillSprite, Sprite btnSprite)
        {
            GameObject modal = CreateUI("Panel_Keluar", parent);
            RectTransform mr = modal.GetComponent<RectTransform>();
            mr.anchorMin = new Vector2(0.5f, 0.5f); mr.anchorMax = new Vector2(0.5f, 0.5f);
            mr.pivot = new Vector2(0.5f, 0.5f);
            mr.sizeDelta = new Vector2(880, 520);
            mr.anchoredPosition = Vector2.zero;

            Image mi = modal.AddComponent<Image>();
            if (cardSprite != null) { mi.sprite = cardSprite; mi.type = Image.Type.Sliced; }
            mi.color = Color.white;

            // Title
            GameObject titleObj = CreateUI("Title", modal.transform);
            RectTransform tr = titleObj.GetComponent<RectTransform>();
            tr.anchorMin = new Vector2(0.5f, 1); tr.anchorMax = new Vector2(0.5f, 1);
            tr.pivot = new Vector2(0.5f, 1);
            tr.sizeDelta = new Vector2(800, 60);
            tr.anchoredPosition = new Vector2(0, -45);

            TextMeshProUGUI tt = titleObj.AddComponent<TextMeshProUGUI>();
            if (appFont != null) tt.font = appFont;
            tt.text = "<b><color=#DC2626>Keluar Aplikasi?</color></b>";
            tt.fontSize = 38;
            tt.alignment = TextAlignmentOptions.Center;

            // Message
            GameObject msgObj = CreateUI("Message", modal.transform);
            RectTransform msgr = msgObj.GetComponent<RectTransform>();
            msgr.anchorMin = new Vector2(0.5f, 1); msgr.anchorMax = new Vector2(0.5f, 1);
            msgr.pivot = new Vector2(0.5f, 1);
            msgr.sizeDelta = new Vector2(760, 160);
            msgr.anchoredPosition = new Vector2(0, -135);

            TextMeshProUGUI mt = msgObj.AddComponent<TextMeshProUGUI>();
            if (appFont != null) mt.font = appFont;
            mt.text = "<color=#334155>Apakah kamu yakin ingin keluar dari petualangan Satwa Langka AR?</color>";
            mt.fontSize = 26;
            mt.lineSpacing = 1.3f;
            mt.alignment = TextAlignmentOptions.Center;

            // Batal Button (Left)
            GameObject cancelBtn = CreateUI("Btn_Batal", modal.transform);
            RectTransform cbr = cancelBtn.GetComponent<RectTransform>();
            cbr.anchorMin = new Vector2(0.5f, 0); cbr.anchorMax = new Vector2(0.5f, 0);
            cbr.pivot = new Vector2(0.5f, 0);
            cbr.sizeDelta = new Vector2(340, 84);
            cbr.anchoredPosition = new Vector2(-190, 45);

            Image cbi = cancelBtn.AddComponent<Image>();
            if (pillSprite != null) { cbi.sprite = pillSprite; cbi.type = Image.Type.Sliced; }
            cbi.color = new Color(0.94f, 0.96f, 0.98f, 1f);
            Button btnCancel = cancelBtn.AddComponent<Button>();

            GameObject cancelTxt = CreateUI("Text", cancelBtn.transform);
            TextMeshProUGUI cnt = cancelTxt.AddComponent<TextMeshProUGUI>();
            if (appFont != null) cnt.font = appFont;
            cnt.text = "<b><color=#475569>Batal</color></b>";
            cnt.fontSize = 28;
            cnt.alignment = TextAlignmentOptions.Center;
            FillParent(cancelTxt.GetComponent<RectTransform>());

            // Keluar Button (Right)
            GameObject confirmBtn = CreateUI("Btn_Konfirmasi", modal.transform);
            RectTransform cfbr = confirmBtn.GetComponent<RectTransform>();
            cfbr.anchorMin = new Vector2(0.5f, 0); cfbr.anchorMax = new Vector2(0.5f, 0);
            cfbr.pivot = new Vector2(0.5f, 0);
            cfbr.sizeDelta = new Vector2(340, 84);
            cfbr.anchoredPosition = new Vector2(190, 45);

            Image cfbi = confirmBtn.AddComponent<Image>();
            if (pillSprite != null) { cfbi.sprite = pillSprite; cfbi.type = Image.Type.Sliced; }
            cfbi.color = new Color(0.99f, 0.88f, 0.88f, 1f);
            Button btnConfirm = confirmBtn.AddComponent<Button>();

            GameObject confirmTxt = CreateUI("Text", confirmBtn.transform);
            TextMeshProUGUI cft = confirmTxt.AddComponent<TextMeshProUGUI>();
            if (appFont != null) cft.font = appFont;
            cft.text = "<b><color=#DC2626>Ya, Keluar</color></b>";
            cft.fontSize = 28;
            cft.alignment = TextAlignmentOptions.Center;
            FillParent(confirmTxt.GetComponent<RectTransform>());

            modal.SetActive(false);
            return (modal, btnConfirm, btnCancel);
        }

        private static (Slider, TextMeshProUGUI) CreateSliderRow(Transform parent, string name, float posY, string label)
        {
            GameObject row = CreateUI(name, parent);
            RectTransform rr = row.GetComponent<RectTransform>();
            rr.anchorMin = new Vector2(0.5f, 1); rr.anchorMax = new Vector2(0.5f, 1);
            rr.pivot = new Vector2(0.5f, 1);
            rr.sizeDelta = new Vector2(800, 110);
            rr.anchoredPosition = new Vector2(0, posY);

            GameObject lbl = CreateUI("Label", row.transform);
            RectTransform lr = lbl.GetComponent<RectTransform>();
            lr.anchorMin = new Vector2(0, 1); lr.anchorMax = new Vector2(1, 1);
            lr.pivot = new Vector2(0, 1);
            lr.sizeDelta = new Vector2(800, 40);
            lr.anchoredPosition = Vector2.zero;

            TextMeshProUGUI lt = lbl.AddComponent<TextMeshProUGUI>();
            if (appFont != null) lt.font = appFont;
            lt.text = $"<b><color=#0F172A>{label}</color></b>";
            lt.fontSize = 24;

            // Slider Object
            GameObject sObj = CreateUI("Slider", row.transform);
            RectTransform sr = sObj.GetComponent<RectTransform>();
            sr.anchorMin = new Vector2(0, 0); sr.anchorMax = new Vector2(1, 0);
            sr.pivot = new Vector2(0.5f, 0);
            sr.sizeDelta = new Vector2(800, 48);
            sr.anchoredPosition = Vector2.zero;

            Slider slider = sObj.AddComponent<Slider>();
            slider.minValue = 0f;
            slider.maxValue = 1f;

            // Background track
            GameObject sBg = CreateUI("Background", sObj.transform);
            FillParent(sBg.GetComponent<RectTransform>());
            Image sbi = sBg.AddComponent<Image>();
            sbi.color = new Color(0.88f, 0.92f, 0.90f, 1f);

            // Fill Area
            GameObject fa = CreateUI("Fill Area", sObj.transform);
            RectTransform far = fa.GetComponent<RectTransform>();
            far.anchorMin = new Vector2(0, 0.25f); far.anchorMax = new Vector2(1, 0.75f);
            far.offsetMin = new Vector2(10, 0); far.offsetMax = new Vector2(-10, 0);

            GameObject fill = CreateUI("Fill", fa.transform);
            FillParent(fill.GetComponent<RectTransform>());
            Image fi = fill.AddComponent<Image>();
            fi.color = new Color(0.05f, 0.59f, 0.41f, 1f); // Emerald fill

            // Handle Area
            GameObject ha = CreateUI("Handle Slide Area", sObj.transform);
            FillParent(ha.GetComponent<RectTransform>());
            ha.GetComponent<RectTransform>().offsetMin = new Vector2(15, 0);
            ha.GetComponent<RectTransform>().offsetMax = new Vector2(-15, 0);

            GameObject handle = CreateUI("Handle", ha.transform);
            RectTransform hr = handle.GetComponent<RectTransform>();
            hr.sizeDelta = new Vector2(40, 40);
            Image hi = handle.AddComponent<Image>();
            hi.color = Color.white;

            slider.fillRect = fill.GetComponent<RectTransform>();
            slider.handleRect = hr;
            slider.targetGraphic = hi;

            return (slider, lt);
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
