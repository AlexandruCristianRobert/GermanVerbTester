using GermanVerbTester.Data;
using GermanVerbTester.Models;
using GermanVerbTester.Services;
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
        private readonly VerbCacheService _verbCache;

        public TestController(AppDbContext context, VerbCacheService verbCache)
        {
            _context = context;
            _verbCache = verbCache;
        }

        // SPA Homepage - serves the single page
        public async Task<IActionResult> Index()
        {
            // Load verbs into cache if not already loaded
            var verbs = await _verbCache.GetAllVerbsAsync();
            return View();
        }

        // API: Get all verbs as JSON for client-side
        [HttpGet]
        public async Task<IActionResult> GetVerbs()
        {
            var verbs = await _verbCache.GetAllVerbsAsync();
            return Json(verbs);
        }

        // API: Get selected verbs from VerbSelection table
        [HttpGet]
        public async Task<IActionResult> GetSelectedVerbs()
        {
            var selectedVerbs = await _context.VerbSelections
                .Include(vs => vs.Verb)
                .Select(vs => vs.Verb)
                .ToListAsync();

            return Json(selectedVerbs);
        }

        // API: Get test history
        [HttpGet]
        public async Task<IActionResult> GetHistory()
        {
            var history = await _context.TestResults
                .OrderByDescending(t => t.SubmissionDate)
                .ToListAsync();
            return Json(history);
        }

        // API: Save test results (called every 5 minutes or on demand)
        [HttpPost]
        public async Task<IActionResult> SaveTestResults([FromBody] List<TestResultDto> results)
        {
            if (results == null || !results.Any())
            {
                return BadRequest("No results to save");
            }

            foreach (var result in results)
            {
                var testResult = new TestResult
                {
                    CorrectAnswers = result.CorrectAnswers,
                    TotalQuestions = result.TotalQuestions,
                    SubmissionDate = DateTime.UtcNow
                };
                _context.TestResults.Add(testResult);
            }

            await _context.SaveChangesAsync();
            return Ok(new { saved = results.Count });
        }

        // ==========================================================
        // VERB MANAGEMENT SECTION (Temporarily hidden but functional)
        // ==========================================================

        [HttpGet]
        public IActionResult ManageVerbs(string status = "")
        {
            var model = new VerbManagementViewModel { StatusMessage = status };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> AddSingleVerb(VerbManagementViewModel model)
        {
            if (ModelState.IsValid)
            {
                var newVerb = new Verb
                {
                    German = model.GermanVerb?.Trim() ?? string.Empty,
                    English = model.EnglishVerb?.Trim() ?? string.Empty,
                    Category = model.Category?.Trim() ?? string.Empty,
                    Hint = model.Hint?.Trim() ?? string.Empty
                };

                _context.Verbs.Add(newVerb);
                await _context.SaveChangesAsync();

                // Refresh cache after adding
                await _verbCache.RefreshCacheAsync();

                return RedirectToAction("ManageVerbs", new { status = $"Successfully added '{newVerb.German}' ({newVerb.Category})." });
            }

            model.StatusMessage = "Please ensure all fields are filled.";
            return View("ManageVerbs", model);
        }

        [HttpPost]
        public async Task<IActionResult> SelectRandomVerbs(VerbManagementViewModel model)
        {
            if (model.SelectedCategoriesForRandom == null || !model.SelectedCategoriesForRandom.Any())
            {
                return RedirectToAction("ManageVerbs", new { status = "Error: Please select at least one category." });
            }

            try
            {
                // Get verbs filtered by selected categories
                var filteredVerbs = await _context.Verbs
                    .Where(v => model.SelectedCategoriesForRandom.Contains(v.Category))
                    .ToListAsync();

                if (!filteredVerbs.Any())
                {
                    return RedirectToAction("ManageVerbs", new { status = "Error: No verbs found for the selected categories." });
                }

                // Randomly select the specified number of verbs
                var random = new Random();
                var selectedVerbs = filteredVerbs
                    .OrderBy(x => random.Next())
                    .Take(model.NumberOfVerbsToSelect)
                    .ToList();

                // Clear existing selections
                var existingSelections = await _context.VerbSelections.ToListAsync();
                _context.VerbSelections.RemoveRange(existingSelections);

                // Add new selections
                var verbSelections = selectedVerbs.Select(v => new VerbSelection
                {
                    VerbId = v.Id,
                    CreatedAt = DateTime.UtcNow
                }).ToList();

                _context.VerbSelections.AddRange(verbSelections);
                await _context.SaveChangesAsync();

                var categoriesStr = string.Join(", ", model.SelectedCategoriesForRandom);
                return RedirectToAction("ManageVerbs", new { status = $"Successfully selected {selectedVerbs.Count} random verbs from categories: {categoriesStr}." });
            }
            catch (Exception ex)
            {
                return RedirectToAction("ManageVerbs", new { status = $"Error: {ex.Message}" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ClearVerbSelections()
        {
            try
            {
                var existingSelections = await _context.VerbSelections.ToListAsync();
                _context.VerbSelections.RemoveRange(existingSelections);
                await _context.SaveChangesAsync();

                return RedirectToAction("ManageVerbs", new { status = "Successfully cleared all verb selections." });
            }
            catch (Exception ex)
            {
                return RedirectToAction("ManageVerbs", new { status = $"Error: {ex.Message}" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> ViewSelectedVerbs()
        {
            var selectedVerbs = await _context.VerbSelections
                .Include(vs => vs.Verb)
                .OrderBy(vs => vs.CreatedAt)
                .Select(vs => vs.Verb)
                .ToListAsync();

            return Json(selectedVerbs);
        }

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
                            Category = item.Category?.Trim() ?? string.Empty,
                            Hint = item.Hint?.Trim() ?? string.Empty
                        };
                        _context.Verbs.Add(newVerb);
                        count++;
                    }

                    await _context.SaveChangesAsync();

                    // Refresh cache after bulk upload
                    await _verbCache.RefreshCacheAsync();

                    return RedirectToAction("ManageVerbs", new { status = $"Successfully added {count} verbs from the uploaded file." });
                }
            }
            catch (Exception ex)
            {
                model.StatusMessage = $"Upload failed. Error: {ex.Message}";
                return View("ManageVerbs", model);
            }
        }

        public async Task<IActionResult> DownloadVerbsJson()
        {
            var verbs = await _context.Verbs
                .OrderBy(v => v.Category)
                .ThenBy(v => v.German)
                .ToListAsync();

            var verbExport = verbs.Select(v => new VerbJsonDto
            {
                German = v.German,
                English = v.English,
                Category = v.Category,
                Hint = v.Hint
            }).ToList();

            var jsonContent = JsonSerializer.Serialize(verbExport, new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            var bytes = System.Text.Encoding.UTF8.GetBytes(jsonContent);
            return File(bytes, "application/json", $"german-verbs-{DateTime.Now:yyyy-MM-dd}.json");
        }

        private class VerbJsonDto
        {
            public string? German { get; set; }
            public string? English { get; set; }
            public string? Category { get; set; }
            public string? Hint { get; set; }
        }

        public class TestResultDto
        {
            public int CorrectAnswers { get; set; }
            public int TotalQuestions { get; set; }
        }
    }
}