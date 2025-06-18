using HospitalAssetTracker.Data;
using HospitalAssetTracker.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace HospitalAssetTracker.Services
{
    public class VersionService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<VersionService> _logger;
        private const string VERSION_FILE = "version.json";

        public VersionService(ApplicationDbContext context, ILogger<VersionService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<string> GetCurrentVersionAsync()
        {
            try
            {
                if (File.Exists(VERSION_FILE))
                {
                    var versionJson = await File.ReadAllTextAsync(VERSION_FILE);
                    var versionData = JsonSerializer.Deserialize<VersionData>(versionJson);
                    if (versionData != null && !string.IsNullOrEmpty(versionData.Version))
                    {
                        return versionData.Version;
                    }
                }

                var currentVersion = await _context.SystemVersions
                    .Where(v => v.IsCurrent)
                    .OrderByDescending(v => v.ReleaseDate)
                    .FirstOrDefaultAsync();

                return currentVersion?.VersionNumber ?? "1.0.0";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current version");
                return "1.0.0";
            }
        }

        public async Task<string> CreateNewVersionAsync(string releaseNotes = "", int bugsFixed = 0, int featuresAdded = 0)
        {
            try
            {
                var currentVersion = await GetCurrentVersionAsync();
                var newVersionNumber = IncrementVersion(currentVersion);

                var oldVersions = await _context.SystemVersions
                    .Where(v => v.IsCurrent)
                    .ToListAsync();

                foreach (var oldVersion in oldVersions)
                {
                    oldVersion.IsCurrent = false;
                }

                var newVersion = new SystemVersion
                {
                    VersionNumber = newVersionNumber,
                    ReleaseNotes = releaseNotes,
                    BugsFixed = bugsFixed,
                    FeaturesAdded = featuresAdded,
                    ReleaseDate = DateTime.UtcNow,
                    IsCurrent = true,
                    CreatedBy = "System",
                    CreatedAt = DateTime.UtcNow
                };

                _context.SystemVersions.Add(newVersion);
                await _context.SaveChangesAsync();

                await UpdateVersionFileAsync(newVersionNumber);

                _logger.LogInformation($"New version created: {newVersionNumber}");
                return newVersionNumber;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating new version");
                return await GetCurrentVersionAsync();
            }
        }

        private string IncrementVersion(string currentVersion)
        {
            try
            {
                var parts = currentVersion.Split('.');
                if (parts.Length >= 3)
                {
                    var major = int.Parse(parts[0]);
                    var minor = int.Parse(parts[1]);
                    var patch = int.Parse(parts[2]);
                    patch++;
                    return $"{major}.{minor}.{patch}";
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"Error incrementing version {currentVersion}");
            }
            return "1.0.1";
        }

        private async Task UpdateVersionFileAsync(string newVersion)
        {
            try
            {
                var versionData = new VersionData
                {
                    Version = newVersion,
                    BuildDate = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
                    BuildNumber = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString()
                };

                var json = JsonSerializer.Serialize(versionData, new JsonSerializerOptions
                {
                    WriteIndented = true
                });

                await File.WriteAllTextAsync(VERSION_FILE, json);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating version file");
            }
        }

        public async Task<string> IncrementVersionForBugFixAsync(string fixDescription = "")
        {
            try
            {
                var currentVersion = await _context.SystemVersions
                    .FirstOrDefaultAsync(v => v.IsCurrent);

                if (currentVersion != null)
                {
                    currentVersion.BugsFixed++;
                    
                    if (!string.IsNullOrEmpty(fixDescription))
                    {
                        currentVersion.ReleaseNotes += $"\n- Bug fix: {fixDescription}";
                    }

                    await _context.SaveChangesAsync();
                    return currentVersion.VersionNumber;
                }

                return await CreateNewVersionAsync($"Bug fix: {fixDescription}", 1, 0);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error incrementing version for bug fix");
                return await GetCurrentVersionAsync();
            }
        }

        public async Task<List<SystemVersion>> GetVersionHistoryAsync(int limit = 10)
        {
            return await _context.SystemVersions
                .OrderByDescending(v => v.ReleaseDate)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<object> GetVersionStatsAsync()
        {
            try
            {
                var totalVersions = await _context.SystemVersions.CountAsync();
                var totalBugsFixed = await _context.SystemVersions.SumAsync(v => v.BugsFixed);
                var totalFeaturesAdded = await _context.SystemVersions.SumAsync(v => v.FeaturesAdded);
                
                var recentVersions = await _context.SystemVersions
                    .OrderByDescending(v => v.ReleaseDate)
                    .Take(5)
                    .Select(v => new 
                    { 
                        v.VersionNumber, 
                        v.ReleaseDate, 
                        v.BugsFixed, 
                        v.FeaturesAdded,
                        v.IsCurrent 
                    })
                    .ToListAsync();

                return new
                {
                    TotalVersions = totalVersions,
                    TotalBugsFixed = totalBugsFixed,
                    TotalFeaturesAdded = totalFeaturesAdded,
                    CurrentVersion = await GetCurrentVersionAsync(),
                    RecentVersions = recentVersions
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting version stats");
                return new { Error = "Unable to load version stats" };
            }
        }

        // ახალი ვერსიის შექმნა კონკრეტული პარამეტრებით
        public async Task<int> CreateVersionAsync(string versionNumber, string releaseNotes, int featuresAdded, string createdBy)
        {
            try
            {
                // ძველი ვერსიების გამორთვა
                var oldVersions = await _context.SystemVersions
                    .Where(v => v.IsCurrent)
                    .ToListAsync();
                    
                foreach (var oldVersion in oldVersions)
                {
                    oldVersion.IsCurrent = false;
                }

                var newVersion = new SystemVersion
                {
                    VersionNumber = versionNumber,
                    ReleaseNotes = releaseNotes,
                    BugsFixed = 0,
                    FeaturesAdded = featuresAdded,
                    ReleaseDate = DateTime.UtcNow,
                    IsCurrent = true,
                    CreatedBy = createdBy,
                    CreatedAt = DateTime.UtcNow
                };

                _context.SystemVersions.Add(newVersion);
                await _context.SaveChangesAsync();

                await UpdateVersionFileAsync(versionNumber);

                _logger.LogInformation($"New version {versionNumber} created by {createdBy}");
                return newVersion.Id;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error creating version {versionNumber}");
                return 0;
            }
        }
    }

    public class VersionData
    {
        public string Version { get; set; } = string.Empty;
        public string BuildDate { get; set; } = string.Empty;
        public string BuildNumber { get; set; } = string.Empty;
    }
}
