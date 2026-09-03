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
    public static class PengaturanSceneBuilder
    {
        private static TMP_FontAsset appFont => AppTheme.Font;

        [MenuItem("Satwa Langka/Create & Open Pengaturan Scene")]
        public static void BuildPengaturanScene()
        {
            string scenePath = "Assets/Scenes/Pengaturan.unity";
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            SetupCameraAndLighting();
            BuildPengaturanCanvas();

            EditorSceneManager.SaveScene(scene, scenePath);
            UpdateBuildSettings(scenePath);

            Debug.Log("<b>[SATWA AR]</b> Pengaturan scene built and saved at " + scenePath);
        }

        private static void SetupCameraAndLighting()
        {
            GameObject camObj = new GameObject("Main Camera");
            Camera cam = camObj.AddComponent<Camera>();
            camObj.tag = "MainCamera";
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.94f, 0.96f, 0.95f, 1.0f);
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

        private static void BuildPengaturanCanvas()
        {
            GameObject canvasObj = new GameObject("Canvas_Pengaturan");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0.0f;

            canvasObj.AddComponent<GraphicRaycaster>();
            
            PengaturanController controller = canvasObj.AddComponent<PengaturanController>();

            // Sprites
            Sprite bgBackdrop = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Playful/Backdrop_Nature_Playful.png");
            if (bgBackdrop == null) bgBackdrop = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Palette_Daylight_Soft.png");
            Sprite cardSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Playful/Card_Cohesive_Playful.png");
            if (cardSprite == null) cardSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Card_Exact.png");

            Sprite btnSubtle = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Playful/Pill_Badge_Tag.png");
            if (btnSubtle == null) btnSubtle = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Btn_Subtle_Card.png");

            Sprite btnHero = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Playful/Btn_Hero_Playful.png");
            if (btnHero == null) btnHero = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Pill_Hero_Perfect.png");

            Sprite pillSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Playful/Pill_Badge_Tag.png");

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

            // 1.1 TOP BAR (Height 160, safe margin from camera notch)
            GameObject topBar = CreateUI("TopBar", safeArea.transform);
            RectTransform tbRect = topBar.GetComponent<RectTransform>();
            tbRect.anchorMin = new Vector2(0, 1); tbRect.anchorMax = new Vector2(1, 1);
            tbRect.pivot = new Vector2(0.5f, 1);
            tbRect.sizeDelta = new Vector2(0, 160);
            tbRect.anchoredPosition = new Vector2(0, -30);

            // Kembali Button
            GameObject backBtn = CreateUI("Btn_Kembali", topBar.transform);
            RectTransform bbr = backBtn.GetComponent<RectTransform>();
            bbr.anchorMin = new Vector2(0, 0.5f); bbr.anchorMax = new Vector2(0, 0.5f);
            bbr.pivot = new Vector2(0, 0.5f);
            bbr.sizeDelta = new Vector2(170, 60);
            bbr.anchoredPosition = new Vector2(36, 0);

            Image backImg = backBtn.AddComponent<Image>();
            if (btnSubtle != null) { backImg.sprite = btnSubtle; backImg.type = Image.Type.Sliced; }
            Button bKembali = backBtn.AddComponent<Button>();

            GameObject backTxt = CreateUI("Text", backBtn.transform);
            TextMeshProUGUI bkt = backTxt.AddComponent<TextMeshProUGUI>();
            if (appFont != null) bkt.font = appFont;
            bkt.text = "<b><color=#064E3B>‹ Kembali</color></b>";
            bkt.fontSize = 22;
            bkt.alignment = TextAlignmentOptions.Center;
            bkt.raycastTarget = false;
            FillParent(backTxt.GetComponent<RectTransform>());

            // Title
            GameObject titleObj = CreateUI("HeaderTitle", topBar.transform);
            RectTransform tr = titleObj.GetComponent<RectTransform>();
            tr.anchorMin = new Vector2(0.5f, 0.5f); tr.anchorMax = new Vector2(0.5f, 0.5f);
            tr.pivot = new Vector2(0.5f, 0.5f);
            tr.sizeDelta = new Vector2(600, 60);
            tr.anchoredPosition = Vector2.zero;

            TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
            if (appFont != null) titleText.font = appFont;
            titleText.text = "<b><color=#064E3B>Pengaturan Aplikasi</color></b>";
            titleText.fontSize = 28;
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.raycastTarget = false;

            // 1.2 MAIN SETTINGS CARD (Elevated Card Container)
            GameObject setCard = CreateUI("SettingsCard", safeArea.transform);
            RectTransform scr = setCard.GetComponent<RectTransform>();
            scr.anchorMin = new Vector2(0.5f, 0.5f); scr.anchorMax = new Vector2(0.5f, 0.5f);
            scr.pivot = new Vector2(0.5f, 0.5f);
            scr.sizeDelta = new Vector2(960, 1380);
            scr.anchoredPosition = new Vector2(0, -45);

            Image scImg = setCard.AddComponent<Image>();
            if (cardSprite != null) { scImg.sprite = cardSprite; scImg.type = Image.Type.Sliced; }
            scImg.color = Color.white;
            scImg.raycastTarget = false;

            // 2.1 Settings Pill Header
            GameObject sPill = CreateUI("SettingsPill", setCard.transform);
            RectTransform spr = sPill.GetComponent<RectTransform>();
            spr.anchorMin = new Vector2(0.5f, 1); spr.anchorMax = new Vector2(0.5f, 1);
            spr.pivot = new Vector2(0.5f, 1);
            spr.sizeDelta = new Vector2(280, 48);
            spr.anchoredPosition = new Vector2(0, 24);

            Image spImg = sPill.AddComponent<Image>();
            if (pillSprite != null) { spImg.sprite = pillSprite; spImg.type = Image.Type.Sliced; }
            spImg.color = new Color(0.04f, 0.48f, 0.35f, 1f);
            spImg.raycastTarget = false;

            GameObject spTxt = CreateUI("Text", sPill.transform);
            TextMeshProUGUI spt = spTxt.AddComponent<TextMeshProUGUI>();
            if (appFont != null) spt.font = appFont;
            spt.text = "<b>Audio & Suara</b>";
            spt.fontSize = 20;
            spt.alignment = TextAlignmentOptions.Center;
            spt.color = Color.white;
            spt.raycastTarget = false;
            FillParent(spTxt.GetComponent<RectTransform>());

            // Section Title
            GameObject secObj = CreateUI("SectionHeader", setCard.transform);
            RectTransform secRt = secObj.GetComponent<RectTransform>();
            secRt.anchorMin = new Vector2(0.5f, 1); secRt.anchorMax = new Vector2(0.5f, 1);
            secRt.pivot = new Vector2(0.5f, 1);
            secRt.sizeDelta = new Vector2(800, 50);
            secRt.anchoredPosition = new Vector2(0, -90);

            TextMeshProUGUI secText = secObj.AddComponent<TextMeshProUGUI>();
            if (appFont != null) secText.font = appFont;
            secText.text = "<b><color=#064E3B>Konfigurasi Volume Audio</color></b>";
            secText.fontSize = 26;
            secText.alignment = TextAlignmentOptions.Center;
            secText.raycastTarget = false;

            // 2.2 BGM VOLUME ROW CARD
            GameObject bgmCard = CreateUI("Card_BGM", setCard.transform);
            RectTransform bcr = bgmCard.GetComponent<RectTransform>();
            bcr.anchorMin = new Vector2(0.5f, 1); bcr.anchorMax = new Vector2(0.5f, 1);
            bcr.pivot = new Vector2(0.5f, 1);
            bcr.sizeDelta = new Vector2(860, 220);
            bcr.anchoredPosition = new Vector2(0, -180);

            // 1.2 BGM Slider Section
            GameObject bgmSection = CreateUI("BGM_Section", setCard.transform);
            RectTransform bsr = bgmSection.GetComponent<RectTransform>();
            bsr.anchorMin = new Vector2(0.5f, 1); bsr.anchorMax = new Vector2(0.5f, 1);
            bsr.pivot = new Vector2(0.5f, 1);
            bsr.sizeDelta = new Vector2(760, 150);
            bsr.anchoredPosition = new Vector2(0, -180);

            GameObject bgmLabel = CreateUI("Label", bgmSection.transform);
            RectTransform blr = bgmLabel.GetComponent<RectTransform>();
            blr.anchorMin = new Vector2(0, 1); blr.anchorMax = new Vector2(1, 1);
            blr.pivot = new Vector2(0, 1);
            blr.sizeDelta = new Vector2(0, 36);
            blr.anchoredPosition = Vector2.zero;

            TextMeshProUGUI blt = bgmLabel.AddComponent<TextMeshProUGUI>();
            if (appFont != null) blt.font = appFont;
            blt.text = "<b><color=#064E3B>Volume Musik Latar (BGM)</color></b>";
            blt.fontSize = 22;

            GameObject bgmVal = CreateUI("ValText", bgmSection.transform);
            RectTransform bvr = bgmVal.GetComponent<RectTransform>();
            bvr.anchorMin = new Vector2(1, 1); bvr.anchorMax = new Vector2(1, 1);
            bvr.pivot = new Vector2(1, 1);
            bvr.sizeDelta = new Vector2(100, 36);
            bvr.anchoredPosition = Vector2.zero;

            TextMeshProUGUI bvt = bgmVal.AddComponent<TextMeshProUGUI>();
            if (appFont != null) bvt.font = appFont;
            bvt.text = "<b><color=#059669>100%</color></b>";
            bvt.fontSize = 22;
            bvt.alignment = TextAlignmentOptions.Right;

            Slider sBGM = CreateModernSlider(bgmSection.transform, new Vector2(0, -70), 760, 48);

            // 1.3 SFX Slider Section
            GameObject sfxSection = CreateUI("SFX_Section", setCard.transform);
            RectTransform sfsr = sfxSection.GetComponent<RectTransform>();
            sfsr.anchorMin = new Vector2(0.5f, 1); sfsr.anchorMax = new Vector2(0.5f, 1);
            sfsr.pivot = new Vector2(0.5f, 1);
            sfsr.sizeDelta = new Vector2(760, 150);
            sfsr.anchoredPosition = new Vector2(0, -360);

            GameObject sfxLabel = CreateUI("Label", sfxSection.transform);
            RectTransform sflr = sfxLabel.GetComponent<RectTransform>();
            sflr.anchorMin = new Vector2(0, 1); sflr.anchorMax = new Vector2(1, 1);
            sflr.pivot = new Vector2(0, 1);
            sflr.sizeDelta = new Vector2(0, 36);
            sflr.anchoredPosition = Vector2.zero;

            TextMeshProUGUI sflt = sfxLabel.AddComponent<TextMeshProUGUI>();
            if (appFont != null) sflt.font = appFont;
            sflt.text = "<b><color=#064E3B>Volume Efek Suara (SFX Satwa)</color></b>";
            sflt.fontSize = 22;

            GameObject sfxVal = CreateUI("ValText", sfxSection.transform);
            RectTransform sfvr = sfxVal.GetComponent<RectTransform>();
            sfvr.anchorMin = new Vector2(1, 1); sfvr.anchorMax = new Vector2(1, 1);
            sfvr.pivot = new Vector2(1, 1);
            sfvr.sizeDelta = new Vector2(100, 36);
            sfvr.anchoredPosition = Vector2.zero;

            TextMeshProUGUI sfvt = sfxVal.AddComponent<TextMeshProUGUI>();
            if (appFont != null) sfvt.font = appFont;
            sfvt.text = "<b><color=#059669>100%</color></b>";
            sfvt.fontSize = 22;
            sfvt.alignment = TextAlignmentOptions.Right;

            Slider sSFX = CreateModernSlider(sfxSection.transform, new Vector2(0, -70), 760, 48);

            // 1.4 Test Audio Button
            GameObject testBtn = CreateUI("Btn_TestAudio", setCard.transform);
            RectTransform tbr = testBtn.GetComponent<RectTransform>();
            tbr.anchorMin = new Vector2(0.5f, 0); tbr.anchorMax = new Vector2(0.5f, 0);
            tbr.pivot = new Vector2(0.5f, 0);
            tbr.sizeDelta = new Vector2(760, 95);
            tbr.anchoredPosition = new Vector2(0, 50);

            Image tbImg = testBtn.AddComponent<Image>();
            if (btnHero != null) { tbImg.sprite = btnHero; tbImg.type = Image.Type.Sliced; }
            Button bTest = testBtn.AddComponent<Button>();

            GameObject tbTxt = CreateUI("Text", testBtn.transform);
            TextMeshProUGUI tbt = tbTxt.AddComponent<TextMeshProUGUI>();
            if (appFont != null) tbt.font = appFont;
            tbt.text = "<b><color=#FFFFFF>Uji Putar Suara Satwa</color></b>";
            tbt.fontSize = 24;
            tbt.alignment = TextAlignmentOptions.Center;
            FillParent(tbTxt.GetComponent<RectTransform>());

            // Wire Controller Fields
            AudioClip testClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/SFX_SATWA01_Gajah.mp3");
            if (testClip == null) testClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/SFX_SATWA09_MacanTutul.mp3");

            SerializedObject so = new SerializedObject(controller);
            so.FindProperty("btnKembali").objectReferenceValue = bKembali;
            so.FindProperty("sliderBGM").objectReferenceValue = sBGM;
            so.FindProperty("txtBGMPercent").objectReferenceValue = bvt;
            so.FindProperty("sliderSFX").objectReferenceValue = sSFX;
            so.FindProperty("txtSFXPercent").objectReferenceValue = sfvt;
            so.FindProperty("btnTestSFX").objectReferenceValue = bTest;
            so.FindProperty("txtTestBtn").objectReferenceValue = tbt;
            if (testClip != null) so.FindProperty("testAudioClip").objectReferenceValue = testClip;

            so.ApplyModifiedProperties();
        }

        private static Slider CreateModernSlider(Transform parent, Vector2 pos, float width, float height)
        {
            GameObject sliderObj = CreateUI("Slider", parent);
            RectTransform sr = sliderObj.GetComponent<RectTransform>();
            sr.anchorMin = new Vector2(0.5f, 1); sr.anchorMax = new Vector2(0.5f, 1);
            sr.pivot = new Vector2(0.5f, 1);
            sr.sizeDelta = new Vector2(width, height);
            sr.anchoredPosition = pos;

            Slider slider = sliderObj.AddComponent<Slider>();
            slider.minValue = 0f;
            slider.maxValue = 1f;

            // Background Track
            GameObject bgTrack = CreateUI("Background", sliderObj.transform);
            RectTransform btr = bgTrack.GetComponent<RectTransform>();
            btr.anchorMin = new Vector2(0, 0.35f); btr.anchorMax = new Vector2(1, 0.65f);
            btr.offsetMin = btr.offsetMax = Vector2.zero;
            Image bti = bgTrack.AddComponent<Image>();
            bti.color = new Color(0.85f, 0.90f, 0.88f, 1f);

            // Fill Area
            GameObject fillArea = CreateUI("Fill Area", sliderObj.transform);
            RectTransform far = fillArea.GetComponent<RectTransform>();
            far.anchorMin = new Vector2(0, 0.35f); far.anchorMax = new Vector2(1, 0.65f);
            far.offsetMin = new Vector2(5, 0); far.offsetMax = new Vector2(-5, 0);

            GameObject fill = CreateUI("Fill", fillArea.transform);
            RectTransform fr = fill.GetComponent<RectTransform>();
            fr.anchorMin = Vector2.zero; fr.anchorMax = Vector2.one;
            fr.offsetMin = fr.offsetMax = Vector2.zero;
            Image fi = fill.AddComponent<Image>();
            fi.color = new Color(0.04f, 0.65f, 0.45f, 1f);

            // Handle Slide Area
            GameObject handleArea = CreateUI("Handle Slide Area", sliderObj.transform);
            RectTransform har = handleArea.GetComponent<RectTransform>();
            har.anchorMin = Vector2.zero; har.anchorMax = Vector2.one;
            har.offsetMin = new Vector2(15, 0); har.offsetMax = new Vector2(-15, 0);

            GameObject handle = CreateUI("Handle", handleArea.transform);
            RectTransform hr = handle.GetComponent<RectTransform>();
            hr.sizeDelta = new Vector2(40, 40);
            Image hi = handle.AddComponent<Image>();
            hi.color = new Color(0.04f, 0.45f, 0.32f, 1f);

            slider.fillRect = fr;
            slider.handleRect = hr;
            slider.targetGraphic = hi;
            slider.direction = Slider.Direction.LeftToRight;

            return slider;
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
