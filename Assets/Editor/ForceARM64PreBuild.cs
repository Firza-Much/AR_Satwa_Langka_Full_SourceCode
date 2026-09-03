using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

/// <summary>
/// Force-sets ARM64 + IL2CPP before build.
/// ARCore requires ARM64 + IL2CPP - Mono/ARMv7 causes SIGSEGV crash.
/// callbackOrder = -1000 ensures this runs before other preprocessors.
/// </summary>
public class ForceARM64PreBuild : IPreprocessBuildWithReport
{
    public int callbackOrder => -1000;

    public void OnPreprocessBuild(BuildReport report)
    {
        if (report.summary.platform != BuildTarget.Android) return;

        // Force ARM64 ONLY (not ARMv7) - ARCore requires ARM64
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;

        // Force IL2CPP - ARCore requires IL2CPP, not Mono
        PlayerSettings.SetScriptingBackend(UnityEditor.Build.NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
        PlayerSettings.SetIl2CppCompilerConfiguration(UnityEditor.Build.NamedBuildTarget.Android, Il2CppCompilerConfiguration.Debug);
        PlayerSettings.SetManagedStrippingLevel(UnityEditor.Build.NamedBuildTarget.Android, ManagedStrippingLevel.Disabled);
        PlayerSettings.stripEngineCode = false;

        AssetDatabase.SaveAssets();
        Debug.Log($"[ForceARM64] arch={PlayerSettings.Android.targetArchitectures} backend=IL2CPP (Release, OptimizeSize)");
    }
}
