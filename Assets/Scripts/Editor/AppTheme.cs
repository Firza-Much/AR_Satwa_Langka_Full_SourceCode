using UnityEngine;
using UnityEditor;
using TMPro;

namespace SatwaLangka.EditorScripts
{
    public static class AppTheme
    {
        private static TMP_FontAsset _font;

        public static TMP_FontAsset Font
        {
            get
            {
                if (_font == null)
                {
                    _font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/PlusJakartaSans_SDF_UltraCrisp.asset");
                }
                return _font;
            }
        }
    }
}
