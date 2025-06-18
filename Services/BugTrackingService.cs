using HospitalAssetTracker.Data;
using HospitalAssetTracker.Models;
using HospitalAssetTracker.Services;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace HospitalAssetTracker.Services
{
    public class BugTrackingService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<BugTrackingService> _logger;
        private readonly VersionService _versionService;

        public BugTrackingService(ApplicationDbContext context, ILogger<BugTrackingService> logger, VersionService versionService)
        {
            _context = context;
            _logger = logger;
            _versionService = versionService;
        }

        // ავტომატური ბაგის რეგისტრაცია
        public async Task<int> RegisterBugAsync(string title, string description, string moduleName, 
            string errorMessage = "", string stackTrace = "", string severity = "Medium")
        {
            try
            {
                var currentVersion = await _versionService.GetCurrentVersionAsync();
                
                var bug = new BugTracking
                {
                    BugTitle = title,
                    BugDescription = description,
                    ModuleName = moduleName,
                    ErrorMessage = errorMessage,
                    StackTrace = stackTrace,
                    Severity = severity,
                    Status = "Open",
                    VersionFound = currentVersion,
                    ReportedBy = "System Auto-Detection",
                    ReportedDate = DateTime.UtcNow
                };

                _context.BugTrackings.Add(bug);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Bug registered automatically: {title} in module {moduleName}");
                return bug.Id;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error registering bug: {title}");
                return -1;
            }
        }

        // ბაგის გამოსწორების რეგისტრაცია
        public async Task<bool> MarkBugAsFixedAsync(int bugId, string fixDescription, string filesChanged = "")
        {
            try
            {
                var bug = await _context.BugTrackings.FindAsync(bugId);
                if (bug == null) return false;

                var currentVersion = await _versionService.GetCurrentVersionAsync();

                bug.Status = "Fixed";
                bug.FixDescription = fixDescription;
                bug.FixedDate = DateTime.UtcNow;
                bug.VersionFixed = currentVersion;
                bug.UpdatedAt = DateTime.UtcNow;

                // შევქმნათ fix history ჩანაწერი
                var versionEntity = await _context.SystemVersions
                    .FirstOrDefaultAsync(v => v.VersionNumber == currentVersion);

                if (versionEntity != null)
                {
                    var fixHistory = new BugFixHistory
                    {
                        BugId = bugId,
                        VersionId = versionEntity.Id,
                        FixDetails = fixDescription,
                        FilesChanged = filesChanged,
                        TestStatus = "Passed",
                        FixedDate = DateTime.UtcNow,
                        FixedBy = "System"
                    };

                    _context.BugFixHistories.Add(fixHistory);
                    
                    // გავაუმჯობესოთ ვერსიის სტატისტიკა
                    versionEntity.BugsFixed++;
                }

                await _context.SaveChangesAsync();
                
                _logger.LogInformation($"Bug {bugId} marked as fixed in version {currentVersion}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error marking bug {bugId} as fixed");
                return false;
            }
        }

        // მოდულის მიხედვით ბაგების ავტომატური ტრეკინგი
        public async Task TrackModuleBugAsync(string moduleName, Exception exception)
        {
            try
            {
                // შევამოწმოთ არსებობს თუ არა ანალოგიური ბაგი
                var existingBug = await _context.BugTrackings
                    .FirstOrDefaultAsync(b => b.ModuleName == moduleName && 
                                            b.ErrorMessage == exception.Message && 
                                            b.Status != "Closed");

                if (existingBug == null)
                {
                    await RegisterBugAsync(
                        title: $"Error in {moduleName}",
                        description: $"Automatic bug detection in module {moduleName}",
                        moduleName: moduleName,
                        errorMessage: exception.Message,
                        stackTrace: exception.StackTrace ?? "",
                        severity: DetermineSeverity(exception)
                    );
                }
                else
                {
                    // განვაახლოთ არსებული ბაგის ინფორმაცია
                    existingBug.UpdatedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error tracking bug for module {moduleName}");
            }
        }

        // ბაგების ანალიზი და სტატისტიკა
        public async Task<object> GetBugAnalyticsAsync()
        {
            try
            {
                var totalBugs = await _context.BugTrackings.CountAsync();
                var openBugs = await _context.BugTrackings.CountAsync(b => b.Status == "Open");
                var fixedBugs = await _context.BugTrackings.CountAsync(b => b.Status == "Fixed");
                var closedBugs = await _context.BugTrackings.CountAsync(b => b.Status == "Closed");

                var bugsByModule = await _context.BugTrackings
                    .GroupBy(b => b.ModuleName)
                    .Select(g => new { Module = g.Key, Count = g.Count() })
                    .OrderByDescending(x => x.Count)
                    .ToListAsync();

                var bugsBySeverity = await _context.BugTrackings
                    .GroupBy(b => b.Severity)
                    .Select(g => new { Severity = g.Key, Count = g.Count() })
                    .ToListAsync();

                var recentFixes = await _context.BugTrackings
                    .Where(b => b.Status == "Fixed" && b.FixedDate >= DateTime.UtcNow.AddDays(-30))
                    .OrderByDescending(b => b.FixedDate)
                    .Take(10)
                    .Select(b => new { 
                        b.BugTitle, 
                        b.ModuleName, 
                        b.FixedDate, 
                        b.VersionFixed 
                    })
                    .ToListAsync();

                return new
                {
                    Summary = new
                    {
                        TotalBugs = totalBugs,
                        OpenBugs = openBugs,
                        FixedBugs = fixedBugs,
                        ClosedBugs = closedBugs,
                        FixRate = totalBugs > 0 ? Math.Round((double)fixedBugs / totalBugs * 100, 2) : 0
                    },
                    BugsByModule = bugsByModule,
                    BugsBySeverity = bugsBySeverity,
                    RecentFixes = recentFixes
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting bug analytics");
                return new { Error = "Unable to load analytics" };
            }
        }

        private string DetermineSeverity(Exception exception)
        {
            var message = exception.Message.ToLower();
            
            if (message.Contains("database") || message.Contains("connection") || 
                message.Contains("timeout") || message.Contains("sql"))
                return "High";
            
            if (message.Contains("null") || message.Contains("not found") || 
                message.Contains("invalid"))
                return "Medium";
            
            return "Low";
        }

        // ყველა ღია ბაგის მიღება
        public async Task<List<BugTracking>> GetOpenBugsAsync()
        {
            return await _context.BugTrackings
                .Where(b => b.Status == "Open" || b.Status == "InProgress")
                .OrderByDescending(b => b.ReportedDate)
                .ToListAsync();
        }

        // მოდულის მიხედვით ბაგების მიღება
        public async Task<List<BugTracking>> GetBugsByModuleAsync(string moduleName)
        {
            return await _context.BugTrackings
                .Where(b => b.ModuleName.Contains(moduleName))
                .OrderByDescending(b => b.ReportedDate)
                .ToListAsync();
        }

        // ყველა ბაგის მიღება
        public async Task<List<BugTracking>> GetAllBugsAsync()
        {
            return await _context.BugTrackings
                .OrderByDescending(b => b.ReportedDate)
                .ToListAsync();
        }

        // ბაგის სტატისტიკა
        public async Task<object> GetBugStatsAsync()
        {
            var totalBugs = await _context.BugTrackings.CountAsync();
            var openBugs = await _context.BugTrackings.CountAsync(b => b.Status == "Open");
            var fixedBugs = await _context.BugTrackings.CountAsync(b => b.Status == "Fixed");
            var highSeverityBugs = await _context.BugTrackings.CountAsync(b => b.Severity == "High");
            
            var moduleStats = await _context.BugTrackings
                .GroupBy(b => b.ModuleName)
                .Select(g => new { Module = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .ToListAsync();

            var severityStats = await _context.BugTrackings
                .GroupBy(b => b.Severity)
                .Select(g => new { Severity = g.Key, Count = g.Count() })
                .ToListAsync();

            return new
            {
                TotalBugs = totalBugs,
                OpenBugs = openBugs,
                FixedBugs = fixedBugs,
                HighSeverityBugs = highSeverityBugs,
                FixRate = totalBugs > 0 ? (double)fixedBugs / totalBugs * 100 : 0,
                ModuleStats = moduleStats,
                SeverityStats = severityStats
            };
        }
    }
}
