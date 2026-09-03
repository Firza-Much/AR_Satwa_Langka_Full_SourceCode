using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

namespace SatwaLangka.UI
{
    public class MainMenuController : MonoBehaviour
    {
        [Header("Main Navigation Buttons")]
        [SerializeField] private Button btnMulai;
        [SerializeField] private Button btnQuiz;
        [SerializeField] private Button btnPanduan;
        [SerializeField] private Button btnTentang;
        [SerializeField] private Button btnKeluar;
        [SerializeField] private Button btnPengaturan;

        [Header("Popup Modals")]
        [SerializeField] private GameObject modalOverlay;
        [SerializeField] private GameObject panelPengaturan;
        [SerializeField] private GameObject panelPanduan;
        [SerializeField] private GameObject panelTentang;
        [SerializeField] private GameObject panelKeluar;

        [Header("Pengaturan UI")]
        [SerializeField] private Slider sliderBGM;
        [SerializeField] private Slider sliderSFX;
        [SerializeField] private Toggle toggleSound;
        [SerializeField] private Button btnClosePengaturan;

        [Header("Modal Close Buttons")]
        [SerializeField] private Button btnClosePanduan;
        [SerializeField] private Button btnCloseTentang;
        [SerializeField] private Button btnConfirmKeluar;
        [SerializeField] private Button btnCancelKeluar;

        private void Start()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            if (!UnityEngine.Android.Permission.HasUserAuthorizedPermission(UnityEngine.Android.Permission.Camera))
            {
                UnityEngine.Android.Permission.RequestUserPermission(UnityEngine.Android.Permission.Camera);
            }
#endif
            SetupNavigationListeners();
            SetupModalListeners();
            CloseAllModals(false); // Silent close saat Start, tidak perlu putar suara
        }

        private void SetupNavigationListeners()
        {
            if (btnMulai != null) btnMulai.onClick.AddListener(OnMulaiClicked);
            if (btnQuiz != null) btnQuiz.onClick.AddListener(OnQuizClicked);
            if (btnPanduan != null) btnPanduan.onClick.AddListener(OnPanduanClicked);
            if (btnTentang != null) btnTentang.onClick.AddListener(OnTentangClicked);
            if (btnKeluar != null) btnKeluar.onClick.AddListener(OnKeluarClicked);
            if (btnPengaturan != null) btnPengaturan.onClick.AddListener(OnPengaturanClicked);
        }

        private void SetupModalListeners()
        {
            if (btnClosePengaturan != null) btnClosePengaturan.onClick.AddListener(() => CloseAllModals());
            if (btnClosePanduan != null) btnClosePanduan.onClick.AddListener(() => CloseAllModals());
            if (btnCloseTentang != null) btnCloseTentang.onClick.AddListener(() => CloseAllModals());
            if (btnCancelKeluar != null) btnCancelKeluar.onClick.AddListener(() => CloseAllModals());
            if (btnConfirmKeluar != null) btnConfirmKeluar.onClick.AddListener(ExecuteQuit);

            if (sliderBGM != null)
            {
                sliderBGM.value = PlayerPrefs.GetFloat("BGMVolume", PlayerPrefs.GetFloat("VolumeBGM", 0.35f));
                sliderBGM.onValueChanged.AddListener(v => {
                    PlayerPrefs.SetFloat("BGMVolume", v);
                    PlayerPrefs.SetFloat("VolumeBGM", v);
                    UIAudioManager.Instance?.SetBGMVolume(v);
                });
            }
            if (sliderSFX != null)
            {
                sliderSFX.value = PlayerPrefs.GetFloat("SFXVolume", PlayerPrefs.GetFloat("VolumeSFX", 0.80f));
                sliderSFX.onValueChanged.AddListener(v => {
                    PlayerPrefs.SetFloat("SFXVolume", v);
                    PlayerPrefs.SetFloat("VolumeSFX", v);
                    UIAudioManager.Instance?.SetSFXVolume(v);
                });
            }
        }

        public void OnMulaiClicked()
        {
            UIAudioManager.Instance?.PlayClick();

            Debug.Log("<b>[MAIN MENU]</b> Navigating to Pilih Hewan...");
            if (Application.CanStreamedLevelBeLoaded("PilihHewan"))
            {
                SceneManager.LoadScene("PilihHewan");
            }
            else if (Application.CanStreamedLevelBeLoaded("SampleScene"))
            {
                SceneManager.LoadScene("SampleScene");
            }
        }

        public void OnQuizClicked()
        {
            UIAudioManager.Instance?.PlayClick();

            Debug.Log("<b>[MAIN MENU]</b> Opening Quiz Mode...");
            if (Application.CanStreamedLevelBeLoaded("SoalQuiz"))
            {
                SceneManager.LoadScene("SoalQuiz");
            }
            else
            {
                ShowModal(panelPanduan);
            }
        }

        public void OnPanduanClicked()
        {
            UIAudioManager.Instance?.PlayClick();

            Debug.Log("<b>[MAIN MENU]</b> Opening Panduan...");
            if (Application.CanStreamedLevelBeLoaded("Panduan"))
            {
                SceneManager.LoadScene("Panduan");
            }
            else
            {
                ShowModal(panelPanduan);
            }
        }

        public void OnPengaturanClicked()
        {
            UIAudioManager.Instance?.PlayClick();

            Debug.Log("<b>[MAIN MENU]</b> Opening Pengaturan...");
            if (Application.CanStreamedLevelBeLoaded("Pengaturan"))
            {
                SceneManager.LoadScene("Pengaturan");
            }
            else
            {
                ShowModal(panelPengaturan);
            }
        }

        public void OnTentangClicked()
        {
            UIAudioManager.Instance?.PlayClick();
            ShowModal(panelTentang);
        }

        public void OnKeluarClicked()
        {
            UIAudioManager.Instance?.PlayClick();
            ShowModal(panelKeluar);
        }

        private void ShowModal(GameObject targetModal)
        {
            CloseAllModals();
            if (modalOverlay != null) modalOverlay.SetActive(true);
            if (targetModal != null)
            {
                targetModal.transform.localScale = Vector3.one; // Strictly (1, 1, 1) scale!
                targetModal.SetActive(true);
            }
        }

        public void CloseAllModals(bool playSound = true)
        {
            if (playSound) UIAudioManager.Instance?.PlayBack();
            if (modalOverlay != null) modalOverlay.SetActive(false);
            if (panelPengaturan != null) panelPengaturan.SetActive(false);
            if (panelPanduan != null) panelPanduan.SetActive(false);
            if (panelTentang != null) panelTentang.SetActive(false);
            if (panelKeluar != null) panelKeluar.SetActive(false);
        }

        private void ExecuteQuit()
        {
            Debug.Log("<b>[MAIN MENU]</b> Exiting Application...");
            Application.Quit();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
    }
}
