
using Microsoft.AspNetCore.Authorization; // For role-based access
using Microsoft.AspNetCore.Http; // For model binding with form files
using Microsoft.AspNetCore.Identity; // For UserManager
using Microsoft.AspNetCore.Mvc; // For MVC controller base
using Microsoft.EntityFrameworkCore; // For EF Core DB access
using System.Security.Claims; // For accessing logged-in user info
using Tanzania_Artifacts_MarketPlace_System_NEW_1_.Areas.Seller.SellerViewModel;
using Tanzania_Artifacts_MarketPlace_System_NEW_1_.Data; // Your DbContext
using Tanzania_Artifacts_MarketPlace_System_NEW_1_.Models; // Your models

namespace Tanzania_Artifacts_MarketPlace_System_NEW_1_.Areas.Seller.Controllers
{
 
    [Area("Seller")]
    [Authorize(Roles = "Seller")]
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _env;

        public ProductController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IWebHostEnvironment env)
        {
            _context = context;
            _userManager = userManager;
            _env = env;
        }

        // GET: Seller/Product
        public async Task<IActionResult> Index()
        {
            var sellerId = _userManager.GetUserId(User);
            var products = await _context.Products
                .Where(p => p.SellerId == sellerId)
                .Include(p => p.Images)
                .ToListAsync();

            return View(products);
        }

        // GET: Seller/Product/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Seller/Product/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SellerProductVM model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var sellerId = _userManager.GetUserId(User);

            var product = new Product
            {
                Name = model.ProductName,
                Description = model.Description,
                Price = model.Price,
                StockQuantity = model.StockQuantity,
                ProductHistory = model.ProductHistory,
                SellerId = sellerId,
                InStock = model.StockQuantity > 0,
                IsFeatured = model.IsFeatured  
            };

            if (model.Image != null)
            {
                // 🛠 Ensure folder exists
                string uploadsFolder = Path.Combine(_env.WebRootPath, "images/products");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(model.Image.FileName);
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fs = new FileStream(filePath, FileMode.Create))
                {
                    await model.Image.CopyToAsync(fs);
                }

                product.Images = new List<ProductImage>
        {
            new ProductImage
            {
                Url = "/images/products/" + uniqueFileName,
                OriginalFileName = model.Image.FileName,
                IsMain = true
            }
        };
            }

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }


        // GET: Seller/Product/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var sellerId = _userManager.GetUserId(User);
            var product = await _context.Products.Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Id == id && p.SellerId == sellerId);

            if (product == null)
                return NotFound();

            var vm = new SellerProductVM
            {
                ProductId = product.Id,
                ProductName = product.Name,
                Description = product.Description,
                Price = product.Price,
                StockQuantity = product.StockQuantity,
                ProductHistory = product.ProductHistory,
                ExistingImagePath = product.Images.FirstOrDefault()?.Url
            };

            return View(vm);
        }

        // POST: Seller/Product/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, SellerProductVM model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var sellerId = _userManager.GetUserId(User);
            var product = await _context.Products.Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Id == id && p.SellerId == sellerId);

            if (product == null)
                return NotFound();

            product.Name = model.ProductName;
            product.Description = model.Description;
            product.Price = model.Price;
            product.StockQuantity = model.StockQuantity;
            product.InStock = model.StockQuantity > 0;
            product.ProductHistory = model.ProductHistory;
            product.DateUpdated = DateTime.UtcNow;

            if (model.Image != null)
            {
                var oldImages = product.Images.ToList();

                foreach (var oldImage in oldImages)
                {
                    string oldPath = Path.Combine(_env.WebRootPath, oldImage.Url.TrimStart('/'));
                    if (System.IO.File.Exists(oldPath))
                        System.IO.File.Delete(oldPath);
                }

                _context.ProductImages.RemoveRange(oldImages);

                string uploadsFolder = Path.Combine(_env.WebRootPath, "images/products");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(model.Image.FileName);
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fs = new FileStream(filePath, FileMode.Create))
                {
                    await model.Image.CopyToAsync(fs);
                }

                product.Images = new List<ProductImage>
    {
        new ProductImage
        {
            Url = "/images/products/" + uniqueFileName,
            OriginalFileName = model.Image.FileName,
            IsMain = true
        }
    };
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Seller/Product/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var sellerId = _userManager.GetUserId(User);
            var product = await _context.Products
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Id == id && p.SellerId == sellerId);

            if (product == null)
                return NotFound();

            return View(product);
        }

        // POST: Seller/Product/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var sellerId = _userManager.GetUserId(User);
            var product = await _context.Products
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Id == id && p.SellerId == sellerId);

            if (product == null)
                return NotFound();

            // Delete image files
            foreach (var img in product.Images)
            {
                var filePath = Path.Combine(_env.WebRootPath, img.Url.TrimStart('/'));
                if (System.IO.File.Exists(filePath))
                    System.IO.File.Delete(filePath);
            }

            _context.ProductImages.RemoveRange(product.Images);
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
