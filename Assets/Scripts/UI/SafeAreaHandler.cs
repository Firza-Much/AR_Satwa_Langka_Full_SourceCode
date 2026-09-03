using UnityEngine;

namespace SatwaLangka.UI
{
    /// <summary>
    /// Auto-applies safe area padding to the root Canvas RectTransform.
    /// Attach to the root Canvas GameObject in every scene.
    /// Also applies a minimum top/bottom margin to avoid camera notch overlap.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class SafeAreaHandler : MonoBehaviour
    {
        [Header("Extra Margin (px at 1080x1920)")]
        [SerializeField] private float extraTopMargin = 60f;
        [SerializeField] private float extraBottomMargin = 40f;

        private RectTransform rectTransform;
        private Rect lastSafeArea = Rect.zero;
        private Vector2Int lastScreenSize = Vector2Int.zero;
        private ScreenOrientation lastOrientation = ScreenOrientation.AutoRotation;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            ApplySafeArea();
        }

        private void Update()
        {
            if (Screen.safeArea != lastSafeArea ||
                new Vector2Int(Screen.width, Screen.height) != lastScreenSize ||
                Screen.orientation != lastOrientation)
            {
                ApplySafeArea();
            }
        }

        private void ApplySafeArea()
        {
            lastSafeArea = Screen.safeArea;
            lastScreenSize = new Vector2Int(Screen.width, Screen.height);
            lastOrientation = Screen.orientation;

            Rect safe = Screen.safeArea;

            // Apply extra margin on top and bottom
            float scaleY = Screen.height / 1920f;
            safe.y += extraBottomMargin * scaleY;
            safe.height -= (extraTopMargin + extraBottomMargin) * scaleY;

            Vector2 anchorMin = safe.position;
            Vector2 anchorMax = safe.position + safe.size;

            anchorMin.x /= Screen.width;
            anchorMin.y /= Screen.height;
            anchorMax.x /= Screen.width;
            anchorMax.y /= Screen.height;

            // Clamp to valid range
            anchorMin.x = Mathf.Clamp01(anchorMin.x);
            anchorMin.y = Mathf.Clamp01(anchorMin.y);
            anchorMax.x = Mathf.Clamp01(anchorMax.x);
            anchorMax.y = Mathf.Clamp01(anchorMax.y);

            rectTransform.anchorMin = anchorMin;
            rectTransform.anchorMax = anchorMax;
        }
    }
}
