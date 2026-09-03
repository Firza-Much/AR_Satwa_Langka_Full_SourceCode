using System.Collections.Generic;
using UnityEngine;

namespace SatwaLangka.Data
{
    [System.Serializable]
    public class QuizQuestion
    {
        [TextArea(2, 4)]
        public string questionText;
        public Sprite questionImage;
        public string[] options; // 4 options
        public int correctOptionIndex; // 0 to 3
        [TextArea(1, 3)]
        public string explanation;
    }

    [CreateAssetMenu(fileName = "QuizDatabase", menuName = "Satwa Langka/Quiz Database", order = 2)]
    public class QuizDataSO : ScriptableObject
    {
        public List<QuizQuestion> questions = new List<QuizQuestion>();
    }
}
