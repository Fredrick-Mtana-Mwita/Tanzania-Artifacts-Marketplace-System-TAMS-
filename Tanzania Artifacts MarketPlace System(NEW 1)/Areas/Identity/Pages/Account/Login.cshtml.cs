using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Tanzania_Artifacts_MarketPlace_System_NEW_1_.Models;

namespace Tanzania_Artifacts_MarketPlace_System_NEW_1_.Areas.Identity.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<LoginModel> _logger;

        public LoginModel(SignInManager<ApplicationUser> signInManager, ILogger<LoginModel> logger)
        {
            _signInManager = signInManager;
            _logger = logger;

            // Initialize non-nullable properties
            ReturnUrl = string.Empty;
            ErrorMessage = string.Empty;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public IList<AuthenticationScheme> ExternalLogins { get; set; } = new List<AuthenticationScheme>();

        public string ReturnUrl { get; set; } 

        [TempData]
        public string ErrorMessage { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string? Email { get; set; }

            [Required]
            [DataType(DataType.Password)]
            public string? Password { get; set; }

            [Display(Name = "Remember me?")]
            public bool RememberMe { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null!)
        {
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                // Show TempData error (from redirect or failed attempt)
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

            returnUrl ??= Url.Content("~/");

            // Clear external login session
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            // Get external login providers
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null!)
        {
            returnUrl ??= Url.Content("~/");

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (ModelState.IsValid)
            {
                // 🔍 Find user
                var user = await _signInManager.UserManager.FindByEmailAsync(Input.Email!);

                if (user != null)
                {
                    // ⛔ Inactive users
                    if (!user.IsActive)
                    {
                        ModelState.AddModelError(string.Empty, "Your account is inactive. Please contact the administrator.");
                        return Page();
                    }

                    // 📧 Email not confirmed
                    if (!await _signInManager.UserManager.IsEmailConfirmedAsync(user))
                    {
                        ModelState.AddModelError(string.Empty, "You must confirm your email before logging in.");
                        return Page();
                    }
                }

                // ✅ Attempt login
                var result = await _signInManager.PasswordSignInAsync(Input.Email!, Input.Password!, Input.RememberMe, lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    // 👣 Refresh user session
                    await _signInManager.SignOutAsync();
                    await _signInManager.SignInAsync(user!, isPersistent: Input.RememberMe);

                    _logger.LogInformation("Redirecting user with role: {role}", user!.Role);

                    switch (user!.Role)
                    {
                        case Roles.Admin:
                            _logger.LogInformation("Redirecting to Admin Dashboard");
                            return LocalRedirect("/Admin/AdminDashboard");

                        case Roles.Seller:
                            _logger.LogInformation("Redirecting to SellerDashboard in Seller Area");
                            return RedirectToAction("SellerDashboard", "SellerDashboard", new { area = "Seller" });

                        default:
                            _logger.LogInformation("Redirecting to Home/Index");
                            return LocalRedirect("/Home/Index");
                    }

                }

                if (result.RequiresTwoFactor)
                {
                    return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, RememberMe = Input.RememberMe });
                }

                if (result.IsLockedOut)
                {
                    _logger.LogWarning("User account locked out.");
                    return RedirectToPage("./Lockout");
                }

                // ❌ Fallback for invalid attempts
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return Page();
            }

            // ❌ Model validation failed
            return Page();
        }
    }
}
