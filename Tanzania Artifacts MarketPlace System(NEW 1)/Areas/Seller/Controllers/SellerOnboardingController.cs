using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // Add this line
using System.Security.Claims;
using Tanzania_Artifacts_MarketPlace_System_NEW_1_.Interfaces;

namespace Tanzania_Artifacts_MarketPlace_System_NEW_1_.Areas.Seller.Controllers
{
    [Area("Seller")]
    public class SellerOnboardingController : Controller
    {
        private readonly IEmailService _emailService;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly INotificationSender _notificationSender;
        private readonly INotificationRepository _notificationRepository;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public SellerOnboardingController(ApplicationDbContext context,
            UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager,
            INotificationSender notificationSender,
            INotificationRepository notificationRepository, SignInManager<ApplicationUser> signInManager, IEmailService emailService)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _notificationSender = notificationSender;
            _notificationRepository = notificationRepository;
            _signInManager = signInManager;
            _emailService = emailService;
        }

        [HttpGet]
        public async Task<IActionResult> Start()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var existingProfile = await _context.SellerProfiles
                .FirstOrDefaultAsync(p => p.UserId == userId);
            if (existingProfile != null)
                return RedirectToAction("Edit", "SellerProfile");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Start(SellerProfile model)
        {
            if (!ModelState.IsValid)
            {
                foreach (var modelError in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine($"Model Error: {modelError.ErrorMessage}");
                }
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            var existing = await _context.SellerProfiles
                .FirstOrDefaultAsync(p => p.UserId == user.Id);
            if (existing != null)
            {
                ModelState.AddModelError("", "You have already submitted an application.");
                return View(model);
            }

            try
            {
                // Save profile with IsApproved = false
                model.UserId = user.Id;
                model.CreatedAt = DateTime.UtcNow;
                model.IsApproved = false; // Pending admin approval

                _context.SellerProfiles.Add(model);
                await _context.SaveChangesAsync();

                // Notify admin about new seller application
                var admins = await _userManager.GetUsersInRoleAsync("Admin");
                foreach (var admin in admins)
                {
                    await _notificationSender.SendToUser(
                        admin.Id,
                        "New Seller Application",
                        $"A new seller application from {model.BusinessName} ({user.Email}) is awaiting approval."
                    );
                    await _emailService.SendEmailAsync(
                        admin.Email!,
                        "New Seller Application",
                        $"A new seller application from {model.BusinessName} ({user.Email}) is awaiting your approval. Please review in the admin dashboard."
                    );
                }

                // Redirect to Success action
                return RedirectToAction("Success");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in seller registration: {ex.Message}");
                ModelState.AddModelError("", "An error occurred while processing your request.");
                return View(model);
            }
        }


        [HttpGet]
        public IActionResult Success()
        {
            // ✅ Console log to confirm entry
            Console.WriteLine("✅ SellerOnboardingController → Success() called!");
            return View();
        }
    }
}
