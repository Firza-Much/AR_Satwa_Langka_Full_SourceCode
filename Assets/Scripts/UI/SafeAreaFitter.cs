using UnityEngine;
using UnityEngine.UI;

namespace SatwaLangka.UI
{
    /// <summary>
    /// Global Multi-Screen Responsive Safe Area Fitter.
    /// Automatically handles all Android aspect ratios (16:9, 18:9, 19.5:9, 20:9, 21:9),
    /// punch-hole cameras, waterdrop notches, status bars, and navigation gesture bars.
    /// </summary>
    [ExecuteAlways]
    [RequireComponent(typeof(RectTransform))]
    public class SafeAreaFitter : MonoBehaviour
    {
        private RectTransform _rt;
        private Rect _lastSafeArea = Rect.zero;
        private Vector2 _lastScreenSize = Vector2.zero;

        [Header("Canvas Space Insets")]
        [SerializeField] private float minTopInsetCanvas = 50f;   // Safe breathing room below camera punch-hole
        [SerializeField] private float minBottomInsetCanvas = 25f; // Safe breathing room above Android nav bar

        private void Awake()
        {
            _rt = GetComponent<RectTransform>();
            Apply();
        }

        private void OnEnable()
        {
            Apply();
        }

        private void Update()
        {
            if (NeedsUpdate())
            {
                Apply();
            }
        }

        private bool NeedsUpdate()
        {
            return Screen.safeArea != _lastSafeArea
                || new Vector2(Screen.width, Screen.height) != _lastScreenSize;
        }

        public void Apply()
        {
            if (_rt == null) _rt = GetComponent<RectTransform>();
            if (_rt == null) return;

            _lastSafeArea = Screen.safeArea;
            _lastScreenSize = new Vector2(Screen.width, Screen.height);

            var sa = Screen.safeArea;
            float sw = Screen.width;
            float sh = Screen.height;

            if (sw <= 0 || sh <= 0) return;

            // 1. Calculate hardware safe area ratios
            float minX = sa.x / sw;
            float minY = sa.y / sh;
            float maxX = (sa.x + sa.width) / sw;
            float maxY = (sa.y + sa.height) / sh;

            // 2. Guarantee top & bottom clearance for all phone form factors
            float topOffsetRatio = minTopInsetCanvas / 1920f;
            float bottomOffsetRatio = minBottomInsetCanvas / 1920f;

            minY = Mathf.Max(minY, bottomOffsetRatio);
            maxY = Mathf.Min(maxY, 1f - topOffsetRatio);

            _rt.anchorMin = new Vector2(minX, minY);
            _rt.anchorMax = new Vector2(maxX, maxY);
            _rt.offsetMin = Vector2.zero;
            _rt.offsetMax = Vector2.zero;
        }
    }
}
