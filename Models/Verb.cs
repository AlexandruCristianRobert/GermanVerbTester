namespace GermanVerbTester.Models
{
    public class Verb
    {
        public int Id { get; set; }
        public string German { get; set; } = string.Empty;
        public string English { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
    }
}