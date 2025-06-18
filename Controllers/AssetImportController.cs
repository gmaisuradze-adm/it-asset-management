using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using HospitalAssetTracker.Models;
using HospitalAssetTracker.Services;
using System.Text;

namespace HospitalAssetTracker.Controllers
{
    [Authorize(Roles = "Admin,IT Support,Asset Manager")]
    public class AssetImportController : Controller
    {
        private readonly IAssetService _assetService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _environment;

        public AssetImportController(IAssetService assetService, UserManager<ApplicationUser> userManager, IWebHostEnvironment environment)
        {
            _assetService = assetService;
            _userManager = userManager;
            _environment = environment;
        }

        // GET: AssetImport
        public IActionResult Index()
        {
            return View();
        }

        // GET: AssetImport/Template
        public IActionResult DownloadTemplate()
        {
            try
            {
                var csv = new StringBuilder();
                csv.AppendLine("AssetTag,Category,Brand,Model,SerialNumber,Description,Status,LocationId,Department,Supplier,PurchasePrice,WarrantyExpiry,Notes");
                csv.AppendLine("SAMPLE-001,Desktop,Dell,OptiPlex 7090,SAMPLE123,Sample desktop computer,Available,1,IT,Dell Inc,899.99,2027-12-31,Sample notes");
                csv.AppendLine("SAMPLE-002,Laptop,HP,EliteBook 850,SAMPLE456,Sample laptop,InUse,2,Finance,HP Inc,1299.99,2026-06-30,Another sample");

                var bytes = Encoding.UTF8.GetBytes(csv.ToString());
                return File(bytes, "text/csv", $"AssetImportTemplate_{DateTime.Now:yyyyMMdd}.csv");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error generating template: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: AssetImport/Upload
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload(IFormFile file, bool validateOnly = false)
        {
            if (file == null || file.Length == 0)
            {
                TempData["ErrorMessage"] = "Please select a CSV file to upload.";
                return RedirectToAction(nameof(Index));
            }

            if (!file.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
            {
                TempData["ErrorMessage"] = "Only CSV files are supported.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var results = new List<AssetImportResult>();
                var userId = _userManager.GetUserId(User) ?? string.Empty;

                using (var reader = new StreamReader(file.OpenReadStream()))
                {
                    string? line;
                    int lineNumber = 0;
                    bool isHeader = true;

                    while ((line = await reader.ReadLineAsync()) != null)
                    {
                        lineNumber++;
                        
                        if (isHeader)
                        {
                            isHeader = false;
                            continue; // Skip header row
                        }

                        var result = await ProcessImportLine(line, lineNumber, userId, validateOnly);
                        results.Add(result);
                    }
                }

                ViewBag.Results = results;
                ViewBag.ValidateOnly = validateOnly;
                ViewBag.SuccessCount = results.Count(r => r.IsSuccess);
                ViewBag.ErrorCount = results.Count(r => !r.IsSuccess);

                if (!validateOnly)
                {
                    TempData["SuccessMessage"] = $"Import completed. {ViewBag.SuccessCount} assets imported successfully.";
                    if (ViewBag.ErrorCount > 0)
                    {
                        TempData["WarningMessage"] = $"{ViewBag.ErrorCount} rows had errors and were skipped.";
                    }
                }

                return View("Results", results);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error processing file: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        private async Task<AssetImportResult> ProcessImportLine(string line, int lineNumber, string userId, bool validateOnly)
        {
            var result = new AssetImportResult { LineNumber = lineNumber, RawData = line };

            try
            {
                var values = ParseCsvLine(line);
                
                if (values.Length < 8) // Minimum required fields
                {
                    result.ErrorMessage = "Insufficient data columns";
                    return result;
                }

                var asset = new Asset
                {
                    AssetTag = values[0]?.Trim() ?? string.Empty,
                    Category = ParseEnum<AssetCategory>(values[1]),
                    Brand = values[2]?.Trim() ?? string.Empty,
                    Model = values[3]?.Trim() ?? string.Empty,
                    SerialNumber = values[4]?.Trim() ?? string.Empty,
                    Description = values[5]?.Trim() ?? string.Empty,
                    Status = ParseEnum<AssetStatus>(values[6]),
                    LocationId = ParseNullableInt(values[7]),
                    Department = values[8]?.Trim(),
                    Supplier = values.Length > 9 ? values[9]?.Trim() : null,
                    PurchasePrice = values.Length > 10 ? ParseNullableDecimal(values[10]) : null,
                    WarrantyExpiry = values.Length > 11 ? ParseNullableDate(values[11]) : null,
                    Notes = values.Length > 12 ? values[12]?.Trim() : null,
                    InstallationDate = DateTime.UtcNow,
                    CreatedDate = DateTime.UtcNow,
                    LastUpdated = DateTime.UtcNow
                };

                // Validate required fields
                if (string.IsNullOrWhiteSpace(asset.AssetTag))
                {
                    result.ErrorMessage = "Asset Tag is required";
                    return result;
                }

                if (string.IsNullOrWhiteSpace(asset.Brand))
                {
                    result.ErrorMessage = "Brand is required";
                    return result;
                }

                // Check for duplicate asset tag
                if (!await _assetService.IsAssetTagUniqueAsync(asset.AssetTag))
                {
                    result.ErrorMessage = $"Asset Tag '{asset.AssetTag}' already exists";
                    return result;
                }

                if (!validateOnly)
                {
                    await _assetService.CreateAssetAsync(asset, userId);
                }

                result.IsSuccess = true;
                result.AssetTag = asset.AssetTag;
                result.Message = validateOnly ? "Validation passed" : "Asset created successfully";
            }
            catch (Exception ex)
            {
                result.ErrorMessage = ex.Message;
            }

            return result;
        }

        private string[] ParseCsvLine(string line)
        {
            // Simple CSV parser - in production, use a library like CsvHelper
            var values = new List<string>();
            var current = new StringBuilder();
            bool inQuotes = false;

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];
                
                if (c == '"')
                {
                    inQuotes = !inQuotes;
                }
                else if (c == ',' && !inQuotes)
                {
                    values.Add(current.ToString());
                    current.Clear();
                }
                else
                {
                    current.Append(c);
                }
            }
            
            values.Add(current.ToString());
            return values.ToArray();
        }

        private T ParseEnum<T>(string value) where T : struct, Enum
        {
            if (Enum.TryParse<T>(value?.Trim(), true, out T result))
            {
                return result;
            }
            return default(T);
        }

        private int? ParseNullableInt(string value)
        {
            if (int.TryParse(value?.Trim(), out int result))
            {
                return result;
            }
            return null;
        }

        private decimal? ParseNullableDecimal(string value)
        {
            if (decimal.TryParse(value?.Trim(), out decimal result))
            {
                return result;
            }
            return null;
        }

        private DateTime? ParseNullableDate(string value)
        {
            if (DateTime.TryParse(value?.Trim(), out DateTime result))
            {
                return result;
            }
            return null;
        }
    }
}
