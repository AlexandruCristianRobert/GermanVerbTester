using System.Collections.Generic;

namespace GermanVerbTester.Models
{
    public class TestViewModel
    {
        public List<QuestionItem> Questions { get; set; } = new List<QuestionItem>();
        public int Score { get; set; }
        public bool IsCompleted { get; set; } = false;
    }

    public class QuestionItem
    {
        public int VerbId { get; set; }
        public string German { get; set; } = string.Empty;
        public string CorrectEnglish { get; set; } = string.Empty; 
        public string UserAnswer { get; set; } = string.Empty;
        public bool IsCorrect { get; set; }
    }
}