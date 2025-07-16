using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Tanzania_Artifacts_MarketPlace_System_NEW_1_.Data;
using Tanzania_Artifacts_MarketPlace_System_NEW_1_.Helpers;
using Tanzania_Artifacts_MarketPlace_System_NEW_1_.Models;
using Tanzania_Artifacts_MarketPlace_System_NEW_1_.Services;

namespace Tanzania_Artifacts_MarketPlace_System_NEW_1_.Controllers
{
    public class PaymentController : Controller
    {
        private readonly PayPalService _paypalService;
        private readonly ApplicationDbContext _context;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public PaymentController(PayPalService paypalService, ApplicationDbContext context,
            IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _paypalService = paypalService;
            _context = context;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        [HttpPost]
        public async Task<IActionResult> PayWithPayPal(decimal amount, int orderId)
        {
            if (amount <= 0 || orderId <= 0)
                return BadRequest("Invalid order or amount.");

            try
            {
                TempData["PendingOrderId"] = orderId;
                TempData["PendingAmount"] = amount.ToString("F2");

                var approvalUrl = await _paypalService.CreateOrderAsync(amount);
                return Redirect(approvalUrl);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Failed to start payment: " + ex.Message;
                return RedirectToAction("Cancel");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Success(string token)
        {
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Cancel");

            try
            {
                var accessToken = await GetPayPalAccessTokenAsync();

                var client = _httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", accessToken);

                var response = await client.PostAsync(
                    $"https://api-m.sandbox.paypal.com/v2/checkout/orders/{token}/capture", null);

                if (!response.IsSuccessStatusCode)
                    return RedirectToAction("Cancel");

                var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

                var status = json.RootElement.GetProperty("status").GetString();
                var capture = json.RootElement
                    .GetProperty("purchase_units")[0]
                    .GetProperty("payments")
                    .GetProperty("captures")[0];

                var transactionId = capture.GetProperty("id").GetString();

                // ✅ Recover TempData
                var orderId = Convert.ToInt32(TempData["PendingOrderId"]);
                var amount = Convert.ToDecimal(TempData["PendingAmount"]);

                var payment = new Payment
                {
                    OrderId = orderId,
                    Amount = amount,
                    PaymentMethod = "PayPal",
                    TransactionId = transactionId ?? "",
                    Status = status ?? "Success",
                    PaymentDate = DateTime.UtcNow
                };

                _context.Payments.Add(payment);

                var order = _context.Orders.FirstOrDefault(o => o.Id == orderId);
                if (order != null)
                {
                    order.IsPaid = true;
                    order.Status = OrderStatus.Paid;
                }

                await _context.SaveChangesAsync();

                var user = await _context.Users.FindAsync(order?.UserId);
                if (user != null)
                {
                    var message = $@"
                        <h2>Hi {user.FirstName},</h2>
                        <p>Your payment of <strong>{amount:C}</strong> was successful.</p>
                        <p>Transaction ID: {transactionId}</p>
                        <p>Thank you for your order!</p>";

                    await EmailHelper.SendEmailAsync(user.Email!, "Order Payment Confirmation", message, _configuration);
                }

                return RedirectToAction("ThankYou", new { id = payment.Id });
            }
            catch
            {
                return RedirectToAction("Cancel");
            }
        }

        [HttpGet]
        public IActionResult Cancel()
        {
            ViewBag.Message = TempData["Error"]?.ToString() ?? "Payment was cancelled by user.";
            return View();
        }

        public IActionResult ThankYou(int id)
        {
            var payment = _context.Payments.FirstOrDefault(p => p.Id == id);
            if (payment == null) return NotFound();
            return View(payment);
        }

        private async Task<string> GetPayPalAccessTokenAsync()
        {
            var client = _httpClientFactory.CreateClient();

            var byteArray = Encoding.UTF8.GetBytes(
                $"{_configuration["PayPal:ClientId"]}:{_configuration["PayPal:ClientSecret"]}");
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

            var response = await client.PostAsync(
                "https://api-m.sandbox.paypal.com/v1/oauth2/token",
                new FormUrlEncodedContent(new[] {
                    new KeyValuePair<string, string>("grant_type", "client_credentials")
                }));

            response.EnsureSuccessStatusCode();
            var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            return json.RootElement.GetProperty("access_token").GetString()!;
        }
    }
}
