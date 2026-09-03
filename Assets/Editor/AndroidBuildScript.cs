using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace SatwaLangka.EditorScripts
{
    public class AndroidBuildScript
    {
        [MenuItem("Build/Build Android APK")]
        public static void BuildAndroidAPK()
        {
            string buildDir = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "Builds", "Android"));
            if (!Directory.Exists(buildDir)) Directory.CreateDirectory(buildDir);

            string apkPath = Path.Combine(buildDir, "AR_Satwa_Langka.apk");
            string statusPath = Path.Combine(buildDir, "build_status.txt");

            string[] scenePaths = new[]
            {
                "Assets/Scenes/MainMenu.unity",
                "Assets/Scenes/PilihHewan.unity",
                "Assets/Scenes/ScanPlaneDetection.unity",
                "Assets/Scenes/DetailSatwa.unity",
                "Assets/Scenes/SoalQuiz.unity",
                "Assets/Scenes/HasilQuiz.unity",
                "Assets/Scenes/Panduan.unity",
                "Assets/Scenes/Pengaturan.unity"
            };

            // ponytail: 6000.5.9f1 IL2CPP Development is the only available variant for 6000.5; Release not installed. Mono variant not installed so IL2CPP required. ARM64 only for emulator/device compat (x64 not needed).
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
            PlayerSettings.SetScriptingBackend(UnityEditor.Build.NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.SetIl2CppCompilerConfiguration(UnityEditor.Build.NamedBuildTarget.Android, Il2CppCompilerConfiguration.Debug);
            PlayerSettings.SetManagedStrippingLevel(UnityEditor.Build.NamedBuildTarget.Android, ManagedStrippingLevel.Disabled);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            PlayerSettings.stripEngineCode = false;

            EditorUserBuildSettings.buildAppBundle = false;
            EditorUserBuildSettings.androidBuildSubtarget = MobileTextureSubtarget.Generic;
            EditorUserBuildSettings.development = true;
            EditorUserBuildSettings.allowDebugging = true;

            File.WriteAllText(statusPath, $"BUILD_IN_PROGRESS: Started at {System.DateTime.Now}\nBACKEND=IL2CPP ARCH=ARM64 stripEngineCode=False managedStripping=Disabled\n");

            BuildPlayerOptions opts = new BuildPlayerOptions
            {
                scenes = scenePaths,
                locationPathName = apkPath,
                targetGroup = BuildTargetGroup.Android,
                target = BuildTarget.Android,
                options = BuildOptions.Development | BuildOptions.AllowDebugging | BuildOptions.CompressWithLz4
            };

            BuildReport report = BuildPipeline.BuildPlayer(opts);

            string result = $"BUILD_RESULT: {report.summary.result}\nTOTAL_ERRORS: {report.summary.totalErrors}\nTOTAL_WARNINGS: {report.summary.totalWarnings}\nTOTAL_SIZE_MB: {report.summary.totalSize / (1024.0 * 1024.0):F2}\nBUILD_TIME_SEC: {report.summary.totalTime.TotalSeconds:F2}\nAPK_EXISTS: {File.Exists(apkPath)}\n";
            if (File.Exists(apkPath))
            {
                var fi = new FileInfo(apkPath);
                result += $"APK_FULL_PATH: {fi.FullName}\nAPK_SIZE_MB: {fi.Length / (1024.0 * 1024.0):F2}\n";
            }
            foreach (var step in report.steps)
                foreach (var msg in step.messages)
                    if (msg.type == LogType.Error || msg.type == LogType.Exception)
                    {
                        if (msg.content.Contains("Render Graph") || msg.content.Contains("NativePassCompiler") || msg.content.Contains("TaskCanceled")) continue;
                        result += $"[{msg.type}] {msg.content}\n";
                    }

            File.WriteAllText(statusPath, result);
            Debug.Log($"[AndroidBuildScript] Finished: {report.summary.result} -> {apkPath}");
        }
    }
}
