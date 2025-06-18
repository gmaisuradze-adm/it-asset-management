using Microsoft.EntityFrameworkCore;
using ClosedXML.Excel;
using HospitalAssetTracker.Data;
using HospitalAssetTracker.Models;

namespace HospitalAssetTracker.Services
{
    public class ReportService : IReportService
    {
        private readonly ApplicationDbContext _context;

        public ReportService(ApplicationDbContext context)
        {
            _context = context;
        }

        public Task<byte[]> GenerateAssetReportExcelAsync(IEnumerable<Asset> assets)
        {
            return Task.Run(() =>
            {
                using var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("Assets");

                // Headers
                var headers = new[]
                {
                    "Asset Tag", "Category", "Brand", "Model", "Serial Number", "Description",
                    "Status", "Location", "Assigned To", "Department", "Installation Date",
                    "Warranty Expiry", "Notes"
                };

            for (int i = 0; i < headers.Length; i++)
            {
                worksheet.Cell(1, i + 1).Value = headers[i];
                worksheet.Cell(1, i + 1).Style.Font.Bold = true;
                worksheet.Cell(1, i + 1).Style.Fill.BackgroundColor = XLColor.LightGray;
            }

            // Data
            int row = 2;
            foreach (var asset in assets)
            {
                worksheet.Cell(row, 1).Value = asset.AssetTag;
                worksheet.Cell(row, 2).Value = asset.Category.ToString();
                worksheet.Cell(row, 3).Value = asset.Brand;
                worksheet.Cell(row, 4).Value = asset.Model;
                worksheet.Cell(row, 5).Value = asset.SerialNumber;
                worksheet.Cell(row, 6).Value = asset.Description;
                worksheet.Cell(row, 7).Value = asset.Status.ToString();
                worksheet.Cell(row, 8).Value = asset.Location?.FullLocation ?? "";
                worksheet.Cell(row, 9).Value = asset.AssignedToUser?.FullName ?? "";
                worksheet.Cell(row, 10).Value = asset.Department ?? "";
                worksheet.Cell(row, 11).Value = asset.InstallationDate;
                worksheet.Cell(row, 12).Value = asset.WarrantyExpiry?.ToString("yyyy-MM-dd") ?? "";
                worksheet.Cell(row, 13).Value = asset.Notes ?? "";
                row++;
            }

            // Auto-fit columns
            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
            });
        }

        public Task<byte[]> GenerateAssetReportPdfAsync(IEnumerable<Asset> assets)
        {
            return Task.Run(() =>
            {
                // Simple PDF implementation - for now return empty array
                // This would be implemented with a proper PDF library
                return Array.Empty<byte>();
            });
        }

        public Task<byte[]> GenerateMaintenanceReportPdfAsync(IEnumerable<MaintenanceRecord> maintenanceRecords)
        {
            return Task.Run(() =>
            {
                // Simple PDF implementation - for now return empty array
                return Array.Empty<byte>();
            });
        }

        public Task<byte[]> GenerateAuditReportPdfAsync(IEnumerable<AuditLog> auditLogs)
        {
            return Task.Run(() =>
            {
                // Simple PDF implementation - for now return empty array
                return Array.Empty<byte>();
            });
        }

        public async Task<Dictionary<string, object>> GetDashboardDataAsync()
        {
            var totalAssets = await _context.Assets.CountAsync();
            var inUseAssets = await _context.Assets.CountAsync(a => a.Status == AssetStatus.InUse);
            var underRepairAssets = await _context.Assets.CountAsync(a => a.Status == AssetStatus.UnderRepair);
            var expiredWarrantyAssets = await _context.Assets.CountAsync(a => a.WarrantyExpiry.HasValue && a.WarrantyExpiry.Value < DateTime.UtcNow);
            
            var recentMovements = await _context.AssetMovements
                .Include(m => m.Asset)
                .Include(m => m.PerformedByUser)
                .OrderByDescending(m => m.MovementDate)
                .Take(5)
                .ToListAsync();

            var upcomingMaintenance = await _context.MaintenanceRecords
                .Include(m => m.Asset)
                .Where(m => m.Status == MaintenanceStatus.Scheduled && m.ScheduledDate > DateTime.UtcNow)
                .OrderBy(m => m.ScheduledDate)
                .Take(5)
                .ToListAsync();

            return new Dictionary<string, object>
            {
                { "TotalAssets", totalAssets },
                { "InUseAssets", inUseAssets },
                { "UnderRepairAssets", underRepairAssets },
                { "ExpiredWarrantyAssets", expiredWarrantyAssets },
                { "RecentMovements", recentMovements },
                { "UpcomingMaintenance", upcomingMaintenance }
            };
        }

        public async Task<Dictionary<AssetCategory, int>> GetAssetsByCategoryAsync()
        {
            return await _context.Assets
                .GroupBy(a => a.Category)
                .ToDictionaryAsync(g => g.Key, g => g.Count());
        }

        public async Task<Dictionary<AssetStatus, int>> GetAssetsByStatusAsync()
        {
            return await _context.Assets
                .GroupBy(a => a.Status)
                .ToDictionaryAsync(g => g.Key, g => g.Count());
        }

        public async Task<Dictionary<string, int>> GetAssetsByLocationAsync()
        {
            var assets = await _context.Assets
                .Include(a => a.Location)
                .Where(a => a.Location != null)
                .ToListAsync();

            return assets
                .GroupBy(a => a.Location!.FullLocation)
                .ToDictionary(g => g.Key, g => g.Count());
        }
    }
}
