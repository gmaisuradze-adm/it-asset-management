using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HospitalAssetTracker.Data;
using HospitalAssetTracker.Models;

namespace HospitalAssetTracker.Controllers
{
    public class TestDataController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TestDataController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /TestData/AddExpiredWarrantyAssets
        public async Task<IActionResult> AddExpiredWarrantyAssets()
        {
            try
            {
                // Check if we already have these test assets
                var existingTestAsset = await _context.Assets.FirstOrDefaultAsync(a => a.AssetTag == "TEST-EXP-001");
                if (existingTestAsset == null)
                {
                    var testAssets = new List<Asset>
                    {
                        new Asset
                        {
                            AssetTag = "TEST-EXP-001",
                            Category = AssetCategory.Desktop,
                            Brand = "Dell",
                            Model = "OptiPlex 3020",
                            SerialNumber = "TESTEXP001",
                            Description = "Test desktop - warranty expired",
                            Status = AssetStatus.InUse,
                            InstallationDate = DateTime.UtcNow.AddYears(-4),
                            CreatedDate = DateTime.UtcNow,
                            LastUpdated = DateTime.UtcNow,
                            LocationId = 1,
                            Department = "IT Test",
                            WarrantyExpiry = DateTime.UtcNow.AddYears(-1) // Expired 1 year ago
                        },
                        new Asset
                        {
                            AssetTag = "TEST-EXP-002",
                            Category = AssetCategory.Laptop,
                            Brand = "HP",
                            Model = "EliteBook 820",
                            SerialNumber = "TESTEXP002",
                            Description = "Test laptop - warranty expired",
                            Status = AssetStatus.InUse,
                            InstallationDate = DateTime.UtcNow.AddYears(-3),
                            CreatedDate = DateTime.UtcNow,
                            LastUpdated = DateTime.UtcNow,
                            LocationId = 1,
                            Department = "IT Test",
                            WarrantyExpiry = DateTime.UtcNow.AddMonths(-6) // Expired 6 months ago
                        },
                        new Asset
                        {
                            AssetTag = "TEST-EXP-003",
                            Category = AssetCategory.Printer,
                            Brand = "Canon",
                            Model = "LaserJet Old",
                            SerialNumber = "TESTEXP003",
                            Description = "Test printer - warranty expired",
                            Status = AssetStatus.InUse,
                            InstallationDate = DateTime.UtcNow.AddYears(-5),
                            CreatedDate = DateTime.UtcNow,
                            LastUpdated = DateTime.UtcNow,
                            LocationId = 1,
                            Department = "IT Test",
                            WarrantyExpiry = DateTime.UtcNow.AddDays(-30) // Expired 30 days ago
                        }
                    };

                    _context.Assets.AddRange(testAssets);
                    await _context.SaveChangesAsync();

                    return Json(new { success = true, message = $"Added {testAssets.Count} test assets with expired warranties" });
                }
                else
                {
                    return Json(new { success = false, message = "Test assets already exist" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        // GET: /TestData/RemoveTestAssets
        public async Task<IActionResult> RemoveTestAssets()
        {
            try
            {
                var testAssets = await _context.Assets.Where(a => a.AssetTag.StartsWith("TEST-EXP-")).ToListAsync();
                if (testAssets.Any())
                {
                    _context.Assets.RemoveRange(testAssets);
                    await _context.SaveChangesAsync();
                    return Json(new { success = true, message = $"Removed {testAssets.Count} test assets" });
                }
                else
                {
                    return Json(new { success = false, message = "No test assets found" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }
    }
}
