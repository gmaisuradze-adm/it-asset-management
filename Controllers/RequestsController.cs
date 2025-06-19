using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using HospitalAssetTracker.Models;
using HospitalAssetTracker.Services;

namespace HospitalAssetTracker.Controllers
{
    [Authorize]
    public class RequestsController : Controller
    {
        private readonly IRequestService _requestService;
        private readonly IAssetService _assetService;
        private readonly ILocationService _locationService;
        private readonly UserManager<ApplicationUser> _userManager;

        public RequestsController(
            IRequestService requestService,
            IAssetService assetService,
            ILocationService locationService,
            UserManager<ApplicationUser> userManager)
        {
            _requestService = requestService;
            _assetService = assetService;
            _locationService = locationService;
            _userManager = userManager;
        }

        // GET: Requests
        public async Task<IActionResult> Index(RequestSearchModel searchModel)
        {
            var result = await _requestService.GetRequestsAsync(searchModel);
            
            ViewBag.RequestTypes = Enum.GetValues<RequestType>()
                .Select(e => new SelectListItem { Value = e.ToString(), Text = e.ToString().Replace("_", " ") });
            ViewBag.Statuses = Enum.GetValues<RequestStatus>()
                .Select(e => new SelectListItem { Value = e.ToString(), Text = e.ToString().Replace("_", " ") });
            ViewBag.Priorities = Enum.GetValues<RequestPriority>()
                .Select(e => new SelectListItem { Value = e.ToString(), Text = e.ToString() });

            return View(result);
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
            var requests = await _requestService.GetMyRequestsAsync(userId);
            return View(requests);
        }

        // GET: Requests/AssignedToMe
        [Authorize(Roles = "Admin,IT Support")]
        public async Task<IActionResult> AssignedToMe()
        {
            var userId = _userManager.GetUserId(User);
            var requests = await _requestService.GetAssignedRequestsAsync(userId);
            return View(requests);
        }

        // GET: Requests/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var request = await _requestService.GetRequestByIdAsync(id);
            if (request == null)
            {
                return NotFound();
            }

            // Check if user can view this request
            var userId = _userManager.GetUserId(User);
            var userRoles = await _userManager.GetRolesAsync(await _userManager.GetUserAsync(User));
            
            if (request.RequestedByUserId != userId && 
                request.AssignedToUserId != userId && 
                !userRoles.Any(r => r == "Admin" || r == "IT Support" || r == "Asset Manager"))
            {
                return Forbid();
            }

            // Set ViewBag properties for the view
            ViewBag.CanEdit = request.RequestedByUserId == userId || userRoles.Any(r => r == "Admin" || r == "IT Support");
            ViewBag.CanAssign = userRoles.Any(r => r == "Admin" || r == "IT Support" || r == "Asset Manager");
            ViewBag.CanApprove = userRoles.Any(r => r == "Admin" || r == "Asset Manager");
            ViewBag.CurrentUserId = userId;

            // Get IT users for assignment dropdown
            if (ViewBag.CanAssign)
            {
                var itUsers = await _userManager.GetUsersInRoleAsync("IT Support");
                var adminUsers = await _userManager.GetUsersInRoleAsync("Admin");
                var allITUsers = itUsers.Concat(adminUsers).Distinct().ToList();
                
                ViewBag.ITUsers = allITUsers.Select(u => new SelectListItem
                {
                    Value = u.Id,
                    Text = $"{u.FirstName} {u.LastName} ({u.Email})"
                });
            }

            return View(request);
        }

        // GET: Requests/Create
        public async Task<IActionResult> Create()
        {
            await PopulateViewBags();
            return View(new ITRequest());
        }

        // POST: Requests/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ITRequest request)
        {
            // Populate required fields before validation
            var userId = _userManager.GetUserId(User);
            var user = await _userManager.GetUserAsync(User);
            
            if (user == null)
            {
                return Unauthorized();
            }
            
            request.RequestedByUserId = userId ?? string.Empty;
            request.RequestedByUser = user; // Populate the navigation property
            request.Department = user.Department ?? "Unknown";
            request.RequestDate = DateTime.UtcNow;
            request.CreatedDate = DateTime.UtcNow;
            request.Status = RequestStatus.Pending;
            
            // Ensure all DateTime fields are UTC
            if (request.RequiredByDate.HasValue)
            {
                request.RequiredByDate = DateTime.SpecifyKind(request.RequiredByDate.Value, DateTimeKind.Utc);
            }
            
            if (request.DueDate.HasValue)
            {
                request.DueDate = DateTime.SpecifyKind(request.DueDate.Value, DateTimeKind.Utc);
            }
            
            // Generate RequestNumber if not set
            if (string.IsNullOrEmpty(request.RequestNumber))
            {
                request.RequestNumber = $"REQ-{DateTime.UtcNow:yyyyMMdd}-{DateTime.UtcNow.Ticks % 10000:D4}";
            }
            
            // Remove validation errors for fields we just populated
            ModelState.Remove(nameof(request.RequestNumber));
            ModelState.Remove(nameof(request.RequestedByUserId));
            ModelState.Remove(nameof(request.RequestedByUser));
            ModelState.Remove(nameof(request.Department));
            ModelState.Remove(nameof(request.Requester));
            ModelState.Remove(nameof(request.RequestDate));
            ModelState.Remove(nameof(request.CreatedDate));
            ModelState.Remove(nameof(request.Status));
            
            if (ModelState.IsValid)
            {
                try
                {
                    await _requestService.CreateRequestAsync(request, userId);
                    
                    TempData["SuccessMessage"] = $"Request {request.RequestNumber} has been created successfully.";
                    return RedirectToAction(nameof(MyRequests));
                }
                catch (Exception ex)
                {
                    // Log detailed error information
                    var errorMessage = $"Error creating request: {ex.Message}";
                    if (ex.InnerException != null)
                    {
                        errorMessage += $" Inner Exception: {ex.InnerException.Message}";
                    }
                    
                    ModelState.AddModelError("", errorMessage);
                    
                    // Also set TempData for user feedback
                    TempData["ErrorMessage"] = errorMessage;
                }
            }

            await PopulateViewBags(request);
            return View(request);
        }

        // GET: Requests/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var request = await _requestService.GetRequestByIdAsync(id);
            if (request == null)
            {
                return NotFound();
            }

            // Check permissions
            var userId = _userManager.GetUserId(User);
            var userRoles = await _userManager.GetRolesAsync(await _userManager.GetUserAsync(User));
            
            if (request.RequestedByUserId != userId && 
                !userRoles.Any(r => r == "Admin" || r == "IT Support"))
            {
                return Forbid();
            }

            // Can't edit completed or cancelled requests
            if (request.Status == RequestStatus.Completed || request.Status == RequestStatus.Cancelled)
            {
                TempData["ErrorMessage"] = "Cannot edit completed or cancelled requests.";
                return RedirectToAction(nameof(Details), new { id });
            }

            await PopulateViewBags(request);
            return View(request);
        }

        // POST: Requests/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ITRequest request)
        {
            if (id != request.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var userId = _userManager.GetUserId(User);
                    await _requestService.UpdateRequestAsync(request, userId);
                    
                    TempData["SuccessMessage"] = "Request has been updated successfully.";
                    return RedirectToAction(nameof(Details), new { id });
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Error updating request: {ex.Message}");
                }
            }

            await PopulateViewBags(request);
            return View(request);
        }

        // POST: Requests/Assign/5
        [HttpPost]
        [Authorize(Roles = "Admin,IT Support,Asset Manager")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Assign(int id, string assignedToUserId)
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                var success = await _requestService.AssignRequestAsync(id, assignedToUserId, userId);
                
                if (success)
                {
                    TempData["SuccessMessage"] = "Request has been assigned successfully.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to assign request.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error assigning request: {ex.Message}";
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        // POST: Requests/Approve/5
        [HttpPost]
        [Authorize(Roles = "Admin,Asset Manager,Department Head")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id, string? comments)
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                var success = await _requestService.ApproveRequestAsync(id, userId, comments);
                
                if (success)
                {
                    TempData["SuccessMessage"] = "Request has been approved successfully.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to approve request.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error approving request: {ex.Message}";
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        // POST: Requests/Complete/5
        [HttpPost]
        [Authorize(Roles = "Admin,IT Support")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Complete(int id, string? completionNotes)
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                var success = await _requestService.CompleteRequestAsync(id, userId, completionNotes);
                
                if (success)
                {
                    TempData["SuccessMessage"] = "Request has been completed successfully.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to complete request.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error completing request: {ex.Message}";
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        // GET: Requests/Overdue
        [Authorize(Roles = "Admin,IT Support,Asset Manager")]
        public async Task<IActionResult> Overdue()
        {
            var overdueRequests = await _requestService.GetOverdueRequestsAsync();
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

        // GET: API for getting assets for a specific location/department
        [HttpGet]
        public async Task<JsonResult> GetAssetsByLocation(int? locationId, string? department)
        {
            var searchModel = new AssetSearchModel 
            { 
                LocationId = locationId,
                Department = department,
                PageSize = 100
            };
            
            var result = await _assetService.GetAssetsAsync(searchModel);
            var assets = result.Items.Select(a => new { 
                id = a.Id, 
                name = $"{a.AssetTag} - {a.Name}",
                status = a.Status.ToString()
            }).ToList();

            return Json(assets);
        }

        // POST: Requests/Reject/5
        [HttpPost]
        [Authorize(Roles = "Admin,Asset Manager")]
        public async Task<IActionResult> Reject(int id, string rejectionReason)
        {
            try
            {
                var success = await _requestService.RejectRequestAsync(id, rejectionReason);
                
                if (success)
                {
                    return Json(new { success = true, message = "Request has been rejected." });
                }
                else
                {
                    return Json(new { success = false, message = "Failed to reject request." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
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
                    TempData["ErrorMessage"] = "User not found. Please log in again.";
                    return RedirectToAction(nameof(Details), new { id });
                }

                var success = await _requestService.AssignRequestAsync(id, userId, userId);
                
                if (success)
                {
                    TempData["SuccessMessage"] = "Request assigned to you successfully.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to take ownership of request.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error taking ownership: {ex.Message}";
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        // POST: Requests/ChangePriority/5
        [HttpPost]
        [Authorize(Roles = "Admin,IT Support,Asset Manager")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePriority(int id, RequestPriority newPriority, string reason)
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                var request = await _requestService.GetRequestByIdAsync(id);
                
                if (request == null)
                {
                    return Json(new { success = false, message = "Request not found." });
                }

                if (request.Priority == newPriority)
                {
                    return Json(new { success = false, message = "Priority is already set to the requested level." });
                }

                var oldPriority = request.Priority;
                request.Priority = newPriority;
                request.LastModifiedDate = DateTime.UtcNow;

                var updatedRequest = await _requestService.UpdateRequestAsync(request, userId!);
                
                if (updatedRequest != null)
                {
                    // Note: AddRequestNoteAsync would be added to service if audit trail notes are needed
                    
                    return Json(new { success = true, message = "Priority changed successfully." });
                }
                else
                {
                    return Json(new { success = false, message = "Failed to change priority." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST: Requests/Escalate/5
        [HttpPost]
        [Authorize(Roles = "Admin,IT Support,Asset Manager")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Escalate(int id, string escalateToUserId, string reason)
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                var request = await _requestService.GetRequestByIdAsync(id);
                
                if (request == null)
                {
                    return Json(new { success = false, message = "Request not found." });
                }

                if (string.IsNullOrEmpty(escalateToUserId))
                {
                    return Json(new { success = false, message = "Please select a user to escalate to." });
                }

                var escalateToUser = await _userManager.FindByIdAsync(escalateToUserId);
                if (escalateToUser == null)
                {
                    return Json(new { success = false, message = "Target user not found." });
                }

                // Escalate typically means raising priority and reassigning
                if (request.Priority != RequestPriority.Critical)
                {
                    var oldPriority = request.Priority;
                    request.Priority = request.Priority == RequestPriority.High ? RequestPriority.Critical : RequestPriority.High;
                    
                    // Update request with new priority
                    await _requestService.UpdateRequestAsync(request, userId!);
                    
                    // Note: Audit message would be logged here if audit service is available
                }

                // Reassign to escalation target
                var success = await _requestService.AssignRequestAsync(id, escalateToUserId, userId!);
                
                if (success)
                {
                    // Note: Escalation note would be logged here if audit service is available
                    return Json(new { success = true, message = $"Request escalated to {escalateToUser.Email} successfully." });
                }
                else
                {
                    return Json(new { success = false, message = "Failed to escalate request." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST: Requests/Transfer/5
        [HttpPost]
        [Authorize(Roles = "Admin,IT Support,Asset Manager")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Transfer(int id, string transferToUserId, string reason)
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                var request = await _requestService.GetRequestByIdAsync(id);
                
                if (request == null)
                {
                    return Json(new { success = false, message = "Request not found." });
                }

                if (string.IsNullOrEmpty(transferToUserId))
                {
                    return Json(new { success = false, message = "Please select a user to transfer to." });
                }

                var transferToUser = await _userManager.FindByIdAsync(transferToUserId);
                if (transferToUser == null)
                {
                    return Json(new { success = false, message = "Target user not found." });
                }

                var success = await _requestService.AssignRequestAsync(id, transferToUserId, userId!);
                
                if (success)
                {
                    // Note: Transfer note would be logged here if audit service is available
                    return Json(new { success = true, message = $"Request transferred to {transferToUser.Email} successfully." });
                }
                else
                {
                    return Json(new { success = false, message = "Failed to transfer request." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // GET: Requests/GetAvailableUsers
        [HttpGet]
        [Authorize(Roles = "Admin,IT Support,Asset Manager")]
        public async Task<IActionResult> GetAvailableUsers(string? role = null)
        {
            try
            {
                var users = _userManager.Users.AsQueryable();
                
                if (!string.IsNullOrEmpty(role))
                {
                    var roles = role.Split(',').Select(r => r.Trim()).ToArray();
                    var usersInRoles = new List<ApplicationUser>();
                    
                    foreach (var r in roles)
                    {
                        var roleUsers = await _userManager.GetUsersInRoleAsync(r);
                        usersInRoles.AddRange(roleUsers);
                    }
                    
                    users = usersInRoles.AsQueryable();
                }

                var userList = users
                    .Select(u => new
                    {
                        id = u.Id,
                        firstName = u.FirstName,
                        lastName = u.LastName,
                        email = u.Email,
                        department = u.Department
                    })
                    .OrderBy(u => u.firstName)
                    .ThenBy(u => u.lastName)
                    .ToList();

                return Json(userList);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        private async Task PopulateViewBags(ITRequest? request = null)
        {
            ViewBag.RequestTypes = Enum.GetValues<RequestType>()
                .Select(e => new SelectListItem 
                { 
                    Value = ((int)e).ToString(), 
                    Text = e.ToString().Replace("_", " "),
                    Selected = request?.RequestType == e
                });

            ViewBag.Priorities = Enum.GetValues<RequestPriority>()
                .Select(e => new SelectListItem 
                { 
                    Value = ((int)e).ToString(), 
                    Text = e.ToString(),
                    Selected = request?.Priority == e
                });

            // Get locations for dropdown
            var locations = await _locationService.GetAllLocationsAsync();
            ViewBag.Locations = locations.Select(l => new SelectListItem
            {
                Value = l.Id.ToString(),
                Text = $"{l.Building} - {l.Floor} - {l.Room}",
                Selected = request?.RelatedAsset?.LocationId == l.Id
            });

            // Get departments
            var users = _userManager.Users.ToList();
            var departments = users.Where(u => !string.IsNullOrEmpty(u.Department))
                .Select(u => u.Department)
                .Distinct()
                .OrderBy(d => d)
                .ToList();

            ViewBag.Departments = departments.Select(d => new SelectListItem
            {
                Value = d,
                Text = d,
                Selected = request?.Department == d
            });

            // Categories for equipment requests
            ViewBag.ItemCategories = new List<SelectListItem>
            {
                new() { Value = "Desktop Computer", Text = "Desktop Computer" },
                new() { Value = "Laptop", Text = "Laptop" },
                new() { Value = "Printer", Text = "Printer" },
                new() { Value = "Network Equipment", Text = "Network Equipment" },
                new() { Value = "Server", Text = "Server" },
                new() { Value = "Mobile Device", Text = "Mobile Device" },
                new() { Value = "Peripheral", Text = "Peripheral" },
                new() { Value = "Software", Text = "Software" },
                new() { Value = "Other", Text = "Other" }
            };

            // Set current user department for JavaScript
            var currentUser = await _userManager.GetUserAsync(User);
            ViewBag.CurrentUserDepartment = currentUser?.Department ?? "Unknown";
        }
    }
}
