using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace SatwaLangka.UI
{
    public class PanduanController : MonoBehaviour
    {
        [Header("Top Navigation")]
        [SerializeField] private Button btnKembali;

        private void Start()
        {
            // Apply safe margin for HP notch/camera
            var rt = GetComponent<UnityEngine.RectTransform>();
            if (rt == null) rt = GetComponentInChildren<UnityEngine.Canvas>()?.GetComponent<UnityEngine.RectTransform>();
            if (btnKembali != null) btnKembali.onClick.AddListener(OnKembaliClicked);
        }

        public void OnKembaliClicked()
        {
        UIAudioManager.Instance?.PlayBack();

            Debug.Log("<b>[PANDUAN]</b> Returning to Main Menu...");
            if (Application.CanStreamedLevelBeLoaded("MainMenu"))
            {
                SceneManager.LoadScene("MainMenu");
            }
        }
    }
}
