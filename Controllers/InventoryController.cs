using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using HospitalAssetTracker.Models;
using HospitalAssetTracker.Services;
using System.Security.Claims;
using static HospitalAssetTracker.Models.InventorySearchModels;

namespace HospitalAssetTracker.Controllers
{
    [Authorize]
    public class InventoryController : Controller
    {
        private readonly IInventoryService _inventoryService;
        private readonly ILocationService _locationService;
        private readonly IAssetService _assetService;

        public InventoryController(
            IInventoryService inventoryService,
            ILocationService locationService,
            IAssetService assetService)
        {
            _inventoryService = inventoryService;
            _locationService = locationService;
            _assetService = assetService;
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
                var success = await _inventoryService.StockInAsync(id, quantity, unitCost, supplier, reason, userId, purchaseOrderNumber, invoiceNumber);
                
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
        [Authorize(Roles = "Admin,IT Support,Asset Manager,Department Head")]
        public async Task<IActionResult> Alerts()
        {
            var stockAlerts = await _inventoryService.GetStockLevelAlertsAsync();
            var expiryAlerts = await _inventoryService.GetExpiryAlertsAsync(30);
            
            ViewBag.StockAlerts = stockAlerts;
            ViewBag.ExpiryAlerts = expiryAlerts;
            
            return View();
        }

        // GET: Inventory/Dashboard
        [Authorize(Roles = "Admin,IT Support,Asset Manager,Department Head")]
        public async Task<IActionResult> Dashboard()
        {
            var dashboardData = await _inventoryService.GetInventoryDashboardDataAsync();
            return View(dashboardData);
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

            ViewBag.Statuses = new SelectList(Enum.GetValues<InventoryStatus>()
                .Select(s => new { Value = (int)s, Text = s.ToString() }), "Value", "Text");

            ViewBag.Conditions = new SelectList(Enum.GetValues<InventoryCondition>()
                .Select(c => new { Value = (int)c, Text = c.ToString() }), "Value", "Text");
        }

        #endregion
    }
}
