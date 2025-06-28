using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Tanzania_Artifacts_MarketPlace_System_NEW_1_.Areas.Seller.SellerViewModel;
using Tanzania_Artifacts_MarketPlace_System_NEW_1_.Interfaces;
using Tanzania_Artifacts_MarketPlace_System_NEW_1_.Services;

namespace Tanzania_Artifacts_MarketPlace_System_NEW_1_.Areas.Seller.Controllers
{
    [Area("Seller")]
    [Authorize(Roles = "Seller")]
    public class SellerDashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly INotificationSender _notificationSender;

        public SellerDashboardController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            INotificationSender notificationSender)
        {
            _context = context;
            _userManager = userManager;
            _notificationSender = notificationSender;
        }

        // ✅ Dashboard Overview with Orders, Revenue, Recent Orders, and Chart Data
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

            // Chart: Group orders per month
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
    }
}
