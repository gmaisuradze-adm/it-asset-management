using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using HospitalAssetTracker.Models;
using HospitalAssetTracker.Services;
using HospitalAssetTracker.Data;
using System.Linq;
using System.Security.Claims;
using static HospitalAssetTracker.Models.InventorySearchModels;
using Microsoft.AspNetCore.Mvc.Rendering; // Added for SelectList
using Microsoft.EntityFrameworkCore;     // Added for .Include()

namespace HospitalAssetTracker.Controllers
{
    [Authorize]
    public class InventoryController : Controller
    {
        private readonly IInventoryService _inventoryService;
        private readonly ILocationService _locationService;
        private readonly IAssetService _assetService;
        private readonly IWarehouseBusinessLogicService _warehouseService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<InventoryController> _logger;
        private readonly ApplicationDbContext _context;

        public InventoryController(
            IInventoryService inventoryService,
            ILocationService locationService,
            IAssetService assetService,
            IWarehouseBusinessLogicService warehouseService,
            UserManager<ApplicationUser> userManager,
            ILogger<InventoryController> logger,
            ApplicationDbContext context)
        {
            _inventoryService = inventoryService;
            _locationService = locationService;
            _assetService = assetService;
            _warehouseService = warehouseService;
            _userManager = userManager;
            _logger = logger;
            _context = context;
        }

        // GET: Inventory
        [Authorize(Roles = "Admin,IT Support,Asset Manager")]
        public async Task<IActionResult> Index(InventorySearchCriteria? criteria = null)
        {
            criteria ??= new InventorySearchCriteria();
            
            var result = await _inventoryService.GetInventoryItemsPagedAsync(criteria);
            
            await PopulateViewData();
            
            ViewData["SearchCriteria"] = criteria;
            return View(result);
        }

        // GET: Inventory/Details/5
        [Authorize(Roles = "Admin,IT Support,Asset Manager,Department Head")]
        public async Task<IActionResult> Details(int id)
        {
            var item = await _inventoryService.GetInventoryItemByIdAsync(id);
            if (item == null)
            {
                return NotFound();
            }

            // Get movement history
            var movements = await _inventoryService.GetInventoryMovementHistoryAsync(id);
            ViewBag.Movements = movements;

            // Get transaction history
            var transactions = await _inventoryService.GetTransactionHistoryAsync(id);
            ViewBag.Transactions = transactions;

            // Get asset mappings
            var assetMappings = await _inventoryService.GetInventoryAssetMappingsAsync(id);
            ViewBag.AssetMappings = assetMappings;

            return View(item);
        }

        // GET: Inventory/Create
        [Authorize(Roles = "Admin,IT Support,Asset Manager")]
        public async Task<IActionResult> Create()
        {
            await PopulateViewData();
            return View();
        }

        // POST: Inventory/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,IT Support,Asset Manager")]
        public async Task<IActionResult> Create(InventoryItem item)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
                    var createdItem = await _inventoryService.CreateInventoryItemAsync(item, userId);
                    
                    TempData["SuccessMessage"] = $"Inventory item '{createdItem.Name}' created successfully with code '{createdItem.ItemCode}'.";
                    return RedirectToAction(nameof(Details), new { id = createdItem.Id });
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Error creating inventory item: {ex.Message}");
                }
            }

            await PopulateViewData();
            return View(item);
        }

        // GET: Inventory/Edit/5
        [Authorize(Roles = "Admin,IT Support,Asset Manager")]
        public async Task<IActionResult> Edit(int id)
        {
            var item = await _inventoryService.GetInventoryItemByIdAsync(id);
            if (item == null)
            {
                return NotFound();
            }

            await PopulateViewData();
            return View(item);
        }

        // POST: Inventory/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,IT Support,Asset Manager")]
        public async Task<IActionResult> Edit(int id, InventoryItem item)
        {
            if (id != item.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
                    await _inventoryService.UpdateInventoryItemAsync(item, userId);
                    
                    TempData["SuccessMessage"] = $"Inventory item '{item.Name}' updated successfully.";
                    return RedirectToAction(nameof(Details), new { id = item.Id });
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Error updating inventory item: {ex.Message}");
                }
            }

            await PopulateViewData();
            return View(item);
        }

        // GET: Inventory/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _inventoryService.GetInventoryItemByIdAsync(id);
            if (item == null)
            {
                return NotFound();
            }

            return View(item);
        }

        // POST: Inventory/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
                var success = await _inventoryService.DeleteInventoryItemAsync(id, userId);
                if (success)
                {
                    TempData["SuccessMessage"] = "Inventory item deleted successfully.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to delete inventory item.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error deleting inventory item: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Inventory/StockIn/5
        [Authorize(Roles = "Admin,IT Support,Asset Manager")]
        public async Task<IActionResult> StockIn(int id)
        {
            var item = await _inventoryService.GetInventoryItemByIdAsync(id);
            if (item == null)
            {
                return NotFound();
            }

            ViewBag.InventoryItem = item;
            return View();
        }

        // POST: Inventory/StockIn/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,IT Support,Asset Manager")]
        public async Task<IActionResult> StockIn(int id, int quantity, decimal? unitCost, string supplier, string reason, string? purchaseOrderNumber = null, string? invoiceNumber = null)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
                var stockInRequest = new InventorySearchModels.StockInRequest
                {
                    ItemId = id,
                    Quantity = quantity,
                    UnitCost = unitCost,
                    Supplier = supplier,
                    Reason = reason,
                    PurchaseOrderNumber = purchaseOrderNumber,
                    InvoiceNumber = invoiceNumber
                };
                
                var success = await _inventoryService.StockInAsync(stockInRequest, userId);
                
                if (success)
                {
                    TempData["SuccessMessage"] = $"Stock in completed successfully. Added {quantity} units.";
                    return RedirectToAction(nameof(Details), new { id });
                }
                
                TempData["ErrorMessage"] = "Failed to complete stock in operation.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error during stock in: {ex.Message}";
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        // GET: Inventory/StockOut/5
        [Authorize(Roles = "Admin,IT Support,Asset Manager")]
        public async Task<IActionResult> StockOut(int id)
        {
            var item = await _inventoryService.GetInventoryItemByIdAsync(id);
            if (item == null)
            {
                return NotFound();
            }

            ViewBag.InventoryItem = item;
            return View();
        }

        // POST: Inventory/StockOut/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,IT Support,Asset Manager")]
        public async Task<IActionResult> StockOut(int id, int quantity, string reason)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
                var success = await _inventoryService.StockOutAsync(id, quantity, reason, userId);
                
                if (success)
                {
                    TempData["SuccessMessage"] = $"Stock out completed successfully. Removed {quantity} units.";
                    return RedirectToAction(nameof(Details), new { id });
                }
                
                TempData["ErrorMessage"] = "Failed to complete stock out operation.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error during stock out: {ex.Message}";
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        // GET: Inventory/Deploy/5
        [Authorize(Roles = "Admin,IT Support,Asset Manager")]
        public async Task<IActionResult> Deploy(int id)
        {
            var item = await _inventoryService.GetInventoryItemByIdAsync(id);
            if (item == null)
            {
                return NotFound();
            }

            ViewBag.InventoryItem = item;
            
            // Get available assets for deployment
            var assets = await _assetService.GetAllAssetsAsync();
            ViewBag.Assets = new SelectList(assets.Where(a => a.Status == AssetStatus.Available || 
                                                              a.Status == AssetStatus.InUse), 
                                          "Id", "AssetTag");

            return View();
        }

        // POST: Inventory/Deploy/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,IT Support,Asset Manager")]
        public async Task<IActionResult> Deploy(int id, int assetId, int quantity, string reason)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
                var success = await _inventoryService.DeployToAssetAsync(assetId, id, quantity, reason, userId);
                
                if (success)
                {
                    TempData["SuccessMessage"] = $"Successfully deployed {quantity} units to asset.";
                    return RedirectToAction(nameof(Details), new { id });
                }
                
                TempData["ErrorMessage"] = "Failed to deploy to asset.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error during deployment: {ex.Message}";
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        // GET: Inventory/Alerts
        [Authorize(Roles = "Admin,IT Support,Asset Manager,Department Head,Warehouse Manager")]
        public async Task<IActionResult> Alerts()
        {
            try
            {
                var stockAlerts = await _inventoryService.GetStockLevelAlertsAsync() ?? new List<StockLevelAlert>();
                var expiryAlerts = await _inventoryService.GetExpiryAlertsAsync();
                
                ViewBag.StockAlerts = stockAlerts;
                ViewBag.ExpiryAlerts = expiryAlerts;

                // Get all unique InventoryItemIds from both alert lists
                var itemIdsFromStockAlerts = stockAlerts.Select(sa => sa.InventoryItemId);
                var itemIdsFromExpiryAlerts = expiryAlerts.Select(ea => ea.InventoryItemId);
                
                var allAlertedItemIds = itemIdsFromStockAlerts.Union(itemIdsFromExpiryAlerts).Distinct().ToList();

                IEnumerable<InventoryItem> alertedInventoryItems = new List<InventoryItem>();
                if (allAlertedItemIds.Any())
                {
                    // Fetch the InventoryItem details for these alerted items
                    // This assumes GetInventoryItemsByIdsAsync exists or we fetch them one by one (less efficient) or use a WhereIn query.
                    // For simplicity, let's assume we can filter by a list of IDs.
                    // If _context is ApplicationDbContext and InventoryItems is a DbSet.
                    alertedInventoryItems = await _context.InventoryItems
                                                .Include(i => i.Location) // Include any needed navigation properties
                                                .Where(i => allAlertedItemIds.Contains(i.Id))
                                                .ToListAsync();
                }
                
                return View(alertedInventoryItems); // Pass the list of alerted InventoryItem objects as the model
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading alerts");
                TempData["ErrorMessage"] = "Error loading alerts.";
                // Pass an empty list to the view in case of an error to prevent view rendering issues
                return View(new List<InventoryItem>()); 
            }
        }

        // GET: Inventory/Dashboard - Enhanced with warehouse management features
        [Authorize(Roles = "Admin,IT Support,Asset Manager,Department Head,Warehouse Manager")]
        public async Task<IActionResult> Dashboard()
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                
                // Get basic dashboard data
                var dashboardData = await _inventoryService.GetInventoryDashboardDataAsync();
                
                // Get recent movements with enhanced data
                var recentMovements = (await _inventoryService.GetRecentMovementsAsync(7))
                    .Select(m => new InventoryMovementViewModel
                    {
                        Id = m.Id,
                        ItemName = m.InventoryItem?.Name ?? "Unknown",
                        MovementType = m.MovementType,
                        QuantityChanged = m.Quantity,
                        MovementDate = m.MovementDate,
                        MovedBy = m.PerformedByUser?.UserName,
                        Reason = m.Reason,
                        ToLocationName = m.ToLocation?.FullLocation,
                        FromLocationName = m.FromLocation?.FullLocation,
                        PerformedByUserName = m.PerformedByUser?.UserName
                    }).ToList();
                
                // Update dashboard data with enhanced information
                dashboardData.RecentMovements = recentMovements;
                
                // Add warehouse-specific alerts
                var stockAlerts = await _inventoryService.GetStockLevelAlertsAsync();
                dashboardData.LowStockAlerts = stockAlerts.Select(alert => new InventoryAlertViewModel
                {
                    ItemId = alert.InventoryItemId,
                    ItemName = alert.ItemName,
                    AlertType = alert.AlertType,
                    Message = $"{alert.ItemName} is running low (Current: {alert.CurrentStock}, Minimum: {alert.MinimumLevel})",
                    Severity = alert.AlertType == "CriticalStock" ? "Critical" : "Warning",
                    CreatedDate = alert.CreatedDate,
                    CurrentQuantity = alert.CurrentStock,
                    MinimumStock = alert.MinimumLevel
                }).ToList();
                
                return View(dashboardData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading inventory dashboard");
                TempData["ErrorMessage"] = "Error loading dashboard data.";
                return View(new InventoryDashboardData());
            }
        }

        // POST: Inventory/PerformAbcAnalysis - ABC Analysis functionality
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,IT Support,Asset Manager,Warehouse Manager")]
        public async Task<IActionResult> PerformAbcAnalysis([FromBody] AbcAnalysisRequest request)
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                var analysisResult = await _warehouseService.PerformAbcAnalysisAsync(request.AnalysisMonths);
                
                TempData["SuccessMessage"] = $"ABC Analysis completed successfully. Analyzed {analysisResult.TotalItems} items.";
                return Json(new { success = true, message = $"ABC Analysis completed successfully. Analyzed {analysisResult.TotalItems} items." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error performing ABC analysis");
                return Json(new { success = false, message = "Error performing ABC analysis" });
            }
        }

        public class AbcAnalysisRequest
        {
            public int AnalysisMonths { get; set; } = 12;
        }

        // POST: Inventory/ExecuteSmartReplenishment - Smart replenishment functionality
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,IT Support,Asset Manager,Warehouse Manager")]
        public async Task<IActionResult> ExecuteSmartReplenishment()
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                if (string.IsNullOrEmpty(userId))
                {
                    return Json(new { success = false, message = "User authentication failed" });
                }
                
                var replenishmentResult = await _warehouseService.ExecuteSmartReplenishmentAsync(userId);
                
                TempData["SuccessMessage"] = $"Smart replenishment completed. {replenishmentResult.AutoProcurementsCreated} procurement requests created.";
                return Json(new { success = true, message = $"Smart replenishment completed successfully. {replenishmentResult.AutoProcurementsCreated} procurement requests created." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing smart replenishment");
                return Json(new { success = false, message = "Error executing smart replenishment" });
            }
        }

        // POST: Inventory/OptimizeWarehouseLayout - Space optimization functionality
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,IT Support,Asset Manager,Warehouse Manager")]
        public async Task<IActionResult> OptimizeWarehouseLayout([FromBody] SpaceOptimizationRequest request)
        {
            try
            {
                var optimizationResult = await _warehouseService.OptimizeWarehouseLayoutAsync(request.LocationId);
                
                TempData["SuccessMessage"] = $"Space optimization completed. {optimizationResult.TotalRecommendations} recommendations generated.";
                return Json(new { success = true, message = $"Space optimization completed successfully. {optimizationResult.TotalRecommendations} recommendations generated." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error optimizing warehouse layout");
                return Json(new { success = false, message = "Error optimizing warehouse layout" });
            }
        }

        public class SpaceOptimizationRequest
        {
            public int LocationId { get; set; } = 1;
        }

        #region Helper Methods

        private async Task PopulateViewData()
        {
            var locations = await _locationService.GetAllLocationsAsync();
            ViewBag.Locations = new SelectList(locations, "Id", "FullLocation");

            ViewBag.Categories = new SelectList(Enum.GetValues<InventoryCategory>()
                .Select(c => new { Value = (int)c, Text = c.ToString() }), "Value", "Text");

            ViewBag.ItemTypes = new SelectList(Enum.GetValues<InventoryItemType>()
                .Select(t => new { Value = (int)t, Text = t.ToString() }), "Value", "Text");

            ViewBag.Statuses = new SelectList(new[]
            {
                new { Value = 0, Text = "Active" },
                new { Value = 1, Text = "Reserved" },
                new { Value = 2, Text = "Allocated" },
                new { Value = 3, Text = "In Transit" },
                new { Value = 4, Text = "Deployed" },
                new { Value = 5, Text = "On Loan" },
                new { Value = 6, Text = "Under Testing" },
                new { Value = 7, Text = "Quarantine" },
                new { Value = 8, Text = "Awaiting Disposal" },
                new { Value = 9, Text = "Disposed" },
                new { Value = 10, Text = "Lost" },
                new { Value = 11, Text = "Stolen" },
                new { Value = 12, Text = "Damaged" }
            }, "Value", "Text");

            ViewBag.Conditions = new SelectList(Enum.GetValues<InventoryCondition>()
                .Select(c => new { Value = (int)c, Text = c.ToString() }), "Value", "Text");
        }

        #endregion

        // GET: Inventory/IndexAdvanced
        [Authorize(Roles = "Admin,IT Support,Asset Manager")]
        public async Task<IActionResult> IndexAdvanced()
        {
            try
            {
                // Initialize search model with default values
                var searchModel = new InventorySearchModels.AdvancedInventorySearchModel
                {
                    PageNumber = 1,
                    PageSize = 25,
                    SortBy = "ItemCode",
                    SortOrder = "asc"
                };

                // Get initial data
                var result = await _inventoryService.GetInventoryItemsAdvancedAsync(searchModel);
                
                // Get filter data for dropdowns
                ViewBag.Categories = new SelectList(Enum.GetValues<InventoryCategory>()
                    .Select(c => new { Value = (int)c, Text = c.ToString() }), "Value", "Text");
                ViewBag.Statuses = new SelectList(Enum.GetValues<InventoryStatus>()
                    .Select(s => new { Value = (int)s, Text = s.ToString() }), "Value", "Text");
                ViewBag.Conditions = new SelectList(Enum.GetValues<InventoryCondition>()
                    .Select(c => new { Value = (int)c, Text = c.ToString() }), "Value", "Text");
                
                ViewBag.Locations = new SelectList(await _context.Locations
                    .Select(l => new { l.Id, l.Name })
                    .ToListAsync(), "Id", "Name");

                return View(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading advanced inventory index");
                TempData["ErrorMessage"] = "Error loading inventory data.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Inventory/SearchAdvanced
        [HttpPost]
        [Authorize(Roles = "Admin,IT Support,Asset Manager")]
        public async Task<IActionResult> SearchAdvanced([FromBody] InventorySearchModels.AdvancedInventorySearchModel searchModel)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Json(new { success = false, message = "Invalid search parameters" });
                }

                var result = await _inventoryService.GetInventoryItemsAdvancedAsync(searchModel);
                
                return Json(new { 
                    success = true, 
                    data = result.Items,
                    totalCount = result.TotalCount,
                    pageNumber = result.PageNumber,
                    pageSize = result.PageSize,
                    totalPages = result.TotalPages
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error performing advanced search");
                return Json(new { success = false, message = "Search failed" });
            }
        }

        // POST: Inventory/BulkUpdate
        [HttpPost]
        [Authorize(Roles = "Admin,IT Support,Asset Manager")]
        public async Task<IActionResult> BulkUpdate([FromBody] InventorySearchModels.BulkInventoryUpdateRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Json(new { success = false, message = "Invalid bulk update parameters" });
                }

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
                var result = await _inventoryService.BulkUpdateInventoryAsync(request, userId);
                
                return Json(new { 
                    success = result.Success, 
                    message = result.Message,
                    updatedCount = result.AffectedItems
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error performing bulk update");
                return Json(new { success = false, message = "Bulk update failed" });
            }
        }

        // POST: Inventory/BulkExport
        [HttpPost]
        [Authorize(Roles = "Admin,IT Support,Asset Manager")]
        public async Task<IActionResult> BulkExport([FromBody] InventorySearchModels.InventoryExportRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Json(new { success = false, message = "Invalid export parameters" });
                }

                var fileData = await _inventoryService.ExportInventoryAsync(request);
                
                if (fileData == null)
                {
                    return Json(new { success = false, message = "Export failed - no data" });
                }

                var fileName = $"inventory_export_{DateTime.UtcNow:yyyyMMdd_HHmmss}.{request.Format.ToLower()}";
                var contentType = request.Format.ToUpper() == "EXCEL" ? "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" : "application/pdf";
                
                return File(fileData, contentType, fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error performing bulk export");
                return Json(new { success = false, message = "Export failed" });
            }
        }

        // GET: Inventory/QuickFilters
        [HttpGet]
        [Authorize(Roles = "Admin,IT Support,Asset Manager")]
        public async Task<IActionResult> QuickFilters(string filterType)
        {
            try
            {
                var result = filterType switch
                {
                    "lowStock" => await _inventoryService.GetLowStockItemsAsync(),
                    "outOfStock" => await _inventoryService.GetOutOfStockItemsAsync(),
                    "expiringSoon" => await _inventoryService.GetExpiringSoonItemsAsync(),
                    "highValue" => await _inventoryService.GetHighValueItemsAsync(),
                    "recentlyAdded" => await _inventoryService.GetRecentlyAddedItemsAsync(),
                    _ => new List<InventorySearchModels.AdvancedInventorySearchResult>()
                };

                return Json(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error applying quick filter: {FilterType}", filterType);
                return Json(new { success = false, message = "Filter failed" });
            }
        }
    }
}
