using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;

[Authorize]
public class OrderController : Controller
{
    private readonly ICartRepository _cartRepository;
    private readonly IProductRepository _productRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly UserManager<ApplicationUser> _userManager;

    public OrderController(
        ICartRepository cartRepository,
        IProductRepository productRepository,
        IOrderRepository orderRepository,
        UserManager<ApplicationUser> userManager)
    {
        _cartRepository = cartRepository;
        _productRepository = productRepository;
        _orderRepository = orderRepository;
        _userManager = userManager;
    }

    // GET: Order/Checkout
    public async Task<IActionResult> Checkout()
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var cart = await _cartRepository.GetCartByUserIdAsync(user.Id);

        if (cart == null || !cart.Items.Any())
        {
            return RedirectToAction("Index", "Cart");
        }

        var vm = new CheckoutVM
        {
            Items = (List<CartItem>)cart.Items
        };

        return View(vm); 
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PlaceOrder(CheckoutVM vm)
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var cart = await _cartRepository.GetCartByUserIdAsync(user.Id);

        if (cart == null || !cart.Items.Any())
        {
            return RedirectToAction("Index", "Cart");
        }

        var order = new Order
        {
            UserId = user.Id,
            ShippingCity = vm.ShippingCity,
            PaymentMethod = vm.PaymentMethod,
            OrderDate = DateTime.UtcNow,
            Status = OrderStatus.Ordered,
            TotalAmount = cart.Items.Sum(i => i.Product.Price * i.Quantity),
            Items = cart.Items.Select(i => new OrderItem
            {
                ProductId = i.ProductId,
                Quantity = i.Quantity,
                UnitPrice = i.Product.Price
            }).ToList()
        };

        await _orderRepository.AddAsync(order);
        await _cartRepository.ClearCartAsync(user.Id);

        return RedirectToAction("Confirmation", new { id = order.Id });
    }

    public async Task<IActionResult> Confirmation(int id)
    {
        var order = await _orderRepository.GetByIdAsync(id);
        if (order == null)
            return NotFound();

        return View(order);
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AdminOrders(string? search, string? statusFilter, string? paymentFilter)
    {
        var orders = await _orderRepository.GetAllAsync();

        if (!string.IsNullOrEmpty(search))
        {
            orders = orders.Where(o =>
                o.Id.ToString().Contains(search) ||
                (o.User?.FirstName?.ToLower().Contains(search.ToLower()) ?? false)
            ).ToList();
        }

        if (!string.IsNullOrEmpty(statusFilter) && Enum.TryParse<OrderStatus>(statusFilter, out var status))
        {
            orders = orders.Where(o => o.Status == status).ToList();
        }

        if (!string.IsNullOrEmpty(paymentFilter) && Enum.TryParse<PaymentMethod>(paymentFilter, out var payment))
        {
            orders = orders.Where(o => o.PaymentMethod == payment).ToList();
        }

        ViewBag.Search = search;
        ViewBag.SelectedStatus = statusFilter;
        ViewBag.SelectedPayment = paymentFilter;

        return View(orders);
    }



}
