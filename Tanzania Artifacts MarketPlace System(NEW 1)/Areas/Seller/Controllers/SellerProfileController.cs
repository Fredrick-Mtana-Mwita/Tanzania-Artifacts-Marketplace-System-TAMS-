using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Tanzania_Artifacts_MarketPlace_System_NEW_1_.Areas.Seller.SellerViewModel;



namespace Tanzania_Artifacts_MarketPlace_System_NEW_1_.Areas.Seller.Controllers
{
    [Area("Seller")]
    [Authorize(Roles = "Seller")]
    public class SellerProfileController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _env;

        public SellerProfileController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IWebHostEnvironment env)
        {
            _context = context;
            _userManager = userManager;
            _env = env;
        }

        // GET: SellerProfile/Details
        public async Task<IActionResult> Details()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var profile = await _context.SellerProfiles
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (profile == null) return RedirectToAction(nameof(Create));

            return View(profile);
        }

        // GET: SellerProfile/Create
        public IActionResult Create()
        {
            return View(new SellerProfileViewModel());
        }

        // POST: SellerProfile/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SellerProfileViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var profile = new SellerProfile
            {
                UserId = userId!,
                BusinessName = vm.BusinessName,
                TaxId = vm.TaxId,
                StoreDescription = vm.StoreDescription,
                BankAccountInfo = vm.BankAccountInfo,
                ContactEmail = vm.ContactEmail,
                PhoneNumber = vm.PhoneNumber,
                CreatedAt = DateTime.UtcNow
            };

            if (vm.StoreLogo != null)
                profile.StoreLogoPath = await SaveFileAsync(vm.StoreLogo, "logos");

            if (vm.StoreBanner != null)
                profile.StoreBannerPath = await SaveFileAsync(vm.StoreBanner, "banners");

            _context.SellerProfiles.Add(profile);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Details));
        }

        // GET: SellerProfile/Edit
        public async Task<IActionResult> Edit()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var profile = await _context.SellerProfiles.FirstOrDefaultAsync(p => p.UserId == userId);

            if (profile == null) return RedirectToAction(nameof(Create));

            var vm = new SellerProfileViewModel
            {
                BusinessName = profile.BusinessName,
                TaxId = profile.TaxId,
                StoreDescription = profile.StoreDescription,
                BankAccountInfo = profile.BankAccountInfo,
                ContactEmail = profile.ContactEmail,
                PhoneNumber = profile.PhoneNumber,
                ExistingStoreLogoPath = profile.StoreLogoPath,
                ExistingStoreBannerPath = profile.StoreBannerPath
            };

            return View(vm);
        }

        // POST: SellerProfile/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(SellerProfileViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var profile = await _context.SellerProfiles.FirstOrDefaultAsync(p => p.UserId == userId);

            if (profile == null) return NotFound();

            // Update properties
            profile.BusinessName = vm.BusinessName;
            profile.TaxId = vm.TaxId;
            profile.StoreDescription = vm.StoreDescription;
            profile.BankAccountInfo = vm.BankAccountInfo;
            profile.ContactEmail = vm.ContactEmail;
            profile.PhoneNumber = vm.PhoneNumber;
            profile.UpdatedAt = DateTime.UtcNow;

            // Handle file uploads
            if (vm.StoreLogo != null)
                profile.StoreLogoPath = await SaveFileAsync(vm.StoreLogo, "logos");

            if (vm.StoreBanner != null)
                profile.StoreBannerPath = await SaveFileAsync(vm.StoreBanner, "banners");

            _context.Update(profile);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Details));
        }

        // Utility: Save uploaded file to wwwroot/uploads/{folder}/filename
        private async Task<string> SaveFileAsync(IFormFile file, string folderName)
        {
            var uploadsRoot = Path.Combine(_env.WebRootPath, "uploads", folderName);
            Directory.CreateDirectory(uploadsRoot);

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            var fullPath = Path.Combine(uploadsRoot, fileName);

            using var stream = new FileStream(fullPath, FileMode.Create);
            await file.CopyToAsync(stream);

            return $"/uploads/{folderName}/{fileName}";
        }
    }
}
