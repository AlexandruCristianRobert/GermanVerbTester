using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace GermanVerbTester.Models
{
    public class VerbManagementViewModel
    {
        // For single verb input
        [Required(ErrorMessage = "German verb is required.")]
        public string? GermanVerb { get; set; }

        [Required(ErrorMessage = "English translation is required.")]
        public string? EnglishVerb { get; set; }

        // For JSON file upload
        [Display(Name = "JSON File (Key: German, Value: English)")]
        public IFormFile? JsonFile { get; set; }

        // For feedback messages
        public string StatusMessage { get; set; } = string.Empty;
    }
}