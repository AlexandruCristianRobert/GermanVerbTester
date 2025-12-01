namespace GermanVerbTester.Models
{
    public class Preposition
    {
        public int Id { get; set; }
        public string German { get; set; } = string.Empty;
        public string English { get; set; } = string.Empty;
        public string Case { get; set; } = string.Empty; // "Accusative", "Dative", or "Both"
        public string Explanation { get; set; } = string.Empty;
    }
}