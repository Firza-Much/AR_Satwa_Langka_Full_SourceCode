using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using SatwaLangka.Data;
using SatwaLangka.AR;

namespace SatwaLangka.EditorScripts
{
    public static class AnimalModelMasterRebuilder
    {
        [MenuItem("Satwa Langka/Rebuild All 12 Animal 3D Prefabs Clean")]
        public static void RebuildAllPrefabs()
        {
            string prefabDir = "Assets/Prefabs/Animals";
            if (!Directory.Exists(prefabDir)) Directory.CreateDirectory(prefabDir);

            string matDir = "Assets/Materials/Animals";
            if (!Directory.Exists(matDir)) Directory.CreateDirectory(matDir);

            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

            var animalConfigs = new List<(string code, string name, string modelPath, Quaternion rotation, float targetDimension, string explicitTexPath)>()
            {
                ("SATWA01", "GajahSumatra", "Assets/Models/Gajah_Sumatra/Gajah_Sumatra.obj", Quaternion.Euler(0f, 140f, 0f), 0.85f, "Assets/Models/Gajah_Sumatra/textures/Material.005_baseColor.jpeg"),
                ("SATWA02", "BantengJawa", "Assets/Models/Banteng_Jawa/Banteng_Jawa.obj", Quaternion.Euler(0f, 270f, 0f), 0.78f, "Assets/Models/Banteng_Jawa/textures/bull_low_baseColor.jpeg"),
                ("SATWA03", "Anoa", "Assets/Models/Anoa/Anoa.obj", Quaternion.Euler(0f, 205f, 0f), 0.72f, "Assets/Models/Anoa/textures/Anoa_baseColor.png"),
                ("SATWA04", "Babirusa", "Assets/Models/Babirusa/Babirusa.obj", Quaternion.Euler(0f, 30f, 0f), 0.72f, "Assets/Models/Babirusa/textures/image_1.png"),
                ("SATWA05", "SancaBatik", "Assets/Models/Sanca_Anaconda/Sanca_Anaconda.obj", Quaternion.Euler(0f, 45f, 0f), 0.72f, "Assets/Models/Sanca_Anaconda/textures/BODY_LOW_baseColor.jpeg"),
                ("SATWA06", "KuraMoncongBabi", "Assets/Models/Kura_Kura_Rawa/Kura_Kura_Rawa.obj", Quaternion.Euler(0f, 75f, 0f), 0.65f, "Assets/Models/Kura_Kura_Rawa/textures/material_0_baseColor.jpeg"),
                ("SATWA07", "KuraLeherUlar", "Assets/Models/Kura_Kura_Rawa/Kura_Kura_Rawa.obj", Quaternion.Euler(0f, 75f, 0f), 0.65f, "Assets/Models/Kura_Kura_Rawa/textures/material_0_baseColor.jpeg"),
                ("SATWA08", "Trenggiling", "Assets/Models/Trenggiling/Trenggiling.obj", Quaternion.Euler(0f, 225f, 0f), 0.68f, "Assets/Models/Trenggiling/textures/Trenggiling_FullBody_albedo.jpg"),
                ("SATWA09", "MacanTutul", "Assets/Models/Macan_Tutul/Macan_Tutul.obj", Quaternion.Euler(0f, 210f, 0f), 0.75f, "Assets/Models/Macan_Tutul/textures/leopard_Material_baseColor.png"),
                ("SATWA10", "RusaTimor", "Assets/Models/Rusa_Jawa/Rusa_Jawa.obj", Quaternion.Euler(0f, 205f, 0f), 0.78f, "Assets/Models/Rusa_Jawa/textures/Deer_M_diffuse.png"),
                ("SATWA11", "Sigung", "Assets/Models/Sigung/Sigung.obj", Quaternion.Euler(0f, 115f, 0f), 0.62f, "Assets/Models/Sigung/textures/CH_NPC_MOB_Skunk_MI_BYN_diffuse.png"),
                ("SATWA12", "Bekantan", "Assets/Models/Bekantan/Bekantan.obj", Quaternion.Euler(0f, 295f, 0f), 0.70f, "Assets/Models/Bekantan/textures/main_baseColor.jpeg")
            };

            Shader targetShader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard") ?? Shader.Find("Mobile/Diffuse");

            foreach (var cfg in animalConfigs)
            {
                Debug.Log($"<b>[REBUILD 3D]</b> Processing {cfg.code} - {cfg.name}...");

                AssetDatabase.ImportAsset(cfg.modelPath, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
                GameObject sourceModel = AssetDatabase.LoadAssetAtPath<GameObject>(cfg.modelPath);

                if (sourceModel == null)
                {
                    Debug.LogError($"[REBUILD 3D] Could not find source model for {cfg.name} at {cfg.modelPath}");
                    continue;
                }

                GameObject root = new GameObject($"Prefab_{cfg.code}_{cfg.name}");
                GameObject modelInst = UnityEngine.Object.Instantiate(sourceModel, root.transform);
                modelInst.name = "Model";
                modelInst.transform.localPosition = Vector3.zero;
                modelInst.transform.localRotation = cfg.rotation;
                modelInst.transform.localScale = Vector3.one;

                if (PrefabUtility.IsPartOfPrefabInstance(modelInst))
                {
                    PrefabUtility.UnpackPrefabInstance(modelInst, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
                }

                // Deactivate base pedestals or unwanted extra geometry from sketchfab
                foreach (var t in modelInst.GetComponentsInChildren<Transform>(true))
                {
                    if (t.name == "base_0" || t.name == "base" || t.name.Contains("Tree_of_Life") || t.name.Contains("Ground"))
                    {
                        t.gameObject.SetActive(false);
                    }
                }

                // Load explicit main texture for this animal
                Texture2D explicitTex = null;
                if (!string.IsNullOrEmpty(cfg.explicitTexPath))
                {
                    AssetDatabase.ImportAsset(cfg.explicitTexPath, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
                    explicitTex = AssetDatabase.LoadAssetAtPath<Texture2D>(cfg.explicitTexPath);
                }

                // Fix Materials on all renderers
                Renderer[] renderers = modelInst.GetComponentsInChildren<Renderer>(true);
                foreach (var r in renderers)
                {
                    r.gameObject.SetActive(true);
                    r.enabled = true;

                    Material[] origMats = r.sharedMaterials;
                    Material[] newMats = new Material[origMats != null && origMats.Length > 0 ? origMats.Length : 1];

                    for (int m = 0; m < newMats.Length; m++)
                    {
                        Material sourceMat = (origMats != null && m < origMats.Length) ? origMats[m] : null;
                        string matName = sourceMat != null ? sourceMat.name : $"{cfg.name}_Mat_{m}";
                        string matSavePath = $"{matDir}/Mat_{cfg.name}_{m}.mat";

                        Material diskMat = AssetDatabase.LoadAssetAtPath<Material>(matSavePath);
                        if (diskMat == null)
                        {
                            diskMat = new Material(targetShader);
                            diskMat.name = matName;
                            AssetDatabase.CreateAsset(diskMat, matSavePath);
                        }

                        Texture2D mainTex = explicitTex;

                        if (mainTex == null && sourceMat != null && sourceMat.mainTexture != null)
                        {
                            mainTex = sourceMat.mainTexture as Texture2D;
                        }

                        if (mainTex != null)
                        {
                            diskMat.mainTexture = mainTex;
                            if (diskMat.HasProperty("_BaseMap")) diskMat.SetTexture("_BaseMap", mainTex);
                            if (diskMat.HasProperty("_BaseColor")) diskMat.SetColor("_BaseColor", Color.white);
                            diskMat.color = Color.white;
                        }
                        else if (sourceMat != null && sourceMat.color != Color.clear)
                        {
                            diskMat.color = sourceMat.color;
                        }
                        else
                        {
                            diskMat.color = new Color(0.35f, 0.32f, 0.28f, 1f);
                        }

                        // Balanced matte PBR material properties (avoids blown-out specular highlights)
                        if (diskMat.HasProperty("_Metallic")) diskMat.SetFloat("_Metallic", 0.0f);
                        if (diskMat.HasProperty("_Smoothness")) diskMat.SetFloat("_Smoothness", 0.15f);

                        // Ensure double-sided rendering so backfaces are never transparent
                        if (diskMat.HasProperty("_Cull")) diskMat.SetFloat("_Cull", (float)UnityEngine.Rendering.CullMode.Off);

                        EditorUtility.SetDirty(diskMat);
                        newMats[m] = diskMat;
                    }

                    r.sharedMaterials = newMats;
                }

                // Compute exact unscaled bounds in root space
                Bounds b = ComputeRootSpaceBounds(renderers, root.transform);

                if (b.size.sqrMagnitude > 0.00001f)
                {
                    float maxDim = Mathf.Max(b.size.x, b.size.y, b.size.z);
                    if (maxDim <= 0.0001f) maxDim = 1f;

                    // Normalize to target visual dimension
                    float scaleFactor = cfg.targetDimension / maxDim;
                    modelInst.transform.localScale = Vector3.one * scaleFactor;

                    // Align center in XZ and lowest point in Y to 0
                    modelInst.transform.localPosition = Vector3.zero;
                    b = ComputeRootSpaceBounds(renderers, root.transform);
                    modelInst.transform.localPosition = new Vector3(-b.center.x, -b.min.y, -b.center.z);

                    // Final bounds verification
                    b = ComputeRootSpaceBounds(renderers, root.transform);
                }

                // Add TouchManipulator & BoxCollider for AR interaction
                BoxCollider boxCol = root.GetComponent<BoxCollider>();
                if (boxCol == null) boxCol = root.AddComponent<BoxCollider>();
                boxCol.center = new Vector3(0, b.size.y * 0.5f, 0);
                boxCol.size = b.size.sqrMagnitude > 0.0001f ? b.size : new Vector3(0.6f, 0.6f, 0.6f);

                TouchManipulator tm = root.GetComponent<TouchManipulator>();
                if (tm == null) tm = root.AddComponent<TouchManipulator>();

                // Save Prefab
                string savePath = $"{prefabDir}/Prefab_{cfg.code}_{cfg.name}.prefab";
                PrefabUtility.SaveAsPrefabAsset(root, savePath);

                // Copy to legacy aliases for complete backwards compatibility
                if (cfg.code == "SATWA04")
                {
                    PrefabUtility.SaveAsPrefabAsset(root, $"{prefabDir}/Prefab_Babirusa_FullBody.prefab");
                    PrefabUtility.SaveAsPrefabAsset(root, $"{prefabDir}/Prefab_Babirusa.prefab");
                }
                else if (cfg.code == "SATWA05")
                {
                    PrefabUtility.SaveAsPrefabAsset(root, $"{prefabDir}/Prefab_Sanca_Anaconda.prefab");
                }

                UnityEngine.Object.DestroyImmediate(root);

                Debug.Log($"<b>[REBUILD 3D]</b> Successfully saved {savePath} with bounds min.y={b.min.y:F3} max.y={b.max.y:F3} size={b.size.ToString("F3")}!");
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("<b>[REBUILD 3D]</b> All 12 3D animal prefabs rebuilt with native materials & textures!");
        }

        private static float GetTargetDimension(string code)
        {
            if (code == "SATWA01") return 0.85f; // Gajah Sumatra
            if (code == "SATWA02") return 0.78f; // Banteng Jawa
            if (code == "SATWA03") return 0.72f; // Anoa
            if (code == "SATWA04") return 0.72f; // Babirusa
            if (code == "SATWA09") return 0.75f; // Macan Tutul Jawa
            if (code == "SATWA10") return 0.78f; // Rusa Timor
            if (code == "SATWA12") return 0.70f; // Bekantan
            if (code == "SATWA08") return 0.68f; // Trenggiling
            if (code == "SATWA11") return 0.62f; // Sigung
            if (code == "SATWA05") return 0.72f; // Sanca Batik
            if (code == "SATWA06") return 0.65f; // Kura Moncong Babi
            if (code == "SATWA07") return 0.65f; // Kura Leher Ular Rote
            return 0.72f;
        }

        private static Bounds ComputeRootSpaceBounds(Renderer[] renderers, Transform rootTransform)
        {
            Bounds b = new Bounds();
            bool hasBounds = false;

            foreach (var r in renderers)
            {
                if (r != null && r.gameObject.activeInHierarchy)
                {
                    MeshFilter mf = r.GetComponent<MeshFilter>();
                    if (mf != null && mf.sharedMesh != null)
                    {
                        Bounds mb = mf.sharedMesh.bounds;
                        Vector3 min = mb.min;
                        Vector3 max = mb.max;
                        Vector3[] corners = new Vector3[]
                        {
                            new Vector3(min.x, min.y, min.z), new Vector3(max.x, min.y, min.z),
                            new Vector3(min.x, max.y, min.z), new Vector3(max.x, max.y, min.z),
                            new Vector3(min.x, min.y, max.z), new Vector3(max.x, min.y, max.z),
                            new Vector3(min.x, max.y, max.z), new Vector3(max.x, max.y, max.z)
                        };
                        foreach (var c in corners)
                        {
                            Vector3 worldPt = r.transform.TransformPoint(c);
                            Vector3 rootPt = rootTransform.InverseTransformPoint(worldPt);
                            if (!hasBounds) { b = new Bounds(rootPt, Vector3.zero); hasBounds = true; }
                            else b.Encapsulate(rootPt);
                        }
                    }
                    else if (r.bounds.size.sqrMagnitude > 0.00001f)
                    {
                        Vector3 worldMin = r.bounds.min;
                        Vector3 worldMax = r.bounds.max;
                        Vector3[] corners = new Vector3[]
                        {
                            new Vector3(worldMin.x, worldMin.y, worldMin.z), new Vector3(worldMax.x, worldMin.y, worldMin.z),
                            new Vector3(worldMin.x, worldMax.y, worldMin.z), new Vector3(worldMax.x, worldMax.y, worldMin.z),
                            new Vector3(worldMin.x, worldMin.y, worldMax.z), new Vector3(worldMax.x, worldMin.y, worldMax.z),
                            new Vector3(worldMin.x, worldMax.y, worldMax.z), new Vector3(worldMax.x, worldMax.y, worldMax.z)
                        };
                        foreach (var c in corners)
                        {
                            Vector3 rootPt = rootTransform.InverseTransformPoint(c);
                            if (!hasBounds) { b = new Bounds(rootPt, Vector3.zero); hasBounds = true; }
                            else b.Encapsulate(rootPt);
                        }
                    }
                }
            }

            return b;
        }
    }
}
