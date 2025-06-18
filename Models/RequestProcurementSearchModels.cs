using System.ComponentModel.DataAnnotations;

namespace HospitalAssetTracker.Models
{
    public static class RequestSearchModels
    {
        public class RequestSearchCriteria
        {
            public string? SearchTerm { get; set; }
            public RequestType? RequestType { get; set; }
            public RequestPriority? Priority { get; set; }
            public RequestStatus? Status { get; set; }
            public string? Department { get; set; }
            public string? RequestedByUserId { get; set; }
            public string? AssignedToUserId { get; set; }
            public DateTime? RequestDateFrom { get; set; }
            public DateTime? RequestDateTo { get; set; }
            public DateTime? RequiredByDateFrom { get; set; }
            public DateTime? RequiredByDateTo { get; set; }
            public int? RelatedAssetId { get; set; }
            public int? LocationId { get; set; }
            public bool? IsOverdue { get; set; }
            public decimal? EstimatedCostFrom { get; set; }
            public decimal? EstimatedCostTo { get; set; }

            // Pagination
            public int Page { get; set; } = 1;
            public int PageSize { get; set; } = 20;
            public string? SortBy { get; set; } = "RequestDate";
            public string? SortDirection { get; set; } = "desc";
        }

        public class RequestDashboardData
        {
            public int TotalRequests { get; set; }
            public int PendingRequests { get; set; }
            public int InProgressRequests { get; set; }
            public int CompletedRequests { get; set; }
            public int OverdueRequests { get; set; }
            public int CriticalPriorityRequests { get; set; }
            public decimal TotalEstimatedCost { get; set; }
            public decimal TotalActualCost { get; set; }
            public double AverageResolutionTimeHours { get; set; }
            public List<RequestTypeCount> RequestsByType { get; set; } = new();
            public List<DepartmentRequestCount> RequestsByDepartment { get; set; } = new();
            public List<ITRequest> RecentRequests { get; set; } = new();
            public List<ITRequest> OverdueRequestsList { get; set; } = new();
        }

        public class RequestTypeCount
        {
            public RequestType RequestType { get; set; }
            public string TypeName { get; set; } = string.Empty;
            public int Count { get; set; }
        }

        public class DepartmentRequestCount
        {
            public string Department { get; set; } = string.Empty;
            public int Count { get; set; }
            public int PendingCount { get; set; }
            public int CompletedCount { get; set; }
        }

        public class RequestReport
        {
            public string ReportTitle { get; set; } = string.Empty;
            public DateTime GeneratedDate { get; set; }
            public RequestSearchCriteria Criteria { get; set; } = new();
            public List<ITRequest> Requests { get; set; } = new();
            public RequestSummary Summary { get; set; } = new();
        }

        public class RequestSummary
        {
            public int TotalCount { get; set; }
            public int CompletedCount { get; set; }
            public int PendingCount { get; set; }
            public decimal TotalCost { get; set; }
            public double AverageResolutionTimeHours { get; set; }
            public Dictionary<RequestStatus, int> StatusBreakdown { get; set; } = new();
            public Dictionary<RequestType, int> TypeBreakdown { get; set; } = new();
            public Dictionary<RequestPriority, int> PriorityBreakdown { get; set; } = new();
        }

        public class RequestWorkflowData
        {
            public ITRequest Request { get; set; } = null!;
            public List<RequestApproval> Approvals { get; set; } = new();
            public List<RequestComment> Comments { get; set; } = new();
            public List<RequestAttachment> Attachments { get; set; } = new();
            public List<ApplicationUser> AvailableApprovers { get; set; } = new();
            public List<ApplicationUser> AvailableAssignees { get; set; } = new();
            public bool CanEdit { get; set; }
            public bool CanApprove { get; set; }
            public bool CanAssign { get; set; }
            public bool CanComplete { get; set; }
        }

        public class RequestIntegrationData
        {
            public ITRequest Request { get; set; } = null!;
            public Asset? RelatedAsset { get; set; }
            public InventoryItem? RequiredInventoryItem { get; set; }
            public InventoryItem? ProvidedInventoryItem { get; set; }
            public List<InventoryItem> AvailableInventoryItems { get; set; } = new();
            public List<ProcurementRequest> RelatedProcurementRequests { get; set; } = new();
            public bool RequiresProcurement { get; set; }
            public bool InventoryAvailable { get; set; }
        }
    }

    public static class ProcurementSearchModels
    {
        public class ProcurementSearchCriteria
        {
            public string? SearchTerm { get; set; }
            public ProcurementType? ProcurementType { get; set; }
            public ProcurementCategory? Category { get; set; }
            public ProcurementStatus? Status { get; set; }
            public ProcurementMethod? Method { get; set; }
            public ProcurementSource? Source { get; set; }
            public string? Department { get; set; }
            public string? RequestedByUserId { get; set; }
            public string? AssignedToProcurementOfficerId { get; set; }
            public int? VendorId { get; set; }
            public DateTime? RequestDateFrom { get; set; }
            public DateTime? RequestDateTo { get; set; }
            public DateTime? RequiredByDateFrom { get; set; }
            public DateTime? RequiredByDateTo { get; set; }
            public decimal? EstimatedBudgetFrom { get; set; }
            public decimal? EstimatedBudgetTo { get; set; }
            public bool? IsOverdue { get; set; }
            public bool? RequiresTender { get; set; }
            public string? FiscalYear { get; set; }

            // Pagination
            public int Page { get; set; } = 1;
            public int PageSize { get; set; } = 20;
            public string? SortBy { get; set; } = "RequestDate";
            public string? SortDirection { get; set; } = "desc";
        }

        public class ProcurementDashboardData
        {
            public int TotalProcurements { get; set; }
            public int PendingApproval { get; set; }
            public int InProcurement { get; set; }
            public int Completed { get; set; }
            public int Overdue { get; set; }
            public decimal TotalBudget { get; set; }
            public decimal TotalSpent { get; set; }
            public decimal PendingCommitments { get; set; }
            public int ActiveVendors { get; set; }
            public double AverageProcurementTime { get; set; }
            public List<ProcurementTypeSpend> SpendByType { get; set; } = new();
            public List<VendorPerformance> TopVendors { get; set; } = new();
            public List<ProcurementRequest> RecentProcurements { get; set; } = new();
            public List<ProcurementRequest> OverdueProcurements { get; set; } = new();
        }

        public class ProcurementTypeSpend
        {
            public ProcurementType ProcurementType { get; set; }
            public string TypeName { get; set; } = string.Empty;
            public decimal TotalSpend { get; set; }
            public int Count { get; set; }
        }

        public class VendorPerformance
        {
            public Vendor Vendor { get; set; } = null!;
            public decimal TotalSpend { get; set; }
            public int TotalOrders { get; set; }
            public decimal OnTimeDeliveryRate { get; set; }
            public decimal QualityRate { get; set; }
            public decimal PerformanceScore { get; set; }
        }

        public class ProcurementReport
        {
            public string ReportTitle { get; set; } = string.Empty;
            public DateTime GeneratedDate { get; set; }
            public ProcurementSearchCriteria Criteria { get; set; } = new();
            public List<ProcurementRequest> Procurements { get; set; } = new();
            public ProcurementSummary Summary { get; set; } = new();
        }

        public class ProcurementSummary
        {
            public int TotalCount { get; set; }
            public int CompletedCount { get; set; }
            public int PendingCount { get; set; }
            public decimal TotalBudget { get; set; }
            public decimal TotalSpent { get; set; }
            public decimal SavingsAchieved { get; set; }
            public double AverageProcurementDays { get; set; }
            public Dictionary<ProcurementStatus, int> StatusBreakdown { get; set; } = new();
            public Dictionary<ProcurementType, decimal> TypeSpendBreakdown { get; set; } = new();
            public Dictionary<string, decimal> VendorSpendBreakdown { get; set; } = new();
        }

        public class ProcurementWorkflowData
        {
            public ProcurementRequest Request { get; set; } = null!;
            public List<ProcurementApproval> Approvals { get; set; } = new();
            public List<VendorQuote> Quotes { get; set; } = new();
            public List<ProcurementDocument> Documents { get; set; } = new();
            public List<Vendor> AvailableVendors { get; set; } = new();
            public List<ApplicationUser> AvailableApprovers { get; set; } = new();
            public bool CanEdit { get; set; }
            public bool CanApprove { get; set; }
            public bool CanStartProcurement { get; set; }
            public bool CanReceive { get; set; }
            public bool RequiresTender { get; set; }
            public bool RequiresQuotes { get; set; }
        }

        public class ProcurementIntegrationData
        {
            public ProcurementRequest Request { get; set; } = null!;
            public ITRequest? OriginatingRequest { get; set; }
            public Asset? ReplacementForAsset { get; set; }
            public InventoryItem? TriggeredByInventoryItem { get; set; }
            public List<ProcurementItem> Items { get; set; } = new();
            public bool AutoInventoryUpdate { get; set; }
            public bool AutoAssetRegistration { get; set; }
            public bool AutoRequestFulfillment { get; set; }
        }

        public class VendorQuoteComparison
        {
            public ProcurementRequest Request { get; set; } = null!;
            public List<VendorQuoteComparisonItem> QuoteComparisons { get; set; } = new();
            public VendorQuote? RecommendedQuote { get; set; }
            public string? RecommendationReason { get; set; }
        }

        public class VendorQuoteComparisonItem
        {
            public VendorQuote Quote { get; set; } = null!;
            public decimal TotalScore { get; set; }
            public decimal PriceScore { get; set; }
            public decimal DeliveryScore { get; set; }
            public decimal VendorRatingScore { get; set; }
            public bool MeetsSpecifications { get; set; }
            public string? Notes { get; set; }
        }
    }
    
    // Simple search models for the basic implementation
    public class RequestSearchModel
    {
        public string? SearchTerm { get; set; }
        public RequestType? RequestType { get; set; }
        public RequestStatus? Status { get; set; }
        public RequestPriority? Priority { get; set; }
        public string? Department { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string? SortBy { get; set; }
        public bool SortDesc { get; set; }
    }

    public class ProcurementSearchModel
    {
        public string? SearchTerm { get; set; }
        public ProcurementType? ProcurementType { get; set; }
        public ProcurementStatus? Status { get; set; }
        public ProcurementPriority? Priority { get; set; }
        public int? VendorId { get; set; }
        public decimal? AmountFrom { get; set; }
        public decimal? AmountTo { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string? SortBy { get; set; }
        public bool SortDesc { get; set; }
    }
}
