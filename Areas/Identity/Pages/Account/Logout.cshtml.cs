using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using HospitalAssetTracker.Models;
using HospitalAssetTracker.Services;

namespace HospitalAssetTracker.Areas.Identity.Pages.Account
{
    public class LogoutModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<LogoutModel> _logger;
        private readonly IAuditService _auditService;

        public LogoutModel(SignInManager<ApplicationUser> signInManager, ILogger<LogoutModel> logger, IAuditService auditService)
        {
            _signInManager = signInManager;
            _logger = logger;
            _auditService = auditService;
        }

        public async Task<IActionResult> OnGet(string? returnUrl = null)
        {
            if (_signInManager.IsSignedIn(User))
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                await _signInManager.SignOutAsync();
                _logger.LogInformation("User logged out.");
                
                if (!string.IsNullOrEmpty(userId))
                {
                    await _auditService.LogAsync(AuditAction.Logout, "User", null, userId, "User logged out");
                }
            }
            
            if (returnUrl != null)
            {
                return LocalRedirect(returnUrl);
            }
            else
            {
                return Page();
            }
        }

        public async Task<IActionResult> OnPost(string? returnUrl = null)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            await _signInManager.SignOutAsync();
            _logger.LogInformation("User logged out.");
            
            if (!string.IsNullOrEmpty(userId))
            {
                await _auditService.LogAsync(AuditAction.Logout, "User", null, userId, "User logged out");
            }
            
            if (returnUrl != null)
            {
                return LocalRedirect(returnUrl);
            }
            else
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }
        }
    }
}
