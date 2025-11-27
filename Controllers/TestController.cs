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
        public IActionResult Index()
        {
            return View();
        }

        // 2. Generate Test
        [HttpPost]
        public async Task<IActionResult> StartTest(int numberOfVerbs)
        {
            // Fetch random verbs using GUID ordering
            var verbs = await _context.Verbs
                .OrderBy(r => Guid.NewGuid())
                .Take(numberOfVerbs)
                .ToListAsync();

            var viewModel = new TestViewModel();
            foreach (var v in verbs)
            {
                viewModel.Questions.Add(new QuestionItem
                {
                    VerbId = v.Id,
                    // Use property names from the model: GermanVerb and EnglishVerb
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

            // Save Result to DB
            var resultRecord = new TestResult
            {
                CorrectAnswers = score,
                TotalQuestions = model.Questions.Count,
                SubmissionDate = DateTime.Now
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
                    English = model.EnglishVerb?.Trim() ?? string.Empty
                };

                _context.Verbs.Add(newVerb);
                await _context.SaveChangesAsync();

                return RedirectToAction("ManageVerbs", new { status = $"Successfully added '{newVerb.German}'." });
            }

            model.StatusMessage = "Please ensure both German and English fields are filled.";
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

                    // Deserialize JSON into a Dictionary<German, English>
                    var verbDictionary = JsonSerializer.Deserialize<Dictionary<string, string>>(jsonContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = false // Keys are case sensitive (German verbs)
                    });

                    if (verbDictionary == null || !verbDictionary.Any())
                    {
                        throw new Exception("JSON file is empty or formatted incorrectly.");
                    }

                    foreach (var pair in verbDictionary)
                    {
                        var newVerb = new Verb
                        {
                            German = pair.Key.Trim(),
                            English = pair.Value.Trim()
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
                model.StatusMessage = $"Upload failed. Ensure the JSON format is strictly '{{ \"german\": \"english\" }}'. Error: {ex.Message}";
                return View("ManageVerbs", model);
            }
        }
    }
}