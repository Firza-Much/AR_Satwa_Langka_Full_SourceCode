using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using System.IO;
using UnityEngine;

/// <summary>
/// Temporarily renames ARCore test assets before build to prevent
/// arcoreimg build processor from failing on test-only XRReferenceImageLibrary.
/// </summary>
public class ExcludeARCoreTestAssets : IPreprocessBuildWithReport, IPostprocessBuildWithReport
{
    public int callbackOrder => -100; // run early

    private static readonly string[] TestAssetPaths = new[]
    {
        "Library/PackageCache/com.unity.xr.arcore@faa8081af93f/Tests/Editor/Assets/TestReferenceImageLibrary.asset",
        "Library/PackageCache/com.unity.xr.arcore@faa8081af93f/Tests/Editor/Assets/TestReferenceImageLibrary.asset.meta",
    };

    public void OnPreprocessBuild(BuildReport report)
    {
        foreach (var rel in TestAssetPaths)
        {
            var full = Path.Combine(Application.dataPath, "..", rel).Replace('/', Path.DirectorySeparatorChar);
            var renamed = full + ".disabled";
            if (File.Exists(full) && !File.Exists(renamed))
            {
                File.Move(full, renamed);
                Debug.Log($"[ExcludeTestAssets] Disabled: {rel}");
            }
        }
    }

    public void OnPostprocessBuild(BuildReport report)
    {
        // Restore after build
        foreach (var rel in TestAssetPaths)
        {
            var full = Path.Combine(Application.dataPath, "..", rel).Replace('/', Path.DirectorySeparatorChar);
            var renamed = full + ".disabled";
            if (File.Exists(renamed) && !File.Exists(full))
            {
                File.Move(renamed, full);
                Debug.Log($"[ExcludeTestAssets] Restored: {rel}");
            }
        }
    }
}
