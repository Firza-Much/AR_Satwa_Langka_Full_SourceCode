using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using SatwaLangka.AR;

namespace SatwaLangka.EditorScripts
{
    public static class PrefabGenerator
    {
        [MenuItem("Satwa Langka/Generate Clean Animal Prefabs")]
        public static void GeneratePrefabs()
        {
            string prefabDir = "Assets/Prefabs/Animals";
            if (!Directory.Exists(prefabDir))
            {
                Directory.CreateDirectory(prefabDir);
            }

            var rotations = new Dictionary<string, Quaternion>()
            {
                { "Gajah_Sumatra", Quaternion.Euler(-90f, 150f, 0f) },
                { "Banteng_Jawa", Quaternion.Euler(-90f, 180f, 0f) },
                { "Anoa", Quaternion.Euler(0f, 180f, 0f) },
                { "Babirusa", Quaternion.Euler(90f, 180f, 0f) },
                { "Sanca_Anaconda", Quaternion.Euler(-90f, 35f, 0f) },
                { "Kura_Kura_Rawa", Quaternion.Euler(-90f, 180f, 0f) },
                { "Trenggiling", Quaternion.Euler(0f, 180f, 0f) },
                { "Macan_Tutul", Quaternion.Euler(-90f, 180f, 0f) },
                { "Rusa_Jawa", Quaternion.Euler(0f, 180f, 0f) },
                { "Sigung", Quaternion.Euler(0f, 200f, 0f) },
                { "Bekantan", Quaternion.Euler(-90f, 225f, 0f) }
            };

            string[] animalFolders = new[]
            {
                "Gajah_Sumatra", "Banteng_Jawa", "Anoa", "Babirusa",
                "Sanca_Anaconda", "Kura_Kura_Rawa", "Trenggiling",
                "Macan_Tutul", "Rusa_Jawa", "Sigung", "Bekantan"
            };

            foreach (string folder in animalFolders)
            {
                if (folder == "Babirusa")
                {
                    continue;
                }

                string glbPath = $"Assets/Models/{folder}/{folder}.glb";
                GameObject sourceModel = AssetDatabase.LoadMainAssetAtPath(glbPath) as GameObject;
                
                if (sourceModel == null)
                {
                    string gltfPath = $"Assets/Models/{folder}/scene.gltf";
                    sourceModel = AssetDatabase.LoadMainAssetAtPath(gltfPath) as GameObject;
                }

                if (sourceModel != null)
                {
                    GameObject root = new GameObject($"Prefab_{folder}");
                    GameObject modelInstance = Object.Instantiate(sourceModel, root.transform);
                    modelInstance.name = "Model_Mesh";

                    Quaternion rot = rotations.ContainsKey(folder) ? rotations[folder] : Quaternion.identity;
                    modelInstance.transform.localRotation = rot;

                    // Deactivate extraneous bases, terrain, or environments
                    foreach (var t in modelInstance.GetComponentsInChildren<Transform>(true))
                    {
                        if (t.name == "base_0" || t.name == "base" || t.name.Contains("Tree_of_Life"))
                        {
                            t.gameObject.SetActive(false);
                        }
                        if (folder == "Trenggiling")
                        {
                            if (t.name != "Model_Mesh" && t.name != "Icosphere_0" && t.name != "Cube_0" && t.name != root.name && t.GetComponent<Renderer>() != null)
                            {
                                t.gameObject.SetActive(false);
                            }
                        }
                    }

                    // Compute bounds of active renderers
                    Renderer[] renderers = modelInstance.GetComponentsInChildren<Renderer>(false);
                    if (renderers.Length == 0)
                    {
                        Object.DestroyImmediate(root);
                        continue;
                    }

                    Bounds b = renderers[0].bounds;
                    for (int i = 1; i < renderers.Length; i++) b.Encapsulate(renderers[i].bounds);

                    float maxDim = Mathf.Max(b.size.x, b.size.y, b.size.z);
                    if (maxDim <= 0.0001f) maxDim = 1f;

                    // Normalize scale to 1.15m canonical bounding diameter
                    float targetScale = 1.15f / maxDim;
                    modelInstance.transform.localScale = modelInstance.transform.localScale * targetScale;

                    // Recalculate bounds after scale
                    b = renderers[0].bounds;
                    for (int i = 1; i < renderers.Length; i++) b.Encapsulate(renderers[i].bounds);

                    // Shift model so its ground point is at Y=0 and centered at (0,0)
                    Vector3 shift = new Vector3(-b.center.x, -b.min.y, -b.center.z);
                    modelInstance.transform.position += shift;

                    b = renderers[0].bounds;
                    // Recalculate final bounds
                    b = renderers[0].bounds;
                    for (int i = 1; i < renderers.Length; i++) b.Encapsulate(renderers[i].bounds);

                    // Add components
                    if (root.GetComponent<SatwaLangka.AR.TouchManipulator>() == null)
                    {
                        root.AddComponent<SatwaLangka.AR.TouchManipulator>();
                    }
                    BoxCollider box = root.AddComponent<BoxCollider>();
                    box.center = new Vector3(0f, b.size.y * 0.5f, 0f);
                    box.size = new Vector3(Mathf.Max(b.size.x, 0.4f), Mathf.Max(b.size.y, 0.4f), Mathf.Max(b.size.z, 0.4f));

                    string savePath = $"{prefabDir}/Prefab_{folder}.prefab";
                    PrefabUtility.SaveAsPrefabAsset(root, savePath);
                    Object.DestroyImmediate(root);
                    Debug.Log($"<b>[SATWA AR]</b> Generated Normalized Prefab: {savePath} (Size: {b.size}, Bottom Y: {b.min.y:F3})");
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("<b>[SATWA AR]</b> All Animal Prefabs successfully normalized and saved!");
        }
    }
}
