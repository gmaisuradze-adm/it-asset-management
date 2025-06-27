namespace HospitalAssetTracker.Models
{
    /// <summary>
    /// Model for batch assignment operations
    /// </summary>
    public class BatchAssignModel
    {
        public List<int> RequestIds { get; set; } = new();
        public string AssignedToUserId { get; set; } = string.Empty;
        public string? Comments { get; set; }
    }

    /// <summary>
    /// Model for batch status updates
    /// </summary>
    public class BatchStatusUpdateModel
    {
        public List<int> RequestIds { get; set; } = new();
        public RequestStatus NewStatus { get; set; }
        public string? Comments { get; set; }
    }

    /// <summary>
    /// Model for report generation requests
    /// </summary>
    public class ReportGenerationModel
    {
        public string ReportType { get; set; } = string.Empty;
        public int? TimeRangeDays { get; set; }
        public int? TimeRangeMonths { get; set; }
        public int? ForecastDays { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public List<string>? Departments { get; set; }
        public List<RequestStatus>? StatusFilters { get; set; }
        public List<RequestPriority>? PriorityFilters { get; set; }
    }

    /// <summary>
    /// Request template model for creating standardized requests
    /// </summary>
    public class RequestTemplate
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public RequestType RequestType { get; set; }
        public RequestPriority DefaultPriority { get; set; }
        public string Subject { get; set; } = string.Empty;
        public string? ItemCategory { get; set; }
        public string? Department { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public string CreatedBy { get; set; } = string.Empty;
    }

    /// <summary>
    /// Advanced search model for requests with multiple filters
    /// </summary>
    public class AdvancedRequestSearchModel : RequestSearchModel
    {
        public List<string>? Departments { get; set; }
        public List<RequestStatus>? StatusFilters { get; set; }
        public List<RequestPriority>? PriorityFilters { get; set; }
        public List<RequestType>? TypeFilters { get; set; }
        public DateTime? CreatedAfter { get; set; }
        public DateTime? CreatedBefore { get; set; }
        public DateTime? DueAfter { get; set; }
        public DateTime? DueBefore { get; set; }
        public bool? HasAsset { get; set; }
        public bool? IsOverdue { get; set; }
        public string? AssignedToUserId { get; set; }
        public string? RequestedByUserId { get; set; }
        public string? KeywordSearch { get; set; }
        public int? MinDaysOpen { get; set; }
        public int? MaxDaysOpen { get; set; }
    }

    /// <summary>
    /// Request bulk operation result model
    /// </summary>
    public class RequestBulkOperationResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int SuccessCount { get; set; }
        public int FailureCount { get; set; }
        public List<string> Errors { get; set; } = new();
        public List<string> Warnings { get; set; } = new();
        public Dictionary<string, object> AdditionalData { get; set; } = new();
    }

    /// <summary>
    /// Request analytics summary model
    /// </summary>
    public class RequestAnalyticsSummary
    {
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
        public int TotalRequests { get; set; }
        public int OpenRequests { get; set; }
        public int InProgressRequests { get; set; }
        public int CompletedRequests { get; set; }
        public int OverdueRequests { get; set; }
        public double AverageResolutionTimeHours { get; set; }
        public double SlaComplianceRate { get; set; }
        public Dictionary<RequestType, int> RequestsByType { get; set; } = new();
        public Dictionary<RequestPriority, int> RequestsByPriority { get; set; } = new();
        public Dictionary<string, int> RequestsByDepartment { get; set; } = new();
        public List<TopRequestor> TopRequestors { get; set; } = new();
        public List<TopAssignee> TopAssignees { get; set; } = new();
    }

    /// <summary>
    /// Top requestor summary
    /// </summary>
    public class TopRequestor
    {
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public int RequestCount { get; set; }
        public double AverageCompletionTimeHours { get; set; }
    }

    /// <summary>
    /// Top assignee summary
    /// </summary>
    public class TopAssignee
    {
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public int AssignedCount { get; set; }
        public int CompletedCount { get; set; }
        public int OverdueCount { get; set; }
        public double CompletionRate { get; set; }
        public double AverageResolutionTimeHours { get; set; }
    }
}
