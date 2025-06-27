using Microsoft.EntityFrameworkCore;
using HospitalAssetTracker.Models;
using HospitalAssetTracker.Data;

namespace HospitalAssetTracker.Services
{
    public class UnifiedBusinessLogicService : IUnifiedBusinessLogicService
    {
        private readonly ApplicationDbContext _context;
        private readonly IAssetService _assetService;
        private readonly IInventoryService _inventoryService;
        private readonly IProcurementService _procurementService;
        private readonly IRequestService _requestService;
        private readonly ICacheService _cacheService;
        private readonly ILogger<UnifiedBusinessLogicService> _logger;

        public UnifiedBusinessLogicService(
            ApplicationDbContext context,
            IAssetService assetService,
            IInventoryService inventoryService,
            IProcurementService procurementService,
            IRequestService requestService,
            ICacheService cacheService,
            ILogger<UnifiedBusinessLogicService> logger)
        {
            _context = context;
            _assetService = assetService;
            _inventoryService = inventoryService;
            _procurementService = procurementService;
            _requestService = requestService;
            _cacheService = cacheService;
            _logger = logger;
        }

        public async Task<UnifiedDashboardViewModel> GetDashboardDataAsync(string userId, List<string> userRoles)
        {
            try
            {
                var cacheKey = $"dashboard_data_{userId}_{string.Join("_", userRoles)}";
                var cachedData = await _cacheService.GetAsync<UnifiedDashboardViewModel>(cacheKey);
                
                if (cachedData != null)
                {
                    return cachedData;
                }

                var dashboardData = new UnifiedDashboardViewModel
                {
                    AssetSummary = await GetAssetSummaryAsync(userRoles),
                    InventorySummary = await GetInventorySummaryAsync(userRoles),
                    ProcurementSummary = await GetProcurementSummaryAsync(userRoles),
                    RequestSummary = await GetRequestSummaryAsync(userRoles),
                    RecentActivities = await GetRecentActivitiesAsync(userId, userRoles, 10),
                    PendingAlerts = await GetAlertsAsync(userId, userRoles, true),
                    WorkflowSummary = await GetWorkflowSummaryAsync(userId, userRoles),
                    AvailableActions = await GetQuickActionsAsync(userId, userRoles),
                    PerformanceMetrics = await GetPerformanceMetricsAsync(userId, userRoles)
                };

                await _cacheService.SetAsync(cacheKey, dashboardData, TimeSpan.FromMinutes(15));
                return dashboardData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting dashboard data for user {UserId}", userId);
                return new UnifiedDashboardViewModel();
            }
        }

        public async Task<UnifiedActionViewModel> GetActionItemsAsync(string userId, List<string> userRoles)
        {
            try
            {
                var viewModel = new UnifiedActionViewModel
                {
                    PendingApprovals = await GetPendingApprovalsAsync(userId, userRoles),
                    ScheduledTasks = await GetScheduledTasksAsync(userId, userRoles),
                    OverdueItems = await GetOverdueItemsAsync(userId, userRoles),
                    PendingAssignments = await GetPendingAssignmentsAsync(userId, userRoles),
                    UnreadNotifications = await GetUnreadNotificationsAsync(userId),
                    WorkflowStats = await GetWorkflowStatisticsAsync(userId, userRoles)
                };

                return viewModel;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting action items for user {UserId}", userId);
                return new UnifiedActionViewModel();
            }
        }

        public async Task<List<SmartInsight>> GetSmartInsightsAsync(string userId, List<string> userRoles)
        {
            try
            {
                var insights = new List<SmartInsight>();

                // Asset-related insights
                var lowUtilizationAssets = await _context.Assets
                    .Where(a => a.Status == AssetStatus.Available)
                    .CountAsync();

                if (lowUtilizationAssets > 50)
                {
                    insights.Add(new SmartInsight
                    {
                        Id = "asset_utilization",
                        Title = "Low Asset Utilization Detected",
                        Description = $"You have {lowUtilizationAssets} available assets that could be better utilized",
                        Type = InsightType.EfficiencyImprovement,
                        Priority = InsightPriority.Medium,
                        ActionRecommendation = "Review asset assignments and consider redistribution",
                        ActionUrl = "/Assets/Index?status=Available",
                        Confidence = 0.85,
                        GeneratedDate = DateTime.UtcNow,
                        Icon = "fas fa-chart-line",
                        ColorClass = "warning"
                    });
                }

                // Maintenance insights
                var assetsNeedingMaintenance = await _context.Assets
                    .Where(a => a.LastMaintenanceDate.HasValue && 
                               a.LastMaintenanceDate.Value.AddMonths(6) < DateTime.UtcNow)
                    .CountAsync();

                if (assetsNeedingMaintenance > 0)
                {
                    insights.Add(new SmartInsight
                    {
                        Id = "maintenance_due",
                        Title = "Preventive Maintenance Required",
                        Description = $"{assetsNeedingMaintenance} assets are due for maintenance",
                        Type = InsightType.MaintenancePrediction,
                        Priority = InsightPriority.High,
                        ActionRecommendation = "Schedule maintenance to prevent failures",
                        ActionUrl = "/Assets/MaintenanceDue",
                        Confidence = 0.95,
                        GeneratedDate = DateTime.UtcNow,
                        Icon = "fas fa-tools",
                        ColorClass = "danger"
                    });
                }

                // Cost optimization insights
                var expensiveRequests = await _context.ITRequests
                    .Where(r => r.EstimatedCost > 5000 && r.Status == RequestStatus.Pending)
                    .CountAsync();

                if (expensiveRequests > 5)
                {
                    insights.Add(new SmartInsight
                    {
                        Id = "cost_optimization",
                        Title = "High-Cost Requests Pending",
                        Description = $"{expensiveRequests} high-cost requests require attention",
                        Type = InsightType.CostOptimization,
                        Priority = InsightPriority.High,
                        ActionRecommendation = "Review and prioritize high-cost requests",
                        ActionUrl = "/Requests/Index?highCost=true",
                        Confidence = 0.90,
                        GeneratedDate = DateTime.UtcNow,
                        Icon = "fas fa-dollar-sign",
                        ColorClass = "info"
                    });
                }

                return insights.OrderByDescending(i => i.Priority).ThenByDescending(i => i.Confidence).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating smart insights for user {UserId}", userId);
                return new List<SmartInsight>();
            }
        }

        public async Task<List<RecentActivity>> GetRecentActivitiesAsync(string userId, List<string> userRoles, int count = 10)
        {
            try
            {
                var activities = new List<RecentActivity>();

                // Get recent audit logs
                var recentAudits = await _context.AuditLogs
                    .OrderByDescending(a => a.Timestamp)
                    .Take(count)
                    .ToListAsync();

                foreach (var audit in recentAudits)
                {
                    activities.Add(new RecentActivity
                    {
                        Type = GetActivityTypeFromAudit(audit.Action),
                        Action = audit.Action,
                        Description = $"{audit.Action} {audit.EntityName}",
                        UserName = audit.UserName,
                        Timestamp = audit.Timestamp,
                        EntityId = audit.EntityId,
                        EntityType = audit.EntityName,
                        Icon = GetIconForActivity(audit.Action),
                        ColorClass = GetColorForActivity(audit.Action)
                    });
                }

                return activities.OrderByDescending(a => a.Timestamp).Take(count).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting recent activities for user {UserId}", userId);
                return new List<RecentActivity>();
            }
        }

        public async Task<List<Alert>> GetAlertsAsync(string userId, List<string> userRoles, bool unreadOnly = false)
        {
            try
            {
                var alerts = new List<Alert>();

                // Warranty expiration alerts
                var expiringWarranties = await _context.Assets
                    .Where(a => a.WarrantyExpirationDate.HasValue && 
                               a.WarrantyExpirationDate.Value <= DateTime.UtcNow.AddDays(30))
                    .CountAsync();

                if (expiringWarranties > 0)
                {
                    alerts.Add(new Alert
                    {
                        Type = AlertType.WarrantyExpiration,
                        Severity = AlertSeverity.Medium,
                        Title = "Warranties Expiring Soon",
                        Message = $"{expiringWarranties} assets have warranties expiring within 30 days",
                        ActionUrl = "/Assets/WarrantyExpiring",
                        ActionText = "Review Assets",
                        CreatedDate = DateTime.UtcNow,
                        Icon = "fas fa-shield-alt",
                        ColorClass = "warning"
                    });
                }

                // Low stock alerts
                var lowStockItems = await _context.InventoryItems
                    .Where(i => i.Quantity <= i.MinimumStock)
                    .CountAsync();

                if (lowStockItems > 0)
                {
                    alerts.Add(new Alert
                    {
                        Type = AlertType.LowStock,
                        Severity = AlertSeverity.High,
                        Title = "Low Stock Alert",
                        Message = $"{lowStockItems} items are below minimum stock levels",
                        ActionUrl = "/Inventory/LowStock",
                        ActionText = "Reorder Items",
                        CreatedDate = DateTime.UtcNow,
                        Icon = "fas fa-exclamation-triangle",
                        ColorClass = "danger"
                    });
                }

                // Pending approvals
                var pendingApprovals = await _context.ITRequests
                    .Where(r => r.Status == RequestStatus.Pending)
                    .CountAsync();

                if (pendingApprovals > 0 && userRoles.Any(r => r == "Admin" || r == "IT Support"))
                {
                    alerts.Add(new Alert
                    {
                        Type = AlertType.PendingApproval,
                        Severity = AlertSeverity.Medium,
                        Title = "Pending Approvals",
                        Message = $"{pendingApprovals} requests are waiting for approval",
                        ActionUrl = "/Requests/PendingApproval",
                        ActionText = "Review Requests",
                        CreatedDate = DateTime.UtcNow,
                        Icon = "fas fa-clock",
                        ColorClass = "info"
                    });
                }

                return alerts.OrderByDescending(a => a.Severity).ThenByDescending(a => a.CreatedDate).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting alerts for user {UserId}", userId);
                return new List<Alert>();
            }
        }

        public async Task<List<QuickAction>> GetQuickActionsAsync(string userId, List<string> userRoles)
        {
            var actions = new List<QuickAction>();

            // Add Asset
            if (userRoles.Any(r => r == "Admin" || r == "IT Support" || r == "Asset Manager"))
            {
                actions.Add(new QuickAction
                {
                    Id = "add_asset",
                    Title = "Add New Asset",
                    Description = "Register a new IT asset",
                    Icon = "fas fa-plus",
                    ActionUrl = "/Assets/Create",
                    ColorClass = "primary",
                    RequiredRoles = new List<string> { "Admin", "IT Support", "Asset Manager" },
                    Priority = 1
                });
            }

            // Create Request
            actions.Add(new QuickAction
            {
                Id = "create_request",
                Title = "Create Request",
                Description = "Submit a new IT request",
                Icon = "fas fa-ticket-alt",
                ActionUrl = "/Requests/Create",
                ColorClass = "success",
                RequiredRoles = new List<string>(),
                Priority = 2
            });

            // Generate Report
            if (userRoles.Any(r => r == "Admin" || r == "Asset Manager" || r == "Department Head"))
            {
                actions.Add(new QuickAction
                {
                    Id = "generate_report",
                    Title = "Generate Report",
                    Description = "Create asset or activity reports",
                    Icon = "fas fa-chart-bar",
                    ActionUrl = "/Reports/Index",
                    ColorClass = "info",
                    RequiredRoles = new List<string> { "Admin", "Asset Manager", "Department Head" },
                    Priority = 3
                });
            }

            // Bulk Import
            if (userRoles.Any(r => r == "Admin" || r == "Asset Manager"))
            {
                actions.Add(new QuickAction
                {
                    Id = "bulk_import",
                    Title = "Bulk Import",
                    Description = "Import multiple assets from Excel",
                    Icon = "fas fa-upload",
                    ActionUrl = "/AssetImport/Index",
                    ColorClass = "warning",
                    RequiredRoles = new List<string> { "Admin", "Asset Manager" },
                    Priority = 4
                });
            }

            return actions.OrderBy(a => a.Priority).ToList();
        }

        public async Task<bool> MarkAlertAsReadAsync(int alertId, string userId)
        {
            // Implementation would depend on how alerts are stored
            // For now, return true as alerts are generated dynamically
            return await Task.FromResult(true);
        }

        public async Task<bool> DismissAlertAsync(int alertId, string userId)
        {
            // Implementation would depend on how alert dismissals are stored
            return await Task.FromResult(true);
        }

        public async Task<PerformanceMetrics> GetPerformanceMetricsAsync(string userId, List<string> userRoles)
        {
            try
            {
                var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
                
                // Calculate average request processing time
                var completedRequests = await _context.ITRequests
                    .Where(r => r.Status == RequestStatus.Completed && r.CompletedDate.HasValue)
                    .Select(r => new { r.CreatedDate, r.CompletedDate })
                    .ToListAsync();

                var avgProcessingTime = completedRequests.Any() 
                    ? completedRequests.Average(r => (r.CompletedDate.Value - r.CreatedDate).TotalHours)
                    : 0;

                // Calculate asset utilization rate
                var totalAssets = await _context.Assets.CountAsync();
                var inUseAssets = await _context.Assets.Where(a => a.Status == AssetStatus.InUse).CountAsync();
                var utilizationRate = totalAssets > 0 ? (double)inUseAssets / totalAssets * 100 : 0;

                // Calculate maintenance compliance
                var assetsNeedingMaintenance = await _context.Assets
                    .Where(a => a.LastMaintenanceDate.HasValue && 
                               a.LastMaintenanceDate.Value.AddMonths(6) < DateTime.UtcNow)
                    .CountAsync();
                var complianceRate = totalAssets > 0 ? (double)(totalAssets - assetsNeedingMaintenance) / totalAssets * 100 : 100;

                return new PerformanceMetrics
                {
                    AverageRequestProcessingTime = avgProcessingTime,
                    AssetUtilizationRate = utilizationRate,
                    MaintenanceComplianceRate = complianceRate,
                    ProcurementEfficiency = 85.0, // Placeholder
                    UserSatisfactionScore = 8.2, // Placeholder
                    KPIs = new Dictionary<string, double>
                    {
                        ["RequestThroughput"] = completedRequests.Count(r => r.CompletedDate >= thirtyDaysAgo),
                        ["AssetAvailability"] = utilizationRate,
                        ["MaintenanceOnTime"] = complianceRate
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating performance metrics for user {UserId}", userId);
                return new PerformanceMetrics();
            }
        }

        public async Task<WorkflowSummary> GetWorkflowSummaryAsync(string userId, List<string> userRoles)
        {
            try
            {
                var pendingRequests = await _context.ITRequests.Where(r => r.Status == RequestStatus.Pending).CountAsync();
                var inProgressRequests = await _context.ITRequests.Where(r => r.Status == RequestStatus.InProgress).CountAsync();
                var completedToday = await _context.ITRequests
                    .Where(r => r.Status == RequestStatus.Completed && r.CompletedDate.HasValue && 
                               r.CompletedDate.Value.Date == DateTime.UtcNow.Date)
                    .CountAsync();

                return new WorkflowSummary
                {
                    PendingApprovals = pendingRequests,
                    ActiveWorkflows = inProgressRequests,
                    CompletedToday = completedToday,
                    OverdueItems = 0, // Placeholder
                    WorkflowsByType = new Dictionary<string, int>
                    {
                        ["IT Requests"] = pendingRequests,
                        ["Asset Transfers"] = 0,
                        ["Maintenance"] = 0
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting workflow summary for user {UserId}", userId);
                return new WorkflowSummary();
            }
        }

        public async Task<bool> ExecuteQuickActionAsync(string actionId, string userId, Dictionary<string, object>? parameters = null)
        {
            try
            {
                // Implementation would depend on the specific action
                // For now, just log the action
                _logger.LogInformation("User {UserId} executed quick action {ActionId}", userId, actionId);
                return await Task.FromResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing quick action {ActionId} for user {UserId}", actionId, userId);
                return false;
            }
        }

        public async Task RefreshCacheAsync()
        {
            try
            {
                await _cacheService.RemoveByPatternAsync("dashboard_data_*");
                _logger.LogInformation("Dashboard cache refreshed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing cache");
            }
        }

        // Helper methods
        private async Task<AssetSummary> GetAssetSummaryAsync(List<string> userRoles)
        {
            var assets = await _context.Assets.ToListAsync();
            return new AssetSummary
            {
                TotalAssets = assets.Count,
                ActiveAssets = assets.Count(a => a.Status == AssetStatus.Active || a.Status == AssetStatus.InUse),
                AvailableAssets = assets.Count(a => a.Status == AssetStatus.Available),
                InUseAssets = assets.Count(a => a.Status == AssetStatus.InUse),
                InRepairAssets = assets.Count(a => a.Status == AssetStatus.InRepair || a.Status == AssetStatus.UnderRepair),
                DecommissionedAssets = assets.Count(a => a.Status == AssetStatus.Decommissioned),
                AssetsByCategory = assets.GroupBy(a => a.Category).ToDictionary(g => g.Key, g => g.Count()),
                AssetsByStatus = assets.GroupBy(a => a.Status).ToDictionary(g => g.Key, g => g.Count()),
                RecentlyAddedAssets = assets.OrderByDescending(a => a.PurchaseDate ?? a.CreatedDate)
                                          .Take(5).ToList(),
                AssetsNeedingAttention = assets.Where(a => a.Status == AssetStatus.InRepair || 
                                                          a.Status == AssetStatus.UnderMaintenance)
                                              .Take(5).ToList()
            };
        }

        private async Task<InventorySummary> GetInventorySummaryAsync(List<string> userRoles)
        {
            var items = await _context.InventoryItems.ToListAsync();
            return new InventorySummary
            {
                TotalItems = items.Count,
                LowStockItems = items.Count(i => i.Quantity <= i.MinimumStock),
                OutOfStockItems = items.Count(i => i.Quantity == 0),
                TotalInventoryValue = items.Sum(i => i.Quantity * i.UnitCost),
                LowStockAlerts = items.Where(i => i.Quantity <= i.MinimumStock).Take(5).ToList()
            };
        }

        private async Task<ProcurementSummary> GetProcurementSummaryAsync(List<string> userRoles)
        {
            var pos = await _context.ProcurementRequests.ToListAsync();
            return new ProcurementSummary
            {
                TotalPurchaseOrders = pos.Count,
                PendingPOs = pos.Count(p => p.Status == ProcurementStatus.Pending),
                ApprovedPOs = pos.Count(p => p.Status == ProcurementStatus.Approved),
                ReceivedPOs = pos.Count(p => p.Status == ProcurementStatus.Delivered),
                TotalPOValue = pos.Where(p => p.TotalAmount.HasValue).Sum(p => p.TotalAmount.Value),
                PendingPOValue = pos.Where(p => p.Status == ProcurementStatus.Pending && p.TotalAmount.HasValue).Sum(p => p.TotalAmount.Value),
                RecentPOs = pos.OrderByDescending(p => p.RequestDate).Take(5).ToList()
            };
        }

        private async Task<RequestSummary> GetRequestSummaryAsync(List<string> userRoles)
        {
            var requests = await _context.ITRequests.ToListAsync();
            return new RequestSummary
            {
                TotalRequests = requests.Count,
                PendingRequests = requests.Count(r => r.Status == RequestStatus.Pending),
                ApprovedRequests = requests.Count(r => r.Status == RequestStatus.Approved),
                CompletedRequests = requests.Count(r => r.Status == RequestStatus.Completed),
                RejectedRequests = requests.Count(r => r.Status == RequestStatus.Rejected),
                RecentRequests = requests.OrderByDescending(r => r.CreatedDate).Take(5).ToList(),
                HighPriorityRequests = requests.Where(r => r.Priority == Priority.High || r.Priority == Priority.Critical)
                                             .OrderByDescending(r => r.Priority).Take(5).ToList(),
                RequestsByType = requests.GroupBy(r => r.RequestType).ToDictionary(g => g.Key, g => g.Count()),
                RequestsByPriority = requests.GroupBy(r => r.Priority).ToDictionary(g => g.Key, g => g.Count())
            };
        }

        private async Task<List<PendingApproval>> GetPendingApprovalsAsync(string userId, List<string> userRoles)
        {
            var approvals = new List<PendingApproval>();

            // Get pending IT requests
            var pendingRequests = await _context.ITRequests
                .Where(r => r.Status == RequestStatus.Pending)
                .Include(r => r.Requestor)
                .ToListAsync();

            foreach (var request in pendingRequests)
            {
                approvals.Add(new PendingApproval
                {
                    Id = request.Id,
                    Type = "IT Request",
                    Title = request.Title,
                    Requestor = request.Requestor?.UserName ?? "Unknown",
                    SubmittedDate = request.CreatedDate,
                    Priority = request.Priority,
                    Amount = request.EstimatedCost,
                    Status = request.Status.ToString(),
                    ActionUrl = $"/Requests/Details/{request.Id}",
                    DaysWaiting = (DateTime.UtcNow - request.CreatedDate).Days
                });
            }

            return approvals.OrderByDescending(a => a.Priority).ThenBy(a => a.SubmittedDate).ToList();
        }

        private async Task<List<ScheduledTask>> GetScheduledTasksAsync(string userId, List<string> userRoles)
        {
            // This would typically come from a task scheduling system
            // For now, return maintenance-related tasks
            var tasks = new List<ScheduledTask>();

            var maintenanceDueAssets = await _context.Assets
                .Where(a => a.LastMaintenanceDate.HasValue && 
                           a.LastMaintenanceDate.Value.AddMonths(6) < DateTime.UtcNow.AddDays(7))
                .Take(10)
                .ToListAsync();

            foreach (var asset in maintenanceDueAssets)
            {
                tasks.Add(new ScheduledTask
                {
                    Id = asset.Id,
                    Type = "Maintenance",
                    Title = $"Maintenance Due: {asset.AssetTag}",
                    Description = $"Scheduled maintenance for {asset.Name}",
                    ScheduledDate = asset.LastMaintenanceDate?.AddMonths(6) ?? DateTime.UtcNow,
                    AssignedTo = "IT Support",
                    Status = TaskStatus.Pending,
                    Priority = Priority.Medium,
                    ActionUrl = $"/Assets/Details/{asset.Id}"
                });
            }

            return tasks;
        }

        private async Task<List<OverdueItem>> GetOverdueItemsAsync(string userId, List<string> userRoles)
        {
            var overdueItems = new List<OverdueItem>();

            // Get overdue requests
            var overdueRequests = await _context.ITRequests
                .Where(r => r.Status == RequestStatus.InProgress && 
                           r.ExpectedCompletionDate.HasValue && 
                           r.ExpectedCompletionDate.Value < DateTime.UtcNow)
                .ToListAsync();

            foreach (var request in overdueRequests)
            {
                overdueItems.Add(new OverdueItem
                {
                    Id = request.Id,
                    Type = "IT Request",
                    Title = request.Title,
                    DueDate = request.ExpectedCompletionDate.Value,
                    DaysOverdue = (DateTime.UtcNow - request.ExpectedCompletionDate.Value).Days,
                    AssignedTo = request.AssignedTo ?? "Unassigned",
                    Priority = request.Priority,
                    ActionUrl = $"/Requests/Details/{request.Id}"
                });
            }

            return overdueItems.OrderByDescending(i => i.DaysOverdue).ToList();
        }

        private async Task<List<Assignment>> GetPendingAssignmentsAsync(string userId, List<string> userRoles)
        {
            var assignments = new List<Assignment>();

            // Get unassigned requests
            var unassignedRequests = await _context.ITRequests
                .Where(r => r.Status == RequestStatus.Approved && string.IsNullOrEmpty(r.AssignedTo))
                .ToListAsync();

            foreach (var request in unassignedRequests)
            {
                assignments.Add(new Assignment
                {
                    Id = request.Id,
                    Type = "IT Request",
                    Title = request.Title,
                    AssignedDate = request.CreatedDate,
                    Priority = request.Priority,
                    Status = "Pending Assignment",
                    ActionUrl = $"/Requests/Assign/{request.Id}"
                });
            }

            return assignments.OrderByDescending(a => a.Priority).ToList();
        }

        private async Task<List<Notification>> GetUnreadNotificationsAsync(string userId)
        {
            // This would typically come from a notification system
            return new List<Notification>();
        }

        private async Task<WorkflowStatistics> GetWorkflowStatisticsAsync(string userId, List<string> userRoles)
        {
            var totalRequests = await _context.ITRequests.CountAsync();
            var activeRequests = await _context.ITRequests.Where(r => r.Status == RequestStatus.InProgress).CountAsync();
            var completedThisWeek = await _context.ITRequests
                .Where(r => r.Status == RequestStatus.Completed && 
                           r.CompletedDate.HasValue && 
                           r.CompletedDate.Value >= DateTime.UtcNow.AddDays(-7))
                .CountAsync();

            return new WorkflowStatistics
            {
                TotalWorkflows = totalRequests,
                ActiveWorkflows = activeRequests,
                CompletedThisWeek = completedThisWeek,
                CompletedThisMonth = await _context.ITRequests
                    .Where(r => r.Status == RequestStatus.Completed && 
                               r.CompletedDate.HasValue && 
                               r.CompletedDate.Value >= DateTime.UtcNow.AddDays(-30))
                    .CountAsync(),
                AverageCompletionTime = 24.0, // Placeholder
                WorkflowEfficiency = 85.0 // Placeholder
            };
        }

        private string GetActivityTypeFromAudit(string action)
        {
            if (action.Contains("Asset")) return "Asset";
            if (action.Contains("Request")) return "Request";
            if (action.Contains("Purchase") || action.Contains("Procurement")) return "Procurement";
            if (action.Contains("Inventory")) return "Inventory";
            return "System";
        }

        private string GetIconForActivity(string action)
        {
            return action.ToLower() switch
            {
                var a when a.Contains("create") => "fas fa-plus",
                var a when a.Contains("update") => "fas fa-edit",
                var a when a.Contains("delete") => "fas fa-trash",
                var a when a.Contains("approve") => "fas fa-check",
                var a when a.Contains("reject") => "fas fa-times",
                _ => "fas fa-info-circle"
            };
        }

        private string GetColorForActivity(string action)
        {
            return action.ToLower() switch
            {
                var a when a.Contains("create") => "success",
                var a when a.Contains("update") => "info",
                var a when a.Contains("delete") => "danger",
                var a when a.Contains("approve") => "success",
                var a when a.Contains("reject") => "warning",
                _ => "secondary"
            };
        }
    }
}
