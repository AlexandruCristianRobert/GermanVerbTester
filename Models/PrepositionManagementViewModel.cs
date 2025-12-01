using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace GermanVerbTester.Models
{
    public class PrepositionManagementViewModel
    {
        [Required(ErrorMessage = "German preposition is required.")]
        public string? GermanPreposition { get; set; }

        [Required(ErrorMessage = "English translation is required.")]
        public string? EnglishPreposition { get; set; }

        [Required(ErrorMessage = "Case is required.")]
        [Display(Name = "Case (Accusative, Dative, Both)")]
        public string? Case { get; set; }

        [Required(ErrorMessage = "Explanation is required.")]
        [Display(Name = "Explanation")]
        public string? Explanation { get; set; }

        [Display(Name = "JSON File (Array of objects with german, english, case, explanation)")]
        public IFormFile? JsonFile { get; set; }

        public string StatusMessage { get; set; } = string.Empty;
    }
}