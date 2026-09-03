using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using System.IO;

/// <summary>
/// Runs BEFORE and AFTER any other build processor.
/// Cleans XR Simulation temp conflicts that survive failed builds.
/// callbackOrder = -9999 ensures this runs absolutely first.
/// </summary>
public class CleanXRTempPreBuild : IPreprocessBuildWithReport, IPostprocessBuildWithReport
{
    public int callbackOrder => -9999;

    static void CleanXRConflicts()
    {
        var dataPath = Application.dataPath;
        var tempDir = Path.Combine(dataPath, "XR", "Temp");
        if (!Directory.Exists(tempDir)) Directory.CreateDirectory(tempDir);

        string[] leftoverAssets = new[]
        {
            Path.Combine(tempDir, "XRSimulationPreferences.asset"),
            Path.Combine(tempDir, "XRSimulationPreferences.asset.meta"),
            Path.Combine(tempDir, "XRSimulationRuntimeSettings.asset"),
            Path.Combine(tempDir, "XRSimulationRuntimeSettings.asset.meta"),
        };

        int deleted = 0;
        foreach (var p in leftoverAssets)
        {
            if (File.Exists(p)) { File.Delete(p); deleted++; }
        }

        if (deleted > 0)
            Debug.Log($"[CleanXRTemp] Cleaned {deleted} conflict files");
    }

    public void OnPreprocessBuild(BuildReport report)
    {
        if (report.summary.platform != BuildTarget.Android) return;

        CleanXRConflicts();

        // Force ARM64 + IL2CPP (required for ARCore 64-bit)
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
        PlayerSettings.SetScriptingBackend(UnityEditor.Build.NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
        PlayerSettings.SetIl2CppCompilerConfiguration(UnityEditor.Build.NamedBuildTarget.Android, Il2CppCompilerConfiguration.Release);
        PlayerSettings.SetManagedStrippingLevel(UnityEditor.Build.NamedBuildTarget.Android, ManagedStrippingLevel.Minimal);

        FixPilihHewanPrefab();

        // Force reimport Resources prefab so it's included in the build
        AssetDatabase.ImportAsset("Assets/Resources/UI/Animal_Select_Card.prefab",
            ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport);

        Debug.Log($"[CleanXRTemp] Pre-build: arch={PlayerSettings.Android.targetArchitectures} IL2CPP");
    }

    static void FixPilihHewanPrefab()
    {
        var cardGuids = AssetDatabase.FindAssets("Animal_Select_Card t:Prefab", new[]{"Assets/Prefabs"});
        if (cardGuids.Length == 0) { Debug.LogWarning("[CleanXRTemp] Animal_Select_Card not found"); return; }

        var guid = cardGuids[0];
        var scenePath = "Assets/Scenes/PilihHewan.unity";

        // Direct YAML patch — most reliable, bypasses scene loading cache
        var fullPath = System.IO.Path.Combine(Application.dataPath, "..", scenePath);
        var yaml = System.IO.File.ReadAllText(fullPath);

        // Replace any existing animalCardPrefab binding (null or wrong fileID)
        var pattern = new System.Text.RegularExpressions.Regex(
            @"animalCardPrefab: \{[^\}]*\}");
        var replacement = $"animalCardPrefab: {{fileID: 1102032542425419025, guid: {guid}, type: 3}}";
        var newYaml = pattern.Replace(yaml, replacement);

        if (newYaml != yaml) {
            System.IO.File.WriteAllText(fullPath, newYaml);
            // Refresh AssetDatabase BEFORE build pipeline reads the scene
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            Debug.Log("[CleanXRTemp] Patched animalCardPrefab guid=" + guid);
        } else {
            Debug.Log("[CleanXRTemp] animalCardPrefab already set: " + guid);
        }

        // Force reimport scene so build pipeline uses updated YAML
        AssetDatabase.ImportAsset(scenePath,
            ImportAssetOptions.ForceUpdate |
            ImportAssetOptions.ForceSynchronousImport);
    }

    public void OnPostprocessBuild(BuildReport report)
    {
        // Clean up after build regardless of success/failure
        CleanXRConflicts();
    }
}
