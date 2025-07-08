using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Tanzania_Artifacts_MarketPlace_System_NEW_1_.Data;
using Tanzania_Artifacts_MarketPlace_System_NEW_1_.Interfaces;
using Tanzania_Artifacts_MarketPlace_System_NEW_1_.Models;
using Tanzania_Artifacts_MarketPlace_System_NEW_1_.Services;
using Tanzania_Artifacts_MarketPlace_System_NEW_1_.ViewModel;

[Authorize]
public class OrderController : Controller
{
    private readonly ICartRepository _cartRepository;
    private readonly IProductRepository _productRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IEmailService _emailService;

    public OrderController(
        ICartRepository cartRepository,
        IProductRepository productRepository,
        IOrderRepository orderRepository,
        UserManager<ApplicationUser> userManager,
        IEmailService emailService)
    {
        _cartRepository = cartRepository;
        _productRepository = productRepository;
        _orderRepository = orderRepository;
        _userManager = userManager;
        _emailService = emailService;
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateOrder()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Account");

        var cart = await _cartRepository.GetCartByUserIdAsync(user.Id);
        if (cart == null || !cart.Items.Any()) return RedirectToAction("Index", "Cart");

        var order = new Order
        {
            UserId = user.Id,
            ShippingCity = "", // Empty for now
            PaymentMethod = PaymentMethod.Unknown,
            OrderDate = DateTime.UtcNow,
            Status = OrderStatus.Pending,
            TotalAmount = cart.Items.Sum(i => i.Product.Price * i.Quantity),
            Items = cart.Items.Select(i => new OrderItem
            {
                ProductId = i.ProductId,
                Quantity = i.Quantity,
                UnitPrice = i.Product.Price
            }).ToList()
        };

        await _orderRepository.AddAsync(order);

        // DO NOT clear the cart here! We need cart info on Checkout & PlaceOrder

        return RedirectToAction("Checkout", new { orderId = order.Id });
    }

    public async Task<IActionResult> Checkout(int orderId)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Account");

        var order = await _orderRepository.GetByIdAsync(orderId);
        if (order == null || order.UserId != user.Id)
            return RedirectToAction("Index", "Cart");

        // Fetch product details in parallel for all order items
        var items = new List<CartItem>();

        foreach (var item in order.Items)
        {
            var product = await _productRepository.GetByIdAsync(item.ProductId);
            if (product == null) continue; // skip nulls safely

            items.Add(new CartItem
            {
                Product = product,
                ProductId = item.ProductId,
                Quantity = item.Quantity
            });
        }

        var vm = new CheckoutVM
        {
            OrderId = order.Id,
            Items = items,
            ShippingCity = order.ShippingCity,
            PaymentMethod = order.PaymentMethod
        };

        return View(vm);
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PlaceOrder(CheckoutVM vm)
    {
        if (!ModelState.IsValid)
        {
            TempData["Errors"] = string.Join(" | ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
            return RedirectToAction("Checkout", new { orderId = vm.OrderId });
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Account");

        var order = await _orderRepository.GetByIdAsync(vm.OrderId);
        if (order == null || order.UserId != user.Id) return RedirectToAction("Index", "Cart");

        // Update order details from vm
        order.ShippingCity = vm.ShippingCity;
        order.PaymentMethod = vm.PaymentMethod;
        order.Status = OrderStatus.Ordered;
        order.OrderDate = DateTime.UtcNow; // or keep original order date if you want

        // Update total amount if needed
        // If order items or quantity changed, you might want to recalculate total here

        await _orderRepository.UpdateAsync(order);

        // Clear cart now that order is finalized
        await _cartRepository.ClearCartAsync(user.Id);

        // Send confirmation email (same as before)
        string subject = "Order Confirmation - Tanzania Artifacts Marketplace";

        string tableRows = string.Join("", order.Items.Select(item => $@"
        <tr>
            <td style='padding:8px;border:1px solid #ccc'>{item.Product?.Name}</td>
            <td style='padding:8px;border:1px solid #ccc'>{item.Quantity}</td>
            <td style='padding:8px;border:1px solid #ccc'>TSh {item.UnitPrice:N2}</td>
            <td style='padding:8px;border:1px solid #ccc'>TSh {(item.Quantity * item.UnitPrice):N2}</td>
        </tr>
    "));

        string body = $@"
        <div style='font-family:sans-serif;'>
            <h2>Hello {user.FirstName}, thank you for your order!</h2>
            <p>Your order <strong>#{order.Id}</strong> has been successfully placed.</p>
            <p><strong>Shipping City:</strong> {order.ShippingCity}</p>
            <p><strong>Payment Method:</strong> {order.PaymentMethod}</p>
            <p><strong>Order Date:</strong> {order.OrderDate:yyyy-MM-dd HH:mm}</p>
            <p><strong>Total Amount:</strong> TSh {order.TotalAmount:N2}</p>

            <h3 style='margin-top:30px;'>Order Items</h3>
            <table style='border-collapse:collapse;width:100%;margin-top:10px;'>
                <thead>
                    <tr style='background:#f0f0f0;'>
                        <th style='padding:8px;border:1px solid #ccc;'>Product</th>
                        <th style='padding:8px;border:1px solid #ccc;'>Quantity</th>
                        <th style='padding:8px;border:1px solid #ccc;'>Unit Price</th>
                        <th style='padding:8px;border:1px solid #ccc;'>Subtotal</th>
                    </tr>
                </thead>
                <tbody>{tableRows}</tbody>
            </table>

            <p style='margin-top:30px;'>We will notify you once your order is processed.</p>
            <p style='color:#888;'>Tanzania Artifacts Marketplace Team</p>
        </div>
    ";

        await _emailService.SendEmailAsync(user.Email!, subject, body);

        return RedirectToAction("Confirmation", new { id = order.Id });
    }

    public async Task<IActionResult> Confirmation(int id)
    {
        var order = await _orderRepository.GetByIdAsync(id);
        if (order == null) return NotFound();

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
