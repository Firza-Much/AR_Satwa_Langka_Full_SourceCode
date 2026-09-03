using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using SatwaLangka.Data;

namespace SatwaLangka.UI
{
    public class PengaturanController : MonoBehaviour
    {
        [Header("Top Navigation")]
        [SerializeField] private Button btnKembali;

        [Header("BGM Volume Settings")]
        [SerializeField] private Slider sliderBGM;
        [SerializeField] private TextMeshProUGUI txtBGMPercent;

        [Header("SFX Volume Settings")]
        [SerializeField] private Slider sliderSFX;
        [SerializeField] private TextMeshProUGUI txtSFXPercent;
        [SerializeField] private Button btnTestSFX;
        [SerializeField] private TextMeshProUGUI txtTestBtn;
        [SerializeField] private AudioClip testAudioClip;

        private AudioSource audioSource;

        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }

        private void Start()
        {
            if (btnKembali != null) btnKembali.onClick.AddListener(OnKembaliClicked);

            float bgmVol = PlayerPrefs.GetFloat("BGMVolume", 0.35f);
            float sfxVol = PlayerPrefs.GetFloat("SFXVolume", 0.80f);

            if (sliderBGM != null)
            {
                sliderBGM.value = bgmVol;
                sliderBGM.onValueChanged.AddListener(OnBGMChanged);
                UpdateBGMText(bgmVol);
            }

            if (sliderSFX != null)
            {
                sliderSFX.value = sfxVol;
                sliderSFX.onValueChanged.AddListener(OnSFXChanged);
                UpdateSFXText(sfxVol);
            }

            if (btnTestSFX != null) btnTestSFX.onClick.AddListener(OnTestSFXClicked);

#if UNITY_EDITOR
            if (testAudioClip == null)
            {
                testAudioClip = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/SFX_SATWA01_Gajah.mp3");
            }
#endif
        }

        public void OnBGMChanged(float val)
        {
            PlayerPrefs.SetFloat("BGMVolume", val);
            PlayerPrefs.SetFloat("VolumeBGM", val);
            PlayerPrefs.Save();
            UpdateBGMText(val);
            UIAudioManager.Instance?.SetBGMVolume(val);
        }

        public void OnSFXChanged(float val)
        {
            PlayerPrefs.SetFloat("SFXVolume", val);
            PlayerPrefs.SetFloat("VolumeSFX", val);
            PlayerPrefs.Save();
            UpdateSFXText(val);
            UIAudioManager.Instance?.SetSFXVolume(val);
            if (audioSource != null) audioSource.volume = val;
        }

        private void UpdateBGMText(float val)
        {
            if (txtBGMPercent != null) txtBGMPercent.text = $"<b>{Mathf.RoundToInt(val * 100)}%</b>";
        }

        private void UpdateSFXText(float val)
        {
            if (txtSFXPercent != null) txtSFXPercent.text = $"<b>{Mathf.RoundToInt(val * 100)}%</b>";
        }

        public void OnTestSFXClicked()
        {
            if (testAudioClip != null)
            {
                UIAudioManager.Instance?.PlayAnimalSound(testAudioClip);
                StartCoroutine(AnimateTestButton());
            }
            else
            {
                UIAudioManager.Instance?.PlayClick();
            }
        }

        private IEnumerator AnimateTestButton()
        {
            if (txtTestBtn != null)
            {
                string originalText = txtTestBtn.text;
                txtTestBtn.text = "<b><color=#059669>▶ Memutar Suara Satwa...</color></b>";
                yield return new WaitForSeconds(2.0f);
                txtTestBtn.text = originalText;
            }
        }

        public void OnKembaliClicked()
        {
            UIAudioManager.Instance?.PlayBack();
            Debug.Log("<b>[PENGATURAN]</b> Returning to MainMenu...");
            if (Application.CanStreamedLevelBeLoaded("MainMenu"))
            {
                SceneManager.LoadScene("MainMenu");
            }
        }
    }
}
