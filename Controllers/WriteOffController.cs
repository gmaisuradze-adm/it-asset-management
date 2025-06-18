using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using HospitalAssetTracker.Models;
using HospitalAssetTracker.Services;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HospitalAssetTracker.Controllers
{
    [Authorize]
    public class WriteOffController : Controller
    {
        private readonly IWriteOffService _writeOffService;
        private readonly IAssetService _assetService;
        private readonly UserManager<ApplicationUser> _userManager;

        public WriteOffController(
            IWriteOffService writeOffService,
            IAssetService assetService,
            UserManager<ApplicationUser> userManager)
        {
            _writeOffService = writeOffService;
            _assetService = assetService;
            _userManager = userManager;
        }

        // GET: WriteOff
        public async Task<IActionResult> Index()
        {
            // Get assets with WriteOff statuses
            var writeOffAssets = await _assetService.GetWriteOffAssetsAsync();
            return View(writeOffAssets);
        }

        // GET: WriteOff/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var writeOff = await _writeOffService.GetWriteOffRecordByIdAsync(id);
            if (writeOff == null)
            {
                return NotFound();
            }

            return View(writeOff);
        }

        // GET: WriteOff/Create
        public async Task<IActionResult> Create(int? assetId)
        {
            ViewBag.Reasons = new SelectList(Enum.GetValues<WriteOffReason>());
            ViewBag.Methods = new SelectList(Enum.GetValues<WriteOffMethod>());

            if (assetId.HasValue)
            {
                var asset = await _assetService.GetAssetByIdAsync(assetId.Value);
                if (asset != null)
                {
                    ViewBag.AssetId = assetId.Value;
                    ViewBag.AssetTag = asset.AssetTag;
                    ViewBag.AssetDescription = $"{asset.Brand} {asset.Model}";
                }
            }

            return View();
        }

        // POST: WriteOff/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(WriteOffRecord writeOffRecord)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    TempData["ErrorMessage"] = "Unable to identify current user.";
                    ViewBag.Reasons = new SelectList(Enum.GetValues<WriteOffReason>());
                    ViewBag.Methods = new SelectList(Enum.GetValues<WriteOffMethod>());
                    return View(writeOffRecord);
                }

                var result = await _writeOffService.CreateWriteOffRecordAsync(writeOffRecord, user.Id);

                if (result != null)
                {
                    TempData["SuccessMessage"] = "Write-off record created successfully.";
                    return RedirectToAction(nameof(Details), new { id = result.Id });
                }

                ModelState.AddModelError("", "Unable to create write-off record.");
            }

            ViewBag.Reasons = new SelectList(Enum.GetValues<WriteOffReason>());
            ViewBag.Methods = new SelectList(Enum.GetValues<WriteOffMethod>());
            return View(writeOffRecord);
        }

        // GET: WriteOff/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var writeOff = await _writeOffService.GetWriteOffRecordByIdAsync(id);
            if (writeOff == null)
            {
                return NotFound();
            }

            // Only allow editing if not yet approved
            if (writeOff.IsApproved)
            {
                TempData["ErrorMessage"] = "Cannot edit an approved write-off record.";
                return RedirectToAction(nameof(Details), new { id });
            }

            ViewBag.Reasons = new SelectList(Enum.GetValues<WriteOffReason>(), writeOff.Reason);
            ViewBag.Methods = new SelectList(Enum.GetValues<WriteOffMethod>(), writeOff.Method);
            return View(writeOff);
        }

        // POST: WriteOff/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, WriteOffRecord writeOffRecord)
        {
            if (id != writeOffRecord.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    TempData["ErrorMessage"] = "Unable to identify current user.";
                    ViewBag.Reasons = new SelectList(Enum.GetValues<WriteOffReason>(), writeOffRecord.Reason);
                    ViewBag.Methods = new SelectList(Enum.GetValues<WriteOffMethod>(), writeOffRecord.Method);
                    return View(writeOffRecord);
                }

                var result = await _writeOffService.UpdateWriteOffRecordAsync(writeOffRecord, user.Id);

                if (result != null)
                {
                    TempData["SuccessMessage"] = "Write-off record updated successfully.";
                    return RedirectToAction(nameof(Details), new { id });
                }

                ModelState.AddModelError("", "Unable to update write-off record.");
            }

            ViewBag.Reasons = new SelectList(Enum.GetValues<WriteOffReason>(), writeOffRecord.Reason);
            ViewBag.Methods = new SelectList(Enum.GetValues<WriteOffMethod>(), writeOffRecord.Method);
            return View(writeOffRecord);
        }

        // POST: WriteOff/Approve/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Asset Manager")]
        public async Task<IActionResult> Approve(int id, string approvalComments)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                TempData["ErrorMessage"] = "Unable to identify current user.";
                return RedirectToAction(nameof(Details), new { id });
            }

            var result = await _writeOffService.ApproveWriteOffAsync(id, user.Id, approvalComments);

            if (result)
            {
                TempData["SuccessMessage"] = "Write-off record approved successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Unable to approve write-off record.";
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        // POST: WriteOff/Reject/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Asset Manager")]
        public async Task<IActionResult> Reject(int id, string rejectionComments)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                TempData["ErrorMessage"] = "Unable to identify current user.";
                return RedirectToAction(nameof(Details), new { id });
            }

            var result = await _writeOffService.RejectWriteOffAsync(id, user.Id, rejectionComments);

            if (result)
            {
                TempData["SuccessMessage"] = "Write-off record rejected.";
            }
            else
            {
                TempData["ErrorMessage"] = "Unable to reject write-off record.";
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        // GET: WriteOff/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var writeOff = await _writeOffService.GetWriteOffRecordByIdAsync(id);
            if (writeOff == null)
            {
                return NotFound();
            }

            // Only allow deletion if not yet approved
            if (writeOff.IsApproved)
            {
                TempData["ErrorMessage"] = "Cannot delete an approved write-off record.";
                return RedirectToAction(nameof(Details), new { id });
            }

            return View(writeOff);
        }

        // POST: WriteOff/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                TempData["ErrorMessage"] = "Unable to identify current user.";
                return RedirectToAction(nameof(Index));
            }

            var result = await _writeOffService.DeleteWriteOffRecordAsync(id, user.Id);

            if (result)
            {
                TempData["SuccessMessage"] = "Write-off record deleted successfully.";
                return RedirectToAction(nameof(Index));
            }

            TempData["ErrorMessage"] = "Unable to delete write-off record.";
            return RedirectToAction(nameof(Details), new { id });
        }

        // GET: WriteOff/Report
        public async Task<IActionResult> Report(DateTime? startDate, DateTime? endDate, WriteOffReason? reason)
        {
            var summary = await _writeOffService.GetWriteOffSummaryAsync(startDate, endDate, reason);
            
            ViewBag.StartDate = startDate;
            ViewBag.EndDate = endDate;
            ViewBag.Reason = reason;
            ViewBag.Reasons = new SelectList(Enum.GetValues<WriteOffReason>());

            return View(summary);
        }

        // GET: WriteOff/ByAsset/5
        public async Task<IActionResult> ByAsset(int assetId)
        {
            var writeOffs = await _writeOffService.GetWriteOffRecordsByAssetAsync(assetId);
            var asset = await _assetService.GetAssetByIdAsync(assetId);

            if (asset == null)
            {
                return NotFound();
            }

            ViewBag.Asset = asset;
            return View(writeOffs);
        }

        // GET: WriteOff/PendingApproval
        [Authorize(Roles = "Admin,Asset Manager")]
        public async Task<IActionResult> PendingApproval()
        {
            var writeOffs = await _writeOffService.GetPendingApprovalWriteOffsAsync();
            return View(writeOffs);
        }
    }
}
