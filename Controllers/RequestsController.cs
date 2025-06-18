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
            if (ModelState.IsValid)
            {
                try
                {
                    var userId = _userManager.GetUserId(User);
                    var user = await _userManager.GetUserAsync(User);
                    request.Department = user?.Department ?? "Unknown";
                    
                    await _requestService.CreateRequestAsync(request, userId);
                    
                    TempData["SuccessMessage"] = $"Request {request.RequestNumber} has been created successfully.";
                    return RedirectToAction(nameof(MyRequests));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Error creating request: {ex.Message}");
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

        private async Task PopulateViewBags(ITRequest? request = null)
        {
            ViewBag.RequestTypes = Enum.GetValues<RequestType>()
                .Select(e => new SelectListItem 
                { 
                    Value = e.ToString(), 
                    Text = e.ToString().Replace("_", " "),
                    Selected = request?.RequestType == e
                });

            ViewBag.Priorities = Enum.GetValues<RequestPriority>()
                .Select(e => new SelectListItem 
                { 
                    Value = e.ToString(), 
                    Text = e.ToString(),
                    Selected = request?.Priority == e
                });

            // Get locations for dropdown
            var locations = await _locationService.GetAllLocationsAsync();
            ViewBag.Locations = locations.Select(l => new SelectListItem
            {
                Value = l.Id.ToString(),
                Text = $"{l.Building} - {l.Floor} - {l.Room}",
                Selected = request?.Asset?.LocationId == l.Id
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
        }
    }
}
