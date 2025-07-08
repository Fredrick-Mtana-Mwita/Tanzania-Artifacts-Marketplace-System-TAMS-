using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Tanzania_Artifacts_MarketPlace_System_NEW_1_.Areas.Seller.Controllers
{
    [Area("Seller")]
    [Authorize(Roles = "Seller")]
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OrderController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var sellerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var orders = await _context.Orders
                .Include(o => o.Items)
                .ThenInclude(oi => oi.Product)
                .Where(o => o.Items.Any(oi => oi.Product.SellerId == sellerId))
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return View(orders);
        }

        [HttpPost]
        public async Task<IActionResult> MarkAsShipped(int id)
        {
            var order = await _context.Orders
                .Include(o => o.Items)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
                return NotFound();

            var sellerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            bool isSellerInvolved = order.Items.Any(oi => oi.Product.SellerId == sellerId);

            if (!isSellerInvolved)
                return Unauthorized();

            order.Status = OrderStatus.Shipped;
            await _context.SaveChangesAsync();

            TempData["Success"] = "Order marked as shipped.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> MarkItemAsShipped(int itemId)
        {
            var item = await _context.OrderItems
                .Include(i => i.Product)
                .FirstOrDefaultAsync(i => i.Id == itemId);

            if (item == null || item.Product.SellerId != User.FindFirstValue(ClaimTypes.NameIdentifier))
                return Unauthorized();

            item.SellerStatus = SellerOrderStatus.Shipped;
            await _context.SaveChangesAsync();

            TempData["Success"] = "Item marked as shipped.";
            return RedirectToAction(nameof(Index));
        }

    }
}
