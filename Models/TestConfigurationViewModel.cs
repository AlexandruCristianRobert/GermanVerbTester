using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GermanVerbTester.Models
{
    public class TestConfigurationViewModel
    {
        [Required]
        [Range(1, 50, ErrorMessage = "Please select between 1 and 50 verbs.")]
        public int NumberOfVerbs { get; set; } = 10;

        [Required(ErrorMessage = "Please select at least one category.")]
        public List<string> SelectedCategories { get; set; } = new List<string>();

        // Available categories
        public List<string> AvailableCategories { get; set; } = new List<string>
        {
            "A1", "A2", "B1", "B2", "C1", "C2"
        };
    }
}