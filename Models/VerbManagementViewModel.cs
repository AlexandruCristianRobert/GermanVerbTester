using Microsoft.AspNetCore.Http;
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
    }
}