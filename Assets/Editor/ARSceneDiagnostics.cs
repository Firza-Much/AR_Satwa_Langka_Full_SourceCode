using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using SatwaLangka.Data;
using SatwaLangka.UI;
using SatwaLangka.AR;

namespace SatwaLangka.EditorScripts
{
    public static class ARSceneDiagnostics
    {
        [MenuItem("Satwa Langka/Run Full AR Diagnostics & Model Audit")]
        public static void RunAudit()
        {
            // 1. Rebuild all 12 animal prefabs first to ensure clean materials, textures, and models
            AnimalModelMasterRebuilder.RebuildAllPrefabs();

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("==================================================");
            sb.AppendLine("   FULL 3D ANIMAL AR AUDIT & DIAGNOSTIC REPORT   ");
            sb.AppendLine("==================================================");
            sb.AppendLine($"Timestamp: {DateTime.Now:yyyy-MM-dd HH:mm:ss}\n");

            string scenePath = "Assets/Scenes/ScanPlaneDetection.unity";
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

            Camera cam = Camera.main;
            sb.AppendLine("[CAMERA AUDIT]");
            sb.AppendLine($"• Main Camera Found: {(cam != null ? "YES" : "NO")}");
            if (cam != null)
            {
                sb.AppendLine($"• Position: {cam.transform.position}");
                sb.AppendLine($"• Rotation: {cam.transform.rotation.eulerAngles}");
                sb.AppendLine($"• ClearFlags: {cam.clearFlags}, BG Color: {cam.backgroundColor}");
                sb.AppendLine($"• NearClip: {cam.nearClipPlane}, FarClip: {cam.farClipPlane}");
                sb.AppendLine($"• Culling Mask: {cam.cullingMask} (Everything = -1: {(cam.cullingMask == -1)})");
                sb.AppendLine($"• Camera Count in Scene: {UnityEngine.Object.FindObjectsOfType<Camera>().Length}");
            }
            sb.AppendLine();

            string[] animalCodes = new[]
            {
                "SATWA01", "SATWA02", "SATWA03", "SATWA04",
                "SATWA05", "SATWA06", "SATWA07", "SATWA08",
                "SATWA09", "SATWA10", "SATWA11", "SATWA12"
            };

            GameObject stageParent = GameObject.Find("AR_Stage_Parent");
            if (stageParent == null)
            {
                stageParent = new GameObject("AR_Stage_Parent");
                stageParent.transform.position = new Vector3(0, 0.40f, 0);
            }

            sb.AppendLine("[ANIMAL PREFABS & BOUNDS AUDIT]");

            foreach (var code in animalCodes)
            {
                string[] guids = AssetDatabase.FindAssets(code, new[] { "Assets/Data/Animals" });
                if (guids.Length == 0)
                {
                    sb.AppendLine($"❌ {code}: AnimalDataSO not found!");
                    continue;
                }

                string soPath = AssetDatabase.GUIDToAssetPath(guids[0]);
                AnimalDataSO animalSO = AssetDatabase.LoadAssetAtPath<AnimalDataSO>(soPath);
                if (animalSO == null)
                {
                    sb.AppendLine($"❌ {code}: Could not load AnimalDataSO at {soPath}");
                    continue;
                }

                GameObject prefab = animalSO.modelPrefab;
                sb.AppendLine("--------------------------------------------------");
                sb.AppendLine($"SATWA: {animalSO.animalCode} - {animalSO.commonName} ({animalSO.latinName})");
                sb.AppendLine($"• Prefab Assigned: {(prefab != null ? prefab.name : "NULL")}");

                if (prefab == null)
                {
                    sb.AppendLine($"  ❌ FAIL: No modelPrefab assigned to {animalSO.name}");
                    continue;
                }

                // Instantiate temporary test instance
                GameObject testInst = UnityEngine.Object.Instantiate(prefab, stageParent.transform);
                testInst.name = $"Test_{code}";
                testInst.transform.localPosition = Vector3.zero;
                testInst.transform.localRotation = Quaternion.Euler(0, 35, 0);
                testInst.transform.localScale = Vector3.one;

                Renderer[] renderers = testInst.GetComponentsInChildren<Renderer>(true);
                SkinnedMeshRenderer[] skinned = testInst.GetComponentsInChildren<SkinnedMeshRenderer>(true);
                MeshFilter[] meshFilters = testInst.GetComponentsInChildren<MeshFilter>(true);

                sb.AppendLine($"• Total Renderers: {(renderers != null ? renderers.Length : 0)} (MeshRenderer: {meshFilters.Length}, SkinnedMesh: {skinned.Length})");

                if (renderers == null || renderers.Length == 0)
                {
                    sb.AppendLine("  ❌ FAIL: Zero renderers found in hierarchy!");
                    UnityEngine.Object.DestroyImmediate(testInst);
                    continue;
                }

                // Calculate Raw Bounds using r.localBounds and r.bounds
                Bounds rawBounds = new Bounds();
                bool hasRaw = false;
                foreach (var r in renderers)
                {
                    if (r != null && r.gameObject.activeInHierarchy)
                    {
                        Bounds lb = r.localBounds;
                        if (lb.size.sqrMagnitude > 0.00001f)
                        {
                            Vector3 min = lb.min;
                            Vector3 max = lb.max;
                            Vector3[] corners = new Vector3[]
                            {
                                new Vector3(min.x, min.y, min.z), new Vector3(max.x, min.y, min.z),
                                new Vector3(min.x, max.y, min.z), new Vector3(max.x, max.y, min.z),
                                new Vector3(min.x, min.y, max.z), new Vector3(max.x, min.y, max.z),
                                new Vector3(min.x, max.y, max.z), new Vector3(max.x, max.y, max.z)
                            };
                            foreach (var c in corners)
                            {
                                Vector3 pt = r.transform.TransformPoint(c);
                                if (!hasRaw) { rawBounds = new Bounds(pt, Vector3.zero); hasRaw = true; }
                                else rawBounds.Encapsulate(pt);
                            }
                        }
                        else if (r.bounds.size.sqrMagnitude > 0.00001f)
                        {
                            if (!hasRaw) { rawBounds = r.bounds; hasRaw = true; }
                            else rawBounds.Encapsulate(r.bounds);
                        }
                    }
                }

                sb.AppendLine($"• Raw Bounds: Size=({rawBounds.size.x:F3}, {rawBounds.size.y:F3}, {rawBounds.size.z:F3}), Center=({rawBounds.center.x:F3}, {rawBounds.center.y:F3}, {rawBounds.center.z:F3})");
                sb.AppendLine($"• Bounds Min/Max: Min=({rawBounds.min.x:F3}, {rawBounds.min.y:F3}, {rawBounds.min.z:F3}), Max=({rawBounds.max.x:F3}, {rawBounds.max.y:F3}, {rawBounds.max.z:F3})");

                // Check Material & Shader
                foreach (var r in renderers)
                {
                    sb.AppendLine($"  - Renderer: {r.gameObject.name} (Active={r.gameObject.activeInHierarchy}, Enabled={r.enabled}, Layer={LayerMask.LayerToName(r.gameObject.layer)})");
                    if (r.sharedMaterials != null && r.sharedMaterials.Length > 0)
                    {
                        for (int m = 0; m < r.sharedMaterials.Length; m++)
                        {
                            var mat = r.sharedMaterials[m];
                            if (mat == null)
                            {
                                sb.AppendLine($"    ❌ Material [{m}]: NULL/Missing!");
                            }
                            else
                            {
                                sb.AppendLine($"    ✓ Material [{m}]: {mat.name}, Shader: {(mat.shader != null ? mat.shader.name : "NULL")}, Color: {mat.color}");
                            }
                        }
                    }
                    else
                    {
                        sb.AppendLine("    ❌ No materials assigned to renderer!");
                    }
                }

                // Compute Proper Scale & Grounding
                float maxDim = Mathf.Max(rawBounds.size.x, rawBounds.size.y, rawBounds.size.z);
                float targetSize = 0.55f;
                if (maxDim > 0.0001f)
                {
                    float scaleFactor = targetSize / maxDim;
                    testInst.transform.localScale = Vector3.one * scaleFactor;
                    
                    // Recalculate scaled bounds
                    Bounds scaledBounds = renderers[0].bounds;
                    for (int i = 1; i < renderers.Length; i++) scaledBounds.Encapsulate(renderers[i].bounds);
                    
                    float groundOffset = stageParent.transform.position.y - scaledBounds.min.y;
                    testInst.transform.position += new Vector3(0, groundOffset, 0);

                    sb.AppendLine($"• Scaled Bounds: Size=({scaledBounds.size.x:F3}, {scaledBounds.size.y:F3}, {scaledBounds.size.z:F3}), Applied Scale={scaleFactor:F4}");
                    sb.AppendLine($"• Grounding Offset: Y += {groundOffset:F3}m (Ground Level Y={stageParent.transform.position.y:F3})");
                    sb.AppendLine("• RESULT: ✅ PASS (Model Loaded, Renderers Valid, Scaled & Grounded)");
                }
                else
                {
                    sb.AppendLine("  ❌ FAIL: Bounds size is zero (maxDim <= 0.0001)!");
                }

                UnityEngine.Object.DestroyImmediate(testInst);
            }

            sb.AppendLine("\n==================================================");
            sb.AppendLine("             END OF AUDIT REPORT                  ");
            sb.AppendLine("==================================================");

            string outPath = "Assets/Editor/AR_AUDIT_REPORT.txt";
            File.WriteAllText(outPath, sb.ToString());
            Debug.Log($"<b>[AR AUDIT]</b> Complete. Report written to {outPath}");
        }
    }
}
