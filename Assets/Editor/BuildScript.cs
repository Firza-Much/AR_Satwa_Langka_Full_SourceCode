using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class BuildScript
{
    public static void BuildAndroid()
    {
        string[] scenes = {
            "Assets/Scenes/MainMenu.unity",
            "Assets/Scenes/PilihHewan.unity",
            "Assets/Scenes/ScanPlaneDetection.unity",
            "Assets/Scenes/DetailSatwa.unity",
            "Assets/Scenes/SoalQuiz.unity",
            "Assets/Scenes/HasilQuiz.unity",
            "Assets/Scenes/Panduan.unity",
            "Assets/Scenes/Pengaturan.unity"
        };

        var options = new BuildPlayerOptions
        {
            scenes = scenes,
            target = BuildTarget.Android,
            locationPathName = "C:/Users/Dragon/ar/Builds/Android/AR_Satwa_Langka_FINAL.apk",
            options = BuildOptions.None
        };

        BuildReport report = BuildPipeline.BuildPlayer(options);
        BuildSummary summary = report.summary;
        if (summary.result == BuildResult.Succeeded)
        {
            Debug.Log($"[BuildScript] BUILD SUCCESS: {summary.totalSize / 1024 / 1024} MB");
        }
        else
        {
            Debug.LogError($"[BuildScript] BUILD FAILED: {summary.result}");
            EditorApplication.Exit(1);
        }
    }
}
