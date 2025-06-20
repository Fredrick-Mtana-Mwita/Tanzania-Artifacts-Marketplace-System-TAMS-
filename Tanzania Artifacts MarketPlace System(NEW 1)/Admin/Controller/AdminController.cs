
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tanzania_Artifacts_MarketPlace_System_NEW_1_.Admin.ViewModel;
using Tanzania_Artifacts_MarketPlace_System_NEW_1_.Data;
using Tanzania_Artifacts_MarketPlace_System_NEW_1_.Interfaces;
using Tanzania_Artifacts_MarketPlace_System_NEW_1_.Models;
using Tanzania_Artifacts_MarketPlace_System_NEW_1_.Services;

namespace Tanzania_Artifacts_MarketPlace_System_NEW_1_.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ICartRepository _cartRepository;
        private readonly IProductRepository _productRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailService _emailService;

        public AdminController(ApplicationDbContext context,
            ICartRepository cartRepository,
            IProductRepository productRepository,
            IOrderRepository orderRepository,
            UserManager<ApplicationUser> userManager,
            IEmailService emailService)
        {
            _context = context;
            _cartRepository = cartRepository;
            _productRepository = productRepository;
            _orderRepository = orderRepository;
            _userManager = userManager;
            _emailService = emailService;
        }

        public async Task<IActionResult> AdminDashboard()
        {
            var users = await _userManager.Users.ToListAsync();
            var orders = await _orderRepository.GetAllAsync();

            var vm = new AdminDashboardVM
            {
                TotalUsers = users.Count(u => u.Role == Roles.User),
                TotalSellers = users.Count(u => u.Role == Roles.Seller),
                TotalOrders = orders.Count(),
                TotalSales = orders.Where(o => !o.IsCancelled).Sum(o => o.TotalAmount),
                PendingOrders = orders.Count(o => o.Status == OrderStatus.Pending),
                CompletedOrders = orders.Count(o => o.Status == OrderStatus.Delivered),
                RecentOrders = orders.OrderByDescending(o => o.OrderDate).Take(5).ToList()
            };

            return View(vm);
        }

        public async Task<IActionResult> AdminOrders() => View(await _orderRepository.GetAllAsync());

        public async Task<IActionResult> EditOrder(int id)
        {
            var order = await _orderRepository.GetByIdAsync(id);
            return order == null ? NotFound() : View(order);
        }

        [HttpPost]
        public async Task<IActionResult> EditOrder(int id, OrderStatus status)
        {
            var order = await _orderRepository.GetByIdAsync(id);
            if (order == null) return NotFound();

            order.Status = status;
            if (status == OrderStatus.Delivered)
                order.DeliveredDate = DateTime.UtcNow;

            await _orderRepository.UpdateAsync(order);

            await _emailService.SendEmailAsync(order.User.Email!, "Order Status Updated", $"Your order #{order.Id} is now {status}.");

            return RedirectToAction("AdminOrders");
        }

        [HttpPost]
        public async Task<IActionResult> CancelOrder(int id)
        {
            var order = await _orderRepository.GetByIdAsync(id);
            if (order == null || order.IsCancelled) return NotFound();

            order.IsCancelled = true;
            order.Status = OrderStatus.Cancelled;
            order.CancelledDate = DateTime.UtcNow;

            await _orderRepository.UpdateAsync(order);
            await _emailService.SendEmailAsync(order.User.Email!, "Order Cancelled", $"Your order #{order.Id} has been cancelled.");

            return RedirectToAction("AdminOrders");
        }

        [HttpPost]
        public async Task<IActionResult> MarkAsShipped(int id)
        {
            var order = await _orderRepository.GetByIdAsync(id);
            if (order == null) return NotFound();

            order.Status = OrderStatus.Shipped;
            order.ShippedDate = DateTime.UtcNow;

            await _orderRepository.UpdateAsync(order);
            await _emailService.SendEmailAsync(order.User.Email!, "Order Shipped", $"Your order #{order.Id} has been shipped.");

            return RedirectToAction("AdminOrders");
        }

        [HttpPost]
        public async Task<IActionResult> MarkAsDelivered(int id)
        {
            var order = await _orderRepository.GetByIdAsync(id);
            if (order == null || order.IsCancelled || order.Status != OrderStatus.Shipped)
                return NotFound();

            order.Status = OrderStatus.Delivered;
            order.DeliveredDate = DateTime.UtcNow;

            await _orderRepository.UpdateAsync(order);
            await _emailService.SendEmailAsync(order.User.Email!, "Order Delivered", $"Your order #{order.Id} has been delivered. Thank you!");

            return RedirectToAction("AdminOrders");
        }

        public async Task<IActionResult> ManageUser()
        {
            var users = await _userManager.Users.ToListAsync();

            var vm = users.Select(u => new ManageUserVM
            {
                Id = u.Id,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Email = u.Email,
                Role = u.Role
            }).ToList();

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateUserRole(string id, Roles role)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            user.Role = role;
            await _userManager.UpdateAsync(user);

            TempData["Message"] = $"Role updated successfully for {user.FirstName} {user.LastName}.";
            return RedirectToAction("ManageUser");
        }

        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            return user == null ? NotFound() : View(user);
        }

        [HttpPost, ActionName("DeleteUser")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUserConfirmed(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var result = await _userManager.DeleteAsync(user);
            TempData["Message"] = result.Succeeded
                ? $"User {user.FirstName} {user.LastName} deleted successfully."
                : "Error deleting user.";

            return RedirectToAction("ManageUser");
        }

        public async Task<IActionResult> ApproveProduct()
        {
            var unapprovedProducts = await _context.Products
                .Include(p => p.Seller)
                .Where(p => !p.IsApproved)
                .ToListAsync();

            return View(unapprovedProducts);
        }


        [HttpPost]
        public async Task<IActionResult> ApproveProduct(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null) return NotFound();

            product.IsApproved = true;
            await _productRepository.UpdateAsync(product);

            return RedirectToAction("ProductApproval");
        }

        [HttpPost]
        public async Task<IActionResult> RejectProduct(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null) return NotFound();

            await _productRepository.DeleteAsync(product);

            return RedirectToAction("ProductApproval");
        }
        public async Task<IActionResult> Reports()
        {
            var orders = await _orderRepository.GetAllAsync();
            var users = await _userManager.Users.ToListAsync();

            var groupedOrders = orders
                .Where(o => !o.IsCancelled)
                .GroupBy(o => o.OrderDate.ToString("yyyy-MM"))
                .OrderBy(g => g.Key)
                .ToList();

            var viewModel = new ReportViewModel
            {
                TotalSales = orders.Where(o => !o.IsCancelled).Sum(o => o.TotalAmount),
                TotalOrders = orders.Count(),
                TotalUsers = users.Count(u => u.Role == Roles.User),
                TotalSellers = users.Count(u => u.Role == Roles.Seller),
                Months = groupedOrders.Select(g => g.Key).ToList(),
                MonthlySales = groupedOrders.Select(g => g.Sum(o => o.TotalAmount)).ToList(),
                MonthlyOrders = groupedOrders.Select(g => g.Count()).ToList()
            };

            return View(viewModel);
        }

        public async Task<IActionResult> BannerManagement()
        {
            var banners = await _context.Banners.ToListAsync();
            return View(banners);
        }

        [HttpGet]
        public async Task<IActionResult> DeleteBanner(int id)
        {
            var banner = await _context.Banners.FindAsync(id);
            return banner == null ? NotFound() : View(banner);
        }

        [HttpPost, ActionName("DeleteBannerConfirmed")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteBannerConfirmed(int id)
        {
            var banner = await _context.Banners.FindAsync(id);
            if (banner == null)
                return NotFound();

            // Optionally: delete image file from disk
            if (!string.IsNullOrEmpty(banner.ImageUrl))
            {
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", banner.ImageUrl.TrimStart('/'));
                if (System.IO.File.Exists(filePath))
                    System.IO.File.Delete(filePath);
            }

            _context.Banners.Remove(banner);
            await _context.SaveChangesAsync();

            return RedirectToAction("BannerManagement");
        }

        [HttpGet]
        public IActionResult CreateBanner()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateBanner(Banner model, IFormFile ImageFile)
        {
            if (!ModelState.IsValid)
                return View(model);

            if (ImageFile != null && ImageFile.Length > 0)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(ImageFile.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/banners", fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await ImageFile.CopyToAsync(stream);
                }

                model.ImageUrl = "/images/banners/" + fileName;
                model.CreatedAt = DateTime.UtcNow;

                _context.Banners.Add(model);
                await _context.SaveChangesAsync();

                return RedirectToAction("BannerManagement");
            }

            ModelState.AddModelError("", "Image upload failed.");
            return View(model);
        }
        [HttpGet]
        public async Task<IActionResult> EditBanner(int id)
        {
            var banner = await _context.Banners.FindAsync(id);
            return banner == null ? NotFound() : View(banner);
        }

        [HttpPost]
        public async Task<IActionResult> EditBanner(Banner model, IFormFile? ImageFile)
        {
            if (!ModelState.IsValid)
                return View(model);

            var banner = await _context.Banners.FindAsync(model.Id);
            if (banner == null)
                return NotFound();

            banner.Title = model.Title;
            banner.Subtitle = model.Subtitle;
            banner.UpdatedAt = DateTime.UtcNow;

            if (ImageFile != null && ImageFile.Length > 0)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(ImageFile.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/banners", fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await ImageFile.CopyToAsync(stream);
                }

                banner.ImageUrl = "/images/banners/" + fileName;
            }

            _context.Banners.Update(banner);
            await _context.SaveChangesAsync();

            return RedirectToAction("BannerManagement");
        }


    }
}
