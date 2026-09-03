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
    public static class DetailSatwaSceneBuilder
    {
        private static TMP_FontAsset appFont => AppTheme.Font;

        [MenuItem("Satwa Langka/Create & Open DetailSatwa Scene")]
        public static void BuildDetailSatwaScene()
        {
            string scenePath = "Assets/Scenes/DetailSatwa.unity";
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // 1. Setup Camera and Lighting
            SetupCameraAndLighting();

            // 2. Setup 3D Animal Stage (Foto Hewan)
            Transform animalStage = Setup3DAnimalStage();

            // 3. Build UI Canvas
            BuildDetailSatwaCanvas(animalStage);

            // Save Scene
            EditorSceneManager.SaveScene(scene, scenePath);
            UpdateBuildSettings(scenePath);

            Debug.Log("<b>[SATWA AR]</b> DetailSatwa scene built and saved at " + scenePath);
        }

        private static void SetupCameraAndLighting()
        {
            // Camera
            GameObject camObj = new GameObject("Main Camera");
            Camera cam = camObj.AddComponent<Camera>();
            camObj.AddComponent<AudioListener>();
            camObj.tag = "MainCamera";
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.93f, 0.96f, 0.94f, 1.0f);
            cam.transform.position = new Vector3(0f, 1.85f, -2.25f);
            cam.transform.rotation = Quaternion.Euler(10f, 0f, 0f);

            // Studio Lighting
            GameObject keyObj = new GameObject("Key_Light");
            Light key = keyObj.AddComponent<Light>();
            key.type = LightType.Directional;
            key.color = new Color(1.0f, 0.98f, 0.94f);
            key.intensity = 1.35f;
            keyObj.transform.rotation = Quaternion.Euler(45f, -35f, 0f);

            GameObject fillObj = new GameObject("Fill_Light");
            Light fill = fillObj.AddComponent<Light>();
            fill.type = LightType.Directional;
            fill.color = new Color(0.82f, 0.92f, 0.95f);
            fill.intensity = 0.65f;
            fillObj.transform.rotation = Quaternion.Euler(30f, 140f, 0f);

            GameObject rimObj = new GameObject("Rim_Light");
            Light rim = rimObj.AddComponent<Light>();
            rim.type = LightType.Directional;
            rim.color = Color.white;
            rim.intensity = 0.85f;
            rimObj.transform.rotation = Quaternion.Euler(-25f, -170f, 0f);

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

        private static Transform Setup3DAnimalStage()
        {
            GameObject stageRoot = new GameObject("Stage_FotoHewan_3D");
            stageRoot.transform.position = new Vector3(0f, 1.45f, 0f);

            // Circular Pedestal Stand
            GameObject pedestal = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pedestal.name = "Pedestal_Stand";
            pedestal.transform.SetParent(stageRoot.transform, false);
            pedestal.transform.localPosition = new Vector3(0f, 0.0f, 0f);
            pedestal.transform.localScale = new Vector3(1.25f, 0.03f, 1.25f);
            Object.DestroyImmediate(pedestal.GetComponent<Collider>());

            Material pedMat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            pedMat.color = new Color(0.88f, 0.93f, 0.90f);
            pedestal.GetComponent<MeshRenderer>().material = pedMat;

            // Pedestal Ring Shadow
            GameObject shadowObj = GameObject.CreatePrimitive(PrimitiveType.Quad);
            shadowObj.name = "Stand_Shadow";
            shadowObj.transform.SetParent(stageRoot.transform, false);
            shadowObj.transform.localPosition = new Vector3(0f, 0.005f, 0f);
            shadowObj.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            shadowObj.transform.localScale = new Vector3(1.35f, 1.0f, 1f);
            Object.DestroyImmediate(shadowObj.GetComponent<Collider>());

            Material shadowMat = new Material(Shader.Find("Sprites/Default"));
            Sprite shadowSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Natural_Ground_Shadow.png");
            shadowMat.mainTexture = shadowSprite != null ? shadowSprite.texture : null;
            shadowObj.GetComponent<MeshRenderer>().material = shadowMat;

            // Animal Model Container (Dynamic Spawning Parent)
            GameObject animalContainer = new GameObject("Animal_Container");
            animalContainer.transform.SetParent(stageRoot.transform, false);
            animalContainer.transform.localPosition = new Vector3(0f, 0.03f, 0f);

            GameObject gajahPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Animals/Prefab_SATWA01_GajahSumatra.prefab");
            if (gajahPrefab != null)
            {
                GameObject gInst = UnityEngine.Object.Instantiate(gajahPrefab, animalContainer.transform);
                gInst.name = "Spawned_SATWA01_GajahSumatra";
                gInst.transform.localPosition = Vector3.zero;
                gInst.transform.localRotation = Quaternion.identity;
                gInst.transform.localScale = Vector3.one;
            }

            return animalContainer.transform;
        }

        private static void BuildDetailSatwaCanvas(Transform animalStage)
        {
            // UI Textures & Sprites
            Sprite cardSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Playful/Card_Cohesive_Playful.png");
            if (cardSprite == null) cardSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Card_Base_White.png");
            Sprite card2Col = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Playful/Card_Cohesive_Playful.png");
            Sprite pillSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Playful/Pill_Badge_Tag.png");
            Sprite btnSubtle = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Playful/Pill_Badge_Tag.png");
            Sprite btnHero = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Playful/Btn_Hero_Playful.png");

            // Canvas Setup
            GameObject canvasObj = new GameObject("Canvas_DetailSatwa");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = Camera.main;
            canvas.planeDistance = 1.0f;
            canvas.sortingOrder = 100;
            canvasObj.AddComponent<GraphicRaycaster>();
            

            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0.0f;

            DetailSatwaController controller = canvasObj.AddComponent<DetailSatwaController>();

            // ==================== 0. SAFE AREA CONTAINER ====================
            GameObject safeArea = CreateUI("SafeArea", canvasObj.transform);
            FillParent(safeArea.GetComponent<RectTransform>());
            safeArea.AddComponent<SafeAreaFitter>();

            // ==================== 1. TOP BAR ====================
            GameObject topBar = CreateUI("TopBar", safeArea.transform);
            RectTransform tbr = topBar.GetComponent<RectTransform>();
            tbr.anchorMin = new Vector2(0, 1); tbr.anchorMax = new Vector2(1, 1);
            tbr.pivot = new Vector2(0.5f, 1);
            tbr.sizeDelta = new Vector2(0, 160);
            tbr.anchoredPosition = new Vector2(0, -30);

            // 1.1 Back Button (Top Left)
            GameObject backBtn = CreateUI("Btn_Kembali", topBar.transform);
            RectTransform bkr = backBtn.GetComponent<RectTransform>();
            bkr.anchorMin = new Vector2(0, 0.5f); bkr.anchorMax = new Vector2(0, 0.5f);
            bkr.pivot = new Vector2(0, 0.5f);
            bkr.sizeDelta = new Vector2(160, 60);
            bkr.anchoredPosition = new Vector2(40, 0);

            Image backImg = backBtn.AddComponent<Image>();
            if (btnSubtle != null) { backImg.sprite = btnSubtle; backImg.type = Image.Type.Sliced; }
            Button bKembali = backBtn.AddComponent<Button>();

            GameObject backTxt = CreateUI("Text", backBtn.transform);
            TextMeshProUGUI bkt = backTxt.AddComponent<TextMeshProUGUI>();
            if (appFont != null) bkt.font = appFont;
            bkt.text = "<b><color=#0F3B2E>‹ Kembali</color></b>";
            bkt.fontSize = 22;
            bkt.alignment = TextAlignmentOptions.Center;
            FillParent(backTxt.GetComponent<RectTransform>());

            // 1.2 Menu Button (Top Right)
            GameObject menuBtn = CreateUI("Btn_Menu", topBar.transform);
            RectTransform mbr = menuBtn.GetComponent<RectTransform>();
            mbr.anchorMin = new Vector2(1, 0.5f); mbr.anchorMax = new Vector2(1, 0.5f);
            mbr.pivot = new Vector2(1, 0.5f);
            mbr.sizeDelta = new Vector2(180, 60);
            mbr.anchoredPosition = new Vector2(-40, 0);

            Image menuImg = menuBtn.AddComponent<Image>();
            if (btnSubtle != null) { menuImg.sprite = btnSubtle; menuImg.type = Image.Type.Sliced; }
            Button bMenu = menuBtn.AddComponent<Button>();

            GameObject menuTxt = CreateUI("Text", menuBtn.transform);
            TextMeshProUGUI mkt = menuTxt.AddComponent<TextMeshProUGUI>();
            if (appFont != null) mkt.font = appFont;
            mkt.text = "<b><color=#0F3B2E>Menu Utama</color></b>";
            mkt.fontSize = 22;
            mkt.alignment = TextAlignmentOptions.Center;
            FillParent(menuTxt.GetComponent<RectTransform>());

            // 1.3 Center Title
            GameObject titleObj = CreateUI("HeaderTitle", topBar.transform);
            RectTransform tr = titleObj.GetComponent<RectTransform>();
            tr.anchorMin = new Vector2(0.5f, 0.5f); tr.anchorMax = new Vector2(0.5f, 0.5f);
            tr.pivot = new Vector2(0.5f, 0.5f);
            tr.sizeDelta = new Vector2(600, 60);
            tr.anchoredPosition = new Vector2(0, 0);

            TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
            if (appFont != null) titleText.font = appFont;
            titleText.text = "<b><color=#064E3B>Visualisasi 3D Satwa</color></b>";
            titleText.fontSize = 28;
            titleText.alignment = TextAlignmentOptions.Center;

            // ==================== 2. BOTTOM INFO PANEL ====================
            GameObject infoCard = CreateUI("BottomDetailCard", safeArea.transform);
            RectTransform icr = infoCard.GetComponent<RectTransform>();
            icr.anchorMin = new Vector2(0.5f, 0); icr.anchorMax = new Vector2(0.5f, 0);
            icr.pivot = new Vector2(0.5f, 0);
            icr.sizeDelta = new Vector2(980, 800);
            icr.anchoredPosition = new Vector2(0, 30);

            Image cardImg = infoCard.AddComponent<Image>();
            if (card2Col != null) { cardImg.sprite = card2Col; cardImg.type = Image.Type.Sliced; }
            else if (cardSprite != null) { cardImg.sprite = cardSprite; cardImg.type = Image.Type.Sliced; }

            // 2.1 Top Header Pill ([ Informasi Edukasi ])
            GameObject infoPill = CreateUI("InfoPill", infoCard.transform);
            RectTransform ipr = infoPill.GetComponent<RectTransform>();
            ipr.anchorMin = new Vector2(0, 1); ipr.anchorMax = new Vector2(0, 1);
            ipr.pivot = new Vector2(0, 1);
            ipr.sizeDelta = new Vector2(230, 38);
            ipr.anchoredPosition = new Vector2(36, 18);

            Image ipImg = infoPill.AddComponent<Image>();
            if (pillSprite != null) { ipImg.sprite = pillSprite; ipImg.type = Image.Type.Sliced; }
            ipImg.color = new Color(0.04f, 0.45f, 0.32f, 0.95f);

            GameObject ipTxtObj = CreateUI("Text", infoPill.transform);
            TextMeshProUGUI ipt = ipTxtObj.AddComponent<TextMeshProUGUI>();
            if (appFont != null) ipt.font = appFont;
            ipt.text = "<b>INFORMASI RESMI BRIN</b>";
            ipt.fontSize = 15;
            ipt.alignment = TextAlignmentOptions.Center;
            ipt.color = Color.white;
            FillParent(ipTxtObj.GetComponent<RectTransform>());

            // 2.2 Common Name
            GameObject nameObj = CreateUI("Txt_Name", infoCard.transform);
            RectTransform nr = nameObj.GetComponent<RectTransform>();
            nr.anchorMin = new Vector2(0, 1); nr.anchorMax = new Vector2(0, 1);
            nr.pivot = new Vector2(0, 1);
            nr.sizeDelta = new Vector2(440, 48);
            nr.anchoredPosition = new Vector2(36, -26);

            TextMeshProUGUI nt = nameObj.AddComponent<TextMeshProUGUI>();
            if (appFont != null) nt.font = appFont;
            nt.text = "<b>Gajah Sumatra</b>";
            nt.fontSize = 36;
            nt.enableAutoSizing = true;
            nt.fontSizeMin = 22;
            nt.fontSizeMax = 36;
            nt.textWrappingMode = TextWrappingModes.NoWrap;
            nt.color = new Color(0.04f, 0.22f, 0.16f);

            // 2.3 IUCN Status Badge
            GameObject badgeObj = CreateUI("Badge_Status", infoCard.transform);
            RectTransform br = badgeObj.GetComponent<RectTransform>();
            br.anchorMin = new Vector2(0, 1); br.anchorMax = new Vector2(0, 1);
            br.pivot = new Vector2(0, 1);
            br.sizeDelta = new Vector2(190, 42);
            br.anchoredPosition = new Vector2(490, -28);

            Image badgeImg = badgeObj.AddComponent<Image>();
            if (pillSprite != null) { badgeImg.sprite = pillSprite; badgeImg.type = Image.Type.Sliced; }
            badgeImg.color = new Color(0.99f, 0.90f, 0.90f, 1.0f);

            GameObject badgeTxtObj = CreateUI("Text", badgeObj.transform);
            TextMeshProUGUI bt = badgeTxtObj.AddComponent<TextMeshProUGUI>();
            if (appFont != null) bt.font = appFont;
            bt.text = "<b>• Kritis (CR)</b>";
            bt.fontSize = 18;
            bt.alignment = TextAlignmentOptions.Center;
            bt.color = new Color(0.85f, 0.2f, 0.2f);
            FillParent(badgeTxtObj.GetComponent<RectTransform>());

            // 2.4 Danger Badge
            GameObject dangerObj = CreateUI("Badge_Danger", infoCard.transform);
            RectTransform dbr = dangerObj.GetComponent<RectTransform>();
            dbr.anchorMin = new Vector2(0, 1); dbr.anchorMax = new Vector2(0, 1);
            dbr.pivot = new Vector2(0, 1);
            dbr.sizeDelta = new Vector2(230, 42);
            dbr.anchoredPosition = new Vector2(700, -28);

            Image dangerImg = dangerObj.AddComponent<Image>();
            if (pillSprite != null) { dangerImg.sprite = pillSprite; dangerImg.type = Image.Type.Sliced; }
            dangerImg.color = new Color(0.99f, 0.94f, 0.88f, 1.0f);

            GameObject dangerTxtObj = CreateUI("Text", dangerObj.transform);
            TextMeshProUGUI dt = dangerTxtObj.AddComponent<TextMeshProUGUI>();
            if (appFont != null) dt.font = appFont;
            dt.text = "<b>• Cukup Berbahaya</b>";
            dt.fontSize = 18;
            dt.alignment = TextAlignmentOptions.Center;
            dt.color = new Color(0.85f, 0.45f, 0.1f);
            FillParent(dangerTxtObj.GetComponent<RectTransform>());

            // 2.5 Audio Button
            GameObject audioBtn = CreateUI("Btn_Audio", infoCard.transform);
            RectTransform abr = audioBtn.GetComponent<RectTransform>();
            abr.anchorMin = new Vector2(1, 1); abr.anchorMax = new Vector2(1, 1);
            abr.pivot = new Vector2(1, 1);
            abr.sizeDelta = new Vector2(230, 52);
            abr.anchoredPosition = new Vector2(-36, -82);

            Image abImg = audioBtn.AddComponent<Image>();
            if (btnSubtle != null) { abImg.sprite = btnSubtle; abImg.type = Image.Type.Sliced; }
            Button bAudio = audioBtn.AddComponent<Button>();

            GameObject abTxt = CreateUI("Text", audioBtn.transform);
            TextMeshProUGUI abt = abTxt.AddComponent<TextMeshProUGUI>();
            if (appFont != null) abt.font = appFont;
            abt.text = "<b><color=#047857>Putar Suara</color></b>";
            abt.fontSize = 20;
            abt.alignment = TextAlignmentOptions.Center;
            FillParent(abTxt.GetComponent<RectTransform>());

            // 2.6 Latin Name
            GameObject latinObj = CreateUI("Txt_Latin", infoCard.transform);
            RectTransform lr = latinObj.GetComponent<RectTransform>();
            lr.anchorMin = new Vector2(0, 1); lr.anchorMax = new Vector2(0, 1);
            lr.pivot = new Vector2(0, 1);
            lr.sizeDelta = new Vector2(580, 36);
            lr.anchoredPosition = new Vector2(36, -82);

            TextMeshProUGUI lt = latinObj.AddComponent<TextMeshProUGUI>();
            if (appFont != null) lt.font = appFont;
            lt.text = "<i>Elephas maximus sumatranus</i>";
            lt.fontSize = 24;
            lt.color = new Color(0.35f, 0.52f, 0.45f);

            // 2.7 Tab Row (Deskripsi, Habitat & Asal, Status & Mitigasi, Fakta Unik)
            GameObject tabRow = CreateUI("TabRow", infoCard.transform);
            RectTransform trr = tabRow.GetComponent<RectTransform>();
            trr.anchorMin = new Vector2(0, 1); trr.anchorMax = new Vector2(1, 1);
            trr.pivot = new Vector2(0.5f, 1);
            trr.sizeDelta = new Vector2(-72, 54);
            trr.anchoredPosition = new Vector2(0, -145);

            Image imgTDesk, imgTHab, imgTMit, imgTFak;
            Button tDeskripsi = CreateTabBtn(tabRow.transform, "Tab_Deskripsi", "Deskripsi & Ciri", -330, btnSubtle, out imgTDesk);
            Button tHabitat = CreateTabBtn(tabRow.transform, "Tab_Habitat", "Daerah & Habitat", -110, btnSubtle, out imgTHab);
            Button tMitigasi = CreateTabBtn(tabRow.transform, "Tab_Mitigasi", "Bahaya & Mitigasi", 110, btnSubtle, out imgTMit);
            Button tFakta = CreateTabBtn(tabRow.transform, "Tab_Fakta", "Fakta Unik", 330, btnSubtle, out imgTFak);

            // 2.8 Dynamic Content Text Area
            GameObject contentBox = CreateUI("ContentBox", infoCard.transform);
            RectTransform cbr = contentBox.GetComponent<RectTransform>();
            cbr.anchorMin = new Vector2(0, 0); cbr.anchorMax = new Vector2(1, 1);
            cbr.offsetMin = new Vector2(36, 30);
            cbr.offsetMax = new Vector2(-36, -215);

            TextMeshProUGUI contentText = contentBox.AddComponent<TextMeshProUGUI>();
            if (appFont != null) contentText.font = appFont;
            contentText.fontSize = 21;
            contentText.lineSpacing = 1.35f;
            contentText.color = new Color(0.12f, 0.25f, 0.18f);
            contentText.text = "<color=#047857><b>■ PERAN EKOLOGIS & KARAKTERISTIK FISIK:</b></color>\nGajah Sumatra adalah subspesies gajah asia yang hidup di pulau Sumatra. Berperan penting sebagai 'insinyur ekosistem' penyebar biji dan pembuka koridor alami di hutan hujan tropis.\n\n<color=#047857><b>■ POLA MAKAN / DIET:</b></color>\nRumput liar, daun muda, bambu, kulit kayu, dan buah hutan.";

            // Wire Controller Fields
            string[] animalGuids = AssetDatabase.FindAssets("t:AnimalDataSO", new[] { "Assets/Resources/Data/Animals", "Assets/Data/Animals" });
            List<AnimalDataSO> animalsList = new List<AnimalDataSO>();
            foreach (var g in animalGuids)
            {
                string p = AssetDatabase.GUIDToAssetPath(g);
                var a = AssetDatabase.LoadAssetAtPath<AnimalDataSO>(p);
                if (a != null && !animalsList.Contains(a)) animalsList.Add(a);
            }
            animalsList.Sort((x, y) => string.Compare(x.animalCode, y.animalCode));

            AnimalDataSO gajahSO = animalsList.Count > 0 ? animalsList[0] : null;
            SerializedObject so = new SerializedObject(controller);
            so.FindProperty("btnKembali").objectReferenceValue = bKembali;
            so.FindProperty("btnMenu").objectReferenceValue = bMenu;
            so.FindProperty("animalSpawnParent").objectReferenceValue = animalStage;
            so.FindProperty("txtCommonName").objectReferenceValue = nt;
            so.FindProperty("txtLatinName").objectReferenceValue = lt;
            so.FindProperty("txtStatusBadge").objectReferenceValue = bt;
            so.FindProperty("imgStatusBadge").objectReferenceValue = badgeImg;
            so.FindProperty("txtDangerBadge").objectReferenceValue = dt;
            so.FindProperty("imgDangerBadge").objectReferenceValue = dangerImg;
            so.FindProperty("btnPlayAudio").objectReferenceValue = bAudio;

            so.FindProperty("tabDeskripsi").objectReferenceValue = tDeskripsi;
            so.FindProperty("tabHabitat").objectReferenceValue = tHabitat;
            so.FindProperty("tabMitigasi").objectReferenceValue = tMitigasi;
            so.FindProperty("tabFakta").objectReferenceValue = tFakta;

            so.FindProperty("imgTabDeskripsi").objectReferenceValue = imgTDesk;
            so.FindProperty("imgTabHabitat").objectReferenceValue = imgTHab;
            so.FindProperty("imgTabMitigasi").objectReferenceValue = imgTMit;
            so.FindProperty("imgTabFakta").objectReferenceValue = imgTFak;

            so.FindProperty("txtContentBody").objectReferenceValue = contentText;
            so.FindProperty("currentAnimal").objectReferenceValue = gajahSO;

            var propAll = so.FindProperty("allAnimals");
            if (propAll != null)
            {
                propAll.arraySize = animalsList.Count;
                for (int i = 0; i < animalsList.Count; i++)
                {
                    propAll.GetArrayElementAtIndex(i).objectReferenceValue = animalsList[i];
                }
            }

            so.ApplyModifiedProperties();

            controller.ShowTabContent(0);

            if (gajahSO != null && gajahSO.modelPrefab != null && animalStage != null)
            {
                GameObject gModel = (GameObject)PrefabUtility.InstantiatePrefab(gajahSO.modelPrefab, animalStage);
                gModel.name = $"DetailStage_SATWA01_Gajah_Sumatra";
                gModel.transform.localPosition = new Vector3(0f, 0.245f, 0f);
                gModel.transform.localRotation = Quaternion.Euler(-90f, 150f, 0f);
                Vector3 scale = gajahSO.defaultScale * 1.05f;
                if (scale == Vector3.zero) scale = Vector3.one * 0.24f;
                gModel.transform.localScale = scale;

                // Auto align bottom of animal model's feet to stand on top of pedestal
                Renderer[] renderers = gModel.GetComponentsInChildren<Renderer>();
                if (renderers != null && renderers.Length > 0)
                {
                    Bounds b = renderers[0].bounds;
                    for (int i = 1; i < renderers.Length; i++) b.Encapsulate(renderers[i].bounds);
                    float pedTopY = animalStage.position.y + 0.015f;
                    float diff = pedTopY - b.min.y;
                    gModel.transform.position += new Vector3(0f, diff, 0f);
                }
            }
        }

        private static Button CreateTabBtn(Transform parent, string name, string label, float posX, Sprite btnSprite, out Image outImg)
        {
            GameObject btnObj = CreateUI(name, parent);
            RectTransform br = btnObj.GetComponent<RectTransform>();
            br.anchorMin = new Vector2(0.5f, 0.5f); br.anchorMax = new Vector2(0.5f, 0.5f);
            br.pivot = new Vector2(0.5f, 0.5f);
            br.sizeDelta = new Vector2(210, 48);
            br.anchoredPosition = new Vector2(posX, 0);

            Image img = btnObj.AddComponent<Image>();
            if (btnSprite != null) { img.sprite = btnSprite; img.type = Image.Type.Sliced; }
            outImg = img;

            GameObject txtObj = CreateUI("Text", btnObj.transform);
            TextMeshProUGUI txt = txtObj.AddComponent<TextMeshProUGUI>();
            if (appFont != null) txt.font = appFont;
            txt.text = $"<b>{label}</b>";
            txt.fontSize = 18;
            txt.alignment = TextAlignmentOptions.Center;
            txt.color = new Color(0.06f, 0.28f, 0.22f);
            FillParent(txtObj.GetComponent<RectTransform>());

            return btnObj.AddComponent<Button>();
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
