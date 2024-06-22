using Amazon.Lambda.Core;
using Amazon.SQS;
using Amazon.SQS.Model;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace PixReceiverLambda
{
    public class Function
    {
        private static readonly string QueueUrl = Environment.GetEnvironmentVariable("SQS_QUEUE_URL");
        private readonly IAmazonSQS _sqsClient;
        private readonly PlaidService _plaidService;
        
        public Function() : this(new AmazonSQSClient(), new HttpClient(), Environment.GetEnvironmentVariable("PLAID_CLIENT_ID"), Environment.GetEnvironmentVariable("PLAID_SECRET"), Environment.GetEnvironmentVariable("PLAID_ENVIRONMENT") ?? "sandbox") { }

        public Function(IAmazonSQS sqsClient, HttpClient httpClient, string clientId, string secret, string environment)
        {
            _sqsClient = sqsClient;
            _plaidService = new PlaidService(httpClient, clientId, secret, environment);
        }

        public async Task<string> FunctionHandler(PixTransaction pixTransaction, ILambdaContext context)
        {
            try
            {
                var recipientId = await _plaidService.CreateRecipient(pixTransaction.Receiver, "DE89370400440532013000", "123 Main Street", "Anytown", "12345", "DE");
                var consentId = await _plaidService.CreatePaymentConsent(recipientId);
                var paymentId = await _plaidService.ExecutePaymentWithConsent(pixTransaction.Amount, consentId);

                var message = JsonConvert.SerializeObject(new { pixTransaction, paymentId });
                var request = new SendMessageRequest
                {
                    QueueUrl = QueueUrl,
                    MessageBody = message
                };

                await _sqsClient.SendMessageAsync(request);
                return "Transaction received and processed successfully";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing Pix transaction: {ex.Message}");
                throw;
            }
        }
    }

    public class PixTransaction
    {
        public string TransactionId { get; set; } = string.Empty;
        public double Amount { get; set; }
        public string ConsentId { get; set; } = string.Empty;
        public string Sender { get; set; } = string.Empty;
        public string Receiver { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string FinanceData { get; set; } = string.Empty;
    }
}
