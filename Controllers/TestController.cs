using GermanVerbTester.Data;
using GermanVerbTester.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace GermanVerbTester.Controllers
{
    public class TestController : Controller
    {
        private readonly AppDbContext _context;

        public TestController(AppDbContext context)
        {
            _context = context;
        }

        // 1. Homepage: Configuration
        public IActionResult Index(int? numberOfVerbs = null, string? categories = null)
        {
            var model = new TestConfigurationViewModel();

            // If parameters are provided, restore the previous configuration
            if (numberOfVerbs.HasValue)
            {
                model.NumberOfVerbs = numberOfVerbs.Value;
            }

            if (!string.IsNullOrEmpty(categories))
            {
                model.SelectedCategories = categories.Split(',').ToList();
            }

            return View(model);
        }

        // 2. Generate Test
        [HttpPost]
        public async Task<IActionResult> StartTest(TestConfigurationViewModel config)
        {
            if (!ModelState.IsValid)
            {
                return View("Index", config);
            }

            if (config.SelectedCategories == null || !config.SelectedCategories.Any())
            {
                ModelState.AddModelError("SelectedCategories", "Please select at least one category.");
                return View("Index", config);
            }

            // Fetch random verbs filtered by selected categories
            var verbs = await _context.Verbs
                .Where(v => config.SelectedCategories.Contains(v.Category))
                .OrderBy(r => Guid.NewGuid())
                .Take(config.NumberOfVerbs)
                .ToListAsync();

            if (!verbs.Any())
            {
                ModelState.AddModelError("", "No verbs found for the selected categories. Please add verbs first.");
                return View("Index", config);
            }

            if (verbs.Count < config.NumberOfVerbs)
            {
                ModelState.AddModelError("", $"Only {verbs.Count} verbs available for selected categories. Adjust your selection.");
                return View("Index", config);
            }

            var viewModel = new TestViewModel
            {
                // Store the configuration for later use
                NumberOfVerbs = config.NumberOfVerbs,
                SelectedCategories = config.SelectedCategories
            };

            foreach (var v in verbs)
            {
                viewModel.Questions.Add(new QuestionItem
                {
                    VerbId = v.Id,
                    German = v.German,
                    CorrectEnglish = v.English,
                    UserAnswer = ""
                });
            }

            return View("TestExecution", viewModel);
        }

        // 3. Process Results
        [HttpPost]
        public async Task<IActionResult> SubmitTest(TestViewModel model)
        {
            int score = 0;

            // Grading logic
            foreach (var q in model.Questions)
            {
                // Normalize strings for comparison (trim and lowercase)
                var userAns = q.UserAnswer?.Trim().ToLower();
                // Ensure we get the correct verb from the database for reliable comparison
                var correctVerb = await _context.Verbs.FindAsync(q.VerbId);
                var correctAns = correctVerb?.English?.Trim().ToLower();

                if (userAns == correctAns)
                {
                    q.IsCorrect = true;
                    score++;
                }
                else
                {
                    q.IsCorrect = false;
                }

                // Set the correct english on the question item for results display
                q.CorrectEnglish = correctVerb?.English ?? "";
            }

            model.Score = score;
            model.IsCompleted = true;

            // Configuration is already part of the model from hidden fields
            // No need to set it again

            // Save Result to DB
            var resultRecord = new TestResult
            {
                CorrectAnswers = score,
                TotalQuestions = model.Questions.Count,
                SubmissionDate = DateTime.UtcNow
            };
            _context.TestResults.Add(resultRecord);
            await _context.SaveChangesAsync();

            return View("TestExecution", model);
        }

        // 4. History Page
        public async Task<IActionResult> History()
        {
            var history = await _context.TestResults
                .OrderByDescending(t => t.SubmissionDate)
                .ToListAsync();
            return View(history);
        }

        // ==========================================================
        // VERB MANAGEMENT SECTION
        // ==========================================================

        // GET: Display the verb management page
        public IActionResult ManageVerbs(string status = "")
        {
            var model = new VerbManagementViewModel { StatusMessage = status };
            return View(model);
        }

        // POST: Add a single verb
        [HttpPost]
        public async Task<IActionResult> AddSingleVerb(VerbManagementViewModel model)
        {
            if (ModelState.IsValid)
            {
                var newVerb = new Verb
                {
                    German = model.GermanVerb?.Trim() ?? string.Empty,
                    English = model.EnglishVerb?.Trim() ?? string.Empty,
                    Category = model.Category?.Trim() ?? string.Empty
                };

                _context.Verbs.Add(newVerb);
                await _context.SaveChangesAsync();

                return RedirectToAction("ManageVerbs", new { status = $"Successfully added '{newVerb.German}' ({newVerb.Category})." });
            }

            model.StatusMessage = "Please ensure all fields are filled.";
            return View("ManageVerbs", model);
        }

        // POST: Upload JSON verbs
        [HttpPost]
        public async Task<IActionResult> UploadJsonVerbs(VerbManagementViewModel model)
        {
            if (model.JsonFile == null || model.JsonFile.Length == 0)
            {
                model.StatusMessage = "Error: Please select a file to upload.";
                return View("ManageVerbs", model);
            }

            try
            {
                int count = 0;
                using (var reader = new StreamReader(model.JsonFile.OpenReadStream()))
                {
                    var jsonContent = await reader.ReadToEndAsync();

                    // Deserialize JSON into an array of verb objects
                    var verbArray = JsonSerializer.Deserialize<List<VerbJsonDto>>(jsonContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (verbArray == null || !verbArray.Any())
                    {
                        throw new Exception("JSON file is empty or formatted incorrectly.");
                    }

                    foreach (var item in verbArray)
                    {
                        var newVerb = new Verb
                        {
                            German = item.German?.Trim() ?? string.Empty,
                            English = item.English?.Trim() ?? string.Empty,
                            Category = item.Category?.Trim() ?? string.Empty
                        };
                        _context.Verbs.Add(newVerb);
                        count++;
                    }

                    await _context.SaveChangesAsync();
                    return RedirectToAction("ManageVerbs", new { status = $"Successfully added {count} verbs from the uploaded file." });
                }
            }
            catch (Exception ex)
            {
                model.StatusMessage = $"Upload failed. Ensure the JSON format is: [{{\"german\": \"value\", \"english\": \"value\", \"category\": \"A1\"}}]. Error: {ex.Message}";
                return View("ManageVerbs", model);
            }
        }

        public async Task<IActionResult> DownloadVerbsJson()
        {
            // Fetch all verbs from the database
            var verbs = await _context.Verbs
                .OrderBy(v => v.Category)
                .ThenBy(v => v.German)
                .ToListAsync();

            // Map to the DTO format for export
            var verbExport = verbs.Select(v => new VerbJsonDto
            {
                German = v.German,
                English = v.English,
                Category = v.Category
            }).ToList();

            // Serialize to JSON with formatting
            var jsonContent = JsonSerializer.Serialize(verbExport, new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            // Convert to bytes
            var bytes = System.Text.Encoding.UTF8.GetBytes(jsonContent);

            // Return as downloadable file
            return File(bytes, "application/json", $"german-verbs-{DateTime.Now:yyyy-MM-dd}.json");
        }

        private class VerbJsonDto
        {
            public string? German { get; set; }
            public string? English { get; set; }
            public string? Category { get; set; }
        }
    }
   
}