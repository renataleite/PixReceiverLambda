using Amazon.Lambda.Core;
using Amazon.SQS;
using Amazon.SQS.Model;
using Newtonsoft.Json;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace PixReceiverLambda;

public class Function
{
    //private static readonly string QueueUrl = Environment.GetEnvironmentVariable("SQS_QUEUE_URL");
    private static readonly string QueueUrl = "https://sqs.us-east-1.amazonaws.com/440724126630/PixQueue";


    private readonly IAmazonSQS _sqsClient;

    public Function() : this(new AmazonSQSClient()) { }

    public Function(IAmazonSQS sqsClient)
    {
        _sqsClient = sqsClient;
    }

    public async Task<string> FunctionHandler(PixTransaction pixTransaction, ILambdaContext context)
    {
        var message = JsonConvert.SerializeObject(pixTransaction);
        var request = new SendMessageRequest
        {
            QueueUrl = QueueUrl,
            MessageBody = message
        };
        await _sqsClient.SendMessageAsync(request);
        return "Transaction received and sent to SQS";
    }
}

public class PixTransaction
{
    public string TransactionId { get; set; }
    public decimal Amount { get; set; }
    public string Sender { get; set; }
    public string Receiver { get; set; }
    public DateTime Timestamp { get; set; }
}
