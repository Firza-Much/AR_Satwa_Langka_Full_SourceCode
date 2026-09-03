using UnityEditor;
using UnityEngine;
using System.IO;

namespace SatwaLangka.EditorScripts
{
    [InitializeOnLoad]
    public static class AutoBuildRunner
    {
        private static bool isBuilding = false;

        static AutoBuildRunner()
        {
            EditorApplication.update += CheckBuildFlag;
        }

        [InitializeOnLoadMethod]
        private static void OnDomainReload()
        {
            CheckBuildFlag();
        }

        public static void CheckBuildFlag()
        {
            if (isBuilding) return;

            string flagFile = Path.Combine(Application.dataPath, "..", "Builds", "Android", "trigger_build.flag");
            if (File.Exists(flagFile))
            {
                isBuilding = true;
                try { File.Delete(flagFile); } catch { }
                Debug.Log("[AutoBuildRunner] Trigger flag detected! Executing BuildAndroidAPK()...");
                try
                {
                    AndroidBuildScript.BuildAndroidAPK();
                }
                finally
                {
                    isBuilding = false;
                }
            }
        }
    }
}
