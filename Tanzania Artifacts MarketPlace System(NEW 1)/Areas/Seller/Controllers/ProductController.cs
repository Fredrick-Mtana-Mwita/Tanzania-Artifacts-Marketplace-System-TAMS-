
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
    [Area("Seller")] // Indicates this controller belongs to Seller area
    [Authorize(Roles = "Seller")] // Restrict to users in Seller role
    public class ProductController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _env;

        public ProductController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment env,
            IProductRepository productRepository)
        {
            _context = context;
            _userManager = userManager;
            _env = env;
            _productRepository = productRepository;
        }

        // ✅ List all products by this seller
        public async Task<IActionResult> Index()
        {
            var sellerId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var products = await _context.Products
                .Include(p => p.Images)
                .Where(p => p.SellerId == sellerId)
                .OrderByDescending(p => p.DateCreated)
                .ToListAsync();

            return View(products);
        }

        // ✅ Render the create form
        // ✅ new
        public IActionResult Create()
        {
            ViewData["FormAction"] = "Create";
            return View("Create", new SellerProductVM());
        }


        // ✅ POST: Handle product creation 
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SellerProductVM model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                TempData["Error"] = "Validation failed: " + string.Join(" | ", errors);
                return View(model);
            }

            try
            {
                // ✅ Check if user is logged in and has a valid SellerId
                var sellerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(sellerId))
                {
                    TempData["Error"] = "Seller ID is not found. Are you logged in?";
                    return View(model);
                }

                var product = new Product
                {
                    Name = model.ProductName!,
                    Description = model.Description!,
                    Price = model.Price,
                    StockQuantity = model.StockQuantity,
                    Quantity = model.StockQuantity,
                    SellerId = sellerId,
                    DateCreated = DateTime.UtcNow,
                    IsFeatured = model.IsFeatured,
                    IsApproved = false,
                    ProductHistory = model.ProductHistory ?? ""
                };

                _context.Products.Add(product);
                await _context.SaveChangesAsync();

                // ✅ Handle image upload
                if (model.Image != null && model.Image.Length > 0)
                {
                    var ext = Path.GetExtension(model.Image.FileName);
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };

                    if (!allowedExtensions.Contains(ext.ToLower()))
                    {
                        TempData["Error"] = "Invalid image format. Only JPG, PNG, GIF, and WEBP are allowed.";
                        return View(model);
                    }

                    if (model.Image.Length > 5 * 1024 * 1024)
                    {
                        TempData["Error"] = "Image file is too large. Max allowed size is 5MB.";
                        return View(model);
                    }

                    var uniqueName = $"{Guid.NewGuid()}{ext}";
                    var folderPath = Path.Combine(_env.WebRootPath, "uploads", "products");
                    Directory.CreateDirectory(folderPath);
                    var fullPath = Path.Combine(folderPath, uniqueName);

                    using (var stream = new FileStream(fullPath, FileMode.Create))
                    {
                        await model.Image.CopyToAsync(stream);
                    }

                    var productImage = new ProductImage
                    {
                        ProductId = product.Id,
                        Url = $"/uploads/products/{uniqueName}",
                        OriginalFileName = model.Image.FileName,
                        CreatedAt = DateTime.UtcNow,
                        DisplayOrder = 0,
                        AltText = product.Name,
                        IsMain = true
                    };

                    _context.ProductImages.Add(productImage);
                    await _context.SaveChangesAsync();
                }

                TempData["Success"] = "Product submitted successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Unexpected error occurred: " + ex.Message;
                return View(model);
            }
        }

        // ✅ Render edit form
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var product = await _context.Products.Include(p => p.Images).FirstOrDefaultAsync(p => p.Id == id);
            if (product == null || product.SellerId != User.FindFirstValue(ClaimTypes.NameIdentifier))
                return Unauthorized();

            ViewData["FormAction"] = "Edit";
            return View("CreateEdit", product);
        }

        // ✅ POST: Handle product update
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Product updated, List<IFormFile> images)
        {
            if (id != updated.Id) return NotFound();

            var product = await _context.Products.Include(p => p.Images).FirstOrDefaultAsync(p => p.Id == id);
            if (product == null || product.SellerId != User.FindFirstValue(ClaimTypes.NameIdentifier))
                return Unauthorized();

            if (!ModelState.IsValid)
                return View("CreateEdit", updated);

            // Update fields
            product.Name = updated.Name;
            product.Description = updated.Description;
            product.ProductHistory = updated.ProductHistory;
            product.Price = updated.Price;
            product.Quantity = updated.Quantity;
            product.IsFeatured = updated.IsFeatured;
            product.DateUpdated = DateTime.UtcNow;

            await SaveImages(images, product);

            await _context.SaveChangesAsync();
            TempData["Success"] = "Product updated!";
            return RedirectToAction(nameof(Index));
        }

        // ✅ Centralized image saving logic
        private async Task SaveImages(List<IFormFile> images, Product product)
        {
            if (images == null || !images.Any()) return;

            int order = product.Images?.Count ?? 0;

            foreach (var file in images)
            {
                if (file.Length > 5242880) continue; // Skip files > 5MB
                var ext = Path.GetExtension(file.FileName);
                var uniqueName = $"{Guid.NewGuid()}{ext}";
                var savePath = Path.Combine(_env.WebRootPath, "uploads", "products", uniqueName);

                Directory.CreateDirectory(Path.GetDirectoryName(savePath)!);
                using var stream = new FileStream(savePath, FileMode.Create);
                await file.CopyToAsync(stream);

                _context.ProductImages.Add(new ProductImage
                {
                    Url = $"/uploads/products/{uniqueName}",
                    OriginalFileName = file.FileName,
                    ProductId = product.Id,
                    CreatedAt = DateTime.UtcNow,
                    DisplayOrder = order++,
                    AltText = product.Name,
                    IsMain = order == 0
                });
            }

            await _context.SaveChangesAsync();
        }

        // ✅ Update stock quantity for seller’s product
        [HttpPost]
        public async Task<IActionResult> UpdateStock(int productId, int stockQuantity)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var product = await _productRepository.GetByIdAsync(productId);

            if (product == null || product.SellerId != userId)
                return NotFound();

            product.StockQuantity = stockQuantity;
            await _productRepository.UpdateAsync(product);
            return RedirectToAction("MyProducts");
        }

        // ✅ Confirm delete view
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var product = await _context.Products.Include(p => p.Images).FirstOrDefaultAsync(m => m.Id == id && m.SellerId == User.FindFirstValue(ClaimTypes.NameIdentifier));
            if (product == null) return NotFound();

            return View(product);
        }

        // ✅ Handle confirmed product delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.Include(p => p.Images).FirstOrDefaultAsync(p => p.Id == id && p.SellerId == User.FindFirstValue(ClaimTypes.NameIdentifier));
            if (product != null)
            {
                foreach (var img in product.Images)
                {
                    var path = Path.Combine(_env.WebRootPath, img.Url.TrimStart('/'));
                    if (System.IO.File.Exists(path))
                        System.IO.File.Delete(path);
                }
                _context.ProductImages.RemoveRange(product.Images);
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Product deleted.";
            }
            return RedirectToAction(nameof(Index));
        }

        // ✅ View image gallery for a product
        public async Task<IActionResult> Gallery(int id)
        {
            var sellerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var product = await _context.Products.Include(p => p.Images).FirstOrDefaultAsync(p => p.Id == id && p.SellerId == sellerId);
            if (product == null) return NotFound();
            return View(product);
        }

        // ✅ Delete a product image
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteImage(int imageId)
        {
            var sellerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var image = await _context.ProductImages.Include(i => i.Product).FirstOrDefaultAsync(i => i.Id == imageId && i.Product.SellerId == sellerId);

            if (image == null) return NotFound();

            var imagePath = Path.Combine(_env.WebRootPath, image.Url.TrimStart('/'));
            if (System.IO.File.Exists(imagePath))
                System.IO.File.Delete(imagePath);

            _context.ProductImages.Remove(image);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Image deleted.";
            return RedirectToAction("Gallery", new { id = image.ProductId });
        }

        // ✅ Upload more images to a product
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadImages(int productId, List<IFormFile> images)
        {
            var sellerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var product = await _context.Products.Include(p => p.Images).FirstOrDefaultAsync(p => p.Id == productId && p.SellerId == sellerId);
            if (product == null) return NotFound();

            await SaveImages(images, product);
            TempData["Success"] = "Images uploaded.";
            return RedirectToAction("Gallery", new { id = productId });
        }

        // ✅ Set an image as main for a product
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetMainImage(int imageId)
        {
            var sellerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var image = await _context.ProductImages.Include(i => i.Product).Where(i => i.Id == imageId && i.Product.SellerId == sellerId).FirstOrDefaultAsync();
            if (image == null) return NotFound();

            var productImages = _context.ProductImages.Where(p => p.ProductId == image.ProductId);
            foreach (var img in productImages) img.IsMain = false;

            image.IsMain = true;
            await _context.SaveChangesAsync();
            TempData["Success"] = "Main image updated.";
            return RedirectToAction("Gallery", new { id = image.ProductId });
        }

        // ✅ Change display order of an image
        [HttpPost]
        public async Task<IActionResult> ReorderImage(int imageId, int newOrder)
        {
            var image = await _context.ProductImages.Include(i => i.Product).FirstOrDefaultAsync(i => i.Id == imageId);
            if (image == null || image.Product.SellerId != User.FindFirstValue(ClaimTypes.NameIdentifier)) return Unauthorized();

            image.DisplayOrder = newOrder;
            await _context.SaveChangesAsync();
            return Ok();
        }

        // ✅ Show products owned by the logged-in seller (for MyProducts.cshtml)
        public async Task<IActionResult> MyProducts()
        {
            var sellerId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var products = await _context.Products
                .Include(p => p.Images)
                .Where(p => p.SellerId == sellerId)
                .OrderByDescending(p => p.DateCreated)
                .ToListAsync();

            // Set main image URL for display convenience
            foreach (var product in products)
            {
                var mainImage = product.Images.FirstOrDefault(i => i.IsMain);
                product.ProductImage = mainImage?.Url ?? "/images/placeholder.png"; // fallback
            }

            return View(products); // Will map to /Areas/Seller/Views/Product/MyProducts.cshtml
        }
    }
}
