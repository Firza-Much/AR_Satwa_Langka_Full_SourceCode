using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using Unity.XR.CoreUtils;
using UnityEngine.InputSystem.XR;
using TMPro;
using SatwaLangka.Data;
using SatwaLangka.UI;
using SatwaLangka.AR;

namespace SatwaLangka.EditorScripts
{
    public static class ScanPlaneSceneBuilder
    {
        private static TMP_FontAsset appFont => AppTheme.Font;

        [MenuItem("Satwa Langka/Create & Open ScanPlaneDetection Scene (AR Masterpiece)")]
        public static void BuildScanPlaneScene()
        {
            string scenePath = "Assets/Scenes/ScanPlaneDetection.unity";
            var scene = EditorSceneManager.OpenScene(scenePath);

            // Clear old objects
            foreach (var go in scene.GetRootGameObjects())
            {
                Object.DestroyImmediate(go);
            }

            // 1. AR Session
            GameObject sessionObj = new GameObject("AR Session");
            var arSession = sessionObj.AddComponent<ARSession>();
            arSession.attemptUpdate = true;
            sessionObj.AddComponent<ARInputManager>();

            // 2. XR Origin
            GameObject xrOriginObj = new GameObject("XR Origin (Mobile AR)");
            var xrOrigin = xrOriginObj.AddComponent<XROrigin>();
            var planeManager = xrOriginObj.AddComponent<ARPlaneManager>();
            planeManager.requestedDetectionMode = PlaneDetectionMode.Horizontal | PlaneDetectionMode.Vertical;
            var raycastManager = xrOriginObj.AddComponent<ARRaycastManager>();
            var anchorManager = xrOriginObj.AddComponent<ARAnchorManager>();
            var pointCloudManager = xrOriginObj.AddComponent<ARPointCloudManager>();

            GameObject camOffsetObj = new GameObject("Camera Offset");
            camOffsetObj.transform.SetParent(xrOriginObj.transform, false);
            xrOrigin.CameraFloorOffsetObject = camOffsetObj;
            xrOrigin.CameraYOffset = 0f;

            GameObject camObj = new GameObject("Main Camera");
            camObj.tag = "MainCamera";
            camObj.transform.SetParent(camOffsetObj.transform, false);
            Camera mainCam = camObj.AddComponent<Camera>();
            mainCam.clearFlags = CameraClearFlags.SolidColor;
            mainCam.backgroundColor = Color.black;
            mainCam.cullingMask = ~0;
            mainCam.depth = 0;
            mainCam.nearClipPlane = 0.05f;
            mainCam.farClipPlane = 50f;
            mainCam.rect = new Rect(0f, 0f, 1f, 1f);

            var urpCamData = camObj.AddComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>();
            urpCamData.renderType = UnityEngine.Rendering.Universal.CameraRenderType.Base;
            urpCamData.renderShadows = true;

            camObj.AddComponent<ARCameraManager>();
            camObj.AddComponent<ARCameraBackground>();
            camObj.AddComponent<UnityEngine.XR.ARFoundation.ARPoseDriver>();
            camObj.AddComponent<AudioListener>();
            xrOrigin.Camera = mainCam;

            // 3. Directional Lights
            GameObject lightObj = new GameObject("Key Sun Light");
            Light l = lightObj.AddComponent<Light>();
            l.type = LightType.Directional;
            l.color = new Color(1.0f, 0.98f, 0.92f, 1.0f);
            l.intensity = 1.15f;
            lightObj.transform.rotation = Quaternion.Euler(45, -30, 0);

            GameObject fillLightObj = new GameObject("Fill Light");
            Light fl = fillLightObj.AddComponent<Light>();
            fl.type = LightType.Directional;
            fl.color = new Color(0.90f, 0.94f, 1.0f, 1.0f);
            fl.intensity = 0.55f;
            fillLightObj.transform.rotation = Quaternion.Euler(60, 140, 0);

            // 4. Plane Prefab
            string planePrefabPath = "Assets/Prefabs/AR_Plane_Visualizer.prefab";
            GameObject planePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(planePrefabPath);
            if (planePrefab != null) planeManager.planePrefab = planePrefab;

            // 5. AR Stage Parent & Placement Reticle
            GameObject stageParent = new GameObject("AR_Stage_Parent");
            stageParent.transform.position = Vector3.zero;

            GameObject reticlePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/PlacementReticle_FloorRing.prefab");
            GameObject reticle = reticlePrefab != null ? (GameObject)PrefabUtility.InstantiatePrefab(reticlePrefab, stageParent.transform) : null;
            if (reticle != null)
            {
                reticle.transform.localPosition = Vector3.zero;
                reticle.SetActive(false);
            }

            var tm = stageParent.AddComponent<ARModelTrackingManager>();
            if (reticle != null) tm.SetPlacementIndicator(reticle);
            SerializedObject soTM = new SerializedObject(tm);
            soTM.FindProperty("arSession").objectReferenceValue = arSession;
            soTM.FindProperty("xrOrigin").objectReferenceValue = xrOrigin;
            soTM.FindProperty("arCamera").objectReferenceValue = mainCam;
            soTM.FindProperty("planeManager").objectReferenceValue = planeManager;
            soTM.FindProperty("raycastManager").objectReferenceValue = raycastManager;
            soTM.FindProperty("anchorManager").objectReferenceValue = anchorManager;
            soTM.FindProperty("worldStageRoot").objectReferenceValue = stageParent.transform;
            var pReticle = soTM.FindProperty("placementIndicator");
            if (pReticle != null && reticle != null) pReticle.objectReferenceValue = reticle;
            soTM.ApplyModifiedProperties();

            // 6. Event System
            GameObject esObj = new GameObject("EventSystem");
            esObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
            var tInp = System.Type.GetType("UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem");
            if (tInp != null) esObj.AddComponent(tInp);
            else esObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();

            // 7. Canvas & UI
            BuildScanPlaneCanvas(mainCam, stageParent);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, scenePath);

            Debug.Log("<b>[SATWA AR]</b> ScanPlaneDetection scene rebuilt with Real AR Plane Floor Placement!");
        }

        private static void BuildScanPlaneCanvas(Camera cam, GameObject stageParent)
        {
            GameObject canvasObj = new GameObject("Canvas_ScanPlane");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0.5f;

            canvasObj.AddComponent<GraphicRaycaster>();
            ScanPlaneController controller = canvasObj.AddComponent<ScanPlaneController>();

            // UI Sprites
            Sprite cardSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Playful/Card_Cohesive_Playful.png");
            if (cardSprite == null) cardSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Card_Exact.png");

            Sprite btnWhite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Playful/Pill_Badge_Tag.png");
            if (btnWhite == null) btnWhite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Pill_White_Perfect.png");

            // 1. SAFE AREA CONTAINER
            GameObject safeArea = CreateUI("SafeArea", canvasObj.transform);
            FillParent(safeArea.GetComponent<RectTransform>());
            safeArea.AddComponent<SafeAreaFitter>();

            // Top Header Bar
            GameObject topBar = CreateUI("TopBar", safeArea.transform);
            RectTransform tbRect = topBar.GetComponent<RectTransform>();
            tbRect.anchorMin = new Vector2(0f, 1f); tbRect.anchorMax = new Vector2(1f, 1f);
            tbRect.pivot = new Vector2(0.5f, 1f);
            tbRect.sizeDelta = new Vector2(0, 80);
            tbRect.anchoredPosition = new Vector2(0, -12);

            // Kembali Button
            GameObject backBtn = CreateUI("Btn_Kembali", topBar.transform);
            RectTransform bbr = backBtn.GetComponent<RectTransform>();
            bbr.anchorMin = new Vector2(0, 0.5f); bbr.anchorMax = new Vector2(0, 0.5f);
            bbr.pivot = new Vector2(0, 0.5f);
            bbr.sizeDelta = new Vector2(160, 52);
            bbr.anchoredPosition = new Vector2(24, 0);

            Image backImg = backBtn.AddComponent<Image>();
            if (btnWhite != null) { backImg.sprite = btnWhite; backImg.type = Image.Type.Sliced; }
            backImg.color = Color.white;
            Button bKembali = backBtn.AddComponent<Button>();

            GameObject backTxt = CreateUI("Text", backBtn.transform);
            TextMeshProUGUI bkt = backTxt.AddComponent<TextMeshProUGUI>();
            if (appFont != null) bkt.font = appFont;
            bkt.text = "<b><color=#052E16>‹ Kembali</color></b>";
            bkt.fontSize = 22;
            bkt.alignment = TextAlignmentOptions.Center;
            bkt.raycastTarget = false;
            FillParent(backTxt.GetComponent<RectTransform>());

            // Tracking Status Pill
            GameObject trackStatus = CreateUI("TrackStatus", topBar.transform);
            RectTransform tsr = trackStatus.GetComponent<RectTransform>();
            tsr.anchorMin = new Vector2(1, 0.5f); tsr.anchorMax = new Vector2(1, 0.5f);
            tsr.pivot = new Vector2(1, 0.5f);
            tsr.sizeDelta = new Vector2(400, 52);
            tsr.anchoredPosition = new Vector2(-24, 0);

            Image tsi = trackStatus.AddComponent<Image>();
            if (btnWhite != null) { tsi.sprite = btnWhite; tsi.type = Image.Type.Sliced; }
            tsi.color = new Color(0.08f, 0.12f, 0.14f, 0.85f);
            tsi.raycastTarget = false;

            GameObject statusTxt = CreateUI("Text", trackStatus.transform);
            TextMeshProUGUI stt = statusTxt.AddComponent<TextMeshProUGUI>();
            if (appFont != null) stt.font = appFont;
            stt.text = "<b><color=#FFFFFF>Arahkan kamera ke lantai...</color></b>";
            stt.fontSize = 18;
            stt.alignment = TextAlignmentOptions.Center;
            stt.raycastTarget = false;
            FillParent(statusTxt.GetComponent<RectTransform>());

            // Debug HUD Button
            GameObject debugBtnObj = CreateUI("Btn_ToggleDebug", topBar.transform);
            RectTransform dbr = debugBtnObj.GetComponent<RectTransform>();
            dbr.anchorMin = new Vector2(0.5f, 0.5f); dbr.anchorMax = new Vector2(0.5f, 0.5f);
            dbr.pivot = new Vector2(0.5f, 0.5f);
            dbr.sizeDelta = new Vector2(130, 46);
            dbr.anchoredPosition = new Vector2(0, 0);

            Image dbi = debugBtnObj.AddComponent<Image>();
            if (btnWhite != null) { dbi.sprite = btnWhite; dbi.type = Image.Type.Sliced; }
            dbi.color = new Color(0.12f, 0.18f, 0.22f, 0.85f);
            Button bDebug = debugBtnObj.AddComponent<Button>();

            GameObject dbTxt = CreateUI("Text", debugBtnObj.transform);
            TextMeshProUGUI dbt = dbTxt.AddComponent<TextMeshProUGUI>();
            if (appFont != null) dbt.font = appFont;
            dbt.text = "<b><color=#FFFFFF>AR HUD</color></b>";
            dbt.fontSize = 17;
            dbt.alignment = TextAlignmentOptions.Center;
            FillParent(dbTxt.GetComponent<RectTransform>());
            debugBtnObj.SetActive(false);

            // Debug Overlay Panel
            GameObject debugPanel = CreateUI("DebugHUDPanel", safeArea.transform);
            RectTransform dpr = debugPanel.GetComponent<RectTransform>();
            dpr.anchorMin = new Vector2(0.5f, 1); dpr.anchorMax = new Vector2(0.5f, 1);
            dpr.pivot = new Vector2(0.5f, 1);
            dpr.sizeDelta = new Vector2(760, 200);
            dpr.anchoredPosition = new Vector2(0, -100);

            Image dpi = debugPanel.AddComponent<Image>();
            if (cardSprite != null) { dpi.sprite = cardSprite; dpi.type = Image.Type.Sliced; }
            dpi.color = new Color(0.04f, 0.10f, 0.08f, 0.94f);

            GameObject dbInfoObj = CreateUI("Txt_DebugInfo", debugPanel.transform);
            RectTransform dbir = dbInfoObj.GetComponent<RectTransform>();
            dbir.anchorMin = Vector2.zero; dbir.anchorMax = Vector2.one;
            dbir.offsetMin = new Vector2(20, 12); dbir.offsetMax = new Vector2(-20, -12);

            TextMeshProUGUI dbInfoTxt = dbInfoObj.AddComponent<TextMeshProUGUI>();
            if (appFont != null) dbInfoTxt.font = appFont;
            dbInfoTxt.text = "<b>[AR LIVE HUD]</b>\nScanning real floor plane...";
            dbInfoTxt.fontSize = 17;
            dbInfoTxt.color = Color.white;
            debugPanel.SetActive(false);

            // ==================== 2. BOTTOM INFO CARD ====================
            GameObject bottomPanel = CreateUI("BottomInfoCard", safeArea.transform);
            RectTransform bpr = bottomPanel.GetComponent<RectTransform>();
            bpr.anchorMin = new Vector2(0.5f, 0f); bpr.anchorMax = new Vector2(0.5f, 0f);
            bpr.pivot = new Vector2(0.5f, 0f);
            bpr.sizeDelta = new Vector2(1000, 360);
            bpr.anchoredPosition = new Vector2(0, 24);

            Image bpImg = bottomPanel.AddComponent<Image>();
            if (cardSprite != null) { bpImg.sprite = cardSprite; bpImg.type = Image.Type.Sliced; }
            bpImg.color = new Color(1f, 1f, 1f, 0.96f);

            // Top Bar of Bottom Card: Animal Name, Badges & Sound Button
            GameObject cardTop = CreateUI("CardTopHeader", bottomPanel.transform);
            RectTransform ctr = cardTop.GetComponent<RectTransform>();
            ctr.anchorMin = new Vector2(0f, 1f); ctr.anchorMax = new Vector2(1f, 1f);
            ctr.pivot = new Vector2(0.5f, 1f);
            ctr.sizeDelta = new Vector2(0, 90);
            ctr.anchoredPosition = new Vector2(0, -10);

            // Animal Name
            GameObject nameObj = CreateUI("Txt_CommonName", cardTop.transform);
            RectTransform nr = nameObj.GetComponent<RectTransform>();
            nr.anchorMin = new Vector2(0f, 1f); nr.anchorMax = new Vector2(0f, 1f);
            nr.pivot = new Vector2(0f, 1f);
            nr.sizeDelta = new Vector2(480, 42);
            nr.anchoredPosition = new Vector2(28, -6);

            TextMeshProUGUI txtName = nameObj.AddComponent<TextMeshProUGUI>();
            if (appFont != null) txtName.font = appFont;
            txtName.text = "<b>Gajah Sumatra</b>";
            txtName.fontSize = 28;
            txtName.color = new Color(0.04f, 0.22f, 0.16f);

            // Latin Name
            GameObject latinObj = CreateUI("Txt_LatinName", cardTop.transform);
            RectTransform lr = latinObj.GetComponent<RectTransform>();
            lr.anchorMin = new Vector2(0f, 1f); lr.anchorMax = new Vector2(0f, 1f);
            lr.pivot = new Vector2(0f, 1f);
            lr.sizeDelta = new Vector2(480, 30);
            lr.anchoredPosition = new Vector2(28, -48);

            TextMeshProUGUI txtLatin = latinObj.AddComponent<TextMeshProUGUI>();
            if (appFont != null) txtLatin.font = appFont;
            txtLatin.text = "<i>Elephas maximus sumatranus</i>";
            txtLatin.fontSize = 18;
            txtLatin.color = new Color(0.35f, 0.45f, 0.40f);

            // IUCN Badge
            GameObject badgeObj = CreateUI("Badge_Status", cardTop.transform);
            RectTransform bgr = badgeObj.GetComponent<RectTransform>();
            bgr.anchorMin = new Vector2(1f, 0.5f); bgr.anchorMax = new Vector2(1f, 0.5f);
            bgr.pivot = new Vector2(1f, 0.5f);
            bgr.sizeDelta = new Vector2(140, 42);
            bgr.anchoredPosition = new Vector2(-180, 0);

            Image badgeImg = badgeObj.AddComponent<Image>();
            if (btnWhite != null) { badgeImg.sprite = btnWhite; badgeImg.type = Image.Type.Sliced; }
            badgeImg.color = new Color(0.86f, 0.15f, 0.15f, 1f); // Red CR

            GameObject badgeTxtObj = CreateUI("Text", badgeObj.transform);
            TextMeshProUGUI txtBadge = badgeTxtObj.AddComponent<TextMeshProUGUI>();
            if (appFont != null) txtBadge.font = appFont;
            txtBadge.text = "<b>● Kritis (CR)</b>";
            txtBadge.fontSize = 16;
            txtBadge.alignment = TextAlignmentOptions.Center;
            txtBadge.color = Color.white;
            FillParent(badgeTxtObj.GetComponent<RectTransform>());

            // Voiceover / Animal Sound Button
            GameObject voiceObj = CreateUI("Btn_Voiceover", cardTop.transform);
            RectTransform vr = voiceObj.GetComponent<RectTransform>();
            vr.anchorMin = new Vector2(1f, 0.5f); vr.anchorMax = new Vector2(1f, 0.5f);
            vr.pivot = new Vector2(1f, 0.5f);
            vr.sizeDelta = new Vector2(130, 48);
            vr.anchoredPosition = new Vector2(-28, 0);

            Image voiceImg = voiceObj.AddComponent<Image>();
            if (btnWhite != null) { voiceImg.sprite = btnWhite; voiceImg.type = Image.Type.Sliced; }
            voiceImg.color = new Color(0.06f, 0.72f, 0.50f, 1f); // Emerald
            Button bVoice = voiceObj.AddComponent<Button>();

            GameObject voiceTxtObj = CreateUI("Text", voiceObj.transform);
            TextMeshProUGUI vtxt = voiceTxtObj.AddComponent<TextMeshProUGUI>();
            if (appFont != null) vtxt.font = appFont;
            vtxt.text = "<b>▶ Suara</b>";
            vtxt.fontSize = 18;
            vtxt.alignment = TextAlignmentOptions.Center;
            vtxt.color = Color.white;
            FillParent(voiceTxtObj.GetComponent<RectTransform>());

            // 4 Tabs Bar
            GameObject tabsBar = CreateUI("TabsBar", bottomPanel.transform);
            RectTransform tbr = tabsBar.GetComponent<RectTransform>();
            tbr.anchorMin = new Vector2(0f, 1f); tbr.anchorMax = new Vector2(1f, 1f);
            tbr.pivot = new Vector2(0.5f, 1f);
            tbr.sizeDelta = new Vector2(0, 44);
            tbr.anchoredPosition = new Vector2(0, -100);

            var hlg = tabsBar.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 8;
            hlg.padding = new RectOffset(24, 24, 0, 0);
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = true;
            hlg.childForceExpandHeight = true;

            (Button b0, Image img0, TextMeshProUGUI txt0) = CreateTabButton(tabsBar.transform, "Tab_Deskripsi", "Deskripsi", btnWhite);
            (Button b1, Image img1, TextMeshProUGUI txt1) = CreateTabButton(tabsBar.transform, "Tab_Habitat", "Habitat", btnWhite);
            (Button b2, Image img2, TextMeshProUGUI txt2) = CreateTabButton(tabsBar.transform, "Tab_Mitigasi", "Mitigasi", btnWhite);
            (Button b3, Image img3, TextMeshProUGUI txt3) = CreateTabButton(tabsBar.transform, "Tab_Fakta", "Fakta", btnWhite);

            // Scrollable Detail Content
            GameObject scrollObj = CreateUI("ScrollContent", bottomPanel.transform);
            RectTransform scr = scrollObj.GetComponent<RectTransform>();
            scr.anchorMin = Vector2.zero; scr.anchorMax = Vector2.one;
            scr.offsetMin = new Vector2(28, 16); scr.offsetMax = new Vector2(-28, -150);

            ScrollRect sr = scrollObj.AddComponent<ScrollRect>();
            sr.horizontal = false; sr.vertical = true;
            scrollObj.AddComponent<RectMask2D>();

            GameObject content = CreateUI("Content", scrollObj.transform);
            RectTransform cr = content.GetComponent<RectTransform>();
            cr.anchorMin = new Vector2(0, 1); cr.anchorMax = new Vector2(1, 1);
            cr.pivot = new Vector2(0, 1);
            cr.sizeDelta = new Vector2(0, 300);

            var vlg = content.AddComponent<VerticalLayoutGroup>();
            vlg.childControlWidth = true; vlg.childControlHeight = false;
            vlg.childForceExpandWidth = true; vlg.childForceExpandHeight = false;

            var csf = content.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            sr.content = cr;

            GameObject txtDetailObj = CreateUI("Txt_DetailContent", content.transform);
            TextMeshProUGUI txtDetail = txtDetailObj.AddComponent<TextMeshProUGUI>();
            if (appFont != null) txtDetail.font = appFont;
            txtDetail.fontSize = 20;
            txtDetail.lineSpacing = 1.25f;
            txtDetail.color = new Color(0.04f, 0.14f, 0.10f);

            // ==================== 3. CONTROLLER & TRACKING ENGINE WIRING ====================
            string[] animalGuids = AssetDatabase.FindAssets("t:AnimalDataSO", new[] { "Assets/Resources/Data/Animals", "Assets/Data/Animals" });
            List<AnimalDataSO> animalsList = new List<AnimalDataSO>();
            foreach (var g in animalGuids)
            {
                var a = AssetDatabase.LoadAssetAtPath<AnimalDataSO>(AssetDatabase.GUIDToAssetPath(g));
                if (a != null && !animalsList.Contains(a)) animalsList.Add(a);
            }

            var trackingEngine = stageParent.GetComponent<ARModelTrackingManager>();
            if (trackingEngine != null)
            {
                SerializedObject soTM = new SerializedObject(trackingEngine);
                var pOrigin = soTM.FindProperty("xrOrigin");
                if (pOrigin != null) pOrigin.objectReferenceValue = Object.FindAnyObjectByType<XROrigin>();
                var pCam = soTM.FindProperty("arCamera");
                if (pCam != null) pCam.objectReferenceValue = cam;
                var pPM = soTM.FindProperty("planeManager");
                if (pPM != null) pPM.objectReferenceValue = Object.FindAnyObjectByType<ARPlaneManager>();
                var pRM = soTM.FindProperty("raycastManager");
                if (pRM != null) pRM.objectReferenceValue = Object.FindAnyObjectByType<ARRaycastManager>();
                var pAM = soTM.FindProperty("anchorManager");
                if (pAM != null) pAM.objectReferenceValue = Object.FindAnyObjectByType<ARAnchorManager>();

                var pAnimalsTM = soTM.FindProperty("allAnimals");
                if (pAnimalsTM != null)
                {
                    pAnimalsTM.arraySize = animalsList.Count;
                    for (int i = 0; i < animalsList.Count; i++)
                    {
                        pAnimalsTM.GetArrayElementAtIndex(i).objectReferenceValue = animalsList[i];
                    }
                }
                soTM.ApplyModifiedProperties();
            }

            SerializedObject so = new SerializedObject(controller);
            so.FindProperty("trackingManager").objectReferenceValue = trackingEngine;
            so.FindProperty("btnKembali").objectReferenceValue = bKembali;
            so.FindProperty("txtScanningPrompt").objectReferenceValue = stt;
            so.FindProperty("imgTrackingPill").objectReferenceValue = tsi;

            so.FindProperty("debugOverlayPanel").objectReferenceValue = debugPanel;
            so.FindProperty("txtDebugInfo").objectReferenceValue = dbInfoTxt;
            so.FindProperty("btnToggleDebug").objectReferenceValue = bDebug;

            so.FindProperty("bottomInfoCard").objectReferenceValue = bottomPanel;
            so.FindProperty("txtCommonName").objectReferenceValue = txtName;
            so.FindProperty("txtLatinName").objectReferenceValue = txtLatin;
            so.FindProperty("txtStatusBadge").objectReferenceValue = txtBadge;
            so.FindProperty("imgStatusBadge").objectReferenceValue = badgeImg;
            so.FindProperty("btnVoiceover").objectReferenceValue = bVoice;
            so.FindProperty("txtVoiceoverBtn").objectReferenceValue = vtxt;

            so.FindProperty("tabDeskripsi").objectReferenceValue = b0;
            so.FindProperty("tabHabitat").objectReferenceValue = b1;
            so.FindProperty("tabMitigasi").objectReferenceValue = b2;
            so.FindProperty("tabFakta").objectReferenceValue = b3;

            so.FindProperty("imgTabDeskripsi").objectReferenceValue = img0;
            so.FindProperty("imgTabHabitat").objectReferenceValue = img1;
            so.FindProperty("imgTabMitigasi").objectReferenceValue = img2;
            so.FindProperty("imgTabFakta").objectReferenceValue = img3;

            so.FindProperty("txtTabDeskripsi").objectReferenceValue = txt0;
            so.FindProperty("txtTabHabitat").objectReferenceValue = txt1;
            so.FindProperty("txtTabMitigasi").objectReferenceValue = txt2;
            so.FindProperty("txtTabFakta").objectReferenceValue = txt3;

            so.FindProperty("txtDetailContent").objectReferenceValue = txtDetail;
            so.FindProperty("scrollContent").objectReferenceValue = sr;

            so.FindProperty("allAnimals").arraySize = animalsList.Count;
            for (int i = 0; i < animalsList.Count; i++)
            {
                so.FindProperty("allAnimals").GetArrayElementAtIndex(i).objectReferenceValue = animalsList[i];
            }

            so.ApplyModifiedProperties();
        }

        private static (Button, Image, TextMeshProUGUI) CreateTabButton(Transform parent, string name, string label, Sprite pillSprite)
        {
            GameObject tabObj = CreateUI(name, parent);
            Image tabImg = tabObj.AddComponent<Image>();
            if (pillSprite != null) { tabImg.sprite = pillSprite; tabImg.type = Image.Type.Sliced; }
            tabImg.color = new Color(0.93f, 0.98f, 0.95f, 1f);

            Button btn = tabObj.AddComponent<Button>();

            GameObject tObj = CreateUI("Text", tabObj.transform);
            TextMeshProUGUI tt = tObj.AddComponent<TextMeshProUGUI>();
            if (appFont != null) tt.font = appFont;
            tt.text = $"<b>{label}</b>";
            tt.fontSize = 18;
            tt.alignment = TextAlignmentOptions.Center;
            tt.color = new Color(0.04f, 0.22f, 0.16f);
            tt.raycastTarget = false;
            FillParent(tObj.GetComponent<RectTransform>());

            return (btn, tabImg, tt);
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
