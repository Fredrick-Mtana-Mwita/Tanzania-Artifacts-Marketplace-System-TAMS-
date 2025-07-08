using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Tanzania_Artifacts_MarketPlace_System_NEW_1_.Interfaces;

namespace Tanzania_Artifacts_MarketPlace_System_NEW_1_.Areas.Seller.Controllers
{
    [Area("Seller")]
    public class SellerOnboardingController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly INotificationSender _notificationSender;
        private readonly INotificationRepository _notificationRepository;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public SellerOnboardingController(ApplicationDbContext context,
            UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager,
            INotificationSender notificationSender,
            INotificationRepository notificationRepository, SignInManager<ApplicationUser> signInManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _notificationSender = notificationSender;
            _notificationRepository = notificationRepository;
            _signInManager = signInManager;
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
                return View(model);

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            var existing = await _context.SellerProfiles.FirstOrDefaultAsync(p => p.UserId == user.Id);
            if (existing != null)
            {
                ModelState.AddModelError("", "You have already submitted an application.");
                return View(model);
            }

            // ⚠️ Remove 'User' role if present
            var roles = await _userManager.GetRolesAsync(user);
            if (roles.Contains("User"))
                await _userManager.RemoveFromRoleAsync(user, "User");

            // ✅ Assign 'Seller' role
            if (!roles.Contains("Seller"))
                await _userManager.AddToRoleAsync(user, "Seller");

            // ✅ Update Role enum
            user.Role = Roles.Seller;
            await _userManager.UpdateAsync(user);

            // ✅ Save profile
            model.UserId = user.Id;
            model.CreatedAt = DateTime.UtcNow;
            model.IsApproved = false;

            _context.SellerProfiles.Add(model);
            await _context.SaveChangesAsync();

            // ✅ Notify Admins
            var admins = await _userManager.GetUsersInRoleAsync("Admin");
            foreach (var admin in admins)
            {
                var notification = new Notification
                {
                    UserId = admin.Id,
                    Title = "New Seller Application",
                    Message = $"{user.FirstName} {user.LastName} has applied to become a seller.",
                    CreatedAt = DateTime.UtcNow,
                    IsRead = false
                };
                await _notificationRepository.AddAsync(notification);
            }

            await _notificationSender.SendToAdmin("New Seller Application", $"{user.FirstName} {user.LastName} has applied.");

            await _signInManager.SignOutAsync();
            await _signInManager.SignInAsync(user, isPersistent: false);

            // ✅ Redirect to success view
            return RedirectToAction("Success", "SellerOnboarding", new { area = "Seller" });
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
