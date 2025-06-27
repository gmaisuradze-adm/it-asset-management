using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using HospitalAssetTracker.Models;
using HospitalAssetTracker.Services;

namespace HospitalAssetTracker.Controllers
{
    [Authorize(Roles = "Admin,Asset Manager")]
    public class ProcurementController : Controller
    {
        private readonly IProcurementService _procurementService;
        private readonly IRequestService _requestService;
        private readonly UserManager<ApplicationUser> _userManager;

        public ProcurementController(
            IProcurementService procurementService,
            IRequestService requestService,
            UserManager<ApplicationUser> userManager)
        {
            _procurementService = procurementService;
            _requestService = requestService;
            _userManager = userManager;
        }

        // GET: Procurement
        public async Task<IActionResult> Index(ProcurementSearchModel searchModel)
        {
            var result = await _procurementService.GetProcurementRequestsAsync(searchModel);
            
            ViewBag.ProcurementTypes = Enum.GetValues<ProcurementType>()
                .Select(e => new SelectListItem { Value = e.ToString(), Text = e.ToString().Replace("_", " ") });
            ViewBag.Statuses = Enum.GetValues<ProcurementStatus>()
                .Select(e => new SelectListItem { Value = e.ToString(), Text = e.ToString().Replace("_", " ") });
            ViewBag.Priorities = Enum.GetValues<ProcurementPriority>()
                .Select(e => new SelectListItem { Value = e.ToString(), Text = e.ToString() });
            
            var vendors = await _procurementService.GetVendorsAsync();
            ViewBag.Vendors = vendors.Select(v => new SelectListItem { Value = v.Id.ToString(), Text = v.Name });

            return View(result);
        }

        // GET: Procurement/Dashboard
        public async Task<IActionResult> Dashboard()
        {
            var dashboardData = await _procurementService.GetProcurementDashboardDataAsync();
            return View(dashboardData);
        }

        // GET: Procurement/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var procurement = await _procurementService.GetProcurementRequestByIdAsync(id);
            if (procurement == null)
            {
                return NotFound();
            }

            return View(procurement);
        }

        // GET: Procurement/Create
        public async Task<IActionResult> Create(int? requestId = null)
        {
            var procurement = new ProcurementRequest();
            
            // If creating from a request, populate relevant fields
            if (requestId.HasValue)
            {
                var request = await _requestService.GetRequestByIdAsync(requestId.Value);
                if (request != null)
                {
                    procurement.Title = $"Procurement for: {request.Title}";
                    procurement.Description = request.Description;
                    procurement.RequiredByDate = request.RequiredByDate;
                    procurement.EstimatedBudget = request.EstimatedCost ?? 0;
                    procurement.OriginatingRequestId = requestId.Value;
                    procurement.BusinessJustification = request.BusinessJustification;
                    procurement.IsUrgent = request.Priority == RequestPriority.Critical;
                    
                    // Add default item if category is specified
                    if (!string.IsNullOrEmpty(request.RequestedItemCategory))
                    {
                        procurement.Items = new List<ProcurementItem>
                        {
                            new ProcurementItem
                            {
                                ItemName = request.RequestedItemCategory,
                                Description = request.RequestedItemSpecifications ?? "",
                                Quantity = 1,
                                EstimatedUnitPrice = request.EstimatedCost ?? 0
                            }
                        };
                    }
                }
            }

            await PopulateViewBags(procurement);
            return View(procurement);
        }

        // POST: Procurement/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProcurementRequest procurement, List<ProcurementItem> items)
        {
            // DEBUG: Log form submission details
            var logger = HttpContext.RequestServices.GetRequiredService<ILogger<ProcurementController>>();
            logger.LogInformation("Procurement Create POST - ModelState.IsValid: {IsValid}", ModelState.IsValid);
            logger.LogInformation("Procurement Create POST - Title: '{Title}', Description: '{Description}'", procurement.Title, procurement.Description);
            logger.LogInformation("Procurement Create POST - ProcurementType: {Type}, Category: {Category}, Method: {Method}", 
                procurement.ProcurementType, procurement.Category, procurement.Method);
            logger.LogInformation("Procurement Create POST - Items count: {ItemCount}", items?.Count ?? 0);
            
            // DEBUG: Log validation errors
            if (!ModelState.IsValid)
            {
                foreach (var error in ModelState)
                {
                    logger.LogWarning("Validation Error - Field: {Field}, Errors: {Errors}", 
                        error.Key, string.Join(", ", error.Value.Errors.Select(e => e.ErrorMessage)));
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    procurement.Items = items?.Where(i => !string.IsNullOrEmpty(i.ItemName)).ToList() ?? new List<ProcurementItem>();
                    
                    // Calculate total from items
                    procurement.EstimatedBudget = procurement.Items.Sum(i => i.EstimatedTotalPrice);
                    
                    var userId = _userManager.GetUserId(User);
                    if (string.IsNullOrEmpty(userId))
                    {
                        logger.LogError("Procurement Create POST - User not found");
                        ModelState.AddModelError("", "User not found.");
                        await PopulateViewBags(procurement);
                        return View(procurement);
                    }
                    
                    logger.LogInformation("Procurement Create POST - Creating procurement for user: {UserId}", userId);
                    await _procurementService.CreateProcurementRequestAsync(procurement, userId);
                    
                    TempData["SuccessMessage"] = $"Procurement request {procurement.RequestNumber} has been created successfully.";
                    return RedirectToAction(nameof(Details), new { id = procurement.Id });
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Procurement Create POST - Exception occurred: {Message}", ex.Message);
                    ModelState.AddModelError("", $"Error creating procurement request: {ex.Message}");
                }
            }
            else
            {
                logger.LogWarning("Procurement Create POST - ModelState is invalid, returning to form");
            }

            await PopulateViewBags(procurement);
            return View(procurement);
        }

        // GET: Procurement/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var procurement = await _procurementService.GetProcurementRequestByIdAsync(id);
            if (procurement == null)
            {
                return NotFound();
            }

            // Can't edit approved or completed procurements
            if (procurement.Status == ProcurementStatus.Approved || 
                procurement.Status == ProcurementStatus.Received ||
                procurement.Status == ProcurementStatus.Cancelled)
            {
                TempData["ErrorMessage"] = "Cannot edit approved, completed, or cancelled procurement requests.";
                return RedirectToAction(nameof(Details), new { id });
            }

            await PopulateViewBags(procurement);
            return View(procurement);
        }

        // POST: Procurement/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ProcurementRequest procurement, List<ProcurementItem> items)
        {
            if (id != procurement.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    procurement.Items = items?.Where(i => !string.IsNullOrEmpty(i.ItemName)).ToList() ?? new List<ProcurementItem>();
                    procurement.EstimatedBudget = procurement.Items.Sum(i => i.EstimatedTotalPrice);
                    
                    var userId = _userManager.GetUserId(User);
                    if (string.IsNullOrEmpty(userId))
                    {
                        ModelState.AddModelError("", "User not found.");
                        await PopulateViewBags(procurement);
                        return View(procurement);
                    }
                    await _procurementService.UpdateProcurementRequestAsync(procurement, userId);
                    
                    TempData["SuccessMessage"] = "Procurement request has been updated successfully.";
                    return RedirectToAction(nameof(Details), new { id });
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Error updating procurement request: {ex.Message}");
                }
            }

            await PopulateViewBags(procurement);
            return View(procurement);
        }

        // POST: Procurement/SubmitForApproval/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitForApproval(int id)
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                if (string.IsNullOrEmpty(userId))
                {
                    TempData["ErrorMessage"] = "User not found.";
                    return RedirectToAction(nameof(Details), new { id });
                }
                var success = await _procurementService.SubmitForApprovalAsync(id, userId);
                
                if (success)
                {
                    TempData["SuccessMessage"] = "Procurement request has been submitted for approval.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to submit procurement request for approval.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error submitting for approval: {ex.Message}";
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        // POST: Procurement/Approve/5
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id, string? comments)
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                if (string.IsNullOrEmpty(userId))
                {
                    TempData["ErrorMessage"] = "User not found.";
                    return RedirectToAction(nameof(Details), new { id });
                }
                var success = await _procurementService.ApproveProcurementAsync(id, userId, comments);
                
                if (success)
                {
                    TempData["SuccessMessage"] = "Procurement request has been approved successfully.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to approve procurement request.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error approving procurement request: {ex.Message}";
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        // GET: Procurement/Receive/5
        public async Task<IActionResult> Receive(int id)
        {
            var procurement = await _procurementService.GetProcurementRequestByIdAsync(id);
            if (procurement == null)
            {
                return NotFound();
            }

            if (procurement.Status != ProcurementStatus.Ordered)
            {
                TempData["ErrorMessage"] = "Only ordered procurements can be received.";
                return RedirectToAction(nameof(Details), new { id });
            }

            // Create received items based on procurement items
            var receivedItems = procurement.Items?.Select(item => new ProcurementItemReceived
            {
                ProcurementItemId = item.Id,
                ItemName = item.ItemName,
                Description = item.Description,
                OrderedQuantity = item.Quantity,
                ReceivedQuantity = item.PendingQuantity, // Default to pending quantity
                UnitPrice = item.UnitPrice,
                Category = item.ItemName // Default category, can be changed by user
            }).ToList() ?? new List<ProcurementItemReceived>();

            ViewBag.ProcurementId = id;
            ViewBag.ProcurementNumber = procurement.RequestNumber;
            
            return View(receivedItems);
        }

        // POST: Procurement/Receive/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Receive(int id, List<ProcurementItemReceived> receivedItems)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var userId = _userManager.GetUserId(User);
                    if (string.IsNullOrEmpty(userId))
                    {
                        ModelState.AddModelError("", "User not found.");
                        ViewBag.ProcurementId = id;
                        return View(receivedItems);
                    }
                    var success = await _procurementService.ReceiveProcurementAsync(id, userId, receivedItems);
                    
                    if (success)
                    {
                        TempData["SuccessMessage"] = "Procurement items have been received and added to inventory.";
                        return RedirectToAction(nameof(Details), new { id });
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Failed to process received items.";
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Error processing received items: {ex.Message}");
                }
            }

            ViewBag.ProcurementId = id;
            return View(receivedItems);
        }

        // GET: Procurement/Overdue
        public async Task<IActionResult> Overdue()
        {
            var overdueProcurements = await _procurementService.GetOverdueProcurementsAsync();
            return View(overdueProcurements);
        }

        // GET: Procurement/Vendors
        public async Task<IActionResult> Vendors()
        {
            var vendors = await _procurementService.GetVendorsAsync();
            return View(vendors);
        }

        // GET: Procurement/CreateVendor
        public IActionResult CreateVendor()
        {
            return View(new Vendor());
        }

        // POST: Procurement/CreateVendor
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateVendor(Vendor vendor)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var userId = _userManager.GetUserId(User);
                    if (string.IsNullOrEmpty(userId))
                    {
                        ModelState.AddModelError("", "User not found.");
                        return View(vendor);
                    }
                    await _procurementService.CreateVendorAsync(vendor, userId);
                    
                    TempData["SuccessMessage"] = $"Vendor {vendor.Name} has been created successfully.";
                    return RedirectToAction(nameof(Vendors));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Error creating vendor: {ex.Message}");
                }
            }

            return View(vendor);
        }

        // GET: API for getting procurement status counts
        [HttpGet]
        public async Task<JsonResult> GetStatusCounts()
        {
            var dashboardData = await _procurementService.GetProcurementDashboardDataAsync();
            
            var statusCounts = new
            {
                draft = dashboardData.TotalProcurements - dashboardData.PendingApproval - dashboardData.InProgress - dashboardData.CompletedThisMonth,
                pendingApproval = dashboardData.PendingApproval,
                inProgress = dashboardData.InProgress,
                completed = dashboardData.CompletedThisMonth
            };

            return Json(statusCounts);
        }

        // GET: API for calculating item totals
        [HttpPost]
        public JsonResult CalculateItemTotal(decimal unitPrice, int quantity)
        {
            var total = unitPrice * quantity;
            return Json(new { total = total.ToString("C") });
        }

        private async Task PopulateViewBags(ProcurementRequest? procurement = null)
        {
            ViewBag.ProcurementTypes = Enum.GetValues<ProcurementType>()
                .Select(e => new SelectListItem 
                { 
                    Value = e.ToString(), 
                    Text = e.ToString().Replace("_", " "),
                    Selected = procurement?.ProcurementType == e
                });

            ViewBag.Categories = Enum.GetValues<ProcurementCategory>()
                .Select(e => new SelectListItem 
                { 
                    Value = e.ToString(), 
                    Text = e.ToString().Replace("_", " "),
                    Selected = procurement?.Category == e
                });

            ViewBag.Methods = Enum.GetValues<ProcurementMethod>()
                .Select(e => new SelectListItem 
                { 
                    Value = e.ToString(), 
                    Text = e.ToString().Replace("_", " "),
                    Selected = procurement?.Method == e
                });

            ViewBag.Priorities = Enum.GetValues<ProcurementPriority>()
                .Select(e => new SelectListItem 
                { 
                    Value = e.ToString(), 
                    Text = e.ToString(),
                    Selected = procurement?.Priority == e
                });

            var vendors = await _procurementService.GetVendorsAsync();
            ViewBag.Vendors = vendors.Select(v => new SelectListItem
            {
                Value = v.Id.ToString(),
                Text = v.Name,
                Selected = procurement?.VendorId == v.Id
            });

            // Categories for items
            ViewBag.ItemCategories = new List<SelectListItem>
            {
                new() { Value = "Hardware - Desktop Computer", Text = "Hardware - Desktop Computer" },
                new() { Value = "Hardware - Laptop", Text = "Hardware - Laptop" },
                new() { Value = "Hardware - Printer", Text = "Hardware - Printer" },
                new() { Value = "Hardware - Network Equipment", Text = "Hardware - Network Equipment" },
                new() { Value = "Hardware - Server", Text = "Hardware - Server" },
                new() { Value = "Hardware - Mobile Device", Text = "Hardware - Mobile Device" },
                new() { Value = "Hardware - Peripheral", Text = "Hardware - Peripheral" },
                new() { Value = "Software - Operating System", Text = "Software - Operating System" },
                new() { Value = "Software - Application", Text = "Software - Application" },
                new() { Value = "Software - Security", Text = "Software - Security" },
                new() { Value = "Consumables - Cables", Text = "Consumables - Cables" },
                new() { Value = "Consumables - Accessories", Text = "Consumables - Accessories" },
                new() { Value = "Services - Maintenance", Text = "Services - Maintenance" },
                new() { Value = "Services - Installation", Text = "Services - Installation" },
                new() { Value = "Services - Training", Text = "Services - Training" },
                new() { Value = "Other", Text = "Other" }
            };

            // If there's a related request, get its details
            if (procurement?.RelatedRequestId.HasValue == true)
            {
                var relatedRequest = await _requestService.GetRequestByIdAsync(procurement.RelatedRequestId.Value);
                ViewBag.RelatedRequest = relatedRequest;
            }
        }
    }
}
