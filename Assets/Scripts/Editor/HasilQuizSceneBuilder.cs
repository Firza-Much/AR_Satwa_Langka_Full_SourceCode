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
    public static class HasilQuizSceneBuilder
    {
        private static TMP_FontAsset appFont => AppTheme.Font;

        [MenuItem("Satwa Langka/Create & Open HasilQuiz Scene")]
        public static void BuildHasilQuizScene()
        {
            string scenePath = "Assets/Scenes/HasilQuiz.unity";
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            SetupCameraAndLighting();
            BuildHasilQuizCanvas();

            EditorSceneManager.SaveScene(scene, scenePath);
            UpdateBuildSettings(scenePath);

            Debug.Log("<b>[SATWA AR]</b> HasilQuiz scene built and saved at " + scenePath);
        }

        private static void SetupCameraAndLighting()
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

        private static void BuildHasilQuizCanvas()
        {
            GameObject canvasObj = new GameObject("Canvas_HasilQuiz");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0.0f;

            canvasObj.AddComponent<GraphicRaycaster>();
            
            HasilQuizController controller = canvasObj.AddComponent<HasilQuizController>();

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

            GameObject titleObj = CreateUI("HeaderTitle", topBar.transform);
            RectTransform tr = titleObj.GetComponent<RectTransform>();
            tr.anchorMin = new Vector2(0.5f, 0.5f); tr.anchorMax = new Vector2(0.5f, 0.5f);
            tr.pivot = new Vector2(0.5f, 0.5f);
            tr.sizeDelta = new Vector2(600, 60);
            tr.anchoredPosition = Vector2.zero;

            TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
            if (appFont != null) titleText.font = appFont;
            titleText.text = "<b><color=#064E3B>Hasil Evaluasi Kuis</color></b>";
            titleText.fontSize = 28;
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.color = new Color(0.04f, 0.31f, 0.23f, 1f);
            titleText.raycastTarget = false;

            // 1.2 RESULT CONTAINER CARD
            GameObject resultCard = CreateUI("ResultCard", safeArea.transform);
            RectTransform rcr = resultCard.GetComponent<RectTransform>();
            rcr.anchorMin = new Vector2(0.5f, 0.5f); rcr.anchorMax = new Vector2(0.5f, 0.5f);
            rcr.pivot = new Vector2(0.5f, 0.5f);
            rcr.sizeDelta = new Vector2(960, 1420);
            rcr.anchoredPosition = new Vector2(0, -45);

            Image rcImg = resultCard.AddComponent<Image>();
            if (cardSprite != null) { rcImg.sprite = cardSprite; rcImg.type = Image.Type.Sliced; }
            rcImg.color = Color.white;
            rcImg.raycastTarget = false;

            // 2.1 Result Pill Header Badge
            GameObject resPill = CreateUI("ResultPill", resultCard.transform);
            RectTransform rpr = resPill.GetComponent<RectTransform>();
            rpr.anchorMin = new Vector2(0.5f, 1); rpr.anchorMax = new Vector2(0.5f, 1);
            rpr.pivot = new Vector2(0.5f, 1);
            rpr.sizeDelta = new Vector2(260, 48);
            rpr.anchoredPosition = new Vector2(0, 24);

            Image rpImg = resPill.AddComponent<Image>();
            if (pillSprite != null) { rpImg.sprite = pillSprite; rpImg.type = Image.Type.Sliced; }
            rpImg.color = new Color(0.04f, 0.48f, 0.35f, 1f);
            rpImg.raycastTarget = false;

            GameObject rpTxt = CreateUI("Text", resPill.transform);
            TextMeshProUGUI rpt = rpTxt.AddComponent<TextMeshProUGUI>();
            if (appFont != null) rpt.font = appFont;
            rpt.text = "<b>Hasil Quiz</b>";
            rpt.fontSize = 20;
            rpt.alignment = TextAlignmentOptions.Center;
            rpt.color = Color.white;
            rpt.raycastTarget = false;
            FillParent(rpTxt.GetComponent<RectTransform>());

            // 2.2 Score Label Title
            GameObject scoreLabelObj = CreateUI("ScoreLabel", resultCard.transform);
            RectTransform slr = scoreLabelObj.GetComponent<RectTransform>();
            slr.anchorMin = new Vector2(0.5f, 1); slr.anchorMax = new Vector2(0.5f, 1);
            slr.pivot = new Vector2(0.5f, 1);
            slr.sizeDelta = new Vector2(500, 45);
            slr.anchoredPosition = new Vector2(0, -90);

            TextMeshProUGUI slt = scoreLabelObj.AddComponent<TextMeshProUGUI>();
            if (appFont != null) slt.font = appFont;
            slt.text = "<b><color=#059669>NILAI AKHIR ANDA</color></b>";
            slt.fontSize = 22;
            slt.alignment = TextAlignmentOptions.Center;
            slt.color = new Color(0.02f, 0.48f, 0.34f, 1f);
            slt.raycastTarget = false;

            // 2.3 Big Score Number Display
            GameObject scoreNumObj = CreateUI("ScoreNumber", resultCard.transform);
            RectTransform snr = scoreNumObj.GetComponent<RectTransform>();
            snr.anchorMin = new Vector2(0.5f, 1); snr.anchorMax = new Vector2(0.5f, 1);
            snr.pivot = new Vector2(0.5f, 1);
            snr.sizeDelta = new Vector2(600, 160);
            snr.anchoredPosition = new Vector2(0, -150);

            TextMeshProUGUI snt = scoreNumObj.AddComponent<TextMeshProUGUI>();
            if (appFont != null) snt.font = appFont;
            snt.text = "<b><color=#059669>100</color></b><size=52><color=#64748B>/100</color></size>";
            snt.fontSize = 110;
            snt.alignment = TextAlignmentOptions.Center;
            snt.color = new Color(0.02f, 0.48f, 0.34f, 1f);
            snt.raycastTarget = false;

            // 2.4 Statistics Breakdown Pill
            GameObject statsObj = CreateUI("StatsBreakdown", resultCard.transform);
            RectTransform str = statsObj.GetComponent<RectTransform>();
            str.anchorMin = new Vector2(0.5f, 1); str.anchorMax = new Vector2(0.5f, 1);
            str.pivot = new Vector2(0.5f, 1);
            str.sizeDelta = new Vector2(820, 64);
            str.anchoredPosition = new Vector2(0, -350);

            Image stImg = statsObj.AddComponent<Image>();
            if (btnSubtle != null) { stImg.sprite = btnSubtle; stImg.type = Image.Type.Sliced; }
            stImg.color = Color.white;
            stImg.raycastTarget = false;

            GameObject stTxtObj = CreateUI("Text", statsObj.transform);
            TextMeshProUGUI stText = stTxtObj.AddComponent<TextMeshProUGUI>();
            if (appFont != null) stText.font = appFont;
            stText.text = "<b><color=#064E3B>Jawaban Benar:</color></b> <color=#059669>10</color>  |  <b><color=#064E3B>Jawaban Salah:</color></b> <color=#DC2626>0</color>";
            stText.fontSize = 24;
            stText.alignment = TextAlignmentOptions.Center;
            stText.color = new Color(0.04f, 0.22f, 0.16f);
            stText.raycastTarget = false;
            FillParent(stTxtObj.GetComponent<RectTransform>());

            // 2.5 Evaluation Motivation Message Box
            GameObject evalObj = CreateUI("EvaluationMessage", resultCard.transform);
            RectTransform er = evalObj.GetComponent<RectTransform>();
            er.anchorMin = new Vector2(0.5f, 1); er.anchorMax = new Vector2(0.5f, 1);
            er.pivot = new Vector2(0.5f, 1);
            er.sizeDelta = new Vector2(840, 180);
            er.anchoredPosition = new Vector2(0, -470);

            TextMeshProUGUI evalText = evalObj.AddComponent<TextMeshProUGUI>();
            if (appFont != null) evalText.font = appFont;
            evalText.text = "<b><color=#064E3B>Luar Biasa!</color></b>\n<color=#1E293B>Pemahaman Anda tentang fauna endemik nusantara sangat baik.</color>";
            evalText.fontSize = 24;
            evalText.lineSpacing = 1.35f;
            evalText.alignment = TextAlignmentOptions.Center;
            evalText.color = new Color(0.06f, 0.18f, 0.14f, 1f);
            evalText.raycastTarget = false;

            // 3. ACTION BUTTONS (Hero Forest CTA for Ulang, Clean Subtle for Menu)
            // 3.1 Ulang Button (Hero Forest CTA - Green with White text)
            GameObject ulangBtn = CreateUI("Btn_Ulang", resultCard.transform);
            RectTransform ubr = ulangBtn.GetComponent<RectTransform>();
            ubr.anchorMin = new Vector2(0.5f, 0); ubr.anchorMax = new Vector2(0.5f, 0);
            ubr.pivot = new Vector2(0.5f, 0);
            ubr.sizeDelta = new Vector2(820, 110);
            ubr.anchoredPosition = new Vector2(0, 170);

            Image ubImg = ulangBtn.AddComponent<Image>();
            if (btnHero != null) { ubImg.sprite = btnHero; ubImg.type = Image.Type.Sliced; }
            ubImg.color = Color.white;
            ubImg.raycastTarget = true;
            Button bUlang = ulangBtn.AddComponent<Button>();

            GameObject ubTxt = CreateUI("Text", ulangBtn.transform);
            TextMeshProUGUI ubt = ubTxt.AddComponent<TextMeshProUGUI>();
            if (appFont != null) ubt.font = appFont;
            ubt.text = "<b><color=#FFFFFF>Ulang Kuis</color></b>";
            ubt.fontSize = 26;
            ubt.alignment = TextAlignmentOptions.Center;
            ubt.raycastTarget = false;
            FillParent(ubTxt.GetComponent<RectTransform>());

            // 3.2 Menu Button (Clean Card Subtle Button)
            GameObject menuBtn = CreateUI("Btn_Menu", resultCard.transform);
            RectTransform mbr = menuBtn.GetComponent<RectTransform>();
            mbr.anchorMin = new Vector2(0.5f, 0); mbr.anchorMax = new Vector2(0.5f, 0);
            mbr.pivot = new Vector2(0.5f, 0);
            mbr.sizeDelta = new Vector2(820, 100);
            mbr.anchoredPosition = new Vector2(0, 50);

            Image mbImg = menuBtn.AddComponent<Image>();
            if (btnSubtle != null) { mbImg.sprite = btnSubtle; mbImg.type = Image.Type.Sliced; }
            mbImg.color = Color.white;
            mbImg.raycastTarget = true;
            Button bMenu = menuBtn.AddComponent<Button>();

            GameObject mbTxt = CreateUI("Text", menuBtn.transform);
            TextMeshProUGUI mbt = mbTxt.AddComponent<TextMeshProUGUI>();
            if (appFont != null) mbt.font = appFont;
            mbt.text = "<b><color=#064E3B>Kembali ke Menu Utama</color></b>";
            mbt.fontSize = 24;
            mbt.alignment = TextAlignmentOptions.Center;
            mbt.raycastTarget = false;
            FillParent(mbTxt.GetComponent<RectTransform>());

            // Wire Controller Fields
            SerializedObject so = new SerializedObject(controller);
            so.FindProperty("txtHeader").objectReferenceValue = titleText;
            so.FindProperty("txtFinalScore").objectReferenceValue = snt;
            so.FindProperty("txtDetailStats").objectReferenceValue = stText;
            so.FindProperty("txtEvaluationMessage").objectReferenceValue = evalText;
            so.FindProperty("btnUlang").objectReferenceValue = bUlang;
            so.FindProperty("btnMenu").objectReferenceValue = bMenu;

            so.ApplyModifiedProperties();
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
