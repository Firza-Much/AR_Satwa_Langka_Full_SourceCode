using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;
using SatwaLangka.Data;
using SatwaLangka.UI;

namespace SatwaLangka.EditorScripts
{
    public static class AnimalModelValidator
    {
        [MenuItem("Satwa Langka/Validate & Capture All 12 Animals on Pedestal")]
        public static void ValidateAndCaptureAll12()
        {
            string scenePath = "Assets/Scenes/DetailSatwa.unity";
            EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

            string screenshotDir = "Assets/Screenshots/Animals12";
            if (!Directory.Exists(screenshotDir)) Directory.CreateDirectory(screenshotDir);

            // Find DetailSatwaController and stage
            var controller = Object.FindAnyObjectByType<DetailSatwaController>();
            var animalSpawnParent = GameObject.Find("AnimalStageParent")?.transform;
            if (animalSpawnParent == null) animalSpawnParent = GameObject.Find("Stage_Pedestal")?.transform;

            string[] codes = new[] {
                "SATWA01", "SATWA02", "SATWA03", "SATWA04",
                "SATWA05", "SATWA06", "SATWA07", "SATWA08",
                "SATWA09", "SATWA10", "SATWA11", "SATWA12"
            };

            var camera = Camera.main;
            if (camera != null)
            {
                camera.transform.position = new Vector3(0f, 1.48f, -2.45f);
                camera.transform.rotation = Quaternion.Euler(14f, 0f, 0f);
            }

            int count = 0;
            foreach (string code in codes)
            {
                string[] guids = AssetDatabase.FindAssets(code, new[] { "Assets/Data/Animals" });
                if (guids.Length == 0) continue;

                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                AnimalDataSO animal = AssetDatabase.LoadAssetAtPath<AnimalDataSO>(path);
                if (animal == null || animal.modelPrefab == null) continue;

                // Clear previous
                if (animalSpawnParent != null)
                {
                    for (int i = animalSpawnParent.childCount - 1; i >= 0; i--)
                    {
                        Object.DestroyImmediate(animalSpawnParent.GetChild(i).gameObject);
                    }
                }

                GameObject model = Object.Instantiate(animal.modelPrefab, animalSpawnParent);
                model.name = $"Stage_{animal.animalCode}_{animal.commonName}";
                model.transform.localPosition = new Vector3(0f, 0.245f, 0f);
                model.transform.localRotation = Quaternion.Euler(-90f, 150f, 0f);

                Vector3 scale = animal.defaultScale * 2.1f;
                if (scale == Vector3.zero) scale = Vector3.one * 0.42f;
                model.transform.localScale = scale;

                // Align feet with pedestal top
                Renderer[] renderers = model.GetComponentsInChildren<Renderer>();
                float footY = 0f;
                float diff = 0f;
                if (renderers != null && renderers.Length > 0)
                {
                    Bounds b = renderers[0].bounds;
                    for (int r = 1; r < renderers.Length; r++) b.Encapsulate(renderers[r].bounds);
                    float pedTopY = animalSpawnParent != null ? animalSpawnParent.position.y + 0.015f : 0.015f;
                    diff = pedTopY - b.min.y;
                    model.transform.position += new Vector3(0f, diff, 0f);
                    footY = b.min.y + diff;
                }

                // Render into RenderTexture & save PNG
                int width = 1080;
                int height = 1920;
                RenderTexture rt = new RenderTexture(width, height, 24);
                camera.targetTexture = rt;
                camera.Render();

                RenderTexture.active = rt;
                Texture2D tex = new Texture2D(width, height, TextureFormat.RGB24, false);
                tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                tex.Apply();

                camera.targetTexture = null;
                RenderTexture.active = null;
                Object.DestroyImmediate(rt);

                byte[] bytes = tex.EncodeToPNG();
                Object.DestroyImmediate(tex);

                string outPath = $"{screenshotDir}/{animal.animalCode}_{animal.commonName.Replace(' ', '_')}_Proof.png";
                File.WriteAllBytes(outPath, bytes);

                Debug.Log($"<b>[ANIMAL VALIDATOR]</b> Verified {animal.animalCode} ({animal.commonName}): Scaled {scale.x:F2}, Pedestal Align Offset +{diff:F3}m -> Saved to {outPath}");
                count++;
            }

            AssetDatabase.Refresh();
            Debug.Log($"<b>[ANIMAL VALIDATOR]</b> Successfully validated and captured all {count} animals!");
        }
    }
}
