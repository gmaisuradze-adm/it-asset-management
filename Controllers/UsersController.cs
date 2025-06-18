using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using HospitalAssetTracker.Models;
using HospitalAssetTracker.Services;

namespace HospitalAssetTracker.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IAuditService _auditService;

        public UsersController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IAuditService auditService)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _auditService = auditService;
        }

        // GET: Users
        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.ToListAsync();
            return View(users);
        }

        // GET: Users/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var userRoles = await _userManager.GetRolesAsync(user);
            ViewBag.UserRoles = userRoles;

            return View(user);
        }

        // GET: Users/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.Roles = await _roleManager.Roles.ToListAsync();
            return View();
        }

        // POST: Users/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ApplicationUser user, string password, List<string> selectedRoles)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    user.UserName = user.Email;
                    user.CreatedDate = DateTime.UtcNow;
                    user.IsActive = true;

                    var result = await _userManager.CreateAsync(user, password);
                    if (result.Succeeded)
                    {
                        // Add roles
                        if (selectedRoles != null && selectedRoles.Any())
                        {
                            await _userManager.AddToRolesAsync(user, selectedRoles);
                        }

                        var currentUserId = _userManager.GetUserId(User) ?? string.Empty;
                        await _auditService.LogAsync(
                            AuditAction.Create,
                            "User",
                            null,
                            currentUserId,
                            $"User created: {user.Email}"
                        );

                        TempData["SuccessMessage"] = "User created successfully!";
                        return RedirectToAction(nameof(Index));
                    }

                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = $"Error creating user: {ex.Message}";
                }
            }

            ViewBag.Roles = await _roleManager.Roles.ToListAsync();
            return View(user);
        }

        // GET: Users/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var userRoles = await _userManager.GetRolesAsync(user);
            var allRoles = await _roleManager.Roles.ToListAsync();
            
            ViewBag.UserRoles = userRoles;
            ViewBag.AllRoles = allRoles;

            return View(user);
        }

        // POST: Users/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, ApplicationUser user, List<string> selectedRoles)
        {
            if (id != user.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingUser = await _userManager.FindByIdAsync(id);
                    if (existingUser == null)
                    {
                        return NotFound();
                    }

                    // Update user properties
                    existingUser.FirstName = user.FirstName;
                    existingUser.LastName = user.LastName;
                    existingUser.Email = user.Email;
                    existingUser.UserName = user.Email;
                    existingUser.PhoneNumber = user.PhoneNumber;
                    existingUser.Department = user.Department;
                    existingUser.JobTitle = user.JobTitle;
                    existingUser.IsActive = user.IsActive;

                    var result = await _userManager.UpdateAsync(existingUser);
                    if (result.Succeeded)
                    {
                        // Update roles
                        var currentRoles = await _userManager.GetRolesAsync(existingUser);
                        await _userManager.RemoveFromRolesAsync(existingUser, currentRoles);
                        
                        if (selectedRoles != null && selectedRoles.Any())
                        {
                            await _userManager.AddToRolesAsync(existingUser, selectedRoles);
                        }

                        var currentUserId = _userManager.GetUserId(User) ?? string.Empty;
                        await _auditService.LogAsync(
                            AuditAction.Update,
                            "User",
                            null,
                            currentUserId,
                            $"User updated: {existingUser.Email}"
                        );

                        TempData["SuccessMessage"] = "User updated successfully!";
                        return RedirectToAction(nameof(Index));
                    }

                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = $"Error updating user: {ex.Message}";
                }
            }

            var userRoles = await _userManager.GetRolesAsync(user);
            var allRoles = await _roleManager.Roles.ToListAsync();
            
            ViewBag.UserRoles = userRoles;
            ViewBag.AllRoles = allRoles;

            return View(user);
        }

        // GET: Users/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user != null)
                {
                    var result = await _userManager.DeleteAsync(user);
                    if (result.Succeeded)
                    {
                        var currentUserId = _userManager.GetUserId(User) ?? string.Empty;
                        await _auditService.LogAsync(
                            AuditAction.Delete,
                            "User",
                            null,
                            currentUserId,
                            $"User deleted: {user.Email}"
                        );

                        TempData["SuccessMessage"] = "User deleted successfully!";
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Error deleting user.";
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error deleting user: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Users/ResetPassword/5
        [HttpPost]
        public async Task<IActionResult> ResetPassword(string id, string newPassword)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user != null)
                {
                    var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                    var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
                    
                    if (result.Succeeded)
                    {
                        var currentUserId = _userManager.GetUserId(User) ?? string.Empty;
                        await _auditService.LogAsync(
                            AuditAction.Update,
                            "User",
                            null,
                            currentUserId,
                            $"Password reset for user: {user.Email}"
                        );

                        TempData["SuccessMessage"] = "Password reset successfully!";
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Error resetting password.";
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error resetting password: {ex.Message}";
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        // POST: Users/ToggleActive/5
        [HttpPost]
        public async Task<IActionResult> ToggleActive(string id)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user != null)
                {
                    user.IsActive = !user.IsActive;
                    var result = await _userManager.UpdateAsync(user);
                    
                    if (result.Succeeded)
                    {
                        var currentUserId = _userManager.GetUserId(User) ?? string.Empty;
                        await _auditService.LogAsync(
                            AuditAction.Update,
                            "User",
                            null,
                            currentUserId,
                            $"User {(user.IsActive ? "activated" : "deactivated")}: {user.Email}"
                        );

                        TempData["SuccessMessage"] = $"User {(user.IsActive ? "activated" : "deactivated")} successfully!";
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Error updating user status.";
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error updating user: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
