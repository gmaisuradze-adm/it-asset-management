using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using HospitalAssetTracker.Models;
using HospitalAssetTracker.Services;

namespace HospitalAssetTracker.Controllers
{
    [Authorize(Roles = "Admin,IT Support,Asset Manager,Department Head")]
    public class ReportsController : Controller
    {
        private readonly IAssetService _assetService;
        private readonly IReportService _reportService;
        private readonly IAuditService _auditService;

        public ReportsController(IAssetService assetService, IReportService reportService, IAuditService auditService)
        {
            _assetService = assetService;
            _reportService = reportService;
            _auditService = auditService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Assets(AssetCategory? category, AssetStatus? status, int? locationId)
        {
            IEnumerable<Asset> assets;

            if (category.HasValue)
            {
                assets = await _assetService.GetAssetsByCategoryAsync(category.Value);
            }
            else if (status.HasValue)
            {
                assets = await _assetService.GetAssetsByStatusAsync(status.Value);
            }
            else if (locationId.HasValue)
            {
                assets = await _assetService.GetAssetsByLocationAsync(locationId.Value);
            }
            else
            {
                assets = await _assetService.GetAllAssetsAsync();
            }

            return View(assets);
        }

        [HttpPost]
        public async Task<IActionResult> ExportAssetsExcel(AssetCategory? category, AssetStatus? status, int? locationId)
        {
            IEnumerable<Asset> assets;

            if (category.HasValue)
            {
                assets = await _assetService.GetAssetsByCategoryAsync(category.Value);
            }
            else if (status.HasValue)
            {
                assets = await _assetService.GetAssetsByStatusAsync(status.Value);
            }
            else if (locationId.HasValue)
            {
                assets = await _assetService.GetAssetsByLocationAsync(locationId.Value);
            }
            else
            {
                assets = await _assetService.GetAllAssetsAsync();
            }

            var excelData = await _reportService.GenerateAssetReportExcelAsync(assets);
            var fileName = $"Assets_Report_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

            return File(excelData, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        [HttpPost]
        public async Task<IActionResult> ExportAssetsPdf(AssetCategory? category, AssetStatus? status, int? locationId)
        {
            IEnumerable<Asset> assets;

            if (category.HasValue)
            {
                assets = await _assetService.GetAssetsByCategoryAsync(category.Value);
            }
            else if (status.HasValue)
            {
                assets = await _assetService.GetAssetsByStatusAsync(status.Value);
            }
            else if (locationId.HasValue)
            {
                assets = await _assetService.GetAssetsByLocationAsync(locationId.Value);
            }
            else
            {
                assets = await _assetService.GetAllAssetsAsync();
            }

            var pdfData = await _reportService.GenerateAssetReportPdfAsync(assets);
            var fileName = $"Assets_Report_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";

            return File(pdfData, "application/pdf", fileName);
        }

        public async Task<IActionResult> AuditLog(string searchTerm, DateTime? fromDate, DateTime? toDate)
        {
            IEnumerable<AuditLog> auditLogs;

            if (!string.IsNullOrWhiteSpace(searchTerm) || fromDate.HasValue || toDate.HasValue)
            {
                auditLogs = await _auditService.SearchAuditLogsAsync(searchTerm, fromDate, toDate);
            }
            else
            {
                auditLogs = await _auditService.GetAuditLogsAsync();
            }

            ViewBag.SearchTerm = searchTerm;
            ViewBag.FromDate = fromDate?.ToString("yyyy-MM-dd");
            ViewBag.ToDate = toDate?.ToString("yyyy-MM-dd");

            return View(auditLogs);
        }

        [HttpPost]
        public async Task<IActionResult> ExportAuditLogPdf(string searchTerm, DateTime? fromDate, DateTime? toDate)
        {
            IEnumerable<AuditLog> auditLogs;

            if (!string.IsNullOrWhiteSpace(searchTerm) || fromDate.HasValue || toDate.HasValue)
            {
                auditLogs = await _auditService.SearchAuditLogsAsync(searchTerm, fromDate, toDate);
            }
            else
            {
                auditLogs = await _auditService.GetAuditLogsAsync();
            }

            var pdfData = await _reportService.GenerateAuditReportPdfAsync(auditLogs);
            var fileName = $"Audit_Log_Report_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";

            return File(pdfData, "application/pdf", fileName);
        }

        public async Task<IActionResult> MaintenanceSchedule()
        {
            var assets = await _assetService.GetAssetsForMaintenanceAsync();
            return View(assets);
        }

        public async Task<IActionResult> ExpiredWarranties()
        {
            try
            {
                // Debug: Check what we have in the database
                var allAssets = await _assetService.GetAllAssetsAsync();
                Console.WriteLine($"Total assets in database: {allAssets.Count()}");
                
                var assetsWithWarranty = allAssets.Where(a => a.WarrantyExpiry.HasValue);
                Console.WriteLine($"Assets with warranty dates: {assetsWithWarranty.Count()}");
                
                foreach (var asset in assetsWithWarranty.Take(10))
                {
                    if (asset.WarrantyExpiry.HasValue)
                    {
                        var isExpired = asset.WarrantyExpiry.Value <= DateTime.Now;
                        Console.WriteLine($"Asset {asset.AssetTag}: Warranty {asset.WarrantyExpiry.Value.ToString("yyyy-MM-dd")} - Expired: {isExpired}");
                    }
                }
                
                // var expiredAssets = await _assetService.GetExpiredWarrantyAssetsAsync(); // Old call
                var expiredAssets = await _reportService.GetExpiredWarrantyAssetsReportAsync(); // New call
                Console.WriteLine($"ExpiredWarranties: Found {expiredAssets.Count()} assets with expired warranties");
                
                return View(expiredAssets);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in ExpiredWarranties: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                TempData["ErrorMessage"] = "An error occurred while retrieving expired warranty assets.";
                return View(new List<Asset>());
            }
        }
    }
}
