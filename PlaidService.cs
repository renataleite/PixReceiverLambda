using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace PixReceiverLambda
{
    public class PlaidService
    {
        private readonly HttpClient _httpClient;
        private readonly string _clientId;
        private readonly string _secret;
        private readonly string _environment;

        public PlaidService(HttpClient httpClient, string clientId, string secret, string environment)
        {
            _httpClient = httpClient;
            _clientId = clientId;
            _secret = secret;
            _environment = environment;
        }

        public async Task<string> CreateRecipient(string name, string iban, string street, string city, string postalCode, string country)
        {
            var requestContent = new
            {
                client_id = _clientId,
                secret = _secret,
                name,
                iban,
                address = new
                {
                    street = new[] { street },
                    city,
                    postal_code = postalCode,
                    country
                }
            };

            var requestJson = JsonConvert.SerializeObject(requestContent);
            Console.WriteLine($"CreateRecipient Request JSON: {requestJson}");

            var response = await _httpClient.PostAsync(
                $"https://{_environment}.plaid.com/payment_initiation/recipient/create",
                new StringContent(requestJson, Encoding.UTF8, "application/json"));

            var responseContent = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"CreateRecipient Error: {responseContent}");
                throw new HttpRequestException($"Request to Plaid API failed. Status Code: {response.StatusCode}, Content: {responseContent}");
            }

            var responseObject = JsonConvert.DeserializeObject<dynamic>(responseContent);
            if (responseObject?.recipient_id == null)
            {
                throw new InvalidOperationException("Recipient ID not found in the response.");
            }
            return responseObject.recipient_id;
        }

        public async Task<string> CreatePaymentConsent(string recipientId)
        {
            var requestContent = new
            {
                client_id = _clientId,
                secret = _secret,
                recipient_id = recipientId,
                reference = "PixConsent",
                constraints = new
                {
                    max_payment_amount = new { currency = "GBP", value = "100.00" },
                    periodic_amounts = new[]
                    {
                new { amount = new { currency = "GBP", value = "100.00" }, interval = "MONTH", alignment = "CALENDAR" }
            }
                },
                scopes = new[] { "EXTERNAL" }
            };

            var requestJson = JsonConvert.SerializeObject(requestContent);
            Console.WriteLine($"CreatePaymentConsent Request JSON: {requestJson}");

            var response = await _httpClient.PostAsync(
                $"https://{_environment}.plaid.com/payment_initiation/consent/create",
                new StringContent(requestJson, Encoding.UTF8, "application/json"));

            var responseContent = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                // Log the response content for debugging
                Console.WriteLine($"CreatePaymentConsent Error: {responseContent}");
                throw new HttpRequestException($"Request to Plaid API failed. Status Code: {response.StatusCode}, Content: {responseContent}");
            }

            var responseObject = JsonConvert.DeserializeObject<dynamic>(responseContent);
            if (responseObject?.consent_id == null)
            {
                throw new InvalidOperationException("Consent ID not found in the response.");
            }
            return responseObject.consent_id;
        }

        public async Task<string> ExecutePaymentWithConsent(double amount, string consentId)
        {
            var requestContent = new
            {
                client_id = _clientId,
                secret = _secret,
                consent_id = consentId,
                amount = new { currency = "GBP", value = amount },
                idempotency_key = Guid.NewGuid().ToString(),
                reference = "PixPayment"
            };

            var requestJson = JsonConvert.SerializeObject(requestContent);
            Console.WriteLine($"ExecutePaymentWithConsent Request JSON: {requestJson}");

            var response = await _httpClient.PostAsync(
                $"https://{_environment}.plaid.com/payment_initiation/consent/payment/execute",
                new StringContent(requestJson, Encoding.UTF8, "application/json"));

            var responseContent = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"ExecutePaymentWithConsent Error: {responseContent}");
                throw new HttpRequestException($"Request to Plaid API failed. Status Code: {response.StatusCode}, Content: {responseContent}");
            }

            var responseObject = JsonConvert.DeserializeObject<dynamic>(responseContent);
            if (responseObject?.payment_id == null)
            {
                throw new InvalidOperationException("Payment ID not found in the response.");
            }
            return responseObject.payment_id;
        }
    }
}
