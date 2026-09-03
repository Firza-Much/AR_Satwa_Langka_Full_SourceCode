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
    public static class SoalQuizSceneBuilder
    {
        private static TMP_FontAsset appFont => AppTheme.Font;

        [MenuItem("Satwa Langka/Create & Open SoalQuiz Scene")]
        public static void BuildSoalQuizScene()
        {
            string scenePath = "Assets/Scenes/SoalQuiz.unity";
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            SetupCameraAndLighting();
            BuildSoalQuizCanvas();

            EditorSceneManager.SaveScene(scene, scenePath);
            UpdateBuildSettings(scenePath);

            Debug.Log("<b>[SATWA AR]</b> SoalQuiz scene built and saved at " + scenePath);
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

        private static void BuildSoalQuizCanvas()
        {
            GameObject canvasObj = new GameObject("Canvas_SoalQuiz");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0.0f;

            canvasObj.AddComponent<GraphicRaycaster>();
            
            QuizController controller = canvasObj.AddComponent<QuizController>();

            // Sprites
            Sprite bgBackdrop = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Playful/Backdrop_Nature_Playful.png");
            if (bgBackdrop == null) bgBackdrop = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Palette_Daylight_Soft.png");
            Sprite cardSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Playful/Card_Cohesive_Playful.png");
            if (cardSprite == null) cardSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Card_Exact.png");

            Sprite btnSubtle = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Playful/Pill_Badge_Tag.png");
            if (btnSubtle == null) btnSubtle = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Btn_Subtle_Card.png");

            Sprite btnHero = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Playful/Btn_Hero_Playful.png");
            if (btnHero == null) btnHero = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Pill_Hero_Perfect.png");

            Sprite btnWrong = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Btn_Solid_Coral.png");
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

            // 1.1 TOP BAR (Height 140, safe margin from camera notch)
            GameObject topBar = CreateUI("TopBar", safeArea.transform);
            RectTransform tbRect = topBar.GetComponent<RectTransform>();
            tbRect.anchorMin = new Vector2(0, 1); tbRect.anchorMax = new Vector2(1, 1);
            tbRect.pivot = new Vector2(0.5f, 1);
            tbRect.sizeDelta = new Vector2(0, 140);
            tbRect.anchoredPosition = new Vector2(0, -20);

            // Keluar Button
            GameObject backBtn = CreateUI("Btn_Kembali", topBar.transform);
            RectTransform bbr = backBtn.GetComponent<RectTransform>();
            bbr.anchorMin = new Vector2(0, 0.5f); bbr.anchorMax = new Vector2(0, 0.5f);
            bbr.pivot = new Vector2(0, 0.5f);
            bbr.sizeDelta = new Vector2(170, 60);
            bbr.anchoredPosition = new Vector2(36, 0);

            Image backImg = backBtn.AddComponent<Image>();
            if (btnSubtle != null) { backImg.sprite = btnSubtle; backImg.type = Image.Type.Sliced; }
            backImg.color = Color.white;
            Button bKembali = backBtn.AddComponent<Button>();

            GameObject backTxt = CreateUI("Text", backBtn.transform);
            TextMeshProUGUI bkt = backTxt.AddComponent<TextMeshProUGUI>();
            if (appFont != null) bkt.font = appFont;
            bkt.text = "<b><color=#064E3B>‹ Keluar</color></b>";
            bkt.fontSize = 22;
            bkt.alignment = TextAlignmentOptions.Center;
            bkt.raycastTarget = false;
            FillParent(backTxt.GetComponent<RectTransform>());

            // Title
            GameObject titleObj = CreateUI("HeaderTitle", topBar.transform);
            RectTransform tr = titleObj.GetComponent<RectTransform>();
            tr.anchorMin = new Vector2(0.5f, 0.5f); tr.anchorMax = new Vector2(0.5f, 0.5f);
            tr.pivot = new Vector2(0.5f, 0.5f);
            tr.sizeDelta = new Vector2(500, 60);
            tr.anchoredPosition = Vector2.zero;

            TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
            if (appFont != null) titleText.font = appFont;
            titleText.text = "<b><color=#064E3B>Quiz Evaluasi Satwa</color></b>";
            titleText.fontSize = 28;
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.color = new Color(0.04f, 0.31f, 0.23f, 1f);
            titleText.raycastTarget = false;

            // Score Box (Top Right)
            GameObject scoreBox = CreateUI("ScoreBox", topBar.transform);
            RectTransform sbr = scoreBox.GetComponent<RectTransform>();
            sbr.anchorMin = new Vector2(1, 0.5f); sbr.anchorMax = new Vector2(1, 0.5f);
            sbr.pivot = new Vector2(1, 0.5f);
            sbr.sizeDelta = new Vector2(180, 60);
            sbr.anchoredPosition = new Vector2(-36, 0);

            Image sbImg = scoreBox.AddComponent<Image>();
            if (btnSubtle != null) { sbImg.sprite = btnSubtle; sbImg.type = Image.Type.Sliced; }
            sbImg.color = Color.white;
            sbImg.raycastTarget = false;

            GameObject scoreTxt = CreateUI("Text", scoreBox.transform);
            TextMeshProUGUI st = scoreTxt.AddComponent<TextMeshProUGUI>();
            if (appFont != null) st.font = appFont;
            st.text = "<b><color=#059669>Skor: 0</color></b>";
            st.fontSize = 22;
            st.alignment = TextAlignmentOptions.Center;
            st.color = new Color(0.02f, 0.48f, 0.34f, 1f);
            st.raycastTarget = false;
            FillParent(scoreTxt.GetComponent<RectTransform>());

            // 1.2 MAIN QUIZ CARD (Elevated Card)
            GameObject quizCard = CreateUI("QuizCard", safeArea.transform);
            RectTransform qcr = quizCard.GetComponent<RectTransform>();
            qcr.anchorMin = new Vector2(0.5f, 0.5f); qcr.anchorMax = new Vector2(0.5f, 0.5f);
            qcr.pivot = new Vector2(0.5f, 0.5f);
            qcr.sizeDelta = new Vector2(980, 1500);
            qcr.anchoredPosition = new Vector2(0, -30);

            Image qcImg = quizCard.AddComponent<Image>();
            if (cardSprite != null) { qcImg.sprite = cardSprite; qcImg.type = Image.Type.Sliced; }
            qcImg.color = Color.white;
            qcImg.raycastTarget = false;

            // 2.1 Quiz Progress Badge Pill
            GameObject qPill = CreateUI("QuizPill", quizCard.transform);
            RectTransform qpr = qPill.GetComponent<RectTransform>();
            qpr.anchorMin = new Vector2(0.5f, 1); qpr.anchorMax = new Vector2(0.5f, 1);
            qpr.pivot = new Vector2(0.5f, 1);
            qpr.sizeDelta = new Vector2(260, 52);
            qpr.anchoredPosition = new Vector2(0, 26);

            Image qpImg = qPill.AddComponent<Image>();
            if (pillSprite != null) { qpImg.sprite = pillSprite; qpImg.type = Image.Type.Sliced; }
            qpImg.color = new Color(0.04f, 0.48f, 0.35f, 1f);
            qpImg.raycastTarget = false;

            GameObject qpTxt = CreateUI("Text", qPill.transform);
            TextMeshProUGUI qpt = qpTxt.AddComponent<TextMeshProUGUI>();
            if (appFont != null) qpt.font = appFont;
            qpt.text = "<b>Soal 1 / 10</b>";
            qpt.fontSize = 22;
            qpt.alignment = TextAlignmentOptions.Center;
            qpt.color = Color.white;
            qpt.raycastTarget = false;
            FillParent(qpTxt.GetComponent<RectTransform>());

            // 2.2 Question Image Box (Top of card)
            GameObject imgBox = CreateUI("ImageBox", quizCard.transform);
            RectTransform ibr = imgBox.GetComponent<RectTransform>();
            ibr.anchorMin = new Vector2(0.5f, 1); ibr.anchorMax = new Vector2(0.5f, 1);
            ibr.pivot = new Vector2(0.5f, 1);
            ibr.sizeDelta = new Vector2(200, 180);
            ibr.anchoredPosition = new Vector2(0, -40);

            Image qImage = imgBox.AddComponent<Image>();
            qImage.preserveAspect = true;
            qImage.raycastTarget = false;
            imgBox.SetActive(false); // Controlled dynamically by QuizController

            // 2.3 Question Text Box (Positioned cleanly below image at y=-230)
            GameObject questionBox = CreateUI("QuestionText", quizCard.transform);
            RectTransform qbr = questionBox.GetComponent<RectTransform>();
            qbr.anchorMin = new Vector2(0.5f, 1); qbr.anchorMax = new Vector2(0.5f, 1);
            qbr.pivot = new Vector2(0.5f, 1);
            qbr.sizeDelta = new Vector2(880, 160);
            qbr.anchoredPosition = new Vector2(0, -230);

            TextMeshProUGUI qt = questionBox.AddComponent<TextMeshProUGUI>();
            if (appFont != null) qt.font = appFont;
            qt.text = "<b>Satwa endemik pulau Sumatra yang dijuluki sebagai 'insinyur ekosistem' karena peran vitalnya menyebarkan biji pohon di hutan tropis adalah...</b>";
            qt.fontSize = 24;
            qt.lineSpacing = 1.25f;
            qt.alignment = TextAlignmentOptions.Center;
            qt.color = new Color(0.04f, 0.22f, 0.16f);
            qt.raycastTarget = false;

            // 2.4 Options 1-4 (A, B, C, D)
            Image[] optImgs = new Image[4];
            TextMeshProUGUI[] optTxts = new TextMeshProUGUI[4];
            Button[] optBtns = new Button[4];

            string[] defaultOptions = { "A. Gajah Sumatra", "B. Banteng Jawa", "C. Anoa", "D. Babirusa" };
            float startY = -460f;
            float stepY = 120f;

            for (int i = 0; i < 4; i++)
            {
                GameObject optObj = CreateUI($"Btn_Opt_{i}", quizCard.transform);
                RectTransform opr = optObj.GetComponent<RectTransform>();
                opr.anchorMin = new Vector2(0.5f, 1); opr.anchorMax = new Vector2(0.5f, 1);
                opr.pivot = new Vector2(0.5f, 1);
                opr.sizeDelta = new Vector2(880, 100);
                opr.anchoredPosition = new Vector2(0, startY - (i * stepY));

                Image optImg = optObj.AddComponent<Image>();
                if (btnSubtle != null) { optImg.sprite = btnSubtle; optImg.type = Image.Type.Sliced; }
                optImg.color = Color.white;
                optImg.raycastTarget = true;
                Button optBtn = optObj.AddComponent<Button>();

                var cb = optBtn.colors;
                cb.normalColor = Color.white;
                cb.highlightedColor = new Color(0.92f, 0.97f, 0.94f, 1f);
                cb.pressedColor = new Color(0.85f, 0.92f, 0.88f, 1f);
                cb.selectedColor = Color.white;
                optBtn.colors = cb;

                GameObject txtObj = CreateUI("Text", optObj.transform);
                TextMeshProUGUI optTxt = txtObj.AddComponent<TextMeshProUGUI>();
                if (appFont != null) optTxt.font = appFont;
                optTxt.text = $"<b><color=#0F3B2E>{defaultOptions[i]}</color></b>";
                optTxt.fontSize = 24;
                optTxt.alignment = TextAlignmentOptions.Center;
                optTxt.raycastTarget = false;
                FillParent(txtObj.GetComponent<RectTransform>());

                optImgs[i] = optImg;
                optTxts[i] = optTxt;
                optBtns[i] = optBtn;
            }

            // 2.5 Next CTA Button (High Contrast Green Pill)
            GameObject nextBtn = CreateUI("Btn_Next", quizCard.transform);
            RectTransform nbr = nextBtn.GetComponent<RectTransform>();
            nbr.anchorMin = new Vector2(0.5f, 0); nbr.anchorMax = new Vector2(0.5f, 0);
            nbr.pivot = new Vector2(0.5f, 0);
            nbr.sizeDelta = new Vector2(880, 100);
            nbr.anchoredPosition = new Vector2(0, 40);

            Image nextImg = nextBtn.AddComponent<Image>();
            if (btnHero != null) { nextImg.sprite = btnHero; nextImg.type = Image.Type.Sliced; }
            nextImg.color = Color.white;
            nextImg.raycastTarget = true;
            Button bNext = nextBtn.AddComponent<Button>();

            GameObject nextTxt = CreateUI("Text", nextBtn.transform);
            TextMeshProUGUI nt = nextTxt.AddComponent<TextMeshProUGUI>();
            if (appFont != null) nt.font = appFont;
            nt.text = "<b><color=#FFFFFF>Lanjut Soal Berikutnya ›</color></b>";
            nt.fontSize = 24;
            nt.alignment = TextAlignmentOptions.Center;
            nt.raycastTarget = false;
            FillParent(nextTxt.GetComponent<RectTransform>());

            // Wire Controller Fields
            SerializedObject so = new SerializedObject(controller);
            so.FindProperty("btnKembali").objectReferenceValue = bKembali;
            so.FindProperty("txtHeader").objectReferenceValue = titleText;
            so.FindProperty("txtProgress").objectReferenceValue = qpt;
            so.FindProperty("txtScore").objectReferenceValue = st;
            so.FindProperty("imgQuestion").objectReferenceValue = qImage;
            so.FindProperty("txtQuestion").objectReferenceValue = qt;
            so.FindProperty("btnNext").objectReferenceValue = bNext;

            SerializedProperty propBtns = so.FindProperty("optionButtons");
            SerializedProperty propTxts = so.FindProperty("optionTexts");
            SerializedProperty propImgs = so.FindProperty("optionBackgrounds");

            for (int i = 0; i < 4; i++)
            {
                propBtns.GetArrayElementAtIndex(i).objectReferenceValue = optBtns[i];
                propTxts.GetArrayElementAtIndex(i).objectReferenceValue = optTxts[i];
                propImgs.GetArrayElementAtIndex(i).objectReferenceValue = optImgs[i];
            }

            so.FindProperty("normalBtnSprite").objectReferenceValue = btnSubtle;
            so.FindProperty("correctBtnSprite").objectReferenceValue = btnHero;
            QuizDataSO quizSO = AssetDatabase.LoadAssetAtPath<QuizDataSO>("Assets/Data/QuizDatabase.asset");
            if (quizSO == null)
            {
                string[] guids = AssetDatabase.FindAssets("t:QuizDataSO");
                if (guids.Length > 0) quizSO = AssetDatabase.LoadAssetAtPath<QuizDataSO>(AssetDatabase.GUIDToAssetPath(guids[0]));
            }
            if (quizSO != null) so.FindProperty("quizDatabase").objectReferenceValue = quizSO;

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
