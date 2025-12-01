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
    public class PrepositionController : Controller
    {
        private readonly AppDbContext _context;
        private readonly PrepositionCacheService _prepositionCache;

        public PrepositionController(AppDbContext context, PrepositionCacheService prepositionCache)
        {
            _context = context;
            _prepositionCache = prepositionCache;
        }

        // SPA Homepage - serves the single page
        public async Task<IActionResult> Index()
        {
            // Load prepositions into cache if not already loaded
            var prepositions = await _prepositionCache.GetAllPrepositionsAsync();
            return View();
        }

        // API: Get all prepositions as JSON for client-side
        [HttpGet]
        public async Task<IActionResult> GetPrepositions()
        {
            var prepositions = await _prepositionCache.GetAllPrepositionsAsync();
            return Json(prepositions);
        }

        // API: Get test history
        [HttpGet]
        public async Task<IActionResult> GetHistory()
        {
            var history = await _context.PrepositionTestResults
                .OrderByDescending(t => t.SubmissionDate)
                .ToListAsync();
            return Json(history);
        }

        // API: Save test results
        [HttpPost]
        public async Task<IActionResult> SaveTestResults([FromBody] List<PrepositionTestResultDto> results)
        {
            if (results == null || !results.Any())
            {
                return BadRequest("No results to save");
            }

            foreach (var result in results)
            {
                var testResult = new PrepositionTestResult
                {
                    CorrectAnswers = result.CorrectAnswers,
                    TotalQuestions = result.TotalQuestions,
                    SubmissionDate = DateTime.UtcNow
                };
                _context.PrepositionTestResults.Add(testResult);
            }

            await _context.SaveChangesAsync();
            return Ok(new { saved = results.Count });
        }

        // ==========================================================
        // PREPOSITION MANAGEMENT SECTION
        // ==========================================================

        [HttpGet]
        public IActionResult ManagePrepositions(string status = "")
        {
            var model = new PrepositionManagementViewModel { StatusMessage = status };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> AddSinglePreposition(PrepositionManagementViewModel model)
        {
            if (ModelState.IsValid)
            {
                var newPreposition = new Preposition
                {
                    German = model.GermanPreposition?.Trim() ?? string.Empty,
                    English = model.EnglishPreposition?.Trim() ?? string.Empty,
                    Case = model.Case?.Trim() ?? string.Empty,
                    Explanation = model.Explanation?.Trim() ?? string.Empty
                };

                _context.Prepositions.Add(newPreposition);
                await _context.SaveChangesAsync();

                // Refresh cache after adding
                await _prepositionCache.RefreshCacheAsync();

                return RedirectToAction("ManagePrepositions", new { status = $"Successfully added '{newPreposition.German}' ({newPreposition.Case})." });
            }

            model.StatusMessage = "Please ensure all fields are filled.";
            return View("ManagePrepositions", model);
        }

        [HttpPost]
        public async Task<IActionResult> UploadJsonPrepositions(PrepositionManagementViewModel model)
        {
            if (model.JsonFile == null || model.JsonFile.Length == 0)
            {
                model.StatusMessage = "Error: Please select a file to upload.";
                return View("ManagePrepositions", model);
            }

            try
            {
                int count = 0;
                using (var reader = new StreamReader(model.JsonFile.OpenReadStream()))
                {
                    var jsonContent = await reader.ReadToEndAsync();
                    var prepositionArray = JsonSerializer.Deserialize<List<PrepositionJsonDto>>(jsonContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (prepositionArray == null || !prepositionArray.Any())
                    {
                        throw new Exception("JSON file is empty or formatted incorrectly.");
                    }

                    foreach (var item in prepositionArray)
                    {
                        var newPreposition = new Preposition
                        {
                            German = item.German?.Trim() ?? string.Empty,
                            English = item.English?.Trim() ?? string.Empty,
                            Case = item.Case?.Trim() ?? string.Empty,
                            Explanation = item.Explanation?.Trim() ?? string.Empty
                        };
                        _context.Prepositions.Add(newPreposition);
                        count++;
                    }

                    await _context.SaveChangesAsync();

                    // Refresh cache after bulk upload
                    await _prepositionCache.RefreshCacheAsync();

                    return RedirectToAction("ManagePrepositions", new { status = $"Successfully added {count} prepositions from the uploaded file." });
                }
            }
            catch (Exception ex)
            {
                model.StatusMessage = $"Upload failed. Error: {ex.Message}";
                return View("ManagePrepositions", model);
            }
        }

        public async Task<IActionResult> DownloadPrepositionsJson()
        {
            var prepositions = await _context.Prepositions
                .OrderBy(p => p.Case)
                .ThenBy(p => p.German)
                .ToListAsync();

            var prepositionExport = prepositions.Select(p => new PrepositionJsonDto
            {
                German = p.German,
                English = p.English,
                Case = p.Case,
                Explanation = p.Explanation
            }).ToList();

            var jsonContent = JsonSerializer.Serialize(prepositionExport, new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            var bytes = System.Text.Encoding.UTF8.GetBytes(jsonContent);
            return File(bytes, "application/json", $"german-prepositions-{DateTime.Now:yyyy-MM-dd}.json");
        }

        private class PrepositionJsonDto
        {
            public string? German { get; set; }
            public string? English { get; set; }
            public string? Case { get; set; }
            public string? Explanation { get; set; }
        }

        public class PrepositionTestResultDto
        {
            public int CorrectAnswers { get; set; }
            public int TotalQuestions { get; set; }
        }
    }
}