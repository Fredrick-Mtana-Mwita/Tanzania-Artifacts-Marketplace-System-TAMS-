using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;
using Tanzania_Artifacts_MarketPlace_System_NEW_1_.Services;
using Tanzania_Artifacts_MarketPlace_System_NEW_1_.Models;

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

        // ✅ Step 1: Initiate PayPal Payment
        [HttpPost]
        public async Task<IActionResult> PayWithPayPal(decimal amount, int orderId)
        {
            if (amount <= 0 || orderId <= 0)
                return BadRequest("Invalid order or amount.");

            try
            {
                var approvalUrl = await _paypalService.CreateOrderAsync(amount);
                TempData["PendingOrderId"] = orderId;
                TempData["Amount"] = amount.ToString("F2"); // Store as string

                return Redirect(approvalUrl);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Failed to start payment: " + ex.Message;
                return RedirectToAction("Cancel");
            }
        }

        // ✅ Step 2: PayPal redirects here on success
        [HttpGet]
        public async Task<IActionResult> Success(string token)
        {
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Cancel");

            try
            {
                var accessToken = await GetPayPalAccessTokenAsync();

                // Capture order
                var captureClient = _httpClientFactory.CreateClient();
                captureClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", accessToken);

                var captureResponse = await captureClient.PostAsync(
                    $"https://api-m.sandbox.paypal.com/v2/checkout/orders/{token}/capture", null);

                if (!captureResponse.IsSuccessStatusCode)
                    return RedirectToAction("Cancel");

                var captureContent = await captureResponse.Content.ReadAsStringAsync();
                var captureJson = JsonDocument.Parse(captureContent);

                var status = captureJson.RootElement.GetProperty("status").GetString();
                var purchaseUnit = captureJson.RootElement.GetProperty("purchase_units")[0];
                var transactionId = purchaseUnit.GetProperty("payments").GetProperty("captures")[0].GetProperty("id").GetString();
                var amountStr = purchaseUnit.GetProperty("payments").GetProperty("captures")[0].GetProperty("amount").GetProperty("value").GetString();

                // Save to DB
                var orderId = Convert.ToInt32(TempData["PendingOrderId"]);
                var amount = Convert.ToDecimal(TempData["Amount"]);

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
                await _context.SaveChangesAsync();

                return RedirectToAction("ThankYou", new { id = payment.Id });
            }
            catch
            {
                return RedirectToAction("Cancel");
            }
        }

        // ❌ Step 3: User cancels payment
        [HttpGet]
        public IActionResult Cancel()
        {
            ViewBag.Message = TempData["Error"]?.ToString() ?? "Payment was cancelled by user.";
            return View();
        }

        // ✅ Step 4: Show success message and payment details
        public IActionResult ThankYou(int id)
        {
            var payment = _context.Payments.FirstOrDefault(p => p.Id == id);
            if (payment == null) return NotFound();

            return View(payment);
        }

        // 🔑 Utility: Get PayPal access token
        private async Task<string> GetPayPalAccessTokenAsync()
        {
            var client = _httpClientFactory.CreateClient();
            var clientId = _configuration["PayPal:ClientId"];
            var clientSecret = _configuration["PayPal:ClientSecret"]; // ✅ Correct key

            var byteArray = Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}");
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

            var tokenResponse = await client.PostAsync("https://api-m.sandbox.paypal.com/v1/oauth2/token",
                new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("grant_type", "client_credentials")
                }));

            tokenResponse.EnsureSuccessStatusCode();
            var json = JsonDocument.Parse(await tokenResponse.Content.ReadAsStringAsync());
            return json.RootElement.GetProperty("access_token").GetString()!;
        }
    }
}
