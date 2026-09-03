using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using SatwaLangka.AR;

public static class ARInfoPanelBuilder
{
    [MenuItem("Satwa Langka/Build AR Info Panel")]
    public static void Build()
    {
        var scene = EditorSceneManager.OpenScene("Assets/Scenes/ScanPlaneDetection.unity", OpenSceneMode.Single);
        var canvas = Object.FindObjectOfType<Canvas>();
        if (canvas == null) { Debug.LogError("No canvas!"); return; }

        // Hapus panel lama
        var old = GameObject.Find("InfoPanel_Satwa");
        if (old != null) Object.DestroyImmediate(old);

        var forest = new Color(0.055f, 0.478f, 0.353f, 1f);
        var dark   = new Color(0.04f, 0.07f, 0.04f, 0.93f);
        var gray   = new Color(0.2f, 0.2f, 0.2f, 1f);

        // ROOT PANEL - 45% bawah
        var panel = MakeGO("InfoPanel_Satwa", canvas.transform);
        SetRect(panel, 0, 0, 1, 0.45f);
        panel.AddComponent<Image>().color = dark;
        var arInfo = panel.AddComponent<ARInfoPanelController>();
        var audioSrc = panel.AddComponent<AudioSource>();

        // TOGGLE BUTTON
        var btnTogGO = MakeGO("Btn_Toggle", panel.transform);
        var btnTogRT = btnTogGO.GetComponent<RectTransform>();
        btnTogRT.anchorMin = new Vector2(0.5f, 1f);
        btnTogRT.anchorMax = new Vector2(0.5f, 1f);
        btnTogRT.anchoredPosition = new Vector2(0, 20);
        btnTogRT.sizeDelta = new Vector2(120, 36);
        btnTogGO.AddComponent<Image>().color = forest;
        var btnTog = btnTogGO.AddComponent<Button>();
        var togTxt = MakeGO("Text", btnTogGO.transform);
        SetRect(togTxt, 0, 0, 1, 1);
        var togTMP = togTxt.AddComponent<TextMeshProUGUI>();
        togTMP.text = "▲ Info"; togTMP.fontSize = 24; togTMP.color = Color.white;
        togTMP.alignment = TextAlignmentOptions.Center; togTMP.fontStyle = FontStyles.Bold;

        // PANEL CONTENT
        var pcGO = MakeGO("PanelContent", panel.transform);
        SetRect(pcGO, 0, 0, 1, 1);

        // HEADER (top 28%)
        var hGO = MakeGO("Header", pcGO.transform);
        SetRect(hGO, 0, 0.72f, 1, 1, 16, 4, -16, -4);

        var nameGO = MakeGO("Txt_CommonName", hGO.transform);
        SetRect(nameGO, 0, 0.5f, 0.7f, 1f);
        var nameTMP = nameGO.AddComponent<TextMeshProUGUI>();
        nameTMP.text = "Nama Satwa"; nameTMP.fontSize = 36; nameTMP.color = Color.white;
        nameTMP.fontStyle = FontStyles.Bold;

        var latinGO = MakeGO("Txt_LatinName", hGO.transform);
        SetRect(latinGO, 0, 0f, 0.7f, 0.5f);
        var latinTMP = latinGO.AddComponent<TextMeshProUGUI>();
        latinTMP.text = "nama ilmiah"; latinTMP.fontSize = 22;
        latinTMP.color = new Color(0.8f, 0.8f, 0.8f, 1f); latinTMP.fontStyle = FontStyles.Italic;

        var badgeGO = MakeGO("Txt_StatusBadge", hGO.transform);
        SetRect(badgeGO, 0.72f, 0.15f, 1f, 0.85f);
        var badgeTMP = badgeGO.AddComponent<TextMeshProUGUI>();
        badgeTMP.text = "CR"; badgeTMP.fontSize = 22; badgeTMP.fontStyle = FontStyles.Bold;
        badgeTMP.color = new Color(1f, 0.3f, 0.3f, 1f);
        badgeTMP.alignment = TextAlignmentOptions.Right;

        // TAB BAR (62-72%)
        var tabGO = MakeGO("TabBar", pcGO.transform);
        SetRect(tabGO, 0, 0.62f, 1, 0.72f);
        tabGO.AddComponent<Image>().color = new Color(0.08f, 0.12f, 0.08f, 1f);
        string[] tNames = { "Deskripsi", "Habitat", "Mitigasi", "Fakta" };
        var tabBtns = new Button[4];
        var tabImgs = new Image[4];
        for (int i = 0; i < 4; i++)
        {
            float x0 = i / 4f, x1 = (i + 1) / 4f;
            var tGO = MakeGO("Tab_" + tNames[i], tabGO.transform);
            SetRect(tGO, x0, 0, x1, 1);
            tabImgs[i] = tGO.AddComponent<Image>();
            tabImgs[i].color = (i == 0) ? forest : gray;
            tabBtns[i] = tGO.AddComponent<Button>();
            var tTxtGO = MakeGO("Text", tGO.transform);
            SetRect(tTxtGO, 0, 0, 1, 1);
            var tTMP = tTxtGO.AddComponent<TextMeshProUGUI>();
            tTMP.text = tNames[i]; tTMP.fontSize = 22; tTMP.color = Color.white;
            tTMP.alignment = TextAlignmentOptions.Center; tTMP.fontStyle = FontStyles.Bold;
        }

        // SCROLL CONTENT (0-62%)
        var scrollGO = MakeGO("ScrollView", pcGO.transform);
        SetRect(scrollGO, 0, 0.08f, 1, 0.62f, 0, 0, 0, -4);
        var scroll = scrollGO.AddComponent<ScrollRect>();

        var vpGO = MakeGO("Viewport", scrollGO.transform);
        SetRect(vpGO, 0, 0, 1, 1);
        vpGO.AddComponent<Image>().color = Color.clear;
        vpGO.AddComponent<Mask>().showMaskGraphic = false;

        var ciGO = MakeGO("Content", vpGO.transform);
        var ciRT = ciGO.GetComponent<RectTransform>();
        ciRT.anchorMin = new Vector2(0, 1); ciRT.anchorMax = new Vector2(1, 1);
        ciRT.offsetMin = ciRT.offsetMax = Vector2.zero;
        var csf = ciGO.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        var vlg = ciGO.AddComponent<VerticalLayoutGroup>();
        vlg.padding = new RectOffset(16, 16, 8, 8); vlg.spacing = 8;
        vlg.childControlHeight = false; vlg.childControlWidth = true;

        var cTxtGO = MakeGO("Txt_Content", ciGO.transform);
        var cTMP = cTxtGO.AddComponent<TextMeshProUGUI>();
        cTMP.text = "Deskripsi satwa."; cTMP.fontSize = 26;
        cTMP.color = new Color(0.9f, 0.9f, 0.9f, 1f); cTMP.enableWordWrapping = true;
        var cLE = cTxtGO.AddComponent<LayoutElement>(); cLE.minHeight = 100;
        var cCsf = cTxtGO.AddComponent<ContentSizeFitter>();
        cCsf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        scroll.viewport = vpGO.GetComponent<RectTransform>();
        scroll.content = ciRT; scroll.horizontal = false; scroll.vertical = true;

        // AUDIO BUTTON (bottom 8%)
        var audGO = MakeGO("Btn_Audio", pcGO.transform);
        SetRect(audGO, 0.1f, 0.01f, 0.9f, 0.08f);
        audGO.AddComponent<Image>().color = forest;
        var audBtn = audGO.AddComponent<Button>();
        var audTxtGO = MakeGO("Text", audGO.transform);
        SetRect(audTxtGO, 0, 0, 1, 1);
        var audTMP = audTxtGO.AddComponent<TextMeshProUGUI>();
        audTMP.text = "🔊 Putar Suara"; audTMP.fontSize = 26; audTMP.color = Color.white;
        audTMP.alignment = TextAlignmentOptions.Center; audTMP.fontStyle = FontStyles.Bold;

        // Wire ARInfoPanelController
        var so = new SerializedObject(arInfo);
        so.FindProperty("txtCommonName").objectReferenceValue  = nameTMP;
        so.FindProperty("txtLatinName").objectReferenceValue   = latinTMP;
        so.FindProperty("txtStatusBadge").objectReferenceValue = badgeTMP;
        so.FindProperty("imgTabDeskripsi").objectReferenceValue = tabImgs[0];
        so.FindProperty("imgTabHabitat").objectReferenceValue   = tabImgs[1];
        so.FindProperty("imgTabMitigasi").objectReferenceValue  = tabImgs[2];
        so.FindProperty("imgTabFakta").objectReferenceValue     = tabImgs[3];
        so.FindProperty("tabDeskripsi").objectReferenceValue   = tabBtns[0];
        so.FindProperty("tabHabitat").objectReferenceValue     = tabBtns[1];
        so.FindProperty("tabMitigasi").objectReferenceValue    = tabBtns[2];
        so.FindProperty("tabFakta").objectReferenceValue       = tabBtns[3];
        so.FindProperty("txtContent").objectReferenceValue     = cTMP;
        so.FindProperty("btnPlayAudio").objectReferenceValue   = audBtn;
        so.FindProperty("audioSource").objectReferenceValue    = audioSrc;
        so.FindProperty("btnTogglePanel").objectReferenceValue = btnTog;
        so.FindProperty("panelContent").objectReferenceValue   = pcGO;
        so.ApplyModifiedProperties();

        EditorSceneManager.SaveScene(scene);
        Debug.Log("[ARInfoPanelBuilder] InfoPanel built and saved!");
    }

    static GameObject MakeGO(string name, Transform parent)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.AddComponent<RectTransform>();
        return go;
    }

    static void SetRect(GameObject go, float ax0, float ay0, float ax1, float ay1,
                        float ox0 = 0, float oy0 = 0, float ox1 = 0, float oy1 = 0)
    {
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin  = new Vector2(ax0, ay0);
        rt.anchorMax  = new Vector2(ax1, ay1);
        rt.offsetMin  = new Vector2(ox0, oy0);
        rt.offsetMax  = new Vector2(ox1, oy1);
    }
}
