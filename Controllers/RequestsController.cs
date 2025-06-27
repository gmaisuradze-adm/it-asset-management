using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HospitalAssetTracker.Data;
using HospitalAssetTracker.Models;
using HospitalAssetTracker.Services;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Linq;
using HospitalAssetTracker.Models.RequestViewModels;
using HospitalAssetTracker.Extensions;

namespace HospitalAssetTracker.Controllers
{
    [Authorize]
    public class RequestsController : Controller
    {
        // Role constants to avoid magic strings
        private const string AdminRole = "Admin";
        private const string ITSupportRole = "IT Support";
        private const string AssetManagerRole = "Asset Manager";
        private const string DepartmentHeadRole = "Department Head";

        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<RequestsController> _logger;
        private readonly IRequestService _requestService;
        private readonly ILocationService _locationService;
        private readonly IAssetService _assetService;

        public RequestsController(ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            ILogger<RequestsController> logger,
            IRequestService requestService,
            ILocationService locationService,
            IAssetService assetService)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
            _requestService = requestService;
            _locationService = locationService;
            _assetService = assetService;
        }

        private async Task PopulateCreateViewModelAsync(CreateRequestViewModel viewModel)
        {
            viewModel.RequestTypes = Enum.GetValues<RequestType>()
                .Select(e => new SelectListItem { Value = e.ToString(), Text = e.ToString().Replace("_", " ") });

            viewModel.Priorities = Enum.GetValues<RequestPriority>()
                .Select(e => new SelectListItem { Value = e.ToString(), Text = e.ToString() });

            var locations = await _locationService.GetAllLocationsAsync();
            viewModel.Locations = locations.Select(l => new SelectListItem
            {
                Value = l.Id.ToString(),
                Text = $"{l.Building} - {l.Floor} - {l.Room}"
            });

            var activeAssets = await _assetService.GetActiveAssetsAsync();
            viewModel.Assets = activeAssets.Select(a => new SelectListItem
            {
                Value = a.Id.ToString(),
                Text = $"{a.AssetTag} - {a.Name} ({a.Category})"
            }).OrderBy(a => a.Text);

            viewModel.ItemCategories = new List<SelectListItem>
            {
                new() { Value = "Desktop Computer", Text = "Desktop Computer" },
                new() { Value = "Laptop", Text = "Laptop" },
                new() { Value = "Monitor", Text = "Monitor" },
                new() { Value = "Printer", Text = "Printer" },
                new() { Value = "Network Equipment", Text = "Network Equipment" },
                new() { Value = "Server", Text = "Server" },
                new() { Value = "Mobile Device", Text = "Mobile Device" },
                new() { Value = "Peripheral", Text = "Peripheral" },
                new() { Value = "Software", Text = "Software" },
                new() { Value = "Other", Text = "Other" }
            };

            var assignableStaff = await _requestService.GetAssignableITStaffAsync();
            viewModel.AssignableUsers = assignableStaff.Select(u => new SelectListItem
            {
                Value = u.Id,
                Text = u.FullName
            }).OrderBy(u => u.Text);

            var availableInventory = await _requestService.GetRelevantInventoryItemsAsync(category: null);
            viewModel.InventoryItems = availableInventory.Select(i => new SelectListItem
            {
                Value = i.Id.ToString(),
                Text = $"{i.Name} ({i.ItemCode}) - Stock: {i.Quantity}"
            }).OrderBy(i => i.Text);

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser != null)
            {
                viewModel.RequestorName = currentUser.FullName;
                viewModel.RequestorDepartment = currentUser.Department;
                viewModel.RequestorEmail = currentUser.Email;
                viewModel.RequestorPhone = currentUser.PhoneNumber;
            }
        }

        // GET: Requests
        private void PopulateFilterViewBag(RequestSearchModel searchModel)
        {
            ViewBag.RequestTypes = Enum.GetValues<RequestType>()
                .Select(e => new SelectListItem
                {
                    Value = e.ToString(),
                    Text = e.ToString().Replace("_", " "),
                    Selected = e == searchModel.RequestType
                });

            ViewBag.Statuses = Enum.GetValues<RequestStatus>()
                .Select(e => new SelectListItem
                {
                    Value = e.ToString(),
                    Text = e.ToString(),
                    Selected = e == searchModel.Status
                });

            ViewBag.Priorities = Enum.GetValues<RequestPriority>()
                .Select(e => new SelectListItem
                {
                    Value = e.ToString(),
                    Text = e.ToString(),
                    Selected = e == searchModel.Priority
                });

            ViewBag.SearchTerm = searchModel.SearchTerm;
            ViewBag.Department = searchModel.Department;
            ViewBag.RequestType = searchModel.RequestType?.ToString();
            ViewBag.Status = searchModel.Status?.ToString();
            ViewBag.Priority = searchModel.Priority?.ToString();
        }

        public async Task<IActionResult> Index(RequestSearchModel searchModel)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Challenge(); // Or handle the case where user is not found
            }

            PopulateFilterViewBag(searchModel);

            var requests = await _requestService.GetRequestsAsync(searchModel, userId);
            return View(requests);
        }

        // GET: Requests/Dashboard
        [Authorize(Roles = "Admin,IT Support,Asset Manager")]
        public async Task<IActionResult> Dashboard()
        {
            var dashboardData = await _requestService.GetRequestDashboardDataAsync();
            return View(dashboardData);
        }

        // GET: Requests/MyRequests
        public async Task<IActionResult> MyRequests()
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }
            var requests = await _requestService.GetMyRequestsAsync(userId);
            return View(requests);
        }

        // GET: Requests/AssignedToMe
        [Authorize(Roles = "Admin,IT Support")]
        public async Task<IActionResult> AssignedToMe()
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }
            var requests = await _requestService.GetAssignedRequestsAsync(userId);
            return View(requests);
        }

        // GET: Requests/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return BadRequest();
            }

            var request = await _requestService.GetRequestByIdAsync(id.Value);
            if (request == null)
            {
                return NotFound();
            }

            // Check if user can view this request
            var userId = _userManager.GetUserId(User);
            var user = await _userManager.GetUserAsync(User);
            if (user == null || string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }
            var userRoles = await _userManager.GetRolesAsync(user);
            
            if (request.RequestedByUserId != userId && 
                request.AssignedToUserId != userId && 
                !userRoles.Any(r => r == "Admin" || r == "IT Support" || r == "Asset Manager"))
            {
                return Forbid();
            }

            // Set ViewBag properties for the view
            ViewBag.CanEdit = (request.Status == RequestStatus.Submitted || request.Status == RequestStatus.OnHold) && (request.RequestedByUserId == userId || userRoles.Any(r => r == "Admin" || r == "IT Support"));
            ViewBag.CanAssign = userRoles.Any(r => r == "Admin" || r == "IT Support" || r == "Asset Manager");
            ViewBag.CurrentUserId = userId;

            // Get IT users for assignment dropdown
            if (ViewBag.CanAssign)
            {
                var itUsers = await _userManager.GetUsersInRoleAsync("IT Support");
                var adminUsers = await _userManager.GetUsersInRoleAsync("Admin");
                var allITUsers = itUsers.Concat(adminUsers).DistinctBy(u => u.Id).ToList();
                
                ViewBag.ITUsers = allITUsers.Select(u => new SelectListItem
                {
                    Value = u.Id,
                    Text = $"{u.FullName} ({u.Email})"
                });
            }

            return View(request);
        }

        private async Task<JsonResult> GetRequestDetailsJson(int id, bool success, string message)
        {
            var request = await _requestService.GetRequestByIdAsync(id);
            if (request == null)
            {
                return Json(new { success = false, message = "Request not found." });
            }

            var partialViewHtml = await this.RenderViewToStringAsync("_RequestActionsPartial", request);

            return Json(new
            {
                success,
                message,
                requestId = request.Id,
                requestStatus = request.Status.ToString().Replace("_", " "),
                statusClass = GetStatusClass(request.Status),
                assignedTo = request.AssignedToUser != null ? $"{request.AssignedToUser.FullName} <br /><small class='text-muted'>{request.AssignedToUser.Email}</small>" : "<div class='text-muted'><i class='bi bi-person-x'></i> Not yet assigned.</div>",
                actionsHtml = partialViewHtml
            });
        }


        // GET: Requests/Create
        public async Task<IActionResult> Create()
        {
            var viewModel = new CreateRequestViewModel();
            await PopulateCreateViewModelAsync(viewModel);
            return View(viewModel);
        }

        // POST: Requests/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateRequestViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var userId = _userManager.GetUserId(User);
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("User ID not found during request creation.");
                    ModelState.AddModelError("", "User session expired. Please log in again.");
                    await PopulateCreateViewModelAsync(viewModel); // Repopulate dropdowns
                    return View(viewModel);
                }
                var user = await _userManager.FindByIdAsync(userId);

                if (viewModel.RequestType == null || viewModel.Priority == null)
                {
                    ModelState.AddModelError("", "Request Type and Priority are required.");
                    await PopulateCreateViewModelAsync(viewModel);
                    return View(viewModel);
                }

                var request = new ITRequest
                {
                    Title = viewModel.Title,
                    Description = viewModel.Description,
                    RequestType = viewModel.RequestType.Value,
                    Priority = viewModel.Priority.Value,
                    RequiredByDate = viewModel.RequiredByDate,
                    RelatedAssetId = viewModel.AssetId, // This was the original general AssetId
                    RequestedItemCategory = viewModel.RequestedItemCategory,
                    RequestedItemSpecifications = viewModel.RequestedItemSpecifications,
                    EstimatedCost = viewModel.EstimatedCost,
                    BusinessJustification = viewModel.BusinessJustification,
                    LocationId = viewModel.LocationId, // Assuming LocationId is in CreateRequestViewModel
                    
                    // New fields from ViewModel
                    AssignedToUserId = viewModel.AssignedToUserId, // Will be handled by service if set
                    DamagedAssetId = viewModel.DamagedAssetId,
                    DisposalNotesForUnmanagedAsset = viewModel.DisposalNotesForUnmanagedAsset,
                    RequiredInventoryItemId = viewModel.RequiredInventoryItemId,
                    
                    // Set by system
                    RequestedByUserId = userId,
                    Department = user?.Department ?? "N/A", // Ensure user is not null here
                    // RequestDate and Status will be set by the service (CreateRequestAsync)
                };

                try
                {
                    var createdRequest = await _requestService.CreateRequestAsync(request, userId);
                    _logger.LogInformation("User {UserId} created ITRequest {RequestId} with number {RequestNumber}", userId, createdRequest.Id, createdRequest.RequestNumber);
                    TempData["SuccessMessage"] = $"Request {createdRequest.RequestNumber} has been created successfully.";
                    return RedirectToAction(nameof(Details), new { id = createdRequest.Id });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating ITRequest by User {UserId}. Title: {Title}", userId, request.Title);
                    ModelState.AddModelError("", "An unexpected error occurred while creating the request. Please try again.");
                }
            }

            // If we got this far, something failed, redisplay form
            _logger.LogWarning("Create ITRequest failed due to invalid ModelState. User: {User}", User.Identity?.Name);
            TempData["ErrorMessage"] = "Please review the form and correct any errors.";
            await PopulateCreateViewModelAsync(viewModel);
            return View(viewModel);
        }

        private async Task PopulateEditViewModelAsync(EditRequestViewModel viewModel)
        {
            viewModel.RequestTypes = Enum.GetValues<RequestType>()
                .Select(e => new SelectListItem { Value = e.ToString(), Text = e.ToString().Replace("_", " ") });

            viewModel.Priorities = Enum.GetValues<RequestPriority>()
                .Select(e => new SelectListItem { Value = e.ToString(), Text = e.ToString() });

            var locations = await _locationService.GetAllLocationsAsync();
            viewModel.Locations = locations.Select(l => new SelectListItem
            {
                Value = l.Id.ToString(),
                Text = $"{l.Building} - {l.Floor} - {l.Room}"
            });

            var activeAssets = await _assetService.GetActiveAssetsAsync();
            viewModel.Assets = activeAssets.Select(a => new SelectListItem
            {
                Value = a.Id.ToString(),
                Text = $"{a.AssetTag} - {a.Name} ({a.Category})"
            }).OrderBy(a => a.Text);

            viewModel.ItemCategories = new List<SelectListItem>
            {
                new() { Value = "Desktop Computer", Text = "Desktop Computer" },
                new() { Value = "Laptop", Text = "Laptop" },
                new() { Value = "Monitor", Text = "Monitor" },
                new() { Value = "Printer", Text = "Printer" },
                new() { Value = "Network Equipment", Text = "Network Equipment" },
                new() { Value = "Server", Text = "Server" },
                new() { Value = "Mobile Device", Text = "Mobile Device" },
                new() { Value = "Peripheral", Text = "Peripheral" },
                new() { Value = "Software", Text = "Software" },
                new() { Value = "Other", Text = "Other" }
            };

            var assignableStaff = await _requestService.GetAssignableITStaffAsync();
            viewModel.AssignableUsers = assignableStaff.Select(u => new SelectListItem
            {
                Value = u.Id,
                Text = u.FullName
            }).OrderBy(u => u.Text);

            var availableInventory = await _requestService.GetRelevantInventoryItemsAsync(category: null);
            viewModel.InventoryItems = availableInventory.Select(i => new SelectListItem
            {
                Value = i.Id.ToString(),
                Text = $"{i.Name} ({i.ItemCode}) - Stock: {i.Quantity}"
            }).OrderBy(i => i.Text);
        }

        // GET: Requests/Edit/5
        [Authorize(Roles = "Admin,IT Support,Asset Manager")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return BadRequest();
            }
            var request = await _requestService.GetRequestByIdAsync(id.Value);
            if (request == null)
            {
                return NotFound();
            }

            var userId = _userManager.GetUserId(User);
            var user = await _userManager.GetUserAsync(User);
            if (user == null || string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }
            var userRoles = await _userManager.GetRolesAsync(user);

            // Check if the user is the requester or has a role that allows editing.
            // Users can only edit if they are the requester or an Admin/IT Support.
            if (request.RequestedByUserId != userId && !userRoles.Any(r => r == "Admin" || r == "IT Support"))
            {
                return Forbid();
            }

            if (request.Status == RequestStatus.Completed || request.Status == RequestStatus.Cancelled)
            {
                TempData["ErrorMessage"] = "Cannot edit completed or cancelled requests.";
                return RedirectToAction(nameof(Details), new { id });
            }

            var viewModel = new EditRequestViewModel
            {
                Id = request.Id,
                RequestNumber = request.RequestNumber,
                Title = request.Title,
                Description = request.Description,
                RequestType = request.RequestType,
                Priority = request.Priority,
                RequiredByDate = request.RequiredByDate ?? DateTime.UtcNow.AddDays(7),
                AssetId = request.RelatedAssetId, // Corrected from request.AssetId to request.RelatedAssetId
                RequestedItemCategory = request.RequestedItemCategory,
                RequestedItemSpecifications = request.RequestedItemSpecifications,
                EstimatedCost = request.EstimatedCost,
                BusinessJustification = request.BusinessJustification ?? string.Empty,
                LocationId = request.LocationId,
                AssignedToUserId = request.AssignedToUserId,
                DamagedAssetId = request.DamagedAssetId,
                RequiredInventoryItemId = request.RequiredInventoryItemId,
                DisposalNotesForUnmanagedAsset = request.DisposalNotesForUnmanagedAsset,
                Status = request.Status, // For display purposes
                Activities = request.Activities?.ToList() ?? new List<RequestActivity>(),
                RequestedByUserName = request.RequestedByUser?.FullName,
                RequestedByUserDepartment = request.Department,
                RequestDate = request.RequestDate,
                LastUpdatedDate = request.LastUpdatedDate
            };

            await PopulateEditViewModelAsync(viewModel);
            return View(viewModel);
        }

        // POST: Requests/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        // Removed [Authorize(Roles = "Admin,IT Support,Asset Manager")] to allow requesters to edit their own requests before assignment/completion
        public async Task<IActionResult> Edit(int id, EditRequestViewModel viewModel)
        {
            if (id != viewModel.Id)
            {
                return NotFound();
            }

            var requestToUpdate = await _requestService.GetRequestByIdAsync(id);
            if (requestToUpdate == null)
            {
                return NotFound();
            }

            var currentUserId = _userManager.GetUserId(User);
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null || string.IsNullOrEmpty(currentUserId))
            {
                return Challenge();
            }
            var userRoles = await _userManager.GetRolesAsync(currentUser);

            // Authorization: Ensure user can edit this request
            if (requestToUpdate.RequestedByUserId != currentUserId && !userRoles.Any(r => r == "Admin" || r == "IT Support"))
            {
                return Forbid();
            }

            if (requestToUpdate.Status == RequestStatus.Completed || requestToUpdate.Status == RequestStatus.Cancelled)
            {
                TempData["ErrorMessage"] = "Cannot edit completed or cancelled requests.";
                await PopulateEditViewModelAsync(viewModel); // Repopulate for display
                return View(viewModel);
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Add null checks for RequestType and Priority
                    if (viewModel.RequestType == null)
                    {
                        ModelState.AddModelError(nameof(viewModel.RequestType), "Request Type is required.");
                    }
                    if (viewModel.Priority == null)
                    {
                        ModelState.AddModelError(nameof(viewModel.Priority), "Priority is required.");
                    }

                    if (!ModelState.IsValid) // Check ModelState again after adding potential errors
                    {
                        _logger.LogWarning("Edit ITRequest {RequestId} failed due to missing RequestType or Priority. User: {User}", viewModel.Id, User.Identity?.Name);
                        TempData["ErrorMessage"] = "Request Type and Priority are required fields.";
                        await PopulateEditViewModelAsync(viewModel);
                        return View(viewModel);
                    }

                    // Map editable fields from ViewModel to the entity
                    requestToUpdate.Title = viewModel.Title;
                    requestToUpdate.Description = viewModel.Description;
                    requestToUpdate.RequestType = viewModel.RequestType!.Value; // Safe to use ! due to checks above
                    requestToUpdate.Priority = viewModel.Priority!.Value;   // Safe to use ! due to checks above
                    requestToUpdate.RequiredByDate = viewModel.RequiredByDate;
                    requestToUpdate.RelatedAssetId = viewModel.AssetId; // Corrected mapping
                    requestToUpdate.RequestedItemCategory = viewModel.RequestedItemCategory;
                    requestToUpdate.RequestedItemSpecifications = viewModel.RequestedItemSpecifications;
                    requestToUpdate.EstimatedCost = viewModel.EstimatedCost;
                    requestToUpdate.BusinessJustification = viewModel.BusinessJustification;
                    requestToUpdate.LocationId = viewModel.LocationId;
                    requestToUpdate.DamagedAssetId = viewModel.DamagedAssetId;
                    requestToUpdate.RequiredInventoryItemId = viewModel.RequiredInventoryItemId;
                    requestToUpdate.DisposalNotesForUnmanagedAsset = viewModel.DisposalNotesForUnmanagedAsset;

                    // Only allow Admin/IT Support to change assignment
                    if (userRoles.Contains("Admin") || userRoles.Contains("IT Support"))
                    {
                        requestToUpdate.AssignedToUserId = viewModel.AssignedToUserId;
                    }
                    // Status is NOT updated from this form directly. It's managed by specific actions.

                    await _requestService.UpdateRequestAsync(requestToUpdate, currentUserId);
                    _logger.LogInformation("User {UserId} updated ITRequest {RequestId}", currentUserId, viewModel.Id);
                    TempData["SuccessMessage"] = "Request has been updated successfully.";
                    return RedirectToAction(nameof(Details), new { id = viewModel.Id });
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    _logger.LogWarning(ex, "Concurrency conflict when updating ITRequest {RequestId}", viewModel.Id);
                    ModelState.AddModelError("", "The request was modified by another user. Please refresh and try again.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating ITRequest {RequestId} by User {UserId}", viewModel.Id, currentUserId);
                    ModelState.AddModelError("", $"Error updating request: {ex.Message}");
                }
            }
            else
            {
                _logger.LogWarning("Edit ITRequest {RequestId} failed due to invalid ModelState. User: {User}", viewModel.Id, User.Identity?.Name);
                 TempData["ErrorMessage"] = "Please review the form and correct any errors.";
            }

            // If we got this far, something failed or ModelState was invalid, redisplay form
            // Re-populate necessary fields for the view model that might have been lost or need refreshing
            var originalRequest = await _requestService.GetRequestByIdAsync(id); // Get fresh data
            if (originalRequest != null)
            {
                viewModel.Status = originalRequest.Status;
                viewModel.Activities = originalRequest.Activities.OrderByDescending(a => a.ActivityDate).ToList();
                viewModel.RequestNumber = originalRequest.RequestNumber;
                viewModel.RequestedByUserName = originalRequest.RequestedByUser?.FullName ?? "N/A";
                viewModel.RequestedByUserDepartment = originalRequest.Department;
                viewModel.RequestDate = originalRequest.RequestDate;
                viewModel.LastUpdatedDate = originalRequest.LastUpdatedDate;
            }
            await PopulateEditViewModelAsync(viewModel);
            return View(viewModel);
        }

        // POST: Requests/Cancel/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        // [Authorize(Roles = "Admin,IT Support,Asset Manager")] // Roles are checked in service layer
        public async Task<IActionResult> Cancel(int id, string comments) // Changed 'reason' to 'comments' to match JS
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId)) return Json(new { success = false, message = "User not found." });

            try
            {
                var success = await _requestService.CancelRequestAsync(id, userId, comments);
                if (success)
                {
                    _logger.LogInformation("Request {RequestId} cancelled by User {UserId}. Comments: {Comments}", id, userId, comments);
                    return await GetManagementSectionUpdate(id, true, "Request has been cancelled.");
                }
                else
                {
                    return await GetManagementSectionUpdate(id, false, "Failed to cancel the request. It may have already been completed, cancelled, or you lack permission.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling request {RequestId}", id);
                return Json(new { success = false, message = "An unexpected error occurred while cancelling the request." });
            }
        }

        // POST: Requests/PlaceOnHold/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        // [Authorize(Roles = "Admin,IT Support")] // Roles are checked in service layer
        public async Task<IActionResult> PlaceOnHold(int id, string comments) // Changed 'reason' to 'comments' to match JS
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId)) return Json(new { success = false, message = "User not found." });
            
            try
            {
                var success = await _requestService.PlaceRequestOnHoldAsync(id, userId, comments);
                if (success)
                {
                    _logger.LogInformation("Request {RequestId} placed on hold by User {UserId}. Comments: {Comments}", id, userId, comments);
                    return await GetManagementSectionUpdate(id, true, "Request has been placed on hold.");
                }
                else
                {
                    return await GetManagementSectionUpdate(id, false, "Failed to place request on hold. It must be 'In Progress' and assigned to you, or you lack permission.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error placing request {RequestId} on hold", id);
                return Json(new { success = false, message = "An unexpected error occurred while placing the request on hold." });
            }
        }

        // POST: Requests/Resume/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        // [Authorize(Roles = "Admin,IT Support")] // Roles are checked in service layer
        public async Task<IActionResult> Resume(int id, string comments) // Added comments parameter to match JS
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId)) return Json(new { success = false, message = "User not found." });

            try
            {
                var success = await _requestService.ResumeRequestAsync(id, userId, comments); // Pass comments to service
                if (success)
                {
                    _logger.LogInformation("Request {RequestId} resumed by User {UserId}. Comments: {Comments}", id, userId, comments);
                    return await GetManagementSectionUpdate(id, true, "Request has been resumed.");
                }
                else
                {
                    return await GetManagementSectionUpdate(id, false, "Failed to resume request. It must be 'On Hold' and assigned to you, or you lack permission.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resuming request {RequestId}", id);
                return Json(new { success = false, message = "An unexpected error occurred while resuming the request." });
            }
        }

        private async Task PopulateViewBags(ITRequest request)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                ViewBag.ITUsers = new SelectList(Enumerable.Empty<SelectListItem>());
                return;
            }

            var userRoles = await _userManager.GetRolesAsync(user);

            if (userRoles.Any(r => r == "Admin" || r == "IT Support" || r == "Asset Manager"))
            {
                var itUsers = await _userManager.GetUsersInRoleAsync("IT Support");
                var adminUsers = await _userManager.GetUsersInRoleAsync("Admin");
                var allITUsers = itUsers.Concat(adminUsers).DistinctBy(u => u.Id).ToList();

                ViewBag.ITUsers = new SelectList(allITUsers.Select(u => new 
                {
                    Value = u.Id,
                    Text = $"{u.FullName} ({u.Email})"
                }), "Value", "Text", request.AssignedToUserId);
            }
        }

        private async Task<JsonResult> GetManagementSectionUpdate(int requestId, bool success, string message)
        {
            try
            {
                var request = await _requestService.GetRequestByIdAsync(requestId);
                if (request == null)
                {
                    return Json(new { success = false, message = "Request not found after operation." });
                }

                var userId = _userManager.GetUserId(User);
                var user = await _userManager.GetUserAsync(User);
                var userRoles = user != null ? await _userManager.GetRolesAsync(user) : new List<string>();

                // Set ViewBag for partial view
                ViewBag.CanEdit = (request.Status == RequestStatus.Submitted || request.Status == RequestStatus.OnHold) && 
                                 (request.RequestedByUserId == userId || userRoles.Any(r => r == "Admin" || r == "IT Support"));
                ViewBag.CanAssign = userRoles.Any(r => r == "Admin" || r == "IT Support" || r == "Asset Manager");
                ViewBag.CurrentUserId = userId;

                var partialViewHtml = await this.RenderViewToStringAsync("_RequestManagementPartial", request);

                return Json(new
                {
                    success,
                    message,
                    requestId = request.Id,
                    requestStatus = request.Status.ToString(),
                    statusClass = GetStatusClass(request.Status),
                    managementHtml = partialViewHtml
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating management section for request {RequestId}", requestId);
                return Json(new { success = false, message = "Error updating interface after operation." });
            }
        }

        /// <summary>
        /// Enhanced status class helper with better mapping
        /// </summary>
        private static string GetStatusClass(RequestStatus status)
        {
            return status switch
            {
                RequestStatus.Submitted => "bg-info text-white",
                RequestStatus.InProgress => "bg-warning text-dark",
                RequestStatus.OnHold => "bg-secondary text-white",
                RequestStatus.Completed => "bg-success text-white",
                RequestStatus.Cancelled => "bg-danger text-white",
                _ => "bg-light text-dark"
            };
        }

        // POST: Requests/Assign/5
        [HttpPost]
        [Authorize(Roles = "Admin,IT Support,Asset Manager")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Assign(int id, string assignedToUserId)
        {
            if (string.IsNullOrEmpty(assignedToUserId))
            {
                return Json(new { success = false, message = "Please select a user to assign the request to." });
            }

            try
            {
                var currentUserId = _userManager.GetUserId(User);
                if (string.IsNullOrEmpty(currentUserId))
                {
                     return Json(new { success = false, message = "Current user not found." });
                }

                var success = await _requestService.AssignRequestAsync(id, assignedToUserId, currentUserId);
                var assignedUser = await _userManager.FindByIdAsync(assignedToUserId);

                if (success)
                {
                    _logger.LogInformation("Request {RequestId} assigned to User {AssignedToUserId} by User {CurrentUserId}", id, assignedToUserId, currentUserId);
                    return await GetManagementSectionUpdate(id, true, $"Request successfully assigned to {assignedUser?.FullName}.");
                }
                else
                {
                    return await GetManagementSectionUpdate(id, false, "Failed to assign request. It may have been assigned by someone else.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning request {RequestId}", id);
                return Json(new { success = false, message = "An unexpected error occurred while assigning the request." });
            }
        }

        // POST: Requests/TakeOwnership/5
        [HttpPost]
        [Authorize(Roles = "Admin,IT Support")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TakeOwnership(int id)
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                if (string.IsNullOrEmpty(userId))
                {
                    return Json(new { success = false, message = "User not found." });
                }

                var success = await _requestService.AssignRequestAsync(id, userId, userId);

                if (success)
                {
                    _logger.LogInformation("User {UserId} took ownership of Request {RequestId}", userId, id);
                    return await GetManagementSectionUpdate(id, true, "You have successfully taken ownership of the request.");
                }
                else
                {
                    return await GetManagementSectionUpdate(id, false, "Failed to take ownership. The request may have already been assigned.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error taking ownership of request {RequestId}", id);
                return Json(new { success = false, message = "An unexpected error occurred while taking ownership." });
            }
        }


        // POST: Requests/Complete/5
        [HttpPost]
        [Authorize(Roles = "Admin,IT Support")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Complete(int id, string? comments)
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                if (string.IsNullOrEmpty(userId))
                {
                    return Json(new { success = false, message = "User not found." });
                }

                var success = await _requestService.CompleteRequestAsync(id, userId, comments);

                if (success)
                {
                    _logger.LogInformation("Request {RequestId} completed by User {UserId}. Comments: {Comments}", id, userId, comments);
                    return await GetManagementSectionUpdate(id, true, "Request has been marked as complete.");
                }
                else
                {
                    return await GetManagementSectionUpdate(id, false, "Failed to complete request. It may have already been processed or you may lack permission.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing request {RequestId}", id);
                return Json(new { success = false, message = "An unexpected error occurred while completing the request." });
            }
        }

        // GET: Requests/Overdue
        [Authorize(Roles = "Admin,IT Support,Asset Manager")]
        public async Task<IActionResult> Overdue()
        {
            var overdueRequests = await _requestService.GetOverdueRequestsAsync();

            // Populate ViewBag.AssignableUsers for the Assign modal
            var assignableStaff = await _requestService.GetAssignableITStaffAsync();
            ViewBag.AssignableUsers = assignableStaff.Select(u => new SelectListItem
            {
                Value = u.Id,
                Text = u.FullName
            }).OrderBy(u => u.Text);

            return View(overdueRequests);
        }

        // GET: API for getting IT Support users for assignment
        [HttpGet]
        public async Task<JsonResult> GetITSupportUsers()
        {
            var itSupportUsers = await _userManager.GetUsersInRoleAsync("IT Support");
            var adminUsers = await _userManager.GetUsersInRoleAsync("Admin");
            
            var allUsers = itSupportUsers.Union(adminUsers)
                .Select(u => new { id = u.Id, name = $"{u.FirstName} {u.LastName}" })
                .OrderBy(u => u.name)
                .ToList();

            return Json(allUsers);
        }

        // GET: API for getting users for transfer
        [HttpGet]
        [Authorize(Roles = "Admin,IT Support,Asset Manager")]
        public async Task<JsonResult> GetTransferTargets()
        {
            var itSupportUsers = await _userManager.GetUsersInRoleAsync("IT Support");
            var adminUsers = await _userManager.GetUsersInRoleAsync("Admin");
            var assetManagerUsers = await _userManager.GetUsersInRoleAsync("Asset Manager");

            var allUsers = itSupportUsers
                .Union(adminUsers)
                .Union(assetManagerUsers)
                .Select(u => new { id = u.Id, name = $"{u.FirstName} {u.LastName} ({u.Email})" })
                .OrderBy(u => u.name)
                .ToList();

            return Json(allUsers);
        }

        // GET: API for getting assets for a specific location/department
        [HttpGet]
        public async Task<JsonResult> GetAssetsByLocation(int? locationId, string? department)
        {
            var searchModel = new AssetSearchModel 
            { 
                LocationId = locationId,
                Department = department,
                PageSize = 100 // Consider making PageSize configurable or removing if not needed for a dropdown
            };
            
            var result = await _assetService.GetAssetsAsync(searchModel);
            var assets = result.Items.Select(a => new { 
                id = a.Id, 
                name = $"{a.AssetTag} - {a.Name} ({a.Category})",
                status = a.Status.ToString() // Include status if useful for filtering/display in UI
            }).ToList();

            return Json(assets);
        }

        // GET: Requests/GetRecentRequests (AJAX)
        [HttpGet]
        public async Task<IActionResult> GetRecentRequests()
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                if (string.IsNullOrEmpty(userId))
                {
                    return Json(new List<object>());
                }

                var recentRequests = await _requestService.GetMyRequestsAsync(userId);
                var result = recentRequests.Take(10).Select(r => new
                {
                    id = r.Id,
                    requestNumber = r.RequestNumber,
                    title = r.Title,
                    status = r.Status.ToString(),
                    priority = r.Priority.ToString(),
                    requestDate = r.RequestDate,
                    requestType = r.RequestType.ToString()
                });

                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading recent requests for user");
                return Json(new List<object>());
            }
        }
    }
}
