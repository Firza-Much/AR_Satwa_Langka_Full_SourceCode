using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using SatwaLangka.Data;

namespace SatwaLangka.UI
{
    public class QuizController : MonoBehaviour
    {
        [Header("Top Navigation & Status")]
        [SerializeField] private Button btnKembali;
        [SerializeField] private Button btnNext;
        [SerializeField] private TextMeshProUGUI txtHeader;
        [SerializeField] private TextMeshProUGUI txtProgress;
        [SerializeField] private TextMeshProUGUI txtScore;

        [Header("Question UI")]
        [SerializeField] private Image imgQuestion;
        [SerializeField] private TextMeshProUGUI txtQuestion;

        [Header("Option Buttons")]
        [SerializeField] private Button[] optionButtons = new Button[4];
        [SerializeField] private TextMeshProUGUI[] optionTexts = new TextMeshProUGUI[4];
        [SerializeField] private Image[] optionBackgrounds = new Image[4];

        [Header("Sprites")]
        [SerializeField] private Sprite normalBtnSprite;
        [SerializeField] private Sprite correctBtnSprite;
        [SerializeField] private Sprite wrongBtnSprite;

        [Header("Data")]
        [SerializeField] private QuizDataSO quizDatabase;

        private int currentQuestionIndex = 0;
        private int currentScore = 0;
        private bool isAnsweringLocked = false;
        private Coroutine nextCoroutine;

        private void Awake()
        {
            if (quizDatabase == null)
            {
#if UNITY_EDITOR
                quizDatabase = UnityEditor.AssetDatabase.LoadAssetAtPath<QuizDataSO>("Assets/Data/QuizDatabase.asset");
                if (quizDatabase == null)
                {
                    string[] guids = UnityEditor.AssetDatabase.FindAssets("t:QuizDataSO");
                    if (guids.Length > 0) quizDatabase = UnityEditor.AssetDatabase.LoadAssetAtPath<QuizDataSO>(UnityEditor.AssetDatabase.GUIDToAssetPath(guids[0]));
                }
#endif
            }
        }

        private void Start()
        {
            if (quizDatabase == null)
            {
                // Runtime: load dari Resources
                quizDatabase = Resources.Load<QuizDataSO>("Data/QuizDatabase");
#if UNITY_EDITOR
                if (quizDatabase == null)
                    quizDatabase = UnityEditor.AssetDatabase.LoadAssetAtPath<QuizDataSO>("Assets/Data/QuizDatabase.asset");
#endif
            }

            if (btnKembali != null) btnKembali.onClick.AddListener(OnKembaliClicked);
            if (btnNext != null) btnNext.onClick.AddListener(OnNextClicked);

            for (int i = 0; i < optionButtons.Length; i++)
            {
                int index = i;
                if (optionButtons[i] != null)
                {
                    optionButtons[i].onClick.AddListener(() => OnOptionSelected(index));
                }
            }

            currentQuestionIndex = 0;
            currentScore = 0;
            DisplayQuestion(currentQuestionIndex);
        }

        public void DisplayQuestion(int index)
        {
            if (quizDatabase == null || quizDatabase.questions.Count == 0) return;

            if (index >= quizDatabase.questions.Count)
            {
                FinishQuiz();
                return;
            }

            isAnsweringLocked = false;
            QuizQuestion q = quizDatabase.questions[index];

            // Update Progress & Score with high contrast colors
            if (txtProgress != null) txtProgress.text = $"<b>Soal {index + 1} / {quizDatabase.questions.Count}</b>";
            if (txtScore != null) txtScore.text = $"<b><color=#059669>Skor: {currentScore}</color></b>";
            if (txtHeader != null) txtHeader.text = "<b><color=#064E3B>Quiz Evaluasi Satwa</color></b>";

            // Update Image & adjust question text layout dynamically to prevent overlap
            if (imgQuestion != null)
            {
                if (q.questionImage != null)
                {
                    imgQuestion.sprite = q.questionImage;
                    imgQuestion.gameObject.SetActive(true);
                    if (txtQuestion != null)
                    {
                        txtQuestion.rectTransform.anchoredPosition = new Vector2(0, -280);
                        txtQuestion.rectTransform.sizeDelta = new Vector2(880, 220);
                    }
                }
                else
                {
                    imgQuestion.gameObject.SetActive(false);
                    if (txtQuestion != null)
                    {
                        txtQuestion.rectTransform.anchoredPosition = new Vector2(0, -100);
                        txtQuestion.rectTransform.sizeDelta = new Vector2(880, 380);
                    }
                }
            }

            // Update Question Text
            if (txtQuestion != null)
            {
                txtQuestion.text = $"<b><color=#042F2E>{q.questionText}</color></b>";
            }

            // Reset & Update Options
            string[] prefix = new[] { "A. ", "B. ", "C. ", "D. " };
            for (int i = 0; i < optionButtons.Length; i++)
            {
                if (i < q.options.Length)
                {
                    optionButtons[i].gameObject.SetActive(true);
                    optionButtons[i].interactable = true;
                    if (optionTexts[i] != null)
                    {
                        optionTexts[i].text = $"<b><color=#0F3B2E>{prefix[i]}{q.options[i]}</color></b>";
                    }
                    if (optionBackgrounds[i] != null && normalBtnSprite != null)
                    {
                        optionBackgrounds[i].sprite = normalBtnSprite;
                        optionBackgrounds[i].color = Color.white;
                    }
                }
                else
                {
                    optionButtons[i].gameObject.SetActive(false);
                }
            }
        }

        public void OnOptionSelected(int selectedIndex)
        {
            if (isAnsweringLocked || quizDatabase == null) return;

            isAnsweringLocked = true;
            QuizQuestion q = quizDatabase.questions[currentQuestionIndex];
            bool isCorrect = (selectedIndex == q.correctOptionIndex);

            if (isCorrect)
            {
                UIAudioManager.Instance?.PlayCorrect();
                currentScore += 10;
                if (optionBackgrounds[selectedIndex] != null && correctBtnSprite != null)
                {
                    optionBackgrounds[selectedIndex].sprite = correctBtnSprite;
                    optionBackgrounds[selectedIndex].color = Color.white;
                }
                if (optionTexts[selectedIndex] != null)
                {
                    optionTexts[selectedIndex].text = $"<b><color=#FFFFFF>{optionTexts[selectedIndex].text}</color></b>";
                }
            }
            else
            {
                UIAudioManager.Instance?.PlayWrong();
                if (optionBackgrounds[selectedIndex] != null && wrongBtnSprite != null)
                {
                    optionBackgrounds[selectedIndex].sprite = wrongBtnSprite;
                    optionBackgrounds[selectedIndex].color = Color.white;
                }
                if (optionTexts[selectedIndex] != null)
                {
                    optionTexts[selectedIndex].text = $"<b><color=#FFFFFF>{optionTexts[selectedIndex].text}</color></b>";
                }
                // Also highlight the correct answer in green (with bounds check)
                int correctIdx = q.correctOptionIndex;
                if (correctIdx >= 0 && correctIdx < optionBackgrounds.Length)
                {
                    if (optionBackgrounds[correctIdx] != null && correctBtnSprite != null)
                    {
                        optionBackgrounds[correctIdx].sprite = correctBtnSprite;
                        optionBackgrounds[correctIdx].color = Color.white;
                    }
                    if (optionTexts[correctIdx] != null)
                    {
                        optionTexts[correctIdx].text = $"<b><color=#FFFFFF>{optionTexts[correctIdx].text}</color></b>";
                    }
                }
            }

            if (txtScore != null) txtScore.text = $"<b><color=#059669>Skor: {currentScore}</color></b>";

            if (nextCoroutine != null) StopCoroutine(nextCoroutine);
            nextCoroutine = StartCoroutine(NextQuestionDelay(1.2f));
        }

        public void OnNextClicked()
        {
            if (nextCoroutine != null) StopCoroutine(nextCoroutine);
            currentQuestionIndex++;
            DisplayQuestion(currentQuestionIndex);
        }

        private IEnumerator NextQuestionDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            currentQuestionIndex++;
            DisplayQuestion(currentQuestionIndex);
        }

        private void FinishQuiz()
        {
            PlayerPrefs.SetInt("LastQuizScore", currentScore);
            PlayerPrefs.SetInt("TotalQuizQuestions", quizDatabase != null ? quizDatabase.questions.Count : 10);
            PlayerPrefs.Save();

            Debug.Log($"<b>[QUIZ]</b> Finished with Score {currentScore}! Navigating to HasilQuiz...");
            if (Application.CanStreamedLevelBeLoaded("HasilQuiz"))
            {
                SceneManager.LoadScene("HasilQuiz");
            }
            else if (Application.CanStreamedLevelBeLoaded("MainMenu"))
            {
                SceneManager.LoadScene("MainMenu");
            }
        }

        public void OnKembaliClicked()
        {
            UIAudioManager.Instance?.PlayBack();
            Debug.Log("<b>[QUIZ]</b> Returning to MainMenu...");
            if (Application.CanStreamedLevelBeLoaded("MainMenu"))
            {
                SceneManager.LoadScene("MainMenu");
            }
        }
    }
}
