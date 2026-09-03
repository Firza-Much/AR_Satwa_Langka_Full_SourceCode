using UnityEditor;
using UnityEngine;
using System.IO;

public static class BatchBuild
{
    [MenuItem("Build/Build Android APK")]
    public static void BuildAndroid()
    {
        // Clean XR conflicts
        var dataPath = Application.dataPath;
        string[] conflictPaths = new[] {
            Path.Combine(dataPath, "XR", "Temp"),
            Path.Combine(dataPath, "XR", "UserSimulationSettings", "Resources", "XRSimulationPreferences.asset"),
            Path.Combine(dataPath, "XR", "Resources", "XRSimulationRuntimeSettings.asset"),
        };
        foreach (var p in conflictPaths) {
            if (Directory.Exists(p)) Directory.Delete(p, true);
            else if (File.Exists(p)) File.Delete(p);
        }

        // Configure Player Settings for stable Android Build
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
        PlayerSettings.SetScriptingBackend(UnityEditor.Build.NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
        PlayerSettings.SetIl2CppCompilerConfiguration(UnityEditor.Build.NamedBuildTarget.Android, Il2CppCompilerConfiguration.Debug);
        PlayerSettings.SetManagedStrippingLevel(UnityEditor.Build.NamedBuildTarget.Android, ManagedStrippingLevel.Disabled);
        PlayerSettings.stripEngineCode = false;

        string outputDir = Path.Combine(Directory.GetCurrentDirectory(), "Builds", "Android");
        if (!Directory.Exists(outputDir)) Directory.CreateDirectory(outputDir);
        string outputApk = Path.Combine(outputDir, "AR_Satwa_Langka_FINAL.apk");
        if (File.Exists(outputApk)) File.Delete(outputApk);

        var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions {
            scenes = new[] {
                "Assets/Scenes/MainMenu.unity",
                "Assets/Scenes/PilihHewan.unity",
                "Assets/Scenes/ScanPlaneDetection.unity",
                "Assets/Scenes/DetailSatwa.unity",
                "Assets/Scenes/SoalQuiz.unity",
                "Assets/Scenes/HasilQuiz.unity",
                "Assets/Scenes/Panduan.unity",
                "Assets/Scenes/Pengaturan.unity"
            },
            locationPathName = outputApk,
            target = BuildTarget.Android,
            options = BuildOptions.CompressWithLz4 | BuildOptions.Development
        });

        string statusPath = Path.Combine(outputDir, "build_status.txt");
        string result = $"BUILD_RESULT: {report.summary.result}\nTOTAL_ERRORS: {report.summary.totalErrors}\nSIZE_MB: {report.summary.totalSize / (1024.0 * 1024.0):F2}\nAPK_EXISTS: {File.Exists(outputApk)}\n";
        File.WriteAllText(statusPath, result);

        if (report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
            Debug.Log($"[BatchBuild] SUCCESS: {report.summary.outputPath} ({report.summary.totalSize/1024/1024}MB)");
        else
            Debug.LogError($"[BatchBuild] FAILED: {report.summary.totalErrors} errors");
    }
}
