using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

namespace SatwaLangka.UI
{
    public class HasilQuizController : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private TextMeshProUGUI txtHeader;
        [SerializeField] private TextMeshProUGUI txtFinalScore;
        [SerializeField] private TextMeshProUGUI txtEvaluationMessage;
        [SerializeField] private TextMeshProUGUI txtDetailStats;

        [Header("Buttons")]
        [SerializeField] private Button btnUlang;
        [SerializeField] private Button btnMenu;

        private void Start()
        {
            if (btnUlang != null) btnUlang.onClick.AddListener(OnUlangClicked);
            if (btnMenu != null) btnMenu.onClick.AddListener(OnMenuClicked);

            DisplayQuizResults();
        }

        private void DisplayQuizResults()
        {
            int score = PlayerPrefs.GetInt("LastQuizScore", 0);
            int totalQ = PlayerPrefs.GetInt("TotalQuizQuestions", 10);
            int maxScore = totalQ * 10;
            int correctCount = score / 10;
            int wrongCount = totalQ - correctCount;

            if (txtHeader != null) txtHeader.text = "<b><color=#064E3B>Hasil Evaluasi Kuis</color></b>";
            if (txtFinalScore != null) txtFinalScore.text = $"<b><color=#059669>{score}</color></b><size=52><color=#64748B>/{maxScore}</color></size>";

            if (txtDetailStats != null)
            {
                txtDetailStats.text = $"<b><color=#064E3B>Jawaban Benar:</color></b> <color=#047857>{correctCount}</color>  |  <b><color=#064E3B>Jawaban Salah:</color></b> <color=#DC2626>{wrongCount}</color>";
            }

            if (txtEvaluationMessage != null)
            {
                if (score >= 80)
                {
                    txtEvaluationMessage.text = "<b><color=#064E3B>Luar Biasa!</color></b>\n<color=#1E293B>Pemahaman Anda tentang fauna endemik nusantara sangat baik.</color>";
                }
                else if (score >= 60)
                {
                    txtEvaluationMessage.text = "<b><color=#064E3B>Cukup Baik!</color></b>\n<color=#1E293B>Pelajari lagi informasi satwa di menu eksplorasi untuk nilai maksimal.</color>";
                }
                else
                {
                    txtEvaluationMessage.text = "<b><color=#D97706>Ayo Coba Lagi!</color></b>\n<color=#1E293B>Eksplorasi kembali model 3D dan detail satwa untuk memahami materi.</color>";
                }
            }
        }

        public void OnUlangClicked()
        {
            UIAudioManager.Instance?.PlayClick();

            Debug.Log("<b>[HASIL QUIZ]</b> Retrying Quiz...");
            if (Application.CanStreamedLevelBeLoaded("SoalQuiz"))
            {
                SceneManager.LoadScene("SoalQuiz");
            }
        }

        public void OnMenuClicked()
        {
            UIAudioManager.Instance?.PlayBack();

            Debug.Log("<b>[HASIL QUIZ]</b> Returning to Main Menu...");
            if (Application.CanStreamedLevelBeLoaded("MainMenu"))
            {
                SceneManager.LoadScene("MainMenu");
            }
        }
    }
}
