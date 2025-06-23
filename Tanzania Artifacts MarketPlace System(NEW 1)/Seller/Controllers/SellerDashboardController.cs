using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tanzania_Artifacts_MarketPlace_System_NEW_1_.Interfaces;
using Tanzania_Artifacts_MarketPlace_System_NEW_1_.Seller.SellerViewModel;
using Tanzania_Artifacts_MarketPlace_System_NEW_1_.Services;

namespace Tanzania_Artifacts_MarketPlace_System_NEW_1_.Seller.Controllers
{
    [Authorize(Roles = "Seller")]

    public class SellerDashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly INotificationRepository _notificationRepository;
        private readonly IProductRepository _productRepository;
        private readonly IWebHostEnvironment _env;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly INotificationSender _notificationSender;

        public SellerDashboardController(
            IProductRepository productRepository,
            IWebHostEnvironment env,
            UserManager<ApplicationUser> userManager,
            INotificationRepository notificationRepository,
            INotificationSender notificationSender,
            ApplicationDbContext context)
        {
            _productRepository = productRepository;
            _env = env;
            _userManager = userManager;
            _notificationRepository = notificationRepository;
            _notificationSender = notificationSender;
            _context = context;
        }

        public async Task<IActionResult> SellerDashboard()
        {
            var sellerId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var sellerOrderItems = await _context.OrderItems
                .Include(oi => oi.Product)
                .Include(oi => oi.Order)
                .Where(oi => oi.Product.SellerId == sellerId)
                .ToListAsync();

            var totalOrders = sellerOrderItems
                .Select(oi => oi.OrderId)
                .Distinct()
                .Count();

            var totalRevenue = sellerOrderItems
                .Where(oi => oi.Order.Status == OrderStatus.Delivered)
                .Sum(oi => oi.Quantity * oi.UnitPrice);

            var totalProductsSold = sellerOrderItems
                .Where(oi => oi.Order.Status == OrderStatus.Delivered)
                .Sum(oi => oi.Quantity);

            var recentOrders = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.Items)
                .ThenInclude(i => i.Product)
                .Where(o => o.Items.Any(i => i.Product.SellerId == sellerId))
                .OrderByDescending(o => o.OrderDate)
                .Take(5)
                .ToListAsync();

            // Prepare data for charts
            var chartData = await _context.Orders
                .Where(o => o.Items.Any(i => i.Product.SellerId == sellerId))
                .GroupBy(o => o.OrderDate.Month)
                .Select(g => new { Month = g.Key, Count = g.Count() })
                .ToListAsync();

            ViewBag.ChartData = chartData;

            var viewModel = new SellerDashboardVM
            {
                TotalOrders = totalOrders,
                TotalRevenue = totalRevenue,
                TotalProductsSold = totalProductsSold,
                RecentOrders = recentOrders
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> CreateProduct(SellerProductVM model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            string imagePath = "/images/products/default.png";

            if (model.Image != null && model.Image.Length > 0)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(model.Image.FileName);
                var fullPath = Path.Combine(_env.WebRootPath, "images/products", fileName);

                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await model.Image.CopyToAsync(stream);
                }

                imagePath = "/images/products/" + fileName;
            }

            var product = new Product
            {
                Name = model.ProductName!,
                Description = model.Description!,
                Price = model.Price,
                ProductImage = imagePath,
                SellerId = user.Id,
                DateCreated = DateTime.UtcNow,
                IsApproved = false
            };

            await _productRepository.CreateAsync(product);

            var admins = await _userManager.GetUsersInRoleAsync("Admin");
            foreach (var admin in admins)
            {
                var notification = new Notification
                {
                    UserId = admin.Id,
                    Title = "New Product Submission",
                    Message = $"{user.FirstName} {user.LastName} has submitted a new product: <strong>{model.ProductName}</strong>.",
                    CreatedAt = DateTime.UtcNow,
                    IsRead = false
                };
                await _notificationRepository.AddAsync(notification);
            }

            await _notificationSender.SendToAdmin("New Product Submission", $"{user.FirstName} submitted '{product.Name}' for approval.");

            return RedirectToAction("MyProducts");
        }
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

        public async Task<IActionResult> MyProducts()
        {
            var sellerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var products = await _context.Products
                .Where(p => p.SellerId == sellerId)
                .ToListAsync();

            return View(products);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id && p.SellerId == userId);

            if (product == null)
                return NotFound();

            var model = new SellerProductVM
            {
                ProductId = product.Id,
                ProductName = product.Name,
                Description = product.Description,
                Price = product.Price,
                ExistingImagePath = product.ProductImage
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(SellerProductVM model)
        {
            if (!ModelState.IsValid) return View(model);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == model.ProductId && p.SellerId == userId);

            if (product == null)
                return NotFound();

            product.Name = model.ProductName!;
            product.Description = model.Description!;
            product.Price = model.Price;

            if (model.Image != null && model.Image.Length > 0)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(model.Image.FileName);
                var fullPath = Path.Combine(_env.WebRootPath, "images/products", fileName);

                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await model.Image.CopyToAsync(stream);
                }

                product.ProductImage = "/images/products/" + fileName;
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("MyProducts");
        }
        public async Task<IActionResult> Delete(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id && p.SellerId == userId);

            if (product == null)
                return NotFound();

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Product deleted successfully!";
            return RedirectToAction("MyProducts");
        }

    }
}
