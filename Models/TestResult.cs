using System;

namespace GermanVerbTester.Models
{
    public class TestResult
    {
        public int Id { get; set; }
        public int CorrectAnswers { get; set; }
        public int TotalQuestions { get; set; }
        public DateTime SubmissionDate { get; set; }
    }
}