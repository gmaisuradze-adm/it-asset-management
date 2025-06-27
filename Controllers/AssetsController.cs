using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using HospitalAssetTracker.Models;
using HospitalAssetTracker.Services;
using Microsoft.Extensions.Logging;

namespace HospitalAssetTracker.Controllers
{
    [Authorize]
    public class AssetsController : Controller
    {
        private readonly IAssetService _assetService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<AssetsController> _logger; // Added ILogger

        public AssetsController(IAssetService assetService, 
                              UserManager<ApplicationUser> userManager, 
                              IWebHostEnvironment environment,
                              ILogger<AssetsController> logger) // Added logger parameter
        {
            _assetService = assetService;
            _userManager = userManager;
            _environment = environment;
            _logger = logger; // Assign logger
        }

        // GET: Assets
        public async Task<IActionResult> Index(string searchTerm, AssetCategory? category, AssetStatus? status, int? locationId, int page = 1, int pageSize = 25)
        {
            try
            {
                // Use pagination for better performance
                var pagedAssets = await _assetService.GetAssetsPagedAsync(page, pageSize, searchTerm, category, status, locationId);

                // Populate filter dropdowns
                ViewBag.Categories = new SelectList(Enum.GetValues<AssetCategory>().Select(c => new { Value = (int)c, Text = c.ToString() }), "Value", "Text");
                ViewBag.Statuses = new SelectList(Enum.GetValues<AssetStatus>().Select(s => new { Value = (int)s, Text = s.ToString() }), "Value", "Text");
                ViewBag.Locations = new SelectList(await _assetService.GetActiveLocationsAsync(), "Id", "FullLocation");
                
                // Preserve filter values
                ViewBag.SearchTerm = searchTerm;
                ViewBag.SelectedCategory = category;
                ViewBag.SelectedStatus = status;
                ViewBag.SelectedLocationId = locationId;
                ViewBag.CurrentPage = page;
                ViewBag.PageSize = pageSize;

                return View(pagedAssets);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while loading assets for Index page. SearchTerm: {SearchTerm}, Category: {Category}, Status: {Status}, LocationId: {LocationId}", searchTerm, category, status, locationId);
                TempData["ErrorMessage"] = $"An error occurred while loading assets: {ex.Message}"; // Using ex.Message
                
                // Return empty paged result on error
                var emptyResult = new PagedResult<Asset>
                {
                    Items = new List<Asset>(),
                    TotalCount = 0,
                    PageNumber = page,
                    PageSize = pageSize
                };
                
                return View(emptyResult);
            }
        }

        // GET: Assets/Advanced - Advanced search and management interface
        public async Task<IActionResult> IndexAdvanced()
        {
            try
            {
                // Initialize with default search results
                var searchModel = new AdvancedAssetSearchModel();
                var result = await _assetService.AdvancedSearchAsync(searchModel);
                
                // Populate dropdown data
                await PopulateAdvancedSearchViewData();
                
                return View(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while loading advanced assets page");
                TempData["ErrorMessage"] = "An error occurred while loading the advanced search page.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Assets/AdvancedSearch - Process advanced search
        [HttpPost]
        public async Task<IActionResult> AdvancedSearch([FromBody] AdvancedAssetSearchModel searchModel)
        {
            try
            {
                if (searchModel == null)
                {
                    return BadRequest("Invalid search criteria");
                }

                var result = await _assetService.AdvancedSearchAsync(searchModel);
                
                return Json(new
                {
                    success = true,
                    data = result.Assets.Select(a => new
                    {
                        id = a.Id,
                        assetTag = a.AssetTag,
                        category = a.Category.ToString(),
                        brand = a.Brand,
                        model = a.Model,
                        serialNumber = a.SerialNumber,
                        status = a.Status.ToString(),
                        statusClass = GetStatusClass(a.Status),
                        location = a.Location?.FullLocation ?? "Not Set",
                        assignedTo = a.AssignedToUser?.UserName ?? "Unassigned",
                        purchasePrice = a.PurchasePrice?.ToString("C") ?? "N/A",
                        warrantyExpiry = a.WarrantyExpiry?.ToString("MMM dd, yyyy") ?? "N/A",
                        warrantyStatus = GetWarrantyStatus(a.WarrantyExpiry),
                        lastUpdated = a.LastUpdated.ToString("MMM dd, yyyy")
                    }),
                    totalCount = result.TotalCount,
                    filteredCount = result.FilteredCount,
                    page = result.Page,
                    totalPages = result.TotalPages,
                    categoryCounts = result.CategoryCounts,
                    statusCounts = result.StatusCounts,
                    locationCounts = result.LocationCounts,
                    totalValue = result.TotalValue.ToString("C"),
                    warrantyExpiringCount = result.WarrantyExpiringCount,
                    unassignedCount = result.UnassignedCount
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during advanced search");
                return Json(new { success = false, message = "An error occurred while searching assets." });
            }
        }

        // POST: Assets/BulkOperation - Process bulk operations
        [HttpPost]
        public async Task<IActionResult> BulkOperation([FromBody] BulkOperationModel operationModel)
        {
            try
            {
                if (operationModel == null || !operationModel.AssetIds.Any())
                {
                    return BadRequest("No assets selected for bulk operation");
                }

                var result = await _assetService.ProcessBulkOperationAsync(operationModel);
                
                return Json(new
                {
                    success = result.Success,
                    message = result.Message,
                    processedCount = result.ProcessedCount,
                    successCount = result.SuccessCount,
                    failureCount = result.FailureCount,
                    errors = result.Errors,
                    results = result.Results
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during bulk operation: {Operation}", operationModel?.Operation);
                return Json(new { success = false, message = "An error occurred while processing the bulk operation." });
            }
        }

        // GET: Assets/Export - Export assets
        [HttpPost]
        public async Task<IActionResult> ExportAssets([FromBody] AssetExportModel exportModel)
        {
            try
            {
                if (exportModel == null)
                {
                    return BadRequest("Invalid export parameters");
                }

                var fileResult = await _assetService.ExportAssetsAsync(exportModel);
                
                if (fileResult == null)
                {
                    return BadRequest("Failed to generate export file");
                }

                return fileResult;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during asset export: {Format}", exportModel?.Format);
                return Json(new { success = false, message = "An error occurred while exporting assets." });
            }
        }

        // GET: Assets/Compare - Compare multiple assets
        [HttpPost]
        public async Task<IActionResult> CompareAssets([FromBody] List<int> assetIds)
        {
            try
            {
                if (assetIds == null || assetIds.Count < 2)
                {
                    return BadRequest("At least 2 assets must be selected for comparison");
                }

                if (assetIds.Count > 5)
                {
                    return BadRequest("Maximum 5 assets can be compared at once");
                }

                var comparison = await _assetService.CompareAssetsAsync(assetIds);
                
                return Json(new
                {
                    success = true,
                    assets = comparison.Assets.Select(a => new
                    {
                        id = a.Id,
                        assetTag = a.AssetTag,
                        category = a.Category.ToString(),
                        brand = a.Brand,
                        model = a.Model,
                        serialNumber = a.SerialNumber,
                        status = a.Status.ToString(),
                        location = a.Location?.FullLocation ?? "Not Set",
                        assignedTo = a.AssignedToUser?.UserName ?? "Unassigned",
                        purchasePrice = a.PurchasePrice?.ToString("C") ?? "N/A",
                        warrantyExpiry = a.WarrantyExpiry?.ToString("MMM dd, yyyy") ?? "N/A",
                        acquisitionDate = a.AcquisitionDate?.ToString("MMM dd, yyyy") ?? "N/A",
                        installationDate = a.InstallationDate.ToString("MMM dd, yyyy"),
                        description = a.Description ?? "",
                        department = a.Department ?? "",
                        supplier = a.Supplier ?? "",
                        notes = a.Notes ?? ""
                    }),
                    comparisonData = comparison.ComparisonData,
                    differentFields = comparison.DifferentFields
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during asset comparison");
                return Json(new { success = false, message = "An error occurred while comparing assets." });
            }
        }

        // GET: Assets/SearchSuggestions - Get search suggestions
        [HttpGet]
        public async Task<IActionResult> SearchSuggestions(string term, string type = "all")
        {
            try
            {
                if (string.IsNullOrWhiteSpace(term) || term.Length < 2)
                {
                    return Json(new List<object>());
                }

                var suggestions = await _assetService.GetSearchSuggestionsAsync(term, type);
                
                return Json(suggestions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting search suggestions for term: {Term}", term);
                return Json(new List<object>());
            }
        }

        // Helper method to populate ViewData for advanced search
        private async Task PopulateAdvancedSearchViewData()
        {
            var locations = await _assetService.GetActiveLocationsAsync();
            var users = await _userManager.Users.Select(u => new { Id = u.Id, Name = u.UserName }).ToListAsync();
            
            ViewData["Categories"] = Enum.GetValues<AssetCategory>()
                .Select(c => new { Value = (int)c, Text = c.ToString() }).ToList();
            
            ViewData["Statuses"] = Enum.GetValues<AssetStatus>()
                .Select(s => new { Value = (int)s, Text = s.ToString() }).ToList();
            
            ViewData["Locations"] = locations.Select(l => new { Value = l.Id, Text = l.FullLocation }).ToList();
            
            ViewData["Users"] = users;
            
            ViewData["Departments"] = await _assetService.GetDepartmentsAsync();
            
            ViewData["Suppliers"] = await _assetService.GetSuppliersAsync();
        }

        // Helper method to get status CSS class
        private static string GetStatusClass(AssetStatus status)
        {
            return status switch
            {
                AssetStatus.Available => "bg-success",
                AssetStatus.InUse => "bg-primary",
                AssetStatus.UnderMaintenance => "bg-warning",
                AssetStatus.MaintenancePending => "bg-warning",
                AssetStatus.InTransit => "bg-info",
                AssetStatus.Reserved => "bg-info",
                AssetStatus.Lost => "bg-danger",
                AssetStatus.Stolen => "bg-danger",
                AssetStatus.Decommissioned => "bg-dark",
                AssetStatus.PendingApproval => "bg-secondary",
                _ => "bg-secondary"
            };
        }

        // Helper method to get warranty status
        private static string GetWarrantyStatus(DateTime? warrantyExpiry)
        {
            if (!warrantyExpiry.HasValue)
                return "No Warranty";
            
            var daysUntilExpiry = (warrantyExpiry.Value - DateTime.Now).Days;
            
            if (daysUntilExpiry < 0)
                return "Expired";
            else if (daysUntilExpiry <= 30)
                return "Expiring Soon";
            else if (daysUntilExpiry <= 90)
                return "Expiring";
            else
                return "Valid";
        }

        // GET: Assets/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var asset = await _assetService.GetAssetByIdAsync(id.Value);
            if (asset == null) return NotFound();

            // Get movement history
            var movements = await _assetService.GetAssetMovementHistoryAsync(id.Value);
            ViewBag.Movements = movements;

            // Populate dropdowns for move modal
            await PopulateDropdowns();

            return View(asset);
        }

        // GET: Assets/Create
        [Authorize(Roles = "Admin,IT Support,Asset Manager")]
        public async Task<IActionResult> Create()
        {
            // Console.WriteLine("CREATE GET: Starting Create asset page"); // Removed
            
            await PopulateDropdowns();
            
            // Console.WriteLine($"CREATE GET: ViewBag.Locations count: {((SelectList)ViewBag.Locations)?.Count() ?? 0}"); // Removed
            // Console.WriteLine($"CREATE GET: ViewBag.Users count: {((SelectList)ViewBag.Users)?.Count() ?? 0}"); // Removed
            
            var asset = new Asset
            {
                InstallationDate = DateTime.SpecifyKind(DateTime.UtcNow.Date, DateTimeKind.Utc),
                Status = AssetStatus.Available
            };

            // Console.WriteLine("CREATE GET: Returning view"); // Removed
            return View(asset);
        }

        // POST: Assets/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,IT Support,Asset Manager")]
        public async Task<IActionResult> Create([Bind("AssetTag,Category,Brand,Model,SerialNumber,Description,AcquisitionDate,InstallationDate,Status,LocationId,AssignedToUserId,ResponsiblePerson,Department,WarrantyExpiry,Supplier,PurchasePrice,Notes")] Asset asset) // Added AcquisitionDate
        {
            // Console.WriteLine("CREATE POST: Form submitted"); // Removed
            // Console.WriteLine($"CREATE POST: AssetTag={asset.AssetTag}, Brand={asset.Brand}, Model={asset.Model}"); // Removed
            
            try
            {
                // Remove InternalSerialNumber from ModelState since it's auto-generated
                ModelState.Remove("InternalSerialNumber");
                
                // Console.WriteLine($"CREATE POST: ModelState.IsValid={ModelState.IsValid}"); // Removed
                
                // Check if asset tag is unique before other validations
                if (!string.IsNullOrWhiteSpace(asset.AssetTag) && !await _assetService.IsAssetTagUniqueAsync(asset.AssetTag))
                {
                    // Console.WriteLine("CREATE POST: Asset tag not unique"); // Removed
                    ModelState.AddModelError("AssetTag", "Asset tag must be unique.");
                }

                if (ModelState.IsValid)
                {
                    // Console.WriteLine("CREATE POST: ModelState is valid, creating asset"); // Removed
                    var userId = _userManager.GetUserId(User);
                    await _assetService.CreateAssetAsync(asset, userId!);
                    // Console.WriteLine("CREATE POST: Asset created successfully"); // Removed
                    TempData["SuccessMessage"] = "Asset created successfully.";
                    return RedirectToAction(nameof(Index));
                }
                // else
                // {
                //     Console.WriteLine("CREATE POST: ModelState is invalid"); // Removed
                //     foreach (var error in ModelState)
                //     {
                //         Console.WriteLine($"CREATE POST: Error in {error.Key}: {string.Join(", ", error.Value.Errors.Select(e => e.ErrorMessage))}"); // Removed
                //     }
                // }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in Create (POST) action for asset: {AssetTag}", asset.AssetTag);
                ModelState.AddModelError("", "An error occurred while creating the asset: " + ex.Message);
            }

            // Console.WriteLine("CREATE POST: Returning view with errors"); // Removed
            await PopulateDropdowns();
            return View(asset);
        }

        // GET: Assets/Edit/5
        [Authorize(Roles = "Admin,IT Support,Asset Manager")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) 
            {
                return NotFound();
            }

            var asset = await _assetService.GetAssetByIdAsync(id.Value);
            if (asset == null) 
            {
                return NotFound();
            }

            await PopulateDropdowns();
            return View(asset);
        }

        // POST: Assets/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,IT Support,Asset Manager")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,AssetTag,Category,Brand,Model,SerialNumber,InternalSerialNumber,DocumentPaths,ImagePaths,Description,AcquisitionDate,InstallationDate,Status,LocationId,AssignedToUserId,ResponsiblePerson,Department,WarrantyExpiry,Supplier,PurchasePrice,Notes,CreatedDate,LastUpdated")] Asset asset) // Added AcquisitionDate
        {
            if (id != asset.Id) 
            {
                return NotFound();
            }

            // Trim string properties to avoid validation errors due to whitespace
            asset.AssetTag = asset.AssetTag?.Trim() ?? string.Empty;
            asset.Brand = asset.Brand?.Trim() ?? string.Empty;
            asset.Model = asset.Model?.Trim() ?? string.Empty;
            asset.SerialNumber = asset.SerialNumber?.Trim() ?? string.Empty;
            asset.ResponsiblePerson = asset.ResponsiblePerson?.Trim(); // Nullable
            asset.Department = asset.Department?.Trim(); // Nullable
            asset.Supplier = asset.Supplier?.Trim(); // Nullable
            asset.Description = asset.Description?.Trim() ?? string.Empty;
            asset.Notes = asset.Notes?.Trim(); // Nullable
            asset.InternalSerialNumber = asset.InternalSerialNumber?.Trim() ?? string.Empty;
            asset.DocumentPaths = asset.DocumentPaths?.Trim();
            asset.ImagePaths = asset.ImagePaths?.Trim();

            // Remove CreatedDate and LastUpdated from ModelState as they are system-managed
            ModelState.Remove("CreatedDate");
            ModelState.Remove("LastUpdated");

            if (ModelState.IsValid)
            {
                // Check if asset tag is unique (excluding current asset)
                if (!string.IsNullOrWhiteSpace(asset.AssetTag) && !await _assetService.IsAssetTagUniqueAsync(asset.AssetTag, asset.Id))
                {
                    ModelState.AddModelError("AssetTag", "Asset tag must be unique.");
                }
                else
                {
                    try
                    {
                        var userId = _userManager.GetUserId(User);
                        if (string.IsNullOrEmpty(userId))
                        {
                            ModelState.AddModelError("", "Unable to identify current user.");
                        }
                        else
                        {
                            await _assetService.UpdateAssetAsync(asset, userId);
                            TempData["SuccessMessage"] = "Asset updated successfully.";
                            return RedirectToAction(nameof(Details), new { id = asset.Id });
                        }
                    }
                    catch (DbUpdateConcurrencyException ex) // Specific exception
                    {
                        _logger.LogWarning(ex, "Concurrency error while updating asset {AssetId}.", asset.Id);
                        ModelState.AddModelError("", "This asset was modified by another user. Please reload and try again.");
                    }
                    catch (DbUpdateException dbEx) // Specific exception
                    {
                        _logger.LogError(dbEx, "Database update error while updating asset {AssetId}.", asset.Id);
                        var errorMessage = "An error occurred while saving changes to the database. Please check the data and try again. ";
                        Exception? currentEx = dbEx;
                        int exCount = 0;
                        while (currentEx != null && exCount < 3) // Limit depth to avoid overly long messages
                        {
                            errorMessage += $"Details: {currentEx.Message} ";
                            currentEx = currentEx.InnerException;
                            exCount++;
                        }
                        // Consider logging the full dbEx object here for detailed diagnostics server-side
                        ModelState.AddModelError("", errorMessage);
                    }
                    catch (Exception ex) // General exception
                    {
                        _logger.LogError(ex, "Unexpected error while updating asset {AssetId}.", asset.Id);
                        ModelState.AddModelError("", $"An unexpected error occurred while updating the asset: {ex.Message}");
                    }
                }
            }

            await PopulateDropdowns();
            return View(asset);
        }

        // GET: Assets/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var asset = await _assetService.GetAssetByIdAsync(id.Value);
            if (asset == null) return NotFound();

            return View(asset);
        }

        // POST: Assets/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = _userManager.GetUserId(User);
            var success = await _assetService.DeleteAssetAsync(id, userId!);
            
            if (success)
            {
                TempData["SuccessMessage"] = "Asset deleted successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Asset not found or could not be deleted.";
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Assets/Maintenance
        [Authorize(Roles = "Admin,IT Support,Asset Manager")]
        public async Task<IActionResult> Maintenance()
        {
            var assets = await _assetService.GetAssetsForMaintenanceAsync();

            var viewModel = assets.Select(asset =>
            {
                var lastMaintenance = asset.MaintenanceRecords?.Where(m => m.CompletedDate.HasValue).OrderByDescending(m => m.CompletedDate).FirstOrDefault();
                var maintenanceInProgress = asset.MaintenanceRecords?.Any(m => !m.CompletedDate.HasValue) ?? false;
                var neverMaintained = lastMaintenance == null && !maintenanceInProgress;
                int daysOverdue = 0;
                bool isOverdue = false;

                if (lastMaintenance != null && lastMaintenance.CompletedDate.HasValue)
                {
                    daysOverdue = (int)(DateTime.UtcNow - lastMaintenance.CompletedDate.Value).TotalDays - 90;
                    isOverdue = daysOverdue >= 0;
                }
                else if (neverMaintained)
                {
                    var referenceDate = asset.InstallationDate;
                    if ((DateTime.UtcNow - referenceDate).TotalDays > 90)
                    {
                        daysOverdue = (int)(DateTime.UtcNow - referenceDate).TotalDays - 90;
                        isOverdue = true;
                    }
                }

                return new MaintenanceAssetViewModel
                {
                    Asset = asset,
                    LastMaintenanceDate = lastMaintenance?.CompletedDate,
                    DaysOverdue = daysOverdue,
                    IsOverdue = isOverdue,
                    NeverMaintained = neverMaintained,
                    MaintenanceInProgress = maintenanceInProgress
                };
            })
            .OrderByDescending(vm => vm.IsOverdue)
            .ThenByDescending(vm => vm.DaysOverdue)
            .ToList();

            return View(viewModel);
        }

        /* // Removed ExpiredWarranty action
        // GET: Assets/ExpiredWarranty
        [Authorize(Roles = "Admin,IT Support,Asset Manager")]
        public async Task<IActionResult> ExpiredWarranty()
        {
            try
            {
                // var assets = await _assetService.GetExpiredWarrantyAssetsAsync(); // Original call
                // For now, return an empty list as the service method is removed.
                // Consider redirecting or showing a message if this page is directly accessed.
                var assets = new List<Asset>(); 
                return View(assets);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving expired warranty assets.");
                TempData["ErrorMessage"] = $"An error occurred while retrieving expired warranty assets: {ex.Message}"; // Using ex.Message
                return View(new List<Asset>()); // Return an empty list or an error view
            }
        }
        */

        // GET: Assets/Movements/{id}
        // POST: Assets/Movements/{id}
        // GET: Assets/GenerateQrCode/5
        [HttpGet]
        public async Task<IActionResult> QRCode(int id)
        {
            try
            {
                var qrCodeBytes = await _assetService.GetAssetQRCodeAsync(id);
                if (qrCodeBytes.Length == 0)
                {
                    // Generate QR code if it doesn't exist
                    await _assetService.GenerateAssetQRCodeAsync(id);
                    qrCodeBytes = await _assetService.GetAssetQRCodeAsync(id);
                }
                
                return File(qrCodeBytes, "image/png");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating QR Code for asset ID {AssetId}", id);
                return NotFound();
            }
        }

        // Asset History
        [HttpGet]
        public async Task<IActionResult> History(int id)
        {
            var asset = await _assetService.GetAssetByIdAsync(id);
            if (asset == null) return NotFound();

            var movements = await _assetService.GetAssetMovementHistoryAsync(id);
            ViewBag.Asset = asset;
            return View(movements);
        }

        /*
        // Bulk Actions
        [HttpGet]
        [Authorize(Roles = "Admin,IT Support,Asset Manager")]
        public async Task<IActionResult> BulkActions()
        {
            await PopulateDropdowns();
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin,IT Support,Asset Manager")]
        public async Task<IActionResult> BulkUpdateStatus(List<int> assetIds, AssetStatus newStatus, string reason)
        {
            if (assetIds == null || !assetIds.Any())
            {
                TempData["ErrorMessage"] = "No assets selected for status update.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var userId = _userManager.GetUserId(User);
                if (string.IsNullOrEmpty(userId))
                {
                    TempData["ErrorMessage"] = "User not found. Please log in again.";
                    return RedirectToAction(nameof(Index)); // Or an error view
                }
                // var success = await _assetService.BulkUpdateStatusAsync(assetIds, newStatus, reason, userId); // Original call
                var success = false; // Placeholder after removing BulkUpdateStatusAsync
                // TODO: Implement individual status updates if bulk is removed or re-evaluate this action
                
                if (success)
                {
                    TempData["SuccessMessage"] = $"Successfully updated status for {assetIds.Count} assets.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to update asset statuses (Bulk functionality disabled).";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in BulkUpdateStatus for {AssetCount} assets.", assetIds?.Count ?? 0);
                TempData["ErrorMessage"] = $"Error updating asset statuses: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [Authorize(Roles = "Admin,IT Support,Asset Manager")]
        public async Task<IActionResult> BulkUpdateLocation(List<int> assetIds, int? newLocationId, string reason)
        {
            if (assetIds == null || !assetIds.Any())
            {
                TempData["ErrorMessage"] = "No assets selected for location update.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var userId = _userManager.GetUserId(User);
                if (string.IsNullOrEmpty(userId))
                {
                    TempData["ErrorMessage"] = "User not found. Please log in again.";
                    return RedirectToAction(nameof(Index)); // Or an error view
                }
                // var success = await _assetService.BulkUpdateLocationAsync(assetIds, newLocationId, reason, userId); // Original call
                var success = false; // Placeholder after removing BulkUpdateLocationAsync
                // TODO: Implement individual location updates if bulk is removed or re-evaluate this action

                if (success)
                {
                    TempData["SuccessMessage"] = $"Successfully updated location for {assetIds.Count} assets.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to update asset locations (Bulk functionality disabled).";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in BulkUpdateLocation for {AssetCount} assets.", assetIds?.Count ?? 0);
                TempData["ErrorMessage"] = $"Error updating asset locations: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [Authorize(Roles = "Admin,IT Support,Asset Manager")]
        public async Task<IActionResult> BulkAssignUser(List<int> assetIds, string userId, string notes)
        {
            try
            {
                var currentUserId = _userManager.GetUserId(User) ?? string.Empty;
                // var success = await _assetService.BulkAssignAsync(assetIds, userId, currentUserId); // Original call
                var success = false; // Placeholder after removing BulkAssignAsync
                // TODO: Implement individual assignments if bulk is removed or re-evaluate this action
                
                if (Request.Headers["Accept"].ToString().Contains("application/json"))
                {
                    return Json(new { success = success, message = success ? $"Successfully assigned {assetIds.Count} assets!" : "Failed to assign assets (Bulk functionality disabled)." });
                }
                
                if (success)
                {
                    TempData["SuccessMessage"] = $"Successfully assigned {assetIds.Count} assets!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to assign assets (Bulk functionality disabled).";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in BulkAssignUser for {AssetCount} assets.", assetIds?.Count ?? 0);
                if (Request.Headers["Accept"].ToString().Contains("application/json"))
                {
                    return Json(new { success = false, message = $"Error assigning assets: {ex.Message}" });
                }
                TempData["ErrorMessage"] = $"Error assigning assets: {ex.Message}";
            }
            
            // return RedirectToAction(nameof(BulkActions)); // Original redirect
            return RedirectToAction(nameof(Index)); // Redirect to Index as BulkActions might be removed
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> BulkDelete(List<int> assetIds, string reason, string confirmation)
        {
            if (assetIds == null || !assetIds.Any())
            {
                TempData["ErrorMessage"] = "No assets selected for deletion.";
                // return RedirectToAction(nameof(BulkActions)); // Original redirect
                return RedirectToAction(nameof(Index));
            }

            if (string.IsNullOrWhiteSpace(reason))
            {
                TempData["ErrorMessage"] = "Reason for deletion is required.";
                // return RedirectToAction(nameof(BulkActions)); // Original redirect
                return RedirectToAction(nameof(Index));
            }

            if (confirmation != "DELETE")
            {
                if (Request.Headers["Accept"].ToString().Contains("application/json"))
                {
                    return Json(new { success = false, message = "Confirmation text must be 'DELETE'" });
                }
                TempData["ErrorMessage"] = "Confirmation text must be 'DELETE'";
                // return RedirectToAction(nameof(BulkActions)); // Original redirect
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var currentUserId = _userManager.GetUserId(User) ?? string.Empty;
                int successCount = 0;
                foreach (var assetId in assetIds)
                {
                    // Assuming DeleteAssetAsync handles individual deletion logic and audit
                    var success = await _assetService.DeleteAssetAsync(assetId, currentUserId);
                    if (success) successCount++;
                }

                if (Request.Headers["Accept"].ToString().Contains("application/json"))
                {
                    return Json(new { success = successCount == assetIds.Count, message = $"Successfully deleted {successCount} of {assetIds.Count} assets." });
                }

                TempData["SuccessMessage"] = $"Successfully deleted {successCount} of {assetIds.Count} assets.";
            }
            catch (Exception ex)
            {
                 _logger.LogError(ex, "Error during bulk delete operation for {AssetCount} assets.", assetIds.Count);
                if (Request.Headers["Accept"].ToString().Contains("application/json"))
                {
                    return Json(new { success = false, message = $"An error occurred during bulk deletion: {ex.Message}" });
                }
                TempData["ErrorMessage"] = "An error occurred during bulk deletion.";
            }
            // return RedirectToAction(nameof(BulkActions)); // Original redirect
            return RedirectToAction(nameof(Index));
        }
        */

        // GET: Assets/Export
        [HttpGet]
        [Authorize(Roles = "Admin,IT Support,Asset Manager")]
        public async Task<IActionResult> Export()
        {
            try
            {
                var assets = await _assetService.GetAllAssetsAsync();
                // Generate CSV or other export format
                var csv = "AssetTag,Category,Brand,Model,Status,Location,AssignedTo,InstallationDate\n";
                var csvData = assets.Select(a => $"{a.AssetTag},{a.Category},{a.Brand},{a.Model},{a.Status},{a.Location?.FullLocation},{a.AssignedToUser?.FullName},{a.InstallationDate:yyyy-MM-dd}");
                csv += string.Join("\n", csvData);

                var bytes = System.Text.Encoding.UTF8.GetBytes(csv);
                var stream = new MemoryStream(bytes);

                return File(stream, "text/csv", "assets_export.csv");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting assets.");
                TempData["ErrorMessage"] = "Error exporting assets: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Assets/Import
        [HttpGet]
        [Authorize(Roles = "Admin,IT Support,Asset Manager")]
        public IActionResult Import()
        {
            return View();
        }

        // POST: Assets/Import
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,IT Support,Asset Manager")]
        public async Task<IActionResult> Import(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                TempData["ErrorMessage"] = "Please select a file to upload.";
                return RedirectToAction(nameof(Import));
            }

            try
            {
                // Process the uploaded file (CSV import logic)
                using (var stream = new StreamReader(file.OpenReadStream()))
                {
                    var header = true;
                    while (!stream.EndOfStream)
                    {
                        var line = await stream.ReadLineAsync();
                        if (string.IsNullOrEmpty(line)) // Check if line is null or empty
                        {
                            continue; // Skip empty lines or end of stream
                        }

                        if (header)
                        {
                            header = false;
                            continue; // Skip header line
                        }

                        var values = line.Split(',');
                        if (values.Length < 8) continue; // Invalid line, skip

                        var asset = new Asset
                        {
                            AssetTag = values[0],
                            Category = (AssetCategory)Enum.Parse(typeof(AssetCategory), values[1]),
                            Brand = values[2],
                            Model = values[3],
                            Status = (AssetStatus)Enum.Parse(typeof(AssetStatus), values[4]),
                            InstallationDate = DateTime.Parse(values[7]),
                            // Set other properties as needed, consider using a mapping library for complex mappings
                        };

                        // Create asset
                        var userId = _userManager.GetUserId(User);
                        await _assetService.CreateAssetAsync(asset, userId!);
                    }
                }

                TempData["SuccessMessage"] = "Assets imported successfully.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error importing assets from file {FileName}", file.FileName);
                TempData["ErrorMessage"] = "Error importing assets: " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Assets/Clone/5
        [Authorize(Roles = "Admin,IT Support,Asset Manager")]
        public async Task<IActionResult> Clone(int id)
        {
            try
            {
                var sourceAsset = await _assetService.GetAssetByIdAsync(id);
                if (sourceAsset == null)
                {
                    TempData["ErrorMessage"] = "Asset not found.";
                    return RedirectToAction(nameof(Index));
                }

                await PopulateDropdowns();

                // Create a new asset with cloned data
                var clonedAsset = new Asset
                {
                    Category = sourceAsset.Category,
                    Brand = sourceAsset.Brand,
                    Model = sourceAsset.Model,
                    Description = sourceAsset.Description,
                    Department = sourceAsset.Department,
                    Supplier = sourceAsset.Supplier,
                    PurchasePrice = sourceAsset.PurchasePrice,
                    Notes = sourceAsset.Notes,
                    InstallationDate = DateTime.SpecifyKind(DateTime.UtcNow.Date, DateTimeKind.Utc),
                    Status = AssetStatus.Available,
                    // Clear unique fields that need to be filled manually
                    AssetTag = "",
                    SerialNumber = "",
                    LocationId = null,
                    AssignedToUserId = null,
                    ResponsiblePerson = ""
                };

                ViewBag.SourceAssetTag = sourceAsset.AssetTag;
                ViewBag.SourceAssetInfo = $"{sourceAsset.Brand} {sourceAsset.Model}";
                
                TempData["InfoMessage"] = $"Cloning asset from '{sourceAsset.AssetTag}'. Please provide unique Asset Tag and Serial Number.";
                return View(clonedAsset);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cloning asset from source ID {SourceAssetId}", id);
                TempData["ErrorMessage"] = $"Error cloning asset: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Assets/Move/5
        [HttpGet]
        [Authorize(Roles = "Admin,IT Support,Asset Manager")]
        public async Task<IActionResult> Move(int id)
        {
            var asset = await _assetService.GetAssetByIdAsync(id);
            if (asset == null)
            {
                TempData["ErrorMessage"] = "Asset not found.";
                return RedirectToAction(nameof(Index));
            }

            await PopulateDropdowns();
            return View(asset);
        }

        // POST: Assets/Move
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,IT Support,Asset Manager")]
        public async Task<IActionResult> Move(int id, int? newLocationId, string? newUserId, string reason)
        {
            if (string.IsNullOrWhiteSpace(reason))
            {
                TempData["ErrorMessage"] = "Reason is required for moving an asset.";
                return RedirectToAction(nameof(Move), new { id });
            }

            var asset = await _assetService.GetAssetByIdAsync(id);
            if (asset == null)
            {
                TempData["ErrorMessage"] = "Asset not found.";
                return RedirectToAction(nameof(Index));
            }

            // Check if there's actually a change
            if (asset.LocationId == newLocationId && asset.AssignedToUserId == newUserId)
            {
                TempData["WarningMessage"] = "No changes detected. Asset location and assignment remain the same.";
                return RedirectToAction(nameof(Move), new { id });
            }

            try
            {
                var userId = _userManager.GetUserId(User);
                if (string.IsNullOrEmpty(userId))
                {
                    TempData["ErrorMessage"] = "User not found. Please log in again.";
                    return RedirectToAction(nameof(Move), new { id });
                }

                var success = await _assetService.MoveAssetAsync(id, newLocationId, newUserId, reason, userId);
                
                if (success)
                {
                    TempData["SuccessMessage"] = "Asset moved successfully.";
                    return RedirectToAction(nameof(Details), new { id });
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to move asset.";
                    return RedirectToAction(nameof(Move), new { id });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error moving asset ID {AssetId}", id);
                TempData["ErrorMessage"] = $"Error moving asset: {ex.Message}";
                return RedirectToAction(nameof(Move), new { id });
            }
        }

        // POST: Assets/ChangeStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,IT Support,Asset Manager")]
        public async Task<IActionResult> ChangeStatus(int id, AssetStatus newStatus, string reason)
        {
            if (string.IsNullOrWhiteSpace(reason))
            {
                TempData["ErrorMessage"] = "Reason is required for changing asset status.";
                return RedirectToAction(nameof(Details), new { id });
            }

            try
            {
                var userId = _userManager.GetUserId(User);
                if (string.IsNullOrEmpty(userId))
                {
                    TempData["ErrorMessage"] = "User not found. Please log in again.";
                    return RedirectToAction(nameof(Details), new { id });
                }

                var success = await _assetService.ChangeAssetStatusAsync(id, newStatus, reason, userId);
                
                if (success)
                {
                    TempData["SuccessMessage"] = "Asset status changed successfully.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to change asset status.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing status for asset ID {AssetId}", id);
                TempData["ErrorMessage"] = $"Error changing asset status: {ex.Message}";
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        #region Status Badge Helper Methods

        /// <summary>
        /// Returns the CSS class for asset status badges
        /// </summary>
        private static string GetAssetStatusBadgeClass(AssetStatus status)
        {
            return status switch
            {
                AssetStatus.Available => "badge-asset-status-available",
                AssetStatus.InUse => "badge-asset-status-inuse",
                AssetStatus.UnderMaintenance => "badge-asset-status-maintenance",
                AssetStatus.MaintenancePending => "badge-asset-status-maintenance-pending",
                AssetStatus.InTransit => "badge-asset-status-in-transit",
                AssetStatus.Reserved => "badge-asset-status-reserved",
                AssetStatus.Lost => "badge-asset-status-lost",
                AssetStatus.Stolen => "badge-asset-status-stolen",
                AssetStatus.Decommissioned => "badge-asset-status-decommissioned",
                AssetStatus.PendingApproval => "badge-asset-status-pending",
                _ => "badge-asset-status-unknown"
            };
        }

        /// <summary>
        /// Returns the display text for asset status
        /// </summary>
        private static string GetAssetStatusDisplayText(AssetStatus status)
        {
            return status switch
            {
                AssetStatus.Available => "Available",
                AssetStatus.InUse => "In Use",
                AssetStatus.UnderMaintenance => "Under Maintenance",
                AssetStatus.MaintenancePending => "Maintenance Pending",
                AssetStatus.InTransit => "In Transit",
                AssetStatus.Reserved => "Reserved",
                AssetStatus.Lost => "Lost",
                AssetStatus.Stolen => "Stolen",
                AssetStatus.Decommissioned => "Decommissioned",
                AssetStatus.PendingApproval => "Pending Approval",
                _ => $"Unknown ({status})"
            };
        }

        #endregion

        private async Task PopulateDropdowns()
        {
            // Populate categories dropdown
            ViewBag.Categories = new SelectList(
                Enum.GetValues<AssetCategory>().Select(c => new { Value = (int)c, Text = c.ToString() }), 
                "Value", "Text"
            );

            // Populate statuses dropdown
            ViewBag.Statuses = new SelectList(
                Enum.GetValues<AssetStatus>().Select(s => new { Value = (int)s, Text = s.ToString() }), 
                "Value", "Text"
            );

            // Populate locations dropdown
            try
            {
                var locations = await _assetService.GetActiveLocationsAsync();
                ViewBag.Locations = new SelectList(locations, "Id", "FullLocation");
            }
            catch
            {
                ViewBag.Locations = new SelectList(new List<Location>(), "Id", "FullLocation");
            }

            // Populate users dropdown
            try
            {
                var users = await _userManager.Users.Where(u => u.IsActive).ToListAsync();
                ViewBag.Users = new SelectList(users.Select(u => new { Id = u.Id, Name = u.FullName }), "Id", "Name");
            }
            catch
            {
                ViewBag.Users = new SelectList(new List<object>(), "Id", "Name");
            }
        }

        #region AJAX Asset Management Actions

        /// <summary>
        /// AJAX endpoint to assign an asset to a user
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin,IT Support,Asset Manager")]
        public async Task<JsonResult> AssignAsset(int assetId, string userId, string? notes)
        {
            try
            {
                var currentUserId = _userManager.GetUserId(User);
                if (string.IsNullOrEmpty(currentUserId))
                {
                    return Json(new { success = false, message = "User authentication required." });
                }

                var asset = await _assetService.GetAssetByIdAsync(assetId);
                if (asset == null)
                {
                    return Json(new { success = false, message = "Asset not found." });
                }

                var success = await _assetService.AssignAssetAsync(assetId, userId, currentUserId);
                if (success)
                {
                    return Json(new { 
                        success = true, 
                        message = "Asset assigned successfully.",
                        assetTag = asset.AssetTag
                    });
                }
                else
                {
                    return Json(new { success = false, message = "Failed to assign asset. Asset may already be assigned or user may be invalid." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning asset {AssetId} to user {UserId}", assetId, userId);
                return Json(new { success = false, message = "An error occurred while assigning the asset." });
            }
        }

        /// <summary>
        /// AJAX endpoint to unassign an asset
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin,IT Support,Asset Manager")]
        public async Task<JsonResult> UnassignAsset(int assetId, string? notes)
        {
            try
            {
                var currentUserId = _userManager.GetUserId(User);
                if (string.IsNullOrEmpty(currentUserId))
                {
                    return Json(new { success = false, message = "User authentication required." });
                }

                var asset = await _assetService.GetAssetByIdAsync(assetId);
                if (asset == null)
                {
                    return Json(new { success = false, message = "Asset not found." });
                }

                var success = await _assetService.UnassignAssetAsync(assetId, currentUserId);
                if (success)
                {
                    return Json(new { 
                        success = true, 
                        message = "Asset unassigned successfully.",
                        assetTag = asset.AssetTag
                    });
                }
                else
                {
                    return Json(new { success = false, message = "Failed to unassign asset." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unassigning asset {AssetId}", assetId);
                return Json(new { success = false, message = "An error occurred while unassigning the asset." });
            }
        }

        /// <summary>
        /// AJAX endpoint to change asset status
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin,IT Support,Asset Manager")]
        public async Task<JsonResult> ChangeAssetStatus(int assetId, AssetStatus newStatus, string reason)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(reason))
                {
                    return Json(new { success = false, message = "Reason is required for status change." });
                }

                var currentUserId = _userManager.GetUserId(User);
                if (string.IsNullOrEmpty(currentUserId))
                {
                    return Json(new { success = false, message = "User authentication required." });
                }

                var asset = await _assetService.GetAssetByIdAsync(assetId);
                if (asset == null)
                {
                    return Json(new { success = false, message = "Asset not found." });
                }

                var success = await _assetService.ChangeAssetStatusAsync(assetId, newStatus, reason, currentUserId);
                if (success)
                {
                    return Json(new { 
                        success = true, 
                        message = $"Asset status changed to {GetAssetStatusDisplayText(newStatus)} successfully.",
                        assetTag = asset.AssetTag,
                        newStatus = GetAssetStatusDisplayText(newStatus),
                        statusBadgeClass = GetAssetStatusBadgeClass(newStatus)
                    });
                }
                else
                {
                    return Json(new { success = false, message = "Failed to change asset status." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing status for asset {AssetId} to {NewStatus}", assetId, newStatus);
                return Json(new { success = false, message = "An error occurred while changing the asset status." });
            }
        }

        /// <summary>
        /// AJAX endpoint to get asset quick info for modals
        /// </summary>
        [HttpGet]
        public async Task<JsonResult> GetAssetQuickInfo(int assetId)
        {
            try
            {
                var asset = await _assetService.GetAssetByIdAsync(assetId);
                if (asset == null)
                {
                    return Json(new { success = false, message = "Asset not found." });
                }

                return Json(new { 
                    success = true,
                    asset = new {
                        id = asset.Id,
                        assetTag = asset.AssetTag,
                        brand = asset.Brand,
                        model = asset.Model,
                        status = asset.Status.ToString(),
                        statusDisplay = GetAssetStatusDisplayText(asset.Status),
                        statusBadgeClass = GetAssetStatusBadgeClass(asset.Status),
                        location = asset.Location?.FullLocation ?? "Unassigned",
                        assignedTo = asset.AssignedToUser?.FullName ?? "Unassigned",
                        department = asset.Department ?? "N/A"
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting quick info for asset {AssetId}", assetId);
                return Json(new { success = false, message = "An error occurred while retrieving asset information." });
            }
        }

        /// <summary>
        /// AJAX endpoint to generate the next available asset tag for a given prefix
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> GetNextAssetTag([FromBody] AssetTagRequest request)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Invalid request" });
            }

            try
            {
                var prefix = request.Prefix?.ToUpper() ?? "ASSET";
                
                // Get the latest asset tag with this prefix
                var latestAsset = await _assetService.GetLatestAssetByPrefixAsync(prefix);
                
                int nextNumber = 1000000; // Starting number
                
                if (latestAsset != null)
                {
                    var tagParts = latestAsset.AssetTag.Split('-');
                    if (tagParts.Length == 2 && int.TryParse(tagParts[1], out int currentNumber))
                    {
                        nextNumber = currentNumber + 1;
                    }
                }
                
                // Ensure uniqueness
                string candidateTag;
                int attempts = 0;
                do
                {
                    candidateTag = $"{prefix}-{nextNumber:D7}";
                    var exists = await _assetService.AssetTagExistsAsync(candidateTag);
                    if (!exists) break;
                    
                    nextNumber++;
                    attempts++;
                } while (attempts < 100); // Prevent infinite loops
                
                return Json(new { success = true, assetTag = candidateTag });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating asset tag with prefix {Prefix}", request.Prefix);
                return Json(new { success = false, message = "Error generating asset tag" });
            }
        }

        /// <summary>
        /// AJAX endpoint to check the uniqueness of an asset tag
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CheckAssetTagUniqueness([FromBody] AssetTagCheckRequest request)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { isUnique = false, message = "Invalid request" });
            }

            try
            {
                var assetTag = request.AssetTag?.Trim();
                if (string.IsNullOrEmpty(assetTag))
                {
                    return Json(new { isUnique = false, message = "Asset tag is required" });
                }
                
                var exists = await _assetService.AssetTagExistsAsync(assetTag);
                return Json(new { isUnique = !exists });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking asset tag uniqueness for {AssetTag}", request.AssetTag);
                return Json(new { isUnique = false, message = "Error checking uniqueness" });
            }
        }

        /// <summary>
        /// AJAX endpoint to add a new location
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> AddLocation([FromBody] LocationCreateRequest request)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Invalid request" });
            }

            try
            {
                if (string.IsNullOrWhiteSpace(request.Name))
                {
                    return Json(new { success = false, message = "Location name is required" });
                }
                
                var location = await _assetService.CreateLocationAsync(request.Name.Trim(), 
                    request.Description?.Trim(), request.Building?.Trim(), 
                    request.Floor?.Trim(), request.Room?.Trim());
                
                return Json(new 
                { 
                    success = true, 
                    message = "Location added successfully",
                    location = new { value = location.Id.ToString(), text = location.FullLocation }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding location");
                return Json(new { success = false, message = "Error adding location" });
            }
        }

        #endregion
    }
}
