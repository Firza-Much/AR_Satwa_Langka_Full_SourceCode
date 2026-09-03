using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SatwaLangka.UI
{
    /// <summary>
    /// Centralized Persistent Audio Manager for BGM, UI SFX, and Animal Audio.
    /// 100% 2D Audio, dynamic resource loading for Android standalone, and volume safety.
    /// </summary>
    public class UIAudioManager : MonoBehaviour
    {
        public static UIAudioManager Instance { get; private set; }

        [Header("Audio Clips")]
        [SerializeField] private AudioClip bgmClip;
        [SerializeField] private AudioClip sfxButtonClick;
        [SerializeField] private AudioClip sfxButtonBack;
        [SerializeField] private AudioClip sfxCorrect;
        [SerializeField] private AudioClip sfxWrong;

        private AudioSource _bgmSource;
        private AudioSource _sfxSource;
        private AudioSource _animalSource;

        private float _bgmVolume = 0.50f;
        private float _sfxVolume = 0.90f;
        private float _animalVolume = 1.00f;
        private bool _isDucking = false;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void AutoInitialize()
        {
            if (Instance == null)
            {
                GameObject go = new GameObject("UIAudioManager");
                go.AddComponent<UIAudioManager>();
                DontDestroyOnLoad(go);
            }
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            // DontDestroyOnLoad sudah dipanggil di AutoInitialize jika instance dibuat dari sana
            // Tapi jika dibuat dari scene, pastikan tetap persist
            if (transform.parent == null)
                DontDestroyOnLoad(gameObject);

            InitAudioSources();
            LoadAudioAssetsIfMissing();
            RefreshVolumes();

            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void Start()
        {
            PlayBGM();
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            RefreshVolumes();
            PlayBGM();
        }

        private void InitAudioSources()
        {
            if (_bgmSource == null)
            {
                _bgmSource = gameObject.AddComponent<AudioSource>();
                _bgmSource.playOnAwake = false;
                _bgmSource.loop = true;
                _bgmSource.spatialBlend = 0f; // Pure 2D Sound
                _bgmSource.priority = 128;
                _bgmSource.mute = false;
            }

            if (_sfxSource == null)
            {
                _sfxSource = gameObject.AddComponent<AudioSource>();
                _sfxSource.playOnAwake = false;
                _sfxSource.spatialBlend = 0f; // Pure 2D Sound
                _sfxSource.priority = 64;
                _sfxSource.mute = false;
            }

            if (_animalSource == null)
            {
                _animalSource = gameObject.AddComponent<AudioSource>();
                _animalSource.playOnAwake = false;
                _animalSource.spatialBlend = 0f; // Pure 2D Sound
                _animalSource.priority = 32;
                _animalSource.mute = false;
            }
        }

        private void LoadAudioAssetsIfMissing()
        {
            if (bgmClip == null)
            {
                bgmClip = Resources.Load<AudioClip>("Audio/BGM/BGM_Main");
#if UNITY_EDITOR
                if (bgmClip == null)
                    bgmClip = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/BGM/BGM_Main.mp3");
#endif
            }

            if (sfxButtonClick == null)
            {
                sfxButtonClick = Resources.Load<AudioClip>("Audio/SFX/UI_Click_Soft");
#if UNITY_EDITOR
                if (sfxButtonClick == null)
                    sfxButtonClick = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/UI_Click_Soft.wav");
#endif
            }

            if (sfxButtonBack == null)
            {
                sfxButtonBack = Resources.Load<AudioClip>("Audio/SFX/UI_Back");
#if UNITY_EDITOR
                if (sfxButtonBack == null)
                    sfxButtonBack = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/UI_Back.wav");
#endif
            }

            if (sfxCorrect == null)
            {
                sfxCorrect = Resources.Load<AudioClip>("Audio/SFX/UI_Correct_Chime");
#if UNITY_EDITOR
                if (sfxCorrect == null)
                    sfxCorrect = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/UI_Correct_Chime.wav");
#endif
            }

            if (sfxWrong == null)
            {
                sfxWrong = Resources.Load<AudioClip>("Audio/SFX/UI_Wrong_Buzzer");
#if UNITY_EDITOR
                if (sfxWrong == null)
                    sfxWrong = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/UI_Wrong_Buzzer.wav");
#endif
            }
        }

        public void RefreshVolumes()
        {
            // Default volumes ensure audio is audible on fresh start
            _bgmVolume = PlayerPrefs.GetFloat("BGMVolume", 0.50f);
            _sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 0.90f);
            _animalVolume = PlayerPrefs.GetFloat("AnimalVolume", 1.00f);

            // Safety floor so user doesn't accidentally get trapped in silent mode
            if (_bgmVolume < 0.05f && !PlayerPrefs.HasKey("BGMVolume")) _bgmVolume = 0.50f;
            if (_sfxVolume < 0.05f && !PlayerPrefs.HasKey("SFXVolume")) _sfxVolume = 0.90f;

            if (_bgmSource != null && !_isDucking)
            {
                _bgmSource.volume = _bgmVolume;
            }
        }

        public void PlayBGM()
        {
            if (bgmClip == null) LoadAudioAssetsIfMissing();
            if (bgmClip != null && _bgmSource != null)
            {
                if (!_bgmSource.isPlaying || _bgmSource.clip != bgmClip)
                {
                    _bgmSource.clip = bgmClip;
                    _bgmSource.volume = _bgmVolume;
                    _bgmSource.Play();
                }
            }
        }

        public void StopBGM()
        {
            if (_bgmSource != null && _bgmSource.isPlaying)
            {
                _bgmSource.Stop();
            }
        }

        public void SetBGMVolume(float vol)
        {
            _bgmVolume = Mathf.Clamp01(vol);
            PlayerPrefs.SetFloat("BGMVolume", _bgmVolume);
            PlayerPrefs.SetFloat("VolumeBGM", _bgmVolume);
            PlayerPrefs.Save();
            if (!_isDucking && _bgmSource != null)
            {
                _bgmSource.volume = _bgmVolume;
            }
        }

        public void SetSFXVolume(float vol)
        {
            _sfxVolume = Mathf.Clamp01(vol);
            PlayerPrefs.SetFloat("SFXVolume", _sfxVolume);
            PlayerPrefs.SetFloat("VolumeSFX", _sfxVolume);
            PlayerPrefs.Save();
        }

        public void PlayClick()
        {
            if (sfxButtonClick == null) LoadAudioAssetsIfMissing();
            PlaySFX(sfxButtonClick);
        }

        public void PlayBack()
        {
            if (sfxButtonBack == null) LoadAudioAssetsIfMissing();
            PlaySFX(sfxButtonBack != null ? sfxButtonBack : sfxButtonClick);
        }

        public void PlayCorrect()
        {
            if (sfxCorrect == null) LoadAudioAssetsIfMissing();
            PlaySFX(sfxCorrect != null ? sfxCorrect : sfxButtonClick);
        }

        public void PlayWrong()
        {
            if (sfxWrong == null) LoadAudioAssetsIfMissing();
            PlaySFX(sfxWrong != null ? sfxWrong : sfxButtonClick);
        }

        public void PlayAnimalSound(AudioClip clip)
        {
            if (clip == null) return;
            RefreshVolumes();

            if (_animalSource != null)
            {
                _animalSource.volume = _animalVolume;
                _animalSource.PlayOneShot(clip, _animalVolume);
                StopAllCoroutines();
                StartCoroutine(DuckBGMCoroutine(clip.length + 0.5f));
            }
        }

        public void PlayAnimalVoiceover(AudioClip clip) => PlayAnimalSound(clip);

        private IEnumerator DuckBGMCoroutine(float duration)
        {
            _isDucking = true;
            if (_bgmSource != null) _bgmSource.volume = _bgmVolume * 0.20f; // Duck to 20%

            yield return new WaitForSeconds(duration);

            _isDucking = false;
            if (_bgmSource != null) _bgmSource.volume = _bgmVolume;
        }

        private void PlaySFX(AudioClip clip)
        {
            if (clip == null) return;
            RefreshVolumes();
            if (_sfxSource != null)
            {
                _sfxSource.PlayOneShot(clip, _sfxVolume);
            }
        }

        public static void ButtonClick() => Instance?.PlayClick();
        public static void ButtonBack()  => Instance?.PlayBack();
    }
}
