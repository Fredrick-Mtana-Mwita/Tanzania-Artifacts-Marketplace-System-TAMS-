// Seller ProductController Image Upload Enhancements
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Tanzania_Artifacts_MarketPlace_System_NEW_1_.Data;
using Tanzania_Artifacts_MarketPlace_System_NEW_1_.Models;

namespace Tanzania_Artifacts_MarketPlace_System_NEW_1_.Seller.Controllers
{
    
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

        // List all products belonging to the logged-in seller
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

        // Render Create form
        public IActionResult Create()
        {
            return View("CreateEdit", new Product());
        }

        // Handle Create POST logic
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product, List<IFormFile> images)
        {
            if (ModelState.IsValid)
            {
                // Assign seller and defaults
                product.SellerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                product.DateCreated = DateTime.UtcNow;
                product.IsApproved = false;

                _context.Add(product);
                await _context.SaveChangesAsync();

                // Handle uploaded images
                if (images.Any())
                {
                    int order = 0;
                    foreach (var file in images)
                    {
                        var fileName = Path.GetFileNameWithoutExtension(file.FileName);
                        var ext = Path.GetExtension(file.FileName);
                        var uniqueName = $"{Guid.NewGuid()}{ext}";
                        var savePath = Path.Combine(_env.WebRootPath, "uploads", "products", uniqueName);

                        Directory.CreateDirectory(Path.GetDirectoryName(savePath)!);

                        using var stream = new FileStream(savePath, FileMode.Create);
                        await file.CopyToAsync(stream);

                        var image = new ProductImage
                        {
                            Url = $"/uploads/products/{uniqueName}",
                            OriginalFileName = file.FileName,
                            IsMain = order == 0,
                            ProductId = product.Id,
                            DisplayOrder = order++,
                            CreatedAt = DateTime.UtcNow,
                            AltText = product.Name
                        };

                        _context.ProductImages.Add(image);
                    }
                    await _context.SaveChangesAsync();
                }

                TempData["Success"] = "Product submitted successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View("CreateEdit", product);
        }

        // Render Edit form for a seller's product
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var product = await _context.Products
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null || product.SellerId != User.FindFirstValue(ClaimTypes.NameIdentifier))
                return Unauthorized();

            return View("CreateEdit", product);
        }

        // Handle Edit POST logic
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Product updated, List<IFormFile> images)
        {
            if (id != updated.Id) return NotFound();

            var product = await _context.Products
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null || product.SellerId != User.FindFirstValue(ClaimTypes.NameIdentifier))
                return Unauthorized();

            if (ModelState.IsValid)
            {
                // Update product fields
                product.Name = updated.Name;
                product.Description = updated.Description;
                product.ProductHistory = updated.ProductHistory;
                product.Price = updated.Price;
                product.Quantity = updated.Quantity;
                product.IsFeatured = updated.IsFeatured;
                product.DateUpdated = DateTime.UtcNow;

                // Handle new uploaded images
                if (images.Any())
                {
                    int order = product.Images.Count;
                    foreach (var file in images)
                    {
                        var fileName = Path.GetFileNameWithoutExtension(file.FileName);
                        var ext = Path.GetExtension(file.FileName);
                        var uniqueName = $"{Guid.NewGuid()}{ext}";
                        var savePath = Path.Combine(_env.WebRootPath, "uploads", "products", uniqueName);

                        Directory.CreateDirectory(Path.GetDirectoryName(savePath)!);

                        using var stream = new FileStream(savePath, FileMode.Create);
                        await file.CopyToAsync(stream);

                        var image = new ProductImage
                        {
                            Url = $"/uploads/products/{uniqueName}",
                            OriginalFileName = file.FileName,
                            IsMain = false,
                            ProductId = product.Id,
                            DisplayOrder = order++,
                            CreatedAt = DateTime.UtcNow,
                            AltText = product.Name
                        };
                        _context.ProductImages.Add(image);
                    }
                }

                await _context.SaveChangesAsync();
                TempData["Success"] = "Product updated!";
                return RedirectToAction(nameof(Index));
            }

            return View("CreateEdit", updated);
        }

        // Render Delete confirmation view
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var product = await _context.Products
                .Include(p => p.Images)
                .FirstOrDefaultAsync(m => m.Id == id && m.SellerId == User.FindFirstValue(ClaimTypes.NameIdentifier));

            if (product == null) return NotFound();

            return View(product);
        }

        // Handle product deletion
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Id == id && p.SellerId == User.FindFirstValue(ClaimTypes.NameIdentifier));

            if (product != null)
            {
                // Delete associated image files
                foreach (var img in product.Images)
                {
                    var path = Path.Combine(_env.WebRootPath, img.Url.TrimStart('/'));
                    if (System.IO.File.Exists(path))
                        System.IO.File.Delete(path);
                }

                // Remove image records and product
                _context.ProductImages.RemoveRange(product.Images);
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Product deleted.";
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Seller/Product/Gallery/{id}
        public async Task<IActionResult> Gallery(int id)
        {
            var sellerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var product = await _context.Products
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Id == id && p.SellerId == sellerId);

            if (product == null)
                return NotFound();

            return View(product);
        }

        // POST: Seller/Product/DeleteImage
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteImage(int imageId)
        {
            var sellerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var image = await _context.ProductImages
                .Include(i => i.Product)
                .FirstOrDefaultAsync(i => i.Id == imageId && i.Product.SellerId == sellerId);

            if (image == null)
                return NotFound();

            var imagePath = Path.Combine(_env.WebRootPath, image.Url.TrimStart('/'));
            if (System.IO.File.Exists(imagePath))
                System.IO.File.Delete(imagePath);

            _context.ProductImages.Remove(image);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Image deleted.";
            return RedirectToAction("Gallery", new { id = image.ProductId });
        }

        // POST: Seller/Product/UploadImages
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadImages(int productId, List<IFormFile> images)
        {
            var sellerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var product = await _context.Products
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Id == productId && p.SellerId == sellerId);

            if (product == null)
                return NotFound();

            int order = product.Images.Count;

            foreach (var file in images)
            {
                var ext = Path.GetExtension(file.FileName);
                var uniqueName = $"{Guid.NewGuid()}{ext}";
                var savePath = Path.Combine(_env.WebRootPath, "uploads", "products", uniqueName);

                Directory.CreateDirectory(Path.GetDirectoryName(savePath)!);

                using var stream = new FileStream(savePath, FileMode.Create);
                await file.CopyToAsync(stream);

                _context.ProductImages.Add(new ProductImage
                {
                    Url = $"/uploads/products/{uniqueName}",
                    ProductId = product.Id,
                    IsMain = false,
                    OriginalFileName = file.FileName,
                    CreatedAt = DateTime.UtcNow,
                    DisplayOrder = order++,
                    AltText = product.Name
                });
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "Images uploaded.";
            return RedirectToAction("Gallery", new { id = productId });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetMainImage(int imageId)
        {
            var sellerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var image = await _context.ProductImages
                .Include(i => i.Product)
                .Where(i => i.Id == imageId && i.Product.SellerId == sellerId)
                .FirstOrDefaultAsync();

            if (image == null)
                return NotFound();

            // Unset all others for the product
            var productImages = _context.ProductImages.Where(p => p.ProductId == image.ProductId);
            foreach (var img in productImages)
            {
                img.IsMain = false;
            }

            image.IsMain = true;

            await _context.SaveChangesAsync();
            TempData["Success"] = "Main image updated.";
            return RedirectToAction("Gallery", new { id = image.ProductId });
        }
        [HttpPost]
        public async Task<IActionResult> ReorderImage(int imageId, int newOrder)
        {
            var image = await _context.ProductImages
                .Include(i => i.Product)
                .FirstOrDefaultAsync(i => i.Id == imageId);

            if (image == null || image.Product.SellerId != User.FindFirstValue(ClaimTypes.NameIdentifier))
                return Unauthorized();

            image.DisplayOrder = newOrder;
            await _context.SaveChangesAsync();

            return Ok();
        }

    }
}
