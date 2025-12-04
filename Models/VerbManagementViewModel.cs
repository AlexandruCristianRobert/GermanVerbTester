using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GermanVerbTester.Models
{
    public class VerbManagementViewModel
    {
        [Required(ErrorMessage = "German verb is required.")]
        public string? GermanVerb { get; set; }

        [Required(ErrorMessage = "English translation is required.")]
        public string? EnglishVerb { get; set; }

        [Required(ErrorMessage = "Category is required.")]
        [Display(Name = "Category (A1, A2, B1, B2, C1, C2)")]
        public string? Category { get; set; }

        [Display(Name = "JSON File (Array of objects with german, english, category)")]
        public IFormFile? JsonFile { get; set; }

        public string StatusMessage { get; set; } = string.Empty;

        public string? Hint { get; set; }

        // For random selection
        [Range(1, 1000, ErrorMessage = "Please select between 1 and 1000 verbs.")]
        public int NumberOfVerbsToSelect { get; set; } = 100;

        public List<string> SelectedCategoriesForRandom { get; set; } = new List<string>();

        public List<string> AvailableCategories { get; set; } = new List<string>
        {
            "A1", "A2", "B1", "B2", "C1", "C2"
        };
    }
}