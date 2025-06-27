using Microsoft.EntityFrameworkCore;
using ClosedXML.Excel;
using HospitalAssetTracker.Data;
using HospitalAssetTracker.Models;
using System.Text; // Added for StringBuilder

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
                var sb = new StringBuilder();
                sb.AppendLine("Asset Report");
                sb.AppendLine($"Generated on: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
                sb.AppendLine("======================================================================================================================="); // Adjusted width
                sb.AppendLine();

                if (!assets.Any())
                {
                    sb.AppendLine("No assets to report.");
                }
                else
                {
                    sb.AppendLine($"Total Assets: {assets.Count()}\n");
                    sb.AppendLine("-----------------------------------------------------------------------------------------------------------------------"); // Adjusted width
                    sb.AppendLine("| Asset Tag    | Category        | Brand          | Model                | Status         | Location                     | Assigned To      | Serial No.     |"); // Added Serial
                    sb.AppendLine("|--------------|-----------------|----------------|----------------------|----------------|------------------------------|------------------|----------------|"); // Adjusted width

                    foreach (var asset in assets)
                    {
                        sb.AppendFormat("| {0,-12} | {1,-15} | {2,-14} | {3,-20} | {4,-14} | {5,-28} | {6,-16} | {7,-14} |\n", // Added Serial
                            asset.AssetTag?.Truncate(12) ?? "N/A",
                            asset.Category.ToString().Truncate(15),
                            asset.Brand?.Truncate(14) ?? "N/A",
                            asset.Model?.Truncate(20) ?? "N/A",
                            asset.Status.ToString().Truncate(14),
                            asset.Location?.FullLocation?.Truncate(28) ?? "N/A",
                            asset.AssignedToUser?.FullName?.Truncate(16) ?? "N/A",
                            asset.SerialNumber?.Truncate(14) ?? "N/A" // Added Serial
                        );
                    }
                    sb.AppendLine("-----------------------------------------------------------------------------------------------------------------------"); // Adjusted width
                }
                
                string textContent = sb.ToString();
                return Encoding.UTF8.GetBytes(textContent);
            });
        }

        public Task<byte[]> GenerateMaintenanceReportPdfAsync(IEnumerable<MaintenanceRecord> maintenanceRecords)
        {
            return Task.Run(() =>
            {
                var sb = new StringBuilder();
                sb.AppendLine("Maintenance Report");
                sb.AppendLine($"Generated on: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
                sb.AppendLine("================================================================================");
                sb.AppendLine();

                if (!maintenanceRecords.Any())
                {
                    sb.AppendLine("No maintenance records to report.");
                }
                else
                {
                    sb.AppendLine($"Total Records: {maintenanceRecords.Count()}\n");
                    sb.AppendLine("--------------------------------------------------------------------------------");
                    sb.AppendLine("| Asset Tag    | Title                      | Type                | Scheduled    | Completed    | Status         |");
                    sb.AppendLine("|--------------|----------------------------|---------------------|--------------|--------------|----------------|");

                    foreach (var record in maintenanceRecords)
                    {
                        sb.AppendFormat("| {0,-12} | {1,-26} | {2,-19} | {3,-12:yyyy-MM-dd} | {4,-12:yyyy-MM-dd} | {5,-14} |\n",
                            record.Asset?.AssetTag?.Truncate(12) ?? "N/A",
                            record.Title?.Truncate(26) ?? "N/A",
                            record.MaintenanceType.ToString().Truncate(19),
                            record.ScheduledDate,
                            record.CompletedDate,
                            record.Status.ToString().Truncate(14)
                        );
                    }
                    sb.AppendLine("--------------------------------------------------------------------------------");
                }
                return Encoding.UTF8.GetBytes(sb.ToString());
            });
        }

        public Task<byte[]> GenerateAuditReportPdfAsync(IEnumerable<AuditLog> auditLogs)
        {
            return Task.Run(() =>
            {
                var sb = new StringBuilder();
                sb.AppendLine("Audit Log Report");
                sb.AppendLine($"Generated on: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
                sb.AppendLine("====================================================================================================================");
                sb.AppendLine();

                if (!auditLogs.Any())
                {
                    sb.AppendLine("No audit logs to report.");
                }
                else
                {
                    sb.AppendLine($"Total Logs: {auditLogs.Count()}\n");
                    sb.AppendLine("--------------------------------------------------------------------------------------------------------------------");
                    sb.AppendLine("| Timestamp           | User ID      | Action         | Entity Type   | Entity ID | Description                                      |"); // Corrected headers
                    sb.AppendLine("|---------------------|--------------|----------------|---------------|-----------|--------------------------------------------------|");

                    foreach (var log in auditLogs)
                    {
                        sb.AppendFormat("| {0,-19:yyyy-MM-dd HH:mm:ss} | {1,-12} | {2,-14} | {3,-13} | {4,-9} | {5,-48} |\n",
                            log.Timestamp,
                            log.UserId?.Truncate(12) ?? "N/A",
                            log.Action.ToString().Truncate(14), // Corrected: Action is non-nullable enum
                            log.EntityType?.Truncate(13) ?? "N/A",    // Corrected: EntityType
                            log.EntityId?.ToString().Truncate(9) ?? "N/A", // Corrected: EntityId
                            log.Description?.Truncate(48) ?? "N/A"   // Corrected: Description
                        );
                    }
                    sb.AppendLine("--------------------------------------------------------------------------------------------------------------------");
                }
                return Encoding.UTF8.GetBytes(sb.ToString());
            });
        }

        public async Task<Dictionary<string, object>> GetDashboardDataAsync()
        {
            var totalAssets = await _context.Assets.CountAsync();
            var inUseAssets = await _context.Assets.CountAsync(a => a.Status == AssetStatus.InUse);
            var underMaintenanceAssets = await _context.Assets.CountAsync(a => a.Status == AssetStatus.UnderMaintenance);
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
                { "InMaintenanceAssets", underMaintenanceAssets },
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
            var assetsWithLocations = await _context.Assets
                .Include(a => a.Location) // Ensure Location is loaded
                .Where(a => a.LocationId != null && a.Location != null) // Filter for assets that have a location
                .ToListAsync(); // Bring data into memory

            // Perform grouping in memory
            var groupedByLocation = assetsWithLocations
                .GroupBy(a => a.Location!.FullLocation) // Now this uses the C# property on in-memory objects
                .ToDictionary(g => g.Key, g => g.Count());

            return groupedByLocation;
        }

        public async Task<IEnumerable<Asset>> GetExpiredWarrantyAssetsReportAsync()
        {
            var today = DateTime.UtcNow.Date;
            return await _context.Assets
                .Include(a => a.Location) // Include necessary related data for the report
                .Include(a => a.AssignedToUser)
                .Where(a => a.WarrantyExpiry.HasValue && a.WarrantyExpiry.Value.Date <= today)
                .OrderBy(a => a.AssetTag)
                .ToListAsync();
        }
    }

    // Helper extension method for truncating strings (moved to be a top-level static class)
    public static class StringExtensions
    {
        public static string Truncate(this string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value)) return value;
            return value.Length <= maxLength ? value : value.Substring(0, maxLength);
        }
    }
}
