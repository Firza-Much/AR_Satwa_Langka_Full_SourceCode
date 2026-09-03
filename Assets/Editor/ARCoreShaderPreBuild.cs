using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using System.IO;

/// <summary>
/// Pre-build: ensure ARCore background shader is in Always Included Shaders
/// so BuildHelper.FindShaderOrFailBuild() can find it.
/// callbackOrder = -200 so this runs before ARCore's build processor.
/// </summary>
public class ARCoreShaderPreBuild : IPreprocessBuildWithReport
{
    public int callbackOrder => -200;

    public void OnPreprocessBuild(BuildReport report)
    {
        if (report.summary.platform != BuildTarget.Android)
            return;

        EnsureShaderIncluded("Unlit/ARCoreBackground");
        EnsureShaderIncluded("Unlit/ARCoreBackgroundAfterOpaques");
        Debug.Log("[ARCoreShaderPreBuild] Shader include check complete.");
    }

    static void EnsureShaderIncluded(string shaderName)
    {
        // Try find shader
        var shader = Shader.Find(shaderName);

        // If not found, try loading stub from Assets
        if (shader == null)
        {
            string stubName = shaderName.Replace("/", "_").Replace(" ", "_");
            string[] guids = AssetDatabase.FindAssets($"{stubName} t:Shader");
            foreach (var g in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(g);
                shader = AssetDatabase.LoadAssetAtPath<Shader>(path);
                if (shader != null) break;
            }
        }

        // If still not found, try package path
        if (shader == null)
        {
            string[] pkgPaths = new[]
            {
                "Packages/com.unity.xr.arcore/Assets/Shaders/ARCoreBackground.shader",
                "Packages/com.unity.xr.arcore/Assets/Shaders/ARCoreBackgroundAfterOpaques.shader",
            };
            foreach (var pp in pkgPaths)
            {
                shader = AssetDatabase.LoadAssetAtPath<Shader>(pp);
                if (shader != null) break;
            }
        }

        if (shader == null)
        {
            Debug.LogWarning($"[ARCoreShaderPreBuild] Could not find shader '{shaderName}' — skipping.");
            return;
        }

        // Add to GraphicsSettings AlwaysIncluded
        var gsObj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>("ProjectSettings/GraphicsSettings.asset");
        var so = new SerializedObject(gsObj);
        var arr = so.FindProperty("m_AlwaysIncludedShaders");

        for (int i = 0; i < arr.arraySize; i++)
            if (arr.GetArrayElementAtIndex(i).objectReferenceValue == shader) return;

        arr.arraySize++;
        arr.GetArrayElementAtIndex(arr.arraySize - 1).objectReferenceValue = shader;
        so.ApplyModifiedPropertiesWithoutUndo();
        AssetDatabase.SaveAssets();
        Debug.Log($"[ARCoreShaderPreBuild] Added '{shader.name}' to Always Included Shaders.");
    }
}
