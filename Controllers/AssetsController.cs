using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using HospitalAssetTracker.Models;
using HospitalAssetTracker.Services;

namespace HospitalAssetTracker.Controllers
{
    [Authorize]
    public class AssetsController : Controller
    {
        private readonly IAssetService _assetService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _environment;

        public AssetsController(IAssetService assetService, UserManager<ApplicationUser> userManager, IWebHostEnvironment environment)
        {
            _assetService = assetService;
            _userManager = userManager;
            _environment = environment;
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
                Console.WriteLine($"Error in Assets Index: {ex.Message}");
                TempData["ErrorMessage"] = "An error occurred while loading assets.";
                
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
            Console.WriteLine("CREATE GET: Starting Create asset page");
            
            await PopulateDropdowns();
            
            Console.WriteLine($"CREATE GET: ViewBag.Locations count: {((SelectList)ViewBag.Locations)?.Count() ?? 0}");
            Console.WriteLine($"CREATE GET: ViewBag.Users count: {((SelectList)ViewBag.Users)?.Count() ?? 0}");
            
            var asset = new Asset
            {
                InstallationDate = DateTime.SpecifyKind(DateTime.Today, DateTimeKind.Utc),
                Status = AssetStatus.Available
            };

            Console.WriteLine("CREATE GET: Returning view");
            return View(asset);
        }

        // POST: Assets/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,IT Support,Asset Manager")]
        public async Task<IActionResult> Create([Bind("AssetTag,Category,Brand,Model,SerialNumber,Description,InstallationDate,Status,LocationId,AssignedToUserId,ResponsiblePerson,Department,WarrantyExpiry,Supplier,PurchasePrice,Notes")] Asset asset)
        {
            Console.WriteLine("CREATE POST: Form submitted");
            Console.WriteLine($"CREATE POST: AssetTag={asset.AssetTag}, Brand={asset.Brand}, Model={asset.Model}");
            
            try
            {
                // Remove InternalSerialNumber from ModelState since it's auto-generated
                ModelState.Remove("InternalSerialNumber");
                
                Console.WriteLine($"CREATE POST: ModelState.IsValid={ModelState.IsValid}");
                
                // Check if asset tag is unique before other validations
                if (!string.IsNullOrWhiteSpace(asset.AssetTag) && !await _assetService.IsAssetTagUniqueAsync(asset.AssetTag))
                {
                    Console.WriteLine("CREATE POST: Asset tag not unique");
                    ModelState.AddModelError("AssetTag", "Asset tag must be unique.");
                }

                if (ModelState.IsValid)
                {
                    Console.WriteLine("CREATE POST: ModelState is valid, creating asset");
                    var userId = _userManager.GetUserId(User);
                    await _assetService.CreateAssetAsync(asset, userId!);
                    Console.WriteLine("CREATE POST: Asset created successfully");
                    TempData["SuccessMessage"] = "Asset created successfully.";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    Console.WriteLine("CREATE POST: ModelState is invalid");
                    foreach (var error in ModelState)
                    {
                        Console.WriteLine($"CREATE POST: Error in {error.Key}: {string.Join(", ", error.Value.Errors.Select(e => e.ErrorMessage))}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"CREATE POST: Exception: {ex.Message}");
                ModelState.AddModelError("", "An error occurred while creating the asset: " + ex.Message);
            }

            Console.WriteLine("CREATE POST: Returning view with errors");
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
        public async Task<IActionResult> Edit(int id, [Bind("Id,AssetTag,Category,Brand,Model,SerialNumber,InternalSerialNumber,DocumentPaths,ImagePaths,Description,InstallationDate,Status,LocationId,AssignedToUserId,ResponsiblePerson,Department,WarrantyExpiry,Supplier,PurchasePrice,Notes,CreatedDate,LastUpdated")] Asset asset)
        {
            if (id != asset.Id) 
            {
                return NotFound();
            }

            // Trim string properties to avoid validation errors due to whitespace
            asset.AssetTag = asset.AssetTag?.Trim() ?? string.Empty;
            asset.Brand = asset.Brand?.Trim() ?? string.Empty;
            asset.Model = asset.Model?.Trim() ?? string.Empty;
            asset.SerialNumber = asset.SerialNumber?.Trim(); // Nullable, no change needed
            asset.ResponsiblePerson = asset.ResponsiblePerson?.Trim(); // Nullable, no change needed
            asset.Department = asset.Department?.Trim(); // Nullable, no change needed
            asset.Supplier = asset.Supplier?.Trim(); // Nullable, no change needed
            asset.Description = asset.Description?.Trim(); // Nullable, no change needed
            asset.Notes = asset.Notes?.Trim(); // Nullable, no change needed


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
                    catch (DbUpdateConcurrencyException)
                    {
                        // Asset may have been deleted by another user
                        ModelState.AddModelError("", "This asset was modified by another user. Please reload and try again.");
                    }
                    catch (DbUpdateException dbEx) // Catch DbUpdateException specifically
                    {
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
                    catch (Exception ex)
                    {
                        // Consider logging the full ex object here
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
            return View(assets);
        }

        // GET: Assets/ExpiredWarranty
        [Authorize(Roles = "Admin,IT Support,Asset Manager")]
        public async Task<IActionResult> ExpiredWarranty()
        {
            try
            {
                var assets = await _assetService.GetExpiredWarrantyAssetsAsync();
                return View(assets);
            }
            catch (Exception ex)
            {
                // Log the exception (implementation depends on your logging framework)
                Console.WriteLine($"Error retrieving expired warranty assets: {ex.Message}");
                // Optionally, add a model error or return an error view
                TempData["ErrorMessage"] = "An error occurred while retrieving expired warranty assets.";
                return View(new List<Asset>()); // Return an empty list or an error view
            }
        }

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
            catch (Exception)
            {
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
                var success = await _assetService.BulkUpdateStatusAsync(assetIds, newStatus, reason, userId);
                
                if (success)
                {
                    TempData["SuccessMessage"] = $"Successfully updated status for {assetIds.Count} assets.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to update asset statuses.";
                }
            }
            catch (Exception ex)
            {
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
                var success = await _assetService.BulkUpdateLocationAsync(assetIds, newLocationId, reason, userId);
                
                if (success)
                {
                    TempData["SuccessMessage"] = $"Successfully updated location for {assetIds.Count} assets.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to update asset locations.";
                }
            }
            catch (Exception ex)
            {
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
                var success = await _assetService.BulkAssignAsync(assetIds, userId, currentUserId);
                
                if (Request.Headers["Accept"].ToString().Contains("application/json"))
                {
                    return Json(new { success = success, message = success ? $"Successfully assigned {assetIds.Count} assets!" : "Failed to assign assets." });
                }
                
                if (success)
                {
                    TempData["SuccessMessage"] = $"Successfully assigned {assetIds.Count} assets!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to assign assets.";
                }
            }
            catch (Exception ex)
            {
                if (Request.Headers["Accept"].ToString().Contains("application/json"))
                {
                    return Json(new { success = false, message = $"Error assigning assets: {ex.Message}" });
                }
                TempData["ErrorMessage"] = $"Error assigning assets: {ex.Message}";
            }
            
            return RedirectToAction(nameof(BulkActions));
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> BulkDelete(List<int> assetIds, string reason, string confirmation)
        {
            if (confirmation != "DELETE")
            {
                if (Request.Headers["Accept"].ToString().Contains("application/json"))
                {
                    return Json(new { success = false, message = "Confirmation text must be 'DELETE'" });
                }
                TempData["ErrorMessage"] = "Confirmation text must be 'DELETE'";
                return RedirectToAction(nameof(BulkActions));
            }

            try
            {
                var userId = _userManager.GetUserId(User) ?? string.Empty;
                var successCount = 0;
                
                foreach (var assetId in assetIds)
                {
                    var success = await _assetService.DeleteAssetAsync(assetId, userId);
                    if (success) successCount++;
                }
                
                var allSuccess = successCount == assetIds.Count;
                
                if (Request.Headers["Accept"].ToString().Contains("application/json"))
                {
                    return Json(new { success = allSuccess, message = allSuccess ? $"Successfully deleted {successCount} assets!" : $"Deleted {successCount} of {assetIds.Count} assets." });
                }
                
                if (allSuccess)
                {
                    TempData["SuccessMessage"] = $"Successfully deleted {successCount} assets!";
                }
                else
                {
                    TempData["ErrorMessage"] = $"Deleted {successCount} of {assetIds.Count} assets. Some assets could not be deleted.";
                }
            }
            catch (Exception ex)
            {
                if (Request.Headers["Accept"].ToString().Contains("application/json"))
                {
                    return Json(new { success = false, message = $"Error deleting assets: {ex.Message}" });
                }
                TempData["ErrorMessage"] = $"Error deleting assets: {ex.Message}";
            }
            
            return RedirectToAction(nameof(BulkActions));
        }

        // File Upload Actions
        [HttpPost]
        [Authorize(Roles = "Admin,IT Support,Asset Manager")]
        public async Task<IActionResult> UploadDocument(int assetId, IFormFile document)
        {
            if (document == null || document.Length == 0)
            {
                TempData["ErrorMessage"] = "Please select a file to upload.";
                return RedirectToAction(nameof(Details), new { id = assetId });
            }

            try
            {
                var uploadsDir = Path.Combine(_environment.WebRootPath, "uploads", "documents");
                if (!Directory.Exists(uploadsDir))
                {
                    Directory.CreateDirectory(uploadsDir);
                }

                var fileName = $"{assetId}_{Guid.NewGuid()}_{document.FileName}";
                var filePath = Path.Combine(uploadsDir, fileName);
                
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await document.CopyToAsync(stream);
                }

                var userId = _userManager.GetUserId(User) ?? string.Empty;
                var relativePath = $"/uploads/documents/{fileName}";
                await _assetService.AttachDocumentAsync(assetId, relativePath, userId);

                TempData["SuccessMessage"] = "Document uploaded successfully!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error uploading document: {ex.Message}";
            }

            return RedirectToAction(nameof(Details), new { id = assetId });
        }

        [HttpPost]
        [Authorize(Roles = "Admin,IT Support,Asset Manager")]
        public async Task<IActionResult> UploadImage(int assetId, IFormFile image)
        {
            if (image == null || image.Length == 0)
            {
                TempData["ErrorMessage"] = "Please select an image to upload.";
                return RedirectToAction(nameof(Details), new { id = assetId });
            }

            try
            {
                var uploadsDir = Path.Combine(_environment.WebRootPath, "uploads", "images");
                if (!Directory.Exists(uploadsDir))
                {
                    Directory.CreateDirectory(uploadsDir);
                }

                var fileName = $"{assetId}_{Guid.NewGuid()}_{image.FileName}";
                var filePath = Path.Combine(uploadsDir, fileName);
                
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await image.CopyToAsync(stream);
                }

                var userId = _userManager.GetUserId(User) ?? string.Empty;
                var relativePath = $"/uploads/images/{fileName}";
                await _assetService.AttachImageAsync(assetId, relativePath, userId);

                TempData["SuccessMessage"] = "Image uploaded successfully!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error uploading image: {ex.Message}";
            }

            return RedirectToAction(nameof(Details), new { id = assetId });
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
                    InstallationDate = DateTime.SpecifyKind(DateTime.Today, DateTimeKind.Utc),
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

                return View("Create", clonedAsset);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error cloning asset: {ex.Message}");
                TempData["ErrorMessage"] = "Error cloning asset: " + ex.Message;
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
                TempData["ErrorMessage"] = $"Error changing asset status: {ex.Message}";
            }

            return RedirectToAction(nameof(Details), new { id });
        }

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
        

        
        // AJAX: Add new location
        [HttpPost]
        [Authorize(Roles = "Admin,IT Support,Asset Manager")]
        public async Task<IActionResult> AddLocation([FromBody] AddLocationRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Name))
                {
                    return Json(new { success = false, message = "Location name is required." });
                }

                var userId = _userManager.GetUserId(User);
                if (userId == null)
                {
                    return Json(new { success = false, message = "User not authenticated." });
                }

                var location = new Location
                {
                    Room = request.Name.Trim(),
                    Description = request.Description?.Trim(),
                    Building = request.Building?.Trim() ?? "Main Building",
                    Floor = request.Floor?.Trim() ?? "Ground Floor",
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                };

                var createdLocation = await _assetService.CreateLocationAsync(location, userId);
                
                return Json(new { 
                    success = true, 
                    message = "Location added successfully.",
                    location = new { 
                        id = createdLocation.Id, 
                        name = createdLocation.FullLocation,
                        value = createdLocation.Id,
                        text = createdLocation.FullLocation
                    }
                });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "An error occurred while adding the location." });
            }
        }

        // Helper class for AJAX requests
        public class AddLocationRequest
        {
            public string Name { get; set; } = string.Empty;
            public string? Description { get; set; }
            public string? Building { get; set; }
            public string? Floor { get; set; }
            public string? Room { get; set; }
        }
    }
}
