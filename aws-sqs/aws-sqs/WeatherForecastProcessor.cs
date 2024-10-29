using Amazon;
using Amazon.Runtime;
using Amazon.SQS;
using Amazon.SQS.Model;

namespace aws_sqs
{
    public class WeatherForecastProcessor : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Console.WriteLine("Starting Background processor");
            var credentials = new BasicAWSCredentials("ACCESS KEY", "SECRET");
            var client = new AmazonSQSClient(credentials, RegionEndpoint.APSoutheast2);

            while (!stoppingToken.IsCancellationRequested)
            {
                Console.WriteLine($"getting messages from the queue {DateTime.Now}");
                var request = new ReceiveMessageRequest()
                {
                    QueueUrl = "https://sqs.ap-southeast-2.amazonaws.com/189107071895/youtube-demo",
                    //WaitTimeSeconds = 10,
                    VisibilityTimeout = 20
                };

                var response = await client.ReceiveMessageAsync(request);
                foreach(var message in response.Messages)
                {
                    Console.WriteLine(message.Body);
                    if (message.Body.Contains("Exception")) continue;

                    await client.DeleteMessageAsync(
                        "https://sqs.ap-southeast-2.amazonaws.com/189107071895/youtube-demo", message.ReceiptHandle);
                }
            }
        }
    }
}
