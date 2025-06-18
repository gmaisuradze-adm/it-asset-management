using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using HospitalAssetTracker.Data;
using HospitalAssetTracker.Models;
using System.Text.Json;

namespace HospitalAssetTracker.Services
{
    /// <summary>
    /// Advanced Warehouse Business Logic Service - Professional Implementation
    /// Provides sophisticated warehouse management operations with full integration
    /// across Asset, Request, and Procurement modules for optimal efficiency
    /// </summary>
    public class WarehouseBusinessLogicService : IWarehouseBusinessLogicService
    {
        private readonly ApplicationDbContext _context;
        private readonly IInventoryService _inventoryService;
        private readonly IAuditService _auditService;
        private readonly ILogger<WarehouseBusinessLogicService> _logger;

        public WarehouseBusinessLogicService(
            ApplicationDbContext context,
            IInventoryService inventoryService,
            IAuditService auditService,
            ILogger<WarehouseBusinessLogicService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _inventoryService = inventoryService ?? throw new ArgumentNullException(nameof(inventoryService));
            _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #region Smart Replenishment & Optimization

        /// <summary>
        /// Advanced ABC analysis for inventory classification and optimization
        /// Classifies inventory items based on value, usage frequency, and criticality
        /// </summary>
        public async Task<AbcAnalysisResult> PerformAbcAnalysisAsync(int analysisMonths = 12)
        {
            _logger.LogInformation("Performing ABC analysis for {Months} months", analysisMonths);

            var cutoffDate = DateTime.UtcNow.AddMonths(-analysisMonths);
            
            var items = await _context.InventoryItems
                .Include(i => i.Movements.Where(m => m.MovementDate >= cutoffDate))
                .Include(i => i.Transactions.Where(t => t.TransactionDate >= cutoffDate))
                .ToListAsync();

            var analysisData = items.Select(item => new ItemAnalysisData
            {
                InventoryItem = item,
                TotalValue = item.TotalValue ?? 0,
                UsageFrequency = item.Movements.Count(m => m.MovementType == InventoryMovementType.StockOut),
                VelocityScore = CalculateVelocityScore(item),
                CriticalityScore = CalculateCriticalityScore(item),
                CarryingCost = CalculateCarryingCost(item)
            }).ToList();

            // Sort by composite score (value × velocity × criticality)
            var sortedItems = analysisData
                .OrderByDescending(i => (double)i.TotalValue * i.VelocityScore * i.CriticalityScore)
                .ToList();

            var totalCount = sortedItems.Count;
            var aCount = (int)(totalCount * 0.2); // Top 20%
            var bCount = (int)(totalCount * 0.3); // Next 30%
            var cCount = totalCount - aCount - bCount; // Remaining 50%

            var result = new AbcAnalysisResult
            {
                AnalysisDate = DateTime.UtcNow,
                AnalysisPeriodMonths = analysisMonths,
                CategoryA = sortedItems.Take(aCount).ToList(),
                CategoryB = sortedItems.Skip(aCount).Take(bCount).ToList(),
                CategoryC = sortedItems.Skip(aCount + bCount).ToList(),
                TotalItems = totalCount,
                TotalValue = analysisData.Sum(i => i.TotalValue)
            };

            // Generate optimization recommendations
            result.Recommendations = GenerateAbcRecommendations(result);

            _logger.LogInformation("ABC analysis completed: {ACount} A-items, {BCount} B-items, {CCount} C-items", 
                aCount, bCount, cCount);

            return result;
        }

        /// <summary>
        /// Intelligent demand forecasting using historical data and seasonal patterns
        /// </summary>
        public async Task<DemandForecast> GenerateDemandForecastAsync(int inventoryItemId, int forecastDays = 90)
        {
            _logger.LogInformation("Generating demand forecast for item {ItemId} for {Days} days", inventoryItemId, forecastDays);

            var item = await _context.InventoryItems
                .Include(i => i.Movements.Where(m => m.MovementType == InventoryMovementType.StockOut))
                .FirstOrDefaultAsync(i => i.Id == inventoryItemId);

            if (item == null)
                throw new ArgumentException("Inventory item not found", nameof(inventoryItemId));

            var movements = item.Movements
                .Where(m => m.MovementType == InventoryMovementType.StockOut)
                .OrderBy(m => m.MovementDate)
                .ToList();

            // Calculate historical usage patterns
            var dailyUsage = CalculateDailyUsagePattern(movements);
            var weeklyPattern = CalculateWeeklyPattern(movements);
            var monthlyPattern = CalculateMonthlyPattern(movements);
            var seasonalFactors = CalculateSeasonalFactors(movements);

            // Generate forecast using weighted moving average with seasonal adjustment
            var forecast = new DemandForecast
            {
                InventoryItemId = inventoryItemId,
                ItemName = item.Name,
                ForecastDate = DateTime.UtcNow,
                ForecastPeriodDays = forecastDays,
                HistoricalPeriodDays = movements.Any() ? (DateTime.UtcNow - movements.First().MovementDate).Days : 0,
                AverageDailyDemand = dailyUsage.Average(),
                PeakDailyDemand = dailyUsage.Max(),
                MinDailyDemand = dailyUsage.Min(),
                SeasonalityDetected = seasonalFactors.Any(f => Math.Abs(f - 1.0) > 0.1),
                ConfidenceLevel = CalculateConfidenceLevel(movements),
                RecommendedReorderPoint = CalculateOptimalReorderPoint(dailyUsage, forecastDays),
                RecommendedOrderQuantity = CalculateEconomicOrderQuantity(item, dailyUsage.Average())
            };

            forecast.DailyForecast = GenerateDailyForecast(dailyUsage, weeklyPattern, monthlyPattern, seasonalFactors, forecastDays);

            return forecast;
        }

        /// <summary>
        /// Automated smart replenishment based on advanced algorithms
        /// </summary>
        public async Task<SmartReplenishmentResult> ExecuteSmartReplenishmentAsync(string initiatedByUserId)
        {
            _logger.LogInformation("Executing smart replenishment initiated by {UserId}", initiatedByUserId);

            var result = new SmartReplenishmentResult
            {
                ExecutionDate = DateTime.UtcNow,
                InitiatedByUserId = initiatedByUserId,
                ReplenishmentActions = new List<ReplenishmentAction>()
            };

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Get items needing replenishment analysis
                var itemsToAnalyze = await _context.InventoryItems
                    .Where(i => i.Status == InventoryStatus.Active)
                    .Include(i => i.Movements.Where(m => m.MovementDate >= DateTime.UtcNow.AddMonths(-3)))
                    .ToListAsync();

                foreach (var item in itemsToAnalyze)
                {
                    var demandForecast = await GenerateDemandForecastAsync(item.Id, 30);
                    var replenishmentDecision = await MakeReplenishmentDecisionAsync(item, demandForecast);

                    if (replenishmentDecision.ShouldReplenish)
                    {
                        var action = new ReplenishmentAction
                        {
                            InventoryItemId = item.Id,
                            ItemName = item.Name,
                            CurrentStock = item.Quantity,
                            RecommendedOrderQuantity = replenishmentDecision.OrderQuantity,
                            Priority = replenishmentDecision.Priority,
                            Reasoning = replenishmentDecision.Reasoning,
                            EstimatedDeliveryDays = replenishmentDecision.EstimatedDeliveryDays,
                            ActionTaken = false
                        };

                        // Auto-generate procurement request for high-priority items
                        if (replenishmentDecision.Priority == ReplenishmentPriority.Critical)
                        {
                            var procurementCreated = await CreateAutomaticProcurementRequestAsync(
                                item, 
                                replenishmentDecision.OrderQuantity, 
                                initiatedByUserId);

                            if (procurementCreated)
                            {
                                action.ActionTaken = true;
                                action.ProcurementRequestCreated = true;
                            }
                        }

                        result.ReplenishmentActions.Add(action);
                    }
                }

                result.TotalItemsAnalyzed = itemsToAnalyze.Count;
                result.ItemsNeedingReplenishment = result.ReplenishmentActions.Count;
                result.AutoProcurementsCreated = result.ReplenishmentActions.Count(a => a.ProcurementRequestCreated);

                await transaction.CommitAsync();

                _logger.LogInformation("Smart replenishment completed: {Analyzed} items analyzed, {NeedReplenishment} need replenishment, {AutoCreated} auto-procurements created",
                    result.TotalItemsAnalyzed, result.ItemsNeedingReplenishment, result.AutoProcurementsCreated);

                return result;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Smart replenishment failed");
                throw;
            }
        }

        #endregion

        #region Space Optimization & Layout Management

        /// <summary>
        /// Optimize warehouse space allocation based on item velocity and storage requirements
        /// </summary>
        public async Task<SpaceOptimizationResult> OptimizeWarehouseLayoutAsync(int locationId)
        {
            _logger.LogInformation("Optimizing warehouse layout for location {LocationId}", locationId);

            var location = await _context.Locations.FindAsync(locationId);
            if (location == null)
                throw new ArgumentException("Location not found", nameof(locationId));

            var items = await _context.InventoryItems
                .Where(i => i.LocationId == locationId)
                .Include(i => i.Movements.Where(m => m.MovementDate >= DateTime.UtcNow.AddMonths(-6)))
                .ToListAsync();

            var optimization = new SpaceOptimizationResult
            {
                LocationId = locationId,
                LocationName = location.FullLocation,
                OptimizationDate = DateTime.UtcNow,
                CurrentItems = items.Count,
                Recommendations = new List<SpaceOptimizationRecommendation>()
            };

            // Analyze item velocity and suggest optimal zones
            foreach (var item in items)
            {
                var velocity = CalculateVelocityScore(item);
                var recommendedZone = DetermineOptimalZone(velocity, item);
                
                if (recommendedZone != item.StorageZone)
                {
                    optimization.Recommendations.Add(new SpaceOptimizationRecommendation
                    {
                        InventoryItemId = item.Id,
                        ItemName = item.Name,
                        CurrentZone = item.StorageZone,
                        RecommendedZone = recommendedZone,
                        Reason = $"Velocity score: {velocity:F2} - {GetVelocityDescription(velocity)}",
                        Priority = velocity > 0.8 ? "High" : velocity > 0.4 ? "Medium" : "Low"
                    });
                }
            }

            optimization.TotalRecommendations = optimization.Recommendations.Count;
            optimization.EstimatedEfficiencyGain = CalculateEfficiencyGain(optimization.Recommendations);

            return optimization;
        }

        #endregion

        #region Quality & Condition Management

        /// <summary>
        /// Advanced quality control workflow with condition assessment
        /// </summary>
        public async Task<QualityAssessmentResult> PerformQualityAssessmentAsync(int inventoryItemId, string inspectorUserId, QualityChecklistData checklistData)
        {
            _logger.LogInformation("Performing quality assessment for item {ItemId} by {Inspector}", inventoryItemId, inspectorUserId);

            var item = await _context.InventoryItems.FindAsync(inventoryItemId);
            if (item == null)
                throw new ArgumentException("Inventory item not found", nameof(inventoryItemId));

            var assessment = new QualityAssessmentResult
            {
                InventoryItemId = inventoryItemId,
                InspectorUserId = inspectorUserId,
                AssessmentDate = DateTime.UtcNow,
                ChecklistData = checklistData,
                OverallCondition = DetermineOverallCondition(checklistData),
                QualityScore = CalculateQualityScore(checklistData),
                ActionRequired = DetermineRequiredAction(checklistData)
            };

            // Update item condition if changed
            if (assessment.OverallCondition != item.Condition)
            {
                var oldCondition = item.Condition;
                item.Condition = assessment.OverallCondition;
                item.LastUpdatedDate = DateTime.UtcNow;
                item.LastUpdatedByUserId = inspectorUserId;

                await _auditService.LogAsync(
                    AuditAction.Update,
                    "InventoryItem",
                    inventoryItemId,
                    inspectorUserId,
                    $"Condition updated from {oldCondition} to {assessment.OverallCondition} based on quality assessment",
                    new { OldCondition = oldCondition },
                    new { NewCondition = assessment.OverallCondition, QualityScore = assessment.QualityScore }
                );
            }

            // Create quality assessment record
            var qualityRecord = new QualityAssessmentRecord
            {
                InventoryItemId = inventoryItemId,
                AssessmentDate = assessment.AssessmentDate,
                InspectorUserId = inspectorUserId,
                OverallCondition = assessment.OverallCondition,
                QualityScore = assessment.QualityScore,
                ChecklistJson = JsonSerializer.Serialize(checklistData),
                ActionRequired = assessment.ActionRequired,
                Notes = assessment.Notes,
                CreatedDate = DateTime.UtcNow
            };

            _context.QualityAssessmentRecords.Add(qualityRecord);
            await _context.SaveChangesAsync();

            return assessment;
        }

        #endregion

        #region Integration & Workflow Orchestration

        /// <summary>
        /// Intelligent request fulfillment with automatic asset creation
        /// </summary>
        public async Task<RequestFulfillmentResult> FulfillRequestIntelligentlyAsync(int requestId, string fulfillerUserId)
        {
            _logger.LogInformation("Intelligently fulfilling request {RequestId} by {Fulfiller}", requestId, fulfillerUserId);

            var request = await _context.ITRequests
                .Include(r => r.RequestedByUser)
                .FirstOrDefaultAsync(r => r.Id == requestId);

            if (request == null)
                throw new ArgumentException("Request not found", nameof(requestId));

            var result = new RequestFulfillmentResult
            {
                RequestId = requestId,
                FulfillerUserId = fulfillerUserId,
                FulfillmentDate = DateTime.UtcNow,
                Success = false,
                Actions = new List<FulfillmentAction>()
            };

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Intelligent item matching based on request requirements
                var matchingItems = await FindBestMatchingInventoryItemsAsync(request);

                foreach (var match in matchingItems)
                {
                    var fulfillmentAction = new FulfillmentAction
                    {
                        InventoryItemId = match.InventoryItemId,
                        ItemName = match.ItemName,
                        QuantityRequired = match.RequiredQuantity,
                        QuantityAvailable = match.AvailableQuantity,
                        MatchScore = match.MatchScore
                    };

                    // Check availability and reserve
                    if (match.AvailableQuantity >= match.RequiredQuantity)
                    {
                        // Create asset from inventory item
                        var asset = await CreateAssetFromInventoryAsync(match.InventoryItemId, match.RequiredQuantity, fulfillerUserId);
                        
                        if (asset != null)
                        {
                            // Update request with asset assignment
                            request.RelatedAssetId = asset.Id;
                            fulfillmentAction.AssetId = asset.Id;
                            fulfillmentAction.Success = true;
                        }
                    }

                    result.Actions.Add(fulfillmentAction);
                }

                // Update request status
                if (result.Actions.Any(a => a.Success))
                {
                    request.Status = RequestStatus.Completed;
                    request.CompletedDate = DateTime.UtcNow;
                    request.CompletedByUserId = fulfillerUserId;
                    result.Success = true;
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Request fulfillment completed for {RequestId}: {Success}", requestId, result.Success);

                return result;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Request fulfillment failed for {RequestId}", requestId);
                throw;
            }
        }

        #endregion

        #region Private Helper Methods

        private double CalculateVelocityScore(InventoryItem item)
        {
            var recentMovements = item.Movements
                .Where(m => m.MovementDate >= DateTime.UtcNow.AddDays(-90) && m.MovementType == InventoryMovementType.StockOut)
                .Count();

            if (recentMovements == 0) return 0.0;
            
            // Normalize based on 90-day period (max 1 movement per day = score of 1.0)
            return Math.Min(recentMovements / 90.0, 1.0);
        }

        private double CalculateCriticalityScore(InventoryItem item)
        {
            // Score based on category criticality and current stock vs minimum
            var categoryScore = item.Category switch
            {
                InventoryCategory.Server => 1.0,
                InventoryCategory.NetworkDevice => 0.9,
                InventoryCategory.Desktop => 0.7,
                InventoryCategory.Laptop => 0.8,
                _ => 0.5
            };

            var stockRatio = item.MinimumStock > 0 ? (double)item.Quantity / item.MinimumStock : 1.0;
            var stockScore = stockRatio < 0.5 ? 1.0 : stockRatio < 1.0 ? 0.8 : 0.5;

            return categoryScore * stockScore;
        }

        private decimal CalculateCarryingCost(InventoryItem item)
        {
            // Simplified carrying cost calculation (typically 20-30% of inventory value annually)
            var carryingRate = 0.25m; // 25% annually
            return (item.TotalValue ?? 0) * carryingRate / 12; // Monthly carrying cost
        }

        private List<AbcRecommendation> GenerateAbcRecommendations(AbcAnalysisResult analysis)
        {
            var recommendations = new List<AbcRecommendation>();

            // A-items: High control, frequent review
            recommendations.Add(new AbcRecommendation
            {
                Category = "A",
                Recommendation = "Implement tight inventory control with weekly reviews",
                ActionItems = new[] { "Weekly stock reviews", "Vendor-managed inventory consideration", "Safety stock optimization" }
            });

            // B-items: Moderate control
            recommendations.Add(new AbcRecommendation
            {
                Category = "B",
                Recommendation = "Monthly reviews with automated reordering",
                ActionItems = new[] { "Monthly stock reviews", "Automated reorder points", "Quarterly supplier reviews" }
            });

            // C-items: Simple controls
            recommendations.Add(new AbcRecommendation
            {
                Category = "C",
                Recommendation = "Quarterly reviews with simple min/max controls",
                ActionItems = new[] { "Quarterly bulk orders", "Simplified forecasting", "Focus on inventory reduction" }
            });

            return recommendations;
        }

        private async Task<Asset?> CreateAssetFromInventoryAsync(int inventoryItemId, int quantity, string createdByUserId)
        {
            var inventoryItem = await _context.InventoryItems.FindAsync(inventoryItemId);
            if (inventoryItem == null || inventoryItem.Quantity < quantity)
                return null;

            // Create asset from inventory item
            var asset = new Asset
            {
                AssetTag = await GenerateAssetTagAsync(),
                Category = MapInventoryToAssetCategory(inventoryItem.Category),
                Brand = inventoryItem.Brand,
                Model = inventoryItem.Model,
                SerialNumber = inventoryItem.SerialNumber ?? string.Empty,
                InternalSerialNumber = Guid.NewGuid().ToString("N")[..12].ToUpper(),
                Description = inventoryItem.Description ?? string.Empty,
                Status = AssetStatus.Available,
                LocationId = inventoryItem.LocationId,
                InstallationDate = DateTime.UtcNow,
                CreatedDate = DateTime.UtcNow,
                LastUpdated = DateTime.UtcNow,
                PurchasePrice = inventoryItem.UnitCost,
                Supplier = inventoryItem.Supplier
            };

            _context.Assets.Add(asset);

            // Reduce inventory quantity
            inventoryItem.Quantity -= quantity;
            inventoryItem.LastUpdatedDate = DateTime.UtcNow;
            inventoryItem.LastUpdatedByUserId = createdByUserId;

            // Create inventory movement
            var movement = new InventoryMovement
            {
                InventoryItemId = inventoryItemId,
                MovementType = InventoryMovementType.StockOut,
                Quantity = quantity,
                MovementDate = DateTime.UtcNow,
                Reason = $"Converted to Asset {asset.AssetTag}",
                PerformedByUserId = createdByUserId,
                CreatedDate = DateTime.UtcNow
            };

            _context.InventoryMovements.Add(movement);

            // Create asset-inventory mapping
            var mapping = new AssetInventoryMapping
            {
                AssetId = asset.Id,
                InventoryItemId = inventoryItemId,
                Quantity = quantity,
                MappingDate = DateTime.UtcNow,
                Status = AssetInventoryMappingStatus.Active,
                CreatedDate = DateTime.UtcNow,
                CreatedByUserId = createdByUserId
            };

            _context.AssetInventoryMappings.Add(mapping);

            await _context.SaveChangesAsync();

            return asset;
        }

        private async Task<string> GenerateAssetTagAsync()
        {
            var year = DateTime.UtcNow.Year;
            var lastAsset = await _context.Assets
                .Where(a => a.AssetTag.StartsWith($"AST{year}"))
                .OrderByDescending(a => a.AssetTag)
                .FirstOrDefaultAsync();

            int nextNumber = 1;
            if (lastAsset != null && lastAsset.AssetTag.Length >= 7)
            {
                var numberPart = lastAsset.AssetTag[7..];
                if (int.TryParse(numberPart, out int currentNumber))
                {
                    nextNumber = currentNumber + 1;
                }
            }

            return $"AST{year}{nextNumber:D4}";
        }

        private AssetCategory MapInventoryToAssetCategory(InventoryCategory inventoryCategory)
        {
            return inventoryCategory switch
            {
                InventoryCategory.Desktop => AssetCategory.Desktop,
                InventoryCategory.Laptop => AssetCategory.Laptop,
                InventoryCategory.Server => AssetCategory.Server,
                InventoryCategory.NetworkDevice => AssetCategory.NetworkDevice,
                InventoryCategory.Printer => AssetCategory.Printer,
                InventoryCategory.Monitor => AssetCategory.Monitor,
                _ => AssetCategory.Other
            };
        }

        private List<double> CalculateDailyUsagePattern(List<InventoryMovement> movements)
        {
            if (!movements.Any()) return new List<double>();
            
            var dailyUsage = new Dictionary<DateTime, int>();
            foreach (var movement in movements)
            {
                var date = movement.MovementDate.Date;
                dailyUsage[date] = dailyUsage.GetValueOrDefault(date, 0) + movement.Quantity;
            }
            
            return dailyUsage.Values.Select(v => (double)v).ToList();
        }

        private List<double> CalculateWeeklyPattern(List<InventoryMovement> movements)
        {
            var weeklyPattern = new double[7]; // Monday = 0, Sunday = 6
            var weeklyCounts = new int[7];
            
            foreach (var movement in movements)
            {
                var dayOfWeek = (int)movement.MovementDate.DayOfWeek;
                dayOfWeek = dayOfWeek == 0 ? 6 : dayOfWeek - 1; // Convert Sunday=0 to Sunday=6
                weeklyPattern[dayOfWeek] += movement.Quantity;
                weeklyCounts[dayOfWeek]++;
            }
            
            // Calculate averages
            for (int i = 0; i < 7; i++)
            {
                weeklyPattern[i] = weeklyCounts[i] > 0 ? weeklyPattern[i] / weeklyCounts[i] : 0;
            }
            
            return weeklyPattern.ToList();
        }

        private List<double> CalculateMonthlyPattern(List<InventoryMovement> movements)
        {
            var monthlyPattern = new double[12]; // January = 0, December = 11
            var monthlyCounts = new int[12];
            
            foreach (var movement in movements)
            {
                var month = movement.MovementDate.Month - 1; // Convert to 0-based
                monthlyPattern[month] += movement.Quantity;
                monthlyCounts[month]++;
            }
            
            // Calculate averages
            for (int i = 0; i < 12; i++)
            {
                monthlyPattern[i] = monthlyCounts[i] > 0 ? monthlyPattern[i] / monthlyCounts[i] : 0;
            }
            
            return monthlyPattern.ToList();
        }

        private List<double> CalculateSeasonalFactors(List<InventoryMovement> movements)
        {
            var quarterlyUsage = new double[4]; // Q1=0, Q2=1, Q3=2, Q4=3
            var quarterlyCounts = new int[4];
            
            foreach (var movement in movements)
            {
                var quarter = (movement.MovementDate.Month - 1) / 3;
                quarterlyUsage[quarter] += movement.Quantity;
                quarterlyCounts[quarter]++;
            }
            
            // Calculate averages and normalize
            var avgUsage = quarterlyUsage.Where((u, i) => quarterlyCounts[i] > 0).DefaultIfEmpty(0).Average();
            
            for (int i = 0; i < 4; i++)
            {
                if (quarterlyCounts[i] > 0)
                {
                    var avgQuarterUsage = quarterlyUsage[i] / quarterlyCounts[i];
                    quarterlyUsage[i] = avgUsage > 0 ? avgQuarterUsage / avgUsage : 1.0;
                }
                else
                {
                    quarterlyUsage[i] = 1.0;
                }
            }
            
            return quarterlyUsage.ToList();
        }

        private double CalculateConfidenceLevel(List<InventoryMovement> movements)
        {
            if (movements.Count < 10) return 0.3; // Low confidence with limited data
            if (movements.Count < 30) return 0.6; // Medium confidence
            return 0.85; // High confidence with sufficient data
        }

        private int CalculateOptimalReorderPoint(List<double> dailyUsage, int forecastDays)
        {
            if (!dailyUsage.Any()) return 10; // Default safety stock
            
            var avgDemand = dailyUsage.Average();
            var demandStdDev = CalculateStandardDeviation(dailyUsage);
            var leadTime = 7; // Assume 7 days lead time
            var serviceLevel = 0.95; // 95% service level
            var zScore = 1.645; // Z-score for 95% service level
            
            var safetyStock = zScore * demandStdDev * Math.Sqrt(leadTime);
            var reorderPoint = (avgDemand * leadTime) + safetyStock;
            
            return Math.Max(1, (int)Math.Ceiling(reorderPoint));
        }

        private int CalculateEconomicOrderQuantity(InventoryItem item, double averageDemand)
        {
            if (averageDemand <= 0) return item.ReorderLevel > 0 ? item.ReorderLevel : 10;
            
            var annualDemand = averageDemand * 365;
            var orderingCost = 50; // Estimated ordering cost
            var holdingCostRate = 0.25; // 25% of item cost annually
            var itemCost = (double)(item.UnitCost ?? 100); // Default cost if not available
            var holdingCost = itemCost * holdingCostRate;
            
            if (holdingCost <= 0) return item.ReorderLevel > 0 ? item.ReorderLevel : 10;
            
            var eoq = Math.Sqrt((2 * annualDemand * orderingCost) / holdingCost);
            return Math.Max(1, (int)Math.Ceiling(eoq));
        }

        private List<DailyDemandForecast> GenerateDailyForecast(List<double> dailyUsage, List<double> weeklyPattern, 
            List<double> monthlyPattern, List<double> seasonalFactors, int forecastDays)
        {
            var forecast = new List<DailyDemandForecast>();
            var avgDemand = dailyUsage.DefaultIfEmpty(0).Average();
            var currentStock = 0; // This would be passed in or retrieved
            
            for (int day = 0; day < forecastDays; day++)
            {
                var forecastDate = DateTime.UtcNow.AddDays(day);
                var dayOfWeek = (int)forecastDate.DayOfWeek;
                var month = forecastDate.Month - 1;
                var quarter = month / 3;
                
                dayOfWeek = dayOfWeek == 0 ? 6 : dayOfWeek - 1; // Convert Sunday
                
                var weeklyFactor = weeklyPattern.Count > dayOfWeek ? weeklyPattern[dayOfWeek] / avgDemand : 1.0;
                var monthlyFactor = monthlyPattern.Count > month ? monthlyPattern[month] / avgDemand : 1.0;
                var seasonalFactor = seasonalFactors.Count > quarter ? seasonalFactors[quarter] : 1.0;
                
                // Avoid division by zero and NaN
                if (double.IsNaN(weeklyFactor) || double.IsInfinity(weeklyFactor)) weeklyFactor = 1.0;
                if (double.IsNaN(monthlyFactor) || double.IsInfinity(monthlyFactor)) monthlyFactor = 1.0;
                if (double.IsNaN(seasonalFactor) || double.IsInfinity(seasonalFactor)) seasonalFactor = 1.0;
                
                var forecastedDemand = avgDemand * weeklyFactor * monthlyFactor * seasonalFactor;
                currentStock = Math.Max(0, currentStock - (int)forecastedDemand);
                
                var riskLevel = currentStock == 0 ? "Critical" : currentStock < 5 ? "High" : currentStock < 10 ? "Medium" : "Low";
                
                forecast.Add(new DailyDemandForecast
                {
                    Date = forecastDate,
                    ForecastedDemand = Math.Max(0, forecastedDemand),
                    ConfidenceInterval = forecastedDemand * 0.2, // 20% confidence interval
                    ProjectedStock = currentStock,
                    SeasonalFactor = seasonalFactor,
                    RiskLevel = riskLevel
                });
            }
            
            return forecast;
        }

        private double CalculateStandardDeviation(List<double> values)
        {
            if (values.Count < 2) return 0;
            
            var mean = values.Average();
            var variance = values.Select(v => Math.Pow(v - mean, 2)).Average();
            return Math.Sqrt(variance);
        }

        private async Task<ReplenishmentDecision> MakeReplenishmentDecisionAsync(InventoryItem item, DemandForecast forecast)
        {
            var decision = new ReplenishmentDecision();
            
            // Check if item is below reorder level
            if (item.Quantity <= item.ReorderLevel)
            {
                decision.ShouldReplenish = true;
                decision.OrderQuantity = Math.Max(forecast.RecommendedOrderQuantity, item.ReorderLevel);
                
                if (item.Quantity == 0)
                {
                    decision.Priority = ReplenishmentPriority.Critical;
                    decision.Reasoning = "Item is out of stock";
                }
                else if (item.Quantity <= item.MinimumStock)
                {
                    decision.Priority = ReplenishmentPriority.High;
                    decision.Reasoning = "Item is below minimum stock level";
                }
                else
                {
                    decision.Priority = ReplenishmentPriority.Medium;
                    decision.Reasoning = "Item has reached reorder point";
                }
                
                decision.EstimatedDeliveryDays = 7; // Default lead time
                decision.EstimatedCost = decision.OrderQuantity * (item.UnitCost ?? 0);
            }
            
            return decision;
        }

        private async Task<bool> CreateAutomaticProcurementRequestAsync(InventoryItem item, int quantity, string userId)
        {
            try
            {
                var procurementRequest = new ProcurementRequest
                {
                    ProcurementNumber = await GenerateProcurementNumberAsync(),
                    Title = $"Auto-Replenishment: {item.Name}",
                    Description = $"Automatic replenishment for {item.Name} - Stock level: {item.Quantity}, Reorder level: {item.ReorderLevel}",
                    ProcurementType = ProcurementType.Equipment,
                    Category = ProcurementCategory.ITEquipment,
                    Status = ProcurementStatus.Draft,
                    Method = ProcurementMethod.DirectPurchase,
                    Source = ProcurementSource.AutoGenerated,
                    RequestDate = DateTime.UtcNow,
                    RequiredByDate = DateTime.UtcNow.AddDays(14),
                    RequestedByUserId = userId,
                    Department = "IT",
                    EstimatedBudget = quantity * (item.UnitCost ?? 0),
                    Priority = ProcurementPriority.High
                };

                _context.ProcurementRequests.Add(procurementRequest);
                await _context.SaveChangesAsync();

                var procurementItem = new ProcurementItem
                {
                    ProcurementRequestId = procurementRequest.Id,
                    ItemName = item.Name,
                    Description = item.Description,
                    TechnicalSpecifications = item.Specifications,
                    Quantity = quantity,
                    EstimatedUnitPrice = item.UnitCost ?? 0
                };

                _context.ProcurementItems.Add(procurementItem);
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create automatic procurement request for item {ItemId}", item.Id);
                return false;
            }
        }

        private async Task<string> GenerateProcurementNumberAsync()
        {
            var year = DateTime.UtcNow.Year;
            var lastProcurement = await _context.ProcurementRequests
                .Where(p => p.ProcurementNumber.StartsWith($"PR{year}"))
                .OrderByDescending(p => p.ProcurementNumber)
                .FirstOrDefaultAsync();

            int nextNumber = 1;
            if (lastProcurement != null && lastProcurement.ProcurementNumber.Length >= 6)
            {
                var numberPart = lastProcurement.ProcurementNumber[6..];
                if (int.TryParse(numberPart, out int currentNumber))
                {
                    nextNumber = currentNumber + 1;
                }
            }

            return $"PR{year}{nextNumber:D4}";
        }

        private string DetermineOptimalZone(double velocity, InventoryItem item)
        {
            if (velocity > 0.8) return "A"; // High velocity - easily accessible
            if (velocity > 0.4) return "B"; // Medium velocity - moderately accessible
            return "C"; // Low velocity - long-term storage
        }

        private string GetVelocityDescription(double velocity)
        {
            return velocity switch
            {
                > 0.8 => "High velocity - frequent movement",
                > 0.4 => "Medium velocity - regular movement",
                > 0.1 => "Low velocity - occasional movement",
                _ => "Very low velocity - rare movement"
            };
        }

        private double CalculateEfficiencyGain(List<SpaceOptimizationRecommendation> recommendations)
        {
            // Simplified efficiency calculation based on velocity improvements
            var totalRecommendations = recommendations.Count;
            var highPriorityRecommendations = recommendations.Count(r => r.Priority == "High");
            
            if (totalRecommendations == 0) return 0;
            
            return (highPriorityRecommendations * 0.3 + (totalRecommendations - highPriorityRecommendations) * 0.1);
        }

        private InventoryCondition DetermineOverallCondition(QualityChecklistData checklist)
        {
            var positiveChecks = 0;
            var totalChecks = 8; // Number of boolean checks in checklist
            
            if (checklist.PhysicalConditionGood) positiveChecks++;
            if (checklist.AllComponentsPresent) positiveChecks++;
            if (checklist.FunctionalityTested) positiveChecks++;
            if (checklist.CosmeticConditionAcceptable) positiveChecks++;
            if (checklist.PackagingIntact) positiveChecks++;
            if (checklist.DocumentationIncluded) positiveChecks++;
            if (checklist.SerialNumberVerified) positiveChecks++;
            if (checklist.WarrantyValid) positiveChecks++;
            
            var score = (double)positiveChecks / totalChecks;
            
            return score switch
            {
                >= 0.9 => InventoryCondition.New,
                >= 0.8 => InventoryCondition.Excellent,
                >= 0.6 => InventoryCondition.Good,
                >= 0.4 => InventoryCondition.Fair,
                >= 0.2 => InventoryCondition.Poor,
                _ => InventoryCondition.Obsolete
            };
        }

        private double CalculateQualityScore(QualityChecklistData checklist)
        {
            var score = 0.0;
            
            // Weight different aspects
            if (checklist.PhysicalConditionGood) score += 20;
            if (checklist.AllComponentsPresent) score += 15;
            if (checklist.FunctionalityTested) score += 25;
            if (checklist.CosmeticConditionAcceptable) score += 10;
            if (checklist.PackagingIntact) score += 5;
            if (checklist.DocumentationIncluded) score += 10;
            if (checklist.SerialNumberVerified) score += 10;
            if (checklist.WarrantyValid) score += 5;
            
            // Overall rating contributes to final score (1-10 scale mapped to 0-100)
            score = (score * 0.7) + (checklist.OverallRating * 3); // 70% checklist, 30% rating
            
            return Math.Min(100, Math.Max(0, score));
        }

        private string DetermineRequiredAction(QualityChecklistData checklist)
        {
            if (!checklist.FunctionalityTested) return "Functionality test required";
            if (!checklist.PhysicalConditionGood) return "Physical repair required";
            if (!checklist.AllComponentsPresent) return "Component replacement needed";
            if (checklist.IssuesFound.Any()) return "Address identified issues";
            if (checklist.OverallRating < 5) return "Detailed inspection and refurbishment required";
            if (checklist.OverallRating < 7) return "Minor refurbishment recommended";
            
            return "Ready for deployment";
        }

        private async Task<List<InventoryMatch>> FindBestMatchingInventoryItemsAsync(ITRequest request)
        {
            var matches = new List<InventoryMatch>();
            
            // This is a simplified matching algorithm - in practice, this would be much more sophisticated
            // Based on request type, find suitable inventory items
            var searchTerms = ExtractSearchTermsFromRequest(request);
            
            foreach (var term in searchTerms)
            {
                var items = await _context.InventoryItems
                    .Where(i => i.Status == InventoryStatus.Active && 
                               i.Quantity > 0 &&
                               (i.Name.Contains(term) || 
                                i.Brand.Contains(term) || 
                                i.Model.Contains(term)))
                    .ToListAsync();
                
                foreach (var item in items)
                {
                    var matchScore = CalculateMatchScore(request, item);
                    if (matchScore > 0.5) // Only include good matches
                    {
                        matches.Add(new InventoryMatch
                        {
                            InventoryItemId = item.Id,
                            ItemName = item.Name,
                            RequiredQuantity = 1, // Simplified - would be determined by request analysis
                            AvailableQuantity = item.Quantity,
                            MatchScore = matchScore,
                            MatchReasoning = $"Matched on {term} with score {matchScore:F2}"
                        });
                    }
                }
            }
            
            return matches.OrderByDescending(m => m.MatchScore).Take(5).ToList();
        }

        private List<string> ExtractSearchTermsFromRequest(ITRequest request)
        {
            var terms = new List<string>();
            
            // Extract meaningful terms from request description and title
            var text = $"{request.Title} {request.Description}".ToLower();
            
            // Common IT equipment terms
            var keywords = new[] { "laptop", "desktop", "monitor", "printer", "keyboard", "mouse", "server", "switch", "router" };
            
            foreach (var keyword in keywords)
            {
                if (text.Contains(keyword))
                {
                    terms.Add(keyword);
                }
            }
            
            // If no specific terms found, use request type
            if (!terms.Any())
            {
                terms.Add(request.RequestType.ToString().ToLower());
            }
            
            return terms;
        }

        private double CalculateMatchScore(ITRequest request, InventoryItem item)
        {
            double score = 0.0;
            
            // Basic matching based on request type and item category
            if (request.RequestType == RequestType.NewEquipment)
            {
                score += item.Condition == InventoryCondition.New ? 1.0 : 0.5;
            }
            
            if (request.RequestType == RequestType.HardwareReplacement)
            {
                score += item.Condition >= InventoryCondition.Good ? 0.8 : 0.3;
            }
            
            // Availability score
            score += item.Quantity > 0 ? 0.5 : 0.0;
            
            // Condition score
            score += item.Condition switch
            {
                InventoryCondition.New => 1.0,
                InventoryCondition.Excellent => 0.9,
                InventoryCondition.Good => 0.7,
                InventoryCondition.Fair => 0.5,
                _ => 0.2
            } * 0.3;
            
            return Math.Min(1.0, score);
        }

        #endregion
    }

    #region Supporting Models and Interfaces

    public interface IWarehouseBusinessLogicService
    {
        Task<AbcAnalysisResult> PerformAbcAnalysisAsync(int analysisMonths = 12);
        Task<DemandForecast> GenerateDemandForecastAsync(int inventoryItemId, int forecastDays = 90);
        Task<SmartReplenishmentResult> ExecuteSmartReplenishmentAsync(string initiatedByUserId);
        Task<SpaceOptimizationResult> OptimizeWarehouseLayoutAsync(int locationId);
        Task<QualityAssessmentResult> PerformQualityAssessmentAsync(int inventoryItemId, string inspectorUserId, QualityChecklistData checklistData);
        Task<RequestFulfillmentResult> FulfillRequestIntelligentlyAsync(int requestId, string fulfillerUserId);
    }

    // Supporting model classes would be defined in Models/WarehouseModels.cs
    
    #endregion
}
