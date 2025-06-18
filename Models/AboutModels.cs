using System.ComponentModel.DataAnnotations;

namespace HospitalAssetTracker.Models
{
    public class AboutViewModel
    {
        public ApplicationInfo ApplicationInfo { get; set; } = new();
        public VersionInfo VersionInfo { get; set; } = new();
        public List<BugReport> BugReports { get; set; } = new();
        public SystemHealth SystemHealth { get; set; } = new();
        public List<ChangeLogEntry> ChangeLog { get; set; } = new();
        public DevelopmentInfo? DevelopmentInfo { get; set; }
    }

    public class ApplicationInfo
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Company { get; set; } = string.Empty;
        public string Copyright { get; set; } = string.Empty;
        public string Environment { get; set; } = string.Empty;
        public string Framework { get; set; } = string.Empty;
        public string Platform { get; set; } = string.Empty;
        public string Architecture { get; set; } = string.Empty;
        public DateTime ServerTime { get; set; }
        public string Timezone { get; set; } = string.Empty;
    }

    public class VersionInfo
    {
        public string Version { get; set; } = string.Empty;
        public DateTime BuildDate { get; set; }
        public string BuildMachine { get; set; } = string.Empty;
        public string BuildUser { get; set; } = string.Empty;
        public string GitCommit { get; set; } = string.Empty;
        public string? ReleaseNotes { get; set; }
    }

    public class BugReport
    {
        public string Id { get; set; } = string.Empty;
        
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;
        
        [Required]
        [StringLength(2000)]
        public string Description { get; set; } = string.Empty;
        
        [Required]
        public string Severity { get; set; } = "Medium"; // Low, Medium, High, Critical
        
        [Required]
        public string Category { get; set; } = "General"; // UI, Backend, Database, Performance, Security
        
        public string ReportedBy { get; set; } = string.Empty;
        public DateTime ReportedDate { get; set; }
        public string Status { get; set; } = "Open"; // Open, In Progress, Resolved, Closed
        public string? AssignedTo { get; set; }
        public DateTime? ResolvedDate { get; set; }
        public string? Resolution { get; set; }
        public string? UserAgent { get; set; }
        public string? RequestUrl { get; set; }
        public string? StackTrace { get; set; }
        public List<string> Screenshots { get; set; } = new();
        public int Priority { get; set; } = 3; // 1-5 scale
        public List<string> Tags { get; set; } = new();
    }

    public class SystemHealth
    {
        public string Status { get; set; } = string.Empty; // Healthy, Warning, Critical
        public TimeSpan Uptime { get; set; }
        public string MemoryUsage { get; set; } = string.Empty;
        public string DiskSpace { get; set; } = string.Empty;
        public int ActiveConnections { get; set; }
        public DateTime LastBackup { get; set; }
        public DateTime NextMaintenance { get; set; }
        public List<string> Warnings { get; set; } = new();
        public double CpuUsage { get; set; }
        public double DatabaseSize { get; set; }
        public int QueuedJobs { get; set; }
    }

    public class ChangeLogEntry
    {
        public string Version { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string Type { get; set; } = string.Empty; // Feature, Bug Fix, Enhancement, Security
        public string Description { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public List<string> IssueNumbers { get; set; } = new();
        public bool IsBreakingChange { get; set; }
        public string? MigrationRequired { get; set; }
    }

    public class DevelopmentInfo
    {
        public DateTime LastCompiled { get; set; }
        public string DatabaseProvider { get; set; } = string.Empty;
        public string CacheProvider { get; set; } = string.Empty;
        public string LogLevel { get; set; } = string.Empty;
        public bool HotReload { get; set; }
        public bool DebugMode { get; set; }
        public List<string> EnabledFeatures { get; set; } = new();
        public Dictionary<string, string> ConfigSettings { get; set; } = new();
    }

    public class BugReportRequest
    {
        [Required]
        public string Title { get; set; } = string.Empty;
        
        [Required]
        public string Description { get; set; } = string.Empty;
        
        [Required]
        public string Severity { get; set; } = string.Empty;
        
        [Required]
        public string Category { get; set; } = string.Empty;
        
        public string? StackTrace { get; set; }
        public List<string> Screenshots { get; set; } = new();
        public string? ReproductionSteps { get; set; }
    }

    public class VersionUpdateRequest
    {
        [Required]
        public string VersionType { get; set; } = string.Empty; // major, minor, patch, build
        
        public string? ReleaseNotes { get; set; }
        public bool CreateBackup { get; set; } = true;
        public bool NotifyUsers { get; set; } = false;
    }

    public class AutoFixResult
    {
        public bool Success { get; set; }
        public List<string> FixedIssues { get; set; } = new();
        public List<string> RemainingIssues { get; set; } = new();
        public TimeSpan ExecutionTime { get; set; }
        public string? ErrorMessage { get; set; }
    }

    public class SystemStatistics
    {
        public int TotalAssets { get; set; }
        public int TotalUsers { get; set; }
        public int TotalRequests { get; set; }
        public int OpenBugs { get; set; }
        public int ResolvedBugs { get; set; }
        public double SystemUptime { get; set; }
        public DateTime LastUpdate { get; set; }
        public Dictionary<string, int> ModuleUsage { get; set; } = new();
    }
}
