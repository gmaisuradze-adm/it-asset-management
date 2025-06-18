using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using HospitalAssetTracker.Services;
using HospitalAssetTracker.Models;
using System.Text.Json;

namespace HospitalAssetTracker.Controllers
{
    [Authorize]
    public class BugTrackingController : Controller
    {
        private readonly BugTrackingService _bugTrackingService;
        private readonly VersionService _versionService;
        private readonly ILogger<BugTrackingController> _logger;

        public BugTrackingController(BugTrackingService bugTrackingService, 
            VersionService versionService, 
            ILogger<BugTrackingController> logger)
        {
            _bugTrackingService = bugTrackingService;
            _versionService = versionService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var analytics = await _bugTrackingService.GetBugAnalyticsAsync();
                var openBugs = await _bugTrackingService.GetOpenBugsAsync();
                var versionStats = await _versionService.GetVersionStatsAsync();

                ViewBag.Analytics = analytics;
                ViewBag.VersionStats = versionStats;
                
                return View(openBugs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading bug tracking dashboard");
                TempData["ErrorMessage"] = "შეცდომა ბაგ ტრეკინგის ჩატვირთვისას";
                return View(new List<BugTracking>());
            }
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BugTracking model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var bugId = await _bugTrackingService.RegisterBugAsync(
                        model.BugTitle,
                        model.BugDescription,
                        model.ModuleName,
                        model.ErrorMessage,
                        model.StackTrace,
                        model.Severity
                    );

                    if (bugId > 0)
                    {
                        TempData["SuccessMessage"] = "ბაგი წარმატებით დარეგისტრირდა";
                        return RedirectToAction(nameof(Index));
                    }
                }

                TempData["ErrorMessage"] = "შეცდომა ბაგის რეგისტრაციისას";
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating bug record");
                TempData["ErrorMessage"] = "შეცდომა ბაგის რეგისტრაციისას";
                return View(model);
            }
        }

        [HttpPost]
        public async Task<IActionResult> MarkAsFixed(int id, string fixDescription, string filesChanged = "")
        {
            try
            {
                var result = await _bugTrackingService.MarkBugAsFixedAsync(id, fixDescription, filesChanged);
                
                if (result)
                {
                    // ვერსიის განახლება
                    await _versionService.IncrementVersionForBugFixAsync(fixDescription);
                    
                    return Json(new { success = true, message = "ბაგი მონიშნულია როგორც გამოსწორებული" });
                }
                
                return Json(new { success = false, message = "შეცდომა ბაგის განახლებისას" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error marking bug {id} as fixed");
                return Json(new { success = false, message = "შეცდომა ბაგის განახლებისას" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Analytics()
        {
            try
            {
                var analytics = await _bugTrackingService.GetBugAnalyticsAsync();
                return Json(analytics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting bug analytics");
                return Json(new { error = "შეცდომა ანალიტიკის ჩატვირთვისას" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> ModuleBugs(string moduleName)
        {
            try
            {
                var bugs = await _bugTrackingService.GetBugsByModuleAsync(moduleName);
                return PartialView("_ModuleBugs", bugs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting bugs for module {moduleName}");
                return PartialView("_ModuleBugs", new List<BugTracking>());
            }
        }

        [HttpGet]
        public async Task<IActionResult> VersionHistory()
        {
            try
            {
                var versions = await _versionService.GetVersionHistoryAsync();
                return PartialView("_VersionHistory", versions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting version history");
                return PartialView("_VersionHistory", new List<SystemVersion>());
            }
        }

        // ავტომატური ბაგ რეპორტი
        [HttpPost]
        public async Task<IActionResult> AutoReport([FromBody] AutoBugReportModel model)
        {
            try
            {
                await _bugTrackingService.RegisterBugAsync(
                    model.Title,
                    model.Description,
                    model.ModuleName,
                    model.ErrorMessage,
                    model.StackTrace,
                    "Medium"
                );

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in auto bug report");
                return Json(new { success = false });
            }
        }

        // ვერსიის მენეჯმენტი
        [HttpGet]
        public async Task<IActionResult> VersionManagement()
        {
            try
            {
                var currentVersion = await _versionService.GetCurrentVersionAsync();
                var versionHistory = await _versionService.GetVersionHistoryAsync();
                
                ViewBag.CurrentVersion = currentVersion;
                return View(versionHistory);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading version management");
                TempData["ErrorMessage"] = "შეცდომა ვერსიის მენეჯმენტის ჩატვირთვისას";
                return View(new List<SystemVersion>());
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateNewVersion([FromBody] CreateVersionModel model)
        {
            try
            {
                var versionId = await _versionService.CreateVersionAsync(
                    model.VersionNumber,
                    model.ReleaseNotes,
                    model.FeaturesAdded,
                    User.Identity?.Name ?? "System"
                );

                if (versionId > 0)
                {
                    return Json(new { success = true, message = "ახალი ვერსია წარმატებით შეიქმნა" });
                }

                return Json(new { success = false, message = "შეცდომა ვერსიის შექმნისას" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating new version");
                return Json(new { success = false, message = "შეცდომა ვერსიის შექმნისას" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> BugStats()
        {
            try
            {
                var stats = await _bugTrackingService.GetBugStatsAsync();
                return Json(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting bug stats");
                return Json(new { error = "შეცდომა სტატისტიკის ჩატვირთვისას" });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> BulkFixBugs([FromBody] BulkFixModel model)
        {
            try
            {
                var results = new List<string>();
                
                foreach (var bugId in model.BugIds)
                {
                    var success = await _bugTrackingService.MarkBugAsFixedAsync(
                        bugId, 
                        model.FixDescription, 
                        model.FilesChanged
                    );
                    
                    if (success)
                    {
                        results.Add($"Bug #{bugId} - გამოსწორდა");
                    }
                    else
                    {
                        results.Add($"Bug #{bugId} - შეცდომა");
                    }
                }

                // ვერსიის განახლება
                if (results.Any(r => r.Contains("გამოსწორდა")))
                {
                    await _versionService.IncrementVersionForBugFixAsync($"Bulk fix: {model.FixDescription}");
                }

                return Json(new { success = true, results = results });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in bulk fix");
                return Json(new { success = false, message = "შეცდომა ყველა ბაგის გამოსწორებისას" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> ExportBugs(string format = "json")
        {
            try
            {
                var bugs = await _bugTrackingService.GetAllBugsAsync();
                
                if (format.ToLower() == "csv")
                {
                    var csv = GenerateCsv(bugs);
                    return File(System.Text.Encoding.UTF8.GetBytes(csv), "text/csv", $"bugs_{DateTime.Now:yyyyMMdd}.csv");
                }
                else
                {
                    var json = System.Text.Json.JsonSerializer.Serialize(bugs, new JsonSerializerOptions { WriteIndented = true });
                    return File(System.Text.Encoding.UTF8.GetBytes(json), "application/json", $"bugs_{DateTime.Now:yyyyMMdd}.json");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting bugs");
                TempData["ErrorMessage"] = "შეცდომა ბაგების ექსპორტისას";
                return RedirectToAction(nameof(Index));
            }
        }

        private string GenerateCsv(List<BugTracking> bugs)
        {
            var csv = new System.Text.StringBuilder();
            csv.AppendLine("ID,Title,Description,Status,Severity,Module,ReportedBy,ReportedDate,FixedDate");
            
            foreach (var bug in bugs)
            {
                csv.AppendLine($"{bug.Id},\"{bug.BugTitle}\",\"{bug.BugDescription}\",{bug.Status},{bug.Severity},{bug.ModuleName},{bug.ReportedBy},{bug.ReportedDate},{bug.FixedDate}");
            }
            
            return csv.ToString();
        }
    }

    public class AutoBugReportModel
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ModuleName { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
        public string StackTrace { get; set; } = string.Empty;
    }

    public class CreateVersionModel
    {
        public string VersionNumber { get; set; } = string.Empty;
        public string ReleaseNotes { get; set; } = string.Empty;
        public int FeaturesAdded { get; set; }
    }

    public class BulkFixModel
    {
        public List<int> BugIds { get; set; } = new List<int>();
        public string FixDescription { get; set; } = string.Empty;
        public string FilesChanged { get; set; } = string.Empty;
    }
}
