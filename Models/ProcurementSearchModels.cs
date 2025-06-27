using System.ComponentModel.DataAnnotations;

namespace HospitalAssetTracker.Models
{
    public class ProcurementSearchModels
    {
        // Enhanced Advanced Search Model for comprehensive procurement filtering
        public class AdvancedProcurementSearchModel
        {
            // Basic Search
            [Display(Name = "Search Term")]
            public string? SearchTerm { get; set; }

            [Display(Name = "Procurement Number")]
            public string? ProcurementNumber { get; set; }

            [Display(Name = "Title")]
            public string? Title { get; set; }

            [Display(Name = "Description")]
            public string? Description { get; set; }

            // Status and Type Filters
            [Display(Name = "Status")]
            public ProcurementStatus? Status { get; set; }

            [Display(Name = "Procurement Type")]
            public ProcurementType? ProcurementType { get; set; }

            [Display(Name = "Category")]
            public ProcurementCategory? Category { get; set; }

            [Display(Name = "Method")]
            public ProcurementMethod? Method { get; set; }

            [Display(Name = "Priority")]
            public ProcurementPriority? Priority { get; set; }

            [Display(Name = "Source")]
            public ProcurementSource? Source { get; set; }

            // Financial Filters
            [Display(Name = "Estimated Cost From")]
            [DataType(DataType.Currency)]
            public decimal? EstimatedCostFrom { get; set; }

            [Display(Name = "Estimated Cost To")]
            [DataType(DataType.Currency)]
            public decimal? EstimatedCostTo { get; set; }

            [Display(Name = "Actual Cost From")]
            [DataType(DataType.Currency)]
            public decimal? ActualCostFrom { get; set; }

            [Display(Name = "Actual Cost To")]
            [DataType(DataType.Currency)]
            public decimal? ActualCostTo { get; set; }

            [Display(Name = "Budget Code")]
            public string? BudgetCode { get; set; }

            [Display(Name = "Cost Center")]
            public string? CostCenter { get; set; }

            // Date Range Filters
            [Display(Name = "Request Date From")]
            [DataType(DataType.Date)]
            public DateTime? RequestDateFrom { get; set; }

            [Display(Name = "Request Date To")]
            [DataType(DataType.Date)]
            public DateTime? RequestDateTo { get; set; }

            [Display(Name = "Required Date From")]
            [DataType(DataType.Date)]
            public DateTime? RequiredDateFrom { get; set; }

            [Display(Name = "Required Date To")]
            [DataType(DataType.Date)]
            public DateTime? RequiredDateTo { get; set; }

            [Display(Name = "Approval Date From")]
            [DataType(DataType.Date)]
            public DateTime? ApprovalDateFrom { get; set; }

            [Display(Name = "Approval Date To")]
            [DataType(DataType.Date)]
            public DateTime? ApprovalDateTo { get; set; }

            [Display(Name = "Delivery Date From")]
            [DataType(DataType.Date)]
            public DateTime? DeliveryDateFrom { get; set; }

            [Display(Name = "Delivery Date To")]
            [DataType(DataType.Date)]
            public DateTime? DeliveryDateTo { get; set; }

            // User and Department Filters
            [Display(Name = "Requested By")]
            public string? RequestedByUserId { get; set; }

            [Display(Name = "Department")]
            public string? Department { get; set; }

            [Display(Name = "Approved By")]
            public string? ApprovedByUserId { get; set; }

            [Display(Name = "Current Approver")]
            public string? CurrentApproverUserId { get; set; }

            // Vendor Filters
            [Display(Name = "Vendor")]
            public int? VendorId { get; set; }

            [Display(Name = "Vendor Name")]
            public string? VendorName { get; set; }

            [Display(Name = "Vendor Category")]
            public string? VendorCategory { get; set; }

            [Display(Name = "Vendor Rating")]
            public int? MinVendorRating { get; set; }

            // Integration Filters
            [Display(Name = "Originating Request")]
            public int? OriginatingRequestId { get; set; }

            [Display(Name = "Triggered By Inventory Item")]
            public int? TriggeredByInventoryItemId { get; set; }

            [Display(Name = "Replacement For Asset")]
            public int? ReplacementForAssetId { get; set; }

            // Boolean Filters
            [Display(Name = "Emergency Procurement")]
            public bool? IsEmergency { get; set; }

            [Display(Name = "Recurring Procurement")]
            public bool? IsRecurring { get; set; }

            [Display(Name = "Over Budget")]
            public bool? IsOverBudget { get; set; }

            [Display(Name = "Requires Approval")]
            public bool? RequiresApproval { get; set; }

            [Display(Name = "Has Vendors")]
            public bool? HasVendors { get; set; }

            [Display(Name = "Delivered")]
            public bool? IsDelivered { get; set; }

            [Display(Name = "Overdue")]
            public bool? IsOverdue { get; set; }

            // Pagination and Sorting
            public int PageNumber { get; set; } = 1;
            public int PageSize { get; set; } = 25;
            public string SortBy { get; set; } = "RequestDate";
            public string SortOrder { get; set; } = "desc";

            // Quick Filter Presets
            public bool ShowPendingApprovalOnly { get; set; }
            public bool ShowOverdueOnly { get; set; }
            public bool ShowEmergencyOnly { get; set; }
            public bool ShowHighValueOnly { get; set; }
            public bool ShowCurrentUserOnly { get; set; }
            public bool ShowDepartmentOnly { get; set; }

            // UI State
            public bool AdvancedFiltersExpanded { get; set; }
            public string? SavedSearchName { get; set; }
        }

        // Enhanced Search Result with computed properties and analytics
        public class AdvancedProcurementSearchResult
        {
            public int Id { get; set; }
            public string ProcurementNumber { get; set; } = string.Empty;
            public string Title { get; set; } = string.Empty;
            public string? Description { get; set; }
            public ProcurementType ProcurementType { get; set; }
            public ProcurementCategory Category { get; set; }
            public ProcurementStatus Status { get; set; }
            public ProcurementMethod Method { get; set; }
            public ProcurementPriority Priority { get; set; }
            public ProcurementSource Source { get; set; }

            // Financial Information
            public decimal? EstimatedCost { get; set; }
            public decimal? ActualCost { get; set; }
            public string? BudgetCode { get; set; }
            public string? CostCenter { get; set; }

            // Dates
            public DateTime RequestDate { get; set; }
            public DateTime? RequiredDate { get; set; }
            public DateTime? ApprovalDate { get; set; }
            public DateTime? DeliveryDate { get; set; }
            public DateTime? CompletionDate { get; set; }

            // User Information
            public string RequestedByUserName { get; set; } = string.Empty;
            public string Department { get; set; } = string.Empty;
            public string? ApprovedByUserName { get; set; }
            public string? CurrentApproverUserName { get; set; }

            // Vendor Information
            public string? VendorName { get; set; }
            public string? VendorCategory { get; set; }
            public int? VendorRating { get; set; }
            public int VendorCount { get; set; }

            // Integration Information
            public string? OriginatingRequestNumber { get; set; }
            public string? TriggeredByInventoryItemCode { get; set; }
            public string? ReplacementForAssetTag { get; set; }

            // Computed Properties
            public string StatusDisplayName
            {
                get
                {
                    return Status switch
                    {
                        ProcurementStatus.Draft => "Draft",
                        ProcurementStatus.PendingApproval => "Pending Approval",
                        ProcurementStatus.Approved => "Approved",
                        ProcurementStatus.Rejected => "Rejected",
                        ProcurementStatus.InProcurement => "In Procurement",
                        ProcurementStatus.OrderPlaced => "Order Placed",
                        ProcurementStatus.PartiallyReceived => "Partially Received",
                        ProcurementStatus.Delivered => "Delivered",
                        ProcurementStatus.Completed => "Completed",
                        ProcurementStatus.Cancelled => "Cancelled",
                        _ => Status.ToString()
                    };
                }
            }

            public string PriorityDisplayName
            {
                get
                {
                    return Priority switch
                    {
                        ProcurementPriority.Low => "Low",
                        ProcurementPriority.Medium => "Medium", 
                        ProcurementPriority.High => "High",
                        ProcurementPriority.Critical => "Critical",
                        _ => Priority.ToString()
                    };
                }
            }

            public string StatusBadgeClass
            {
                get
                {
                    return Status switch
                    {
                        ProcurementStatus.Draft => "badge-secondary",
                        ProcurementStatus.PendingApproval => "badge-warning",
                        ProcurementStatus.Approved => "badge-success",
                        ProcurementStatus.Rejected => "badge-danger",
                        ProcurementStatus.InProcurement => "badge-info",
                        ProcurementStatus.OrderPlaced => "badge-primary",
                        ProcurementStatus.PartiallyReceived => "badge-warning",
                        ProcurementStatus.Delivered => "badge-success",
                        ProcurementStatus.Completed => "badge-success",
                        ProcurementStatus.Cancelled => "badge-danger",
                        _ => "badge-light"
                    };
                }
            }

            public string PriorityBadgeClass
            {
                get
                {
                    return Priority switch
                    {
                        ProcurementPriority.Low => "badge-success",
                        ProcurementPriority.Medium => "badge-info",
                        ProcurementPriority.High => "badge-warning",
                        ProcurementPriority.Critical => "badge-danger",
                        _ => "badge-light"
                    };
                }
            }

            public bool IsOverdue => RequiredDate.HasValue && RequiredDate.Value < DateTime.UtcNow && 
                                   Status != ProcurementStatus.Completed && Status != ProcurementStatus.Cancelled;

            public bool IsEmergency => Priority == ProcurementPriority.Critical || 
                                     Method == ProcurementMethod.EmergencyProcurement;

            public bool IsHighValue => EstimatedCost.HasValue && EstimatedCost.Value > 10000; // Configurable threshold

            public int DaysInProgress
            {
                get
                {
                    var endDate = CompletionDate ?? DateTime.UtcNow;
                    return (int)(endDate - RequestDate).TotalDays;
                }
            }

            public int? DaysUntilRequired
            {
                get
                {
                    if (!RequiredDate.HasValue) return null;
                    var days = (int)(RequiredDate.Value - DateTime.UtcNow).TotalDays;
                    return days > 0 ? days : 0;
                }
            }

            public decimal? CostVariance => ActualCost.HasValue && EstimatedCost.HasValue ? 
                                          ActualCost.Value - EstimatedCost.Value : null;

            public decimal? CostVariancePercentage => EstimatedCost.HasValue && EstimatedCost.Value > 0 && CostVariance.HasValue ?
                                                    (CostVariance.Value / EstimatedCost.Value) * 100 : null;
        }

        // Bulk Operations Models
        public class BulkProcurementOperationRequest
        {
            public List<int> ProcurementIds { get; set; } = new();
            public string Operation { get; set; } = string.Empty; // "approve", "reject", "cancel", "update_status"
            public string? Reason { get; set; }
            public ProcurementStatus? NewStatus { get; set; }
            public string? Comments { get; set; }
        }

        public class BulkProcurementUpdateRequest
        {
            public List<int> ProcurementIds { get; set; } = new();
            public string UpdateType { get; set; } = string.Empty; // "priority", "vendor", "budget", "dates"
            public object? UpdateValue { get; set; }
            public string Reason { get; set; } = string.Empty;
        }

        public class BulkApprovalRequest
        {
            public List<int> ProcurementIds { get; set; } = new();
            public bool Approve { get; set; } = true;
            public string? Comments { get; set; }
            public string? ApprovalNote { get; set; }
        }

        // Vendor Management Models
        public class VendorEvaluationModel
        {
            public int VendorId { get; set; }
            public string VendorName { get; set; } = string.Empty;
            public int TotalOrders { get; set; }
            public decimal TotalValue { get; set; }
            public decimal AverageDeliveryTime { get; set; }
            public decimal OnTimeDeliveryRate { get; set; }
            public decimal QualityRating { get; set; }
            public decimal PriceCompetitiveness { get; set; }
            public decimal OverallRating { get; set; }
            public DateTime LastOrderDate { get; set; }
            public List<string> Strengths { get; set; } = new();
            public List<string> ImprovementAreas { get; set; } = new();
        }

        public class VendorPerformanceMetrics
        {
            public int VendorId { get; set; }
            public string VendorName { get; set; } = string.Empty;
            public int OrdersCompleted { get; set; }
            public int OrdersOnTime { get; set; }
            public int OrdersDelayed { get; set; }
            public decimal AverageRating { get; set; }
            public decimal TotalSpend { get; set; }
            public decimal CostSavings { get; set; }
            public List<VendorMetricTrend> Trends { get; set; } = new();
        }

        public class VendorMetricTrend
        {
            public DateTime Period { get; set; }
            public decimal Value { get; set; }
            public string MetricType { get; set; } = string.Empty;
        }

        public class VendorComparisonModel
        {
            public List<VendorEvaluationModel> Vendors { get; set; } = new();
            public string ComparisonCriteria { get; set; } = string.Empty;
            public VendorEvaluationModel? RecommendedVendor { get; set; }
            public string RecommendationReason { get; set; } = string.Empty;
        }

        // Budget & Financial Models
        public class BudgetAllocationModel
        {
            public string BudgetCode { get; set; } = string.Empty;
            public string CostCenter { get; set; } = string.Empty;
            public decimal AllocatedAmount { get; set; }
            public decimal SpentAmount { get; set; }
            public decimal CommittedAmount { get; set; }
            public decimal AvailableAmount => AllocatedAmount - SpentAmount - CommittedAmount;
            public decimal UtilizationPercentage => AllocatedAmount > 0 ? (SpentAmount / AllocatedAmount) * 100 : 0;
            public List<BudgetTransaction> Transactions { get; set; } = new();
        }

        public class BudgetTransaction
        {
            public DateTime Date { get; set; }
            public string Description { get; set; } = string.Empty;
            public decimal Amount { get; set; }
            public string Type { get; set; } = string.Empty; // "allocation", "spend", "commitment"
            public int? ProcurementId { get; set; }
        }

        public class CostAnalysisModel
        {
            public decimal TotalCost { get; set; }
            public decimal EstimatedCost { get; set; }
            public decimal ActualCost { get; set; }
            public decimal CostVariance => ActualCost - EstimatedCost;
            public decimal CostVariancePercentage => EstimatedCost > 0 ? (CostVariance / EstimatedCost) * 100 : 0;
            public List<CostBreakdown> Breakdown { get; set; } = new();
            public List<CostTrend> Trends { get; set; } = new();
        }

        public class CostBreakdown
        {
            public string Category { get; set; } = string.Empty;
            public decimal Amount { get; set; }
            public decimal Percentage { get; set; }
        }

        public class CostTrend
        {
            public DateTime Period { get; set; }
            public decimal Amount { get; set; }
            public int Count { get; set; }
        }

        // Approval Workflow Models
        public class ApprovalChainModel
        {
            public int ProcurementId { get; set; }
            public List<ApprovalStepModel> Steps { get; set; } = new();
            public int CurrentStepIndex { get; set; }
            public ApprovalStepModel? CurrentStep => Steps.ElementAtOrDefault(CurrentStepIndex);
            public bool IsComplete => CurrentStepIndex >= Steps.Count;
            public TimeSpan TotalApprovalTime { get; set; }
        }

        public class ApprovalStepModel
        {
            public int StepOrder { get; set; }
            public string ApproverId { get; set; } = string.Empty;
            public string ApproverName { get; set; } = string.Empty;
            public string ApproverRole { get; set; } = string.Empty;
            public decimal ApprovalLimit { get; set; }
            public DateTime? ApprovalDate { get; set; }
            public string? Comments { get; set; }
            public bool IsApproved { get; set; }
            public bool IsRejected { get; set; }
            public bool IsPending => !IsApproved && !IsRejected;
            public TimeSpan? ApprovalTime { get; set; }
        }

        public class EscalationRuleModel
        {
            public string RuleName { get; set; } = string.Empty;
            public TimeSpan EscalationTimeframe { get; set; }
            public string EscalateToUserId { get; set; } = string.Empty;
            public string EscalateToUserName { get; set; } = string.Empty;
            public bool IsActive { get; set; }
            public List<string> Conditions { get; set; } = new();
        }

        // Export Models
        public class ProcurementExportRequest
        {
            public List<int>? ProcurementIds { get; set; }
            public AdvancedProcurementSearchModel? SearchCriteria { get; set; }
            public string Format { get; set; } = "Excel"; // "Excel", "PDF", "CSV"
            public bool IncludeVendorDetails { get; set; }
            public bool IncludeFinancialDetails { get; set; }
            public bool IncludeApprovalHistory { get; set; }
            public bool IncludeDocuments { get; set; }
        }

        public class BulkOperationResult
        {
            public bool Success { get; set; }
            public string Message { get; set; } = string.Empty;
            public int AffectedItems { get; set; }
            public List<string> Errors { get; set; } = new();
            public List<int> SuccessfulIds { get; set; } = new();
            public List<int> FailedIds { get; set; } = new();
        }

        // Analytics & Reporting Models
        public class ProcurementAnalyticsModel
        {
            public DateTime GeneratedDate { get; set; }
            public string GeneratedBy { get; set; } = string.Empty;
            public ProcurementOverviewMetrics Overview { get; set; } = new();
            public List<ProcurementTrendData> Trends { get; set; } = new();
            public List<VendorPerformanceMetrics> VendorMetrics { get; set; } = new();
            public BudgetAllocationModel BudgetSummary { get; set; } = new();
        }

        public class ProcurementOverviewMetrics
        {
            public int TotalRequests { get; set; }
            public int PendingApproval { get; set; }
            public int InProgress { get; set; }
            public int Completed { get; set; }
            public int Overdue { get; set; }
            public decimal TotalValue { get; set; }
            public decimal AverageProcessingTime { get; set; }
            public decimal ApprovalRate { get; set; }
        }

        public class ProcurementTrendData
        {
            public DateTime Period { get; set; }
            public int RequestCount { get; set; }
            public decimal TotalValue { get; set; }
            public decimal AverageValue { get; set; }
            public int CompletedCount { get; set; }
            public decimal CompletionRate { get; set; }
        }

        // Report Models
        public class ProcurementReport
        {
            public DateTime GeneratedDate { get; set; }
            public string GeneratedBy { get; set; } = string.Empty;
            public string ReportType { get; set; } = string.Empty;
            public DateTime FromDate { get; set; }
            public DateTime ToDate { get; set; }
            public List<AdvancedProcurementSearchResult> Items { get; set; } = new();
            public ProcurementReportSummary Summary { get; set; } = new();
        }

        public class ProcurementReportSummary
        {
            public int TotalCount { get; set; }
            public decimal TotalEstimatedValue { get; set; }
            public decimal TotalActualValue { get; set; }
            public decimal CostVariance { get; set; }
            public int OnTimeDeliveries { get; set; }
            public int DelayedDeliveries { get; set; }
            public decimal OnTimeDeliveryRate { get; set; }
            public Dictionary<string, int> StatusBreakdown { get; set; } = new();
            public Dictionary<string, decimal> CategoryBreakdown { get; set; } = new();
        }

        // Quick Filter Models
        public class QuickFilterModel
        {
            public string Name { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            public AdvancedProcurementSearchModel SearchCriteria { get; set; } = new();
            public string BadgeClass { get; set; } = "badge-primary";
            public string Icon { get; set; } = "fas fa-filter";
        }
    }
}
