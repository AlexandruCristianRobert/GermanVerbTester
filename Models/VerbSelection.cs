using System;

namespace GermanVerbTester.Models
{
    public class VerbSelection
    {
        public int Id { get; set; }
        public int VerbId { get; set; }
        public DateTime CreatedAt { get; set; }

        // Navigation property
        public Verb? Verb { get; set; }
    }
}