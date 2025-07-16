using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;
using Tanzania_Artifacts_MarketPlace_System_NEW_1_.Configurations;

namespace Tanzania_Artifacts_MarketPlace_System_NEW_1_.Services
{
    public class PayPalService
    {
        private readonly PayPalSettings _settings;
        private readonly HttpClient _httpClient;

        public PayPalService(IOptions<PayPalSettings> options)
        {
            _settings = options.Value;
            _httpClient = new HttpClient();
        }

        public async Task<string> GetAccessTokenAsync()
        {
            var byteArray = Encoding.UTF8.GetBytes($"{_settings.ClientId}:{_settings.ClientSecret}");
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

            var form = new Dictionary<string, string>
            {
                { "grant_type", "client_credentials" }
            };

            var response = await _httpClient.PostAsync($"{_settings.BaseUrl}/v1/oauth2/token", new FormUrlEncodedContent(form));
            var result = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"PayPal Token Error: {result}");

            dynamic json = JsonConvert.DeserializeObject(result)!;
            return json.access_token;
        }

        public async Task<string> CreateOrderAsync(decimal amount, string currency = "USD")
        {
            var accessToken = await GetAccessTokenAsync();

            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", accessToken);
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            string returnUrl = $"{_settings.BaseAppUrl}/Payment/Success";
            string cancelUrl = $"{_settings.BaseAppUrl}/Payment/Cancel";

            var order = new
            {
                intent = "CAPTURE",
                purchase_units = new[] {
                    new {
                        amount = new {
                            currency_code = currency,
                            value = amount.ToString("F2")
                        }
                    }
                },
                application_context = new
                {
                    return_url = returnUrl,
                    cancel_url = cancelUrl
                }
            };

            var json = JsonConvert.SerializeObject(order);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{_settings.BaseUrl}/v2/checkout/orders", content);
            var responseJson = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"PayPal Order Error: {responseJson}");

            dynamic result = JsonConvert.DeserializeObject(responseJson)!;

            foreach (var link in result.links)
            {
                if (link.rel == "approve")
                    return link.href;
            }

            throw new Exception("PayPal approval link not found.");
        }
    }
}
