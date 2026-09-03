using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using SatwaLangka.Data;
using SatwaLangka.EditorScripts;

namespace SatwaLangka.EditorScripts
{
    public static class MasterLayoutRebuilder
    {
        [MenuItem("Satwa Langka/Rebuild All Scenes Clean (Masterpiece)")]
        public static void RebuildAll()
        {
            Debug.Log("<b>[SATWA AR]</b> Starting Master Rebuild of all scenes, assets, and audio bindings...");

            List<string> logs = new List<string>();
            logs.Add($"[0/9] Fixing 3D Animal Materials & Shaders...");
            AnimalModelMasterRebuilder.RebuildAllPrefabs();
            FixAllAnimalMaterialsAndShaders();
            EnsurePlayfulSpriteImporters();

            logs.Add($"[1/9] Binding 3D prefabs & audio clips to AnimalDataSO...");
            BindAllAnimalAssets();

            logs.Add($"[2/9] Rebuilding Animal_Select_Card.prefab...");
            PilihHewanSceneBuilder.BuildCardPrefab();

            logs.Add($"[3/9] Rebuilding MainMenu...");
            MainMenuSceneBuilder.BuildMainMenuScene();

            logs.Add($"[4/9] Rebuilding PilihHewan...");
            PilihHewanSceneBuilder.BuildPilihHewanScene();

            logs.Add($"[5/9] Rebuilding ScanPlaneDetection (AR)...");
            ScanPlaneSceneBuilder.BuildScanPlaneScene();

            logs.Add($"[6/9] Rebuilding DetailSatwa...");
            DetailSatwaSceneBuilder.BuildDetailSatwaScene();

            logs.Add($"[7/9] Rebuilding SoalQuiz...");
            SoalQuizSceneBuilder.BuildSoalQuizScene();

            logs.Add($"[8/9] Rebuilding HasilQuiz...");
            HasilQuizSceneBuilder.BuildHasilQuizScene();

            logs.Add($"[9/9] Rebuilding Panduan...");
            PanduanSceneBuilder.BuildPanduanScene();

            logs.Add($"[9/9+] Rebuilding Pengaturan...");
            PengaturanSceneBuilder.BuildPengaturanScene();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("<color=#059669><b>[SATWA AR MASTERPIECE]</b> All scenes & prefabs successfully rebuilt with clean Safe Area, modern colors, and polished layouts!</color>");
        }

        public static void FixAllAnimalMaterialsAndShaders()
        {
            string[] guids = AssetDatabase.FindAssets("t:Material", new[] { "Assets/Models", "Assets/Materials" });
            foreach (var g in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(g);
                Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (mat != null)
                {
                    // If shader is Lit or Standard, ensure Cull Off (Double-Sided) to fix any missing/transparent backfaces
                    if (mat.HasProperty("_Cull"))
                    {
                        mat.SetFloat("_Cull", 0f); // 0 = Off (Double-Sided)
                    }
                    if (mat.HasProperty("_Surface"))
                    {
                        mat.SetFloat("_Surface", 0f); // 0 = Opaque
                        mat.SetOverrideTag("RenderType", "Opaque");
                        mat.renderQueue = 2000;
                    }
                    if (mat.HasProperty("_ZWrite"))
                    {
                        mat.SetFloat("_ZWrite", 1f);
                    }
                    EditorUtility.SetDirty(mat);
                }
            }
        }

        public static void BindAllAnimalAssets()
        {
            var dataMap = new (string code, string prefabPath, string audioPath, Vector3 scale)[]
            {
                ("SATWA01", "Assets/Prefabs/Animals/Prefab_SATWA01_GajahSumatra.prefab", "Assets/Resources/Audio/SFX/SFX_SATWA01_Gajah.mp3", new Vector3(0.40f, 0.40f, 0.40f)),
                ("SATWA02", "Assets/Prefabs/Animals/Prefab_SATWA02_BantengJawa.prefab", "Assets/Resources/Audio/SFX/SFX_SATWA02_Banteng.mp3", new Vector3(0.42f, 0.42f, 0.42f)),
                ("SATWA03", "Assets/Prefabs/Animals/Prefab_SATWA03_Anoa.prefab", "Assets/Resources/Audio/SFX/SFX_SATWA03_Anoa.mp3", new Vector3(0.42f, 0.42f, 0.42f)),
                ("SATWA04", "Assets/Prefabs/Animals/Prefab_Babirusa_FullBody.prefab", "Assets/Resources/Audio/SFX/SFX_SATWA04_Babirusa.mp3", new Vector3(0.42f, 0.42f, 0.42f)),
                ("SATWA05", "Assets/Prefabs/Animals/Prefab_SATWA05_SancaBatik.prefab", "Assets/Resources/Audio/SFX/SFX_SATWA05_Sanca.mp3", new Vector3(0.45f, 0.45f, 0.45f)),
                ("SATWA06", "Assets/Prefabs/Animals/Prefab_SATWA06_KuraMoncongBabi.prefab", "Assets/Resources/Audio/SFX/SFX_SATWA06_Penyu.mp3", new Vector3(0.45f, 0.45f, 0.45f)),
                ("SATWA07", "Assets/Prefabs/Animals/Prefab_SATWA07_KuraLeherUlar.prefab", "Assets/Resources/Audio/SFX/SFX_SATWA07_KuraKura.mp3", new Vector3(0.45f, 0.45f, 0.45f)),
                ("SATWA08", "Assets/Prefabs/Animals/Prefab_SATWA08_Trenggiling.prefab", "Assets/Resources/Audio/SFX/SFX_SATWA08_Trenggiling.mp3", new Vector3(0.45f, 0.45f, 0.45f)),
                ("SATWA09", "Assets/Prefabs/Animals/Prefab_SATWA09_MacanTutul.prefab", "Assets/Resources/Audio/SFX/SFX_SATWA09_MacanTutul.mp3", new Vector3(0.42f, 0.42f, 0.42f)),
                ("SATWA10", "Assets/Prefabs/Animals/Prefab_SATWA10_RusaTimor.prefab", "Assets/Resources/Audio/SFX/SFX_SATWA10_Rusa.mp3", new Vector3(0.42f, 0.42f, 0.42f)),
                ("SATWA11", "Assets/Prefabs/Animals/Prefab_SATWA11_Sigung.prefab", "Assets/Resources/Audio/SFX/SFX_SATWA11_Sigung.mp3", new Vector3(0.45f, 0.45f, 0.45f)),
                ("SATWA12", "Assets/Prefabs/Animals/Prefab_SATWA12_Bekantan.prefab", "Assets/Resources/Audio/SFX/SFX_SATWA12_Bekantan.mp3", new Vector3(0.45f, 0.45f, 0.45f))
            };

            foreach (var item in dataMap)
            {
                string[] guids = AssetDatabase.FindAssets(item.code, new[] { "Assets/Resources/Data/Animals", "Assets/Data/Animals" });
                if (guids.Length > 0)
                {
                    string assetPath = AssetDatabase.GUIDToAssetPath(guids[0]);
                    AnimalDataSO so = AssetDatabase.LoadAssetAtPath<AnimalDataSO>(assetPath);
                    GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(item.prefabPath);
                    AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>(item.audioPath);

                    if (so != null)
                    {
                        if (prefab != null) so.modelPrefab = prefab;
                        if (clip != null) so.animalSound = clip;
                        so.defaultScale = item.scale;
                        EditorUtility.SetDirty(so);
                        Debug.Log($"<b>[ASSET BIND]</b> Successfully Bound {item.code} ({so.commonName}): Prefab={(prefab != null ? prefab.name : "NULL")}, Audio={(clip != null ? clip.name : "NULL")}");
                    }
                }
            }
        }

        public static void EnsurePlayfulSpriteImporters()
        {
            string[] playfulDirs = new[] { "Assets/Sprites/Icons/Playful", "Assets/Sprites/UI/Playful", "Assets/Sprites/UI" };
            string[] guids = AssetDatabase.FindAssets("t:Texture2D", playfulDirs);
            foreach (var g in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(g);
                TextureImporter ti = AssetImporter.GetAtPath(path) as TextureImporter;
                if (ti != null)
                {
                    bool changed = false;
                    if (ti.textureType != TextureImporterType.Sprite)
                    {
                        ti.textureType = TextureImporterType.Sprite;
                        ti.spriteImportMode = SpriteImportMode.Single;
                        changed = true;
                    }
                    if (!ti.alphaIsTransparency)
                    {
                        ti.alphaIsTransparency = true;
                        changed = true;
                    }
                    if (ti.mipmapEnabled)
                    {
                        ti.mipmapEnabled = false;
                        changed = true;
                    }
                    if (changed)
                    {
                        ti.SaveAndReimport();
                    }
                }
            }
            AssetDatabase.Refresh();
        }
    }
}
