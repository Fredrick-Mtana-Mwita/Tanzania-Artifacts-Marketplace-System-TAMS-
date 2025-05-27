using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Tanzania_Artifacts_MarketPlace_System_NEW_1_.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ICartRepository _cartRepository;
        private readonly IProductRepository _productRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminController(ApplicationDbContext context, 
            ICartRepository cartRepository, 
            IProductRepository productRepository, 
            IOrderRepository orderRepository, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _cartRepository = cartRepository;
            _productRepository = productRepository;
            _orderRepository = orderRepository;
            _userManager = userManager;
        }


        [Authorize("Admin")]
        public async Task<IActionResult> EditOrder(int id)
        {
            var order = await _orderRepository.GetByIdAsync(id);

            if (order == null)
                return NotFound();
            return View(order);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> EditOrder(int id, OrderStatus status)
        {
            var order = await _orderRepository.GetByIdAsync(id);
            if (order == null) return NotFound();

            order.Status = status;

            if (status == OrderStatus.Delivered)
                order.DeliveredDate = DateTime.UtcNow;

            await _orderRepository.UpdateAsync(order);
            return RedirectToAction("AdminOrders");
        }
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CancelOrder(int id)
        {
            var order = await _orderRepository.GetByIdAsync(id);
            if (order == null || order.IsCancelled) return NotFound();

            order.IsCancelled = true;
            order.CancelledDate = DateTime.UtcNow;
            order.Status = OrderStatus.Cancelled;

            await _orderRepository.UpdateAsync(order);
            return RedirectToAction("AdminOrders");
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> MarkAsShipped(int id)
        {
            var order = await _orderRepository.GetByIdAsync(id);
            if (order == null) return NotFound();

            order.Status = OrderStatus.Shipped;
            order.ShippedDate = DateTime.UtcNow;

            await _orderRepository.UpdateAsync(order);
            TempData["Message"] = $"Order #{order.Id} marked as shipped.";


            return RedirectToAction("AdminOrders");
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> MarkAsDelivered(int id)
        {
            var order = await _orderRepository.GetByIdAsync(id);
            if (order == null || order.IsCancelled || order.Status != OrderStatus.Shipped)
            {
                return NotFound();
            }

            order.Status = OrderStatus.Delivered;
            order.DeliveredDate = DateTime.UtcNow;

            await _orderRepository.UpdateAsync(order);

            TempData["Message"] = $"Order #{order.Id} marked as delivered.";
            return RedirectToAction("AdminOrders");
        }


        
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AdminDashboard()
        {
            var allUsers = await _userManager.Users.ToListAsync();
            var orders = await _orderRepository.GetAllAsync();

            var adminVm = new AdminDashboardVM
            {
                TotalUsers = allUsers.Count(u => u.Role == Roles.User),
                TotalSellers = allUsers.Count(u => u.Role == Roles.Seller),
                TotalOrders = orders.Count(),
                TotalSales = orders.Where(o => !o.IsCancelled).Sum(o => o.TotalAmount),
                PendingOrders = orders.Count(o => o.Status == OrderStatus.Pending),
                CompletedOrders = orders.Count(o => o.Status == OrderStatus.Delivered),
                RecentOrders = orders.OrderByDescending(o => o.OrderDate).Take(5).ToList()
            };

            return View(adminVm);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AdminOrders()
        {
            var orders = await _orderRepository.GetAllAsync();

            var sortedOrders = orders
                .OrderByDescending(o => o.OrderDate)
                .ToList();

            return View(sortedOrders);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ManageUser()
        {
            var users = await _userManager.Users.ToListAsync();

            var userManagerVM = users.Select(u => new ManageUserVM

            {
                Id = u.Id,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Email = u.Email,
                Role = u.Role,
            }).ToList();
               
            return View(userManagerVM);
        }

        [HttpPost] 
        [Authorize(Roles ="Admin")]
        public async Task<IActionResult> UpdateUserRole(string id, Roles role)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            user.Role = role;
            await _userManager.UpdateAsync(user);

            TempData["Message"] = $"Role Upadated successfully for {user.FirstName}{user.LastName}.";
            return RedirectToAction("ManageUser");
        }
    }
}
