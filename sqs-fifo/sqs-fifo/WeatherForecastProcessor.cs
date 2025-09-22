using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.Extensions.Options;

namespace sqs_fifo;

public class WeatherForecastProcessor(
    IAmazonSQS sqsClient, IOptions<SqsSettings> settings) : BackgroundService
{
    private readonly SqsSettings _settings = settings.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Console.WriteLine("Starting Weather Forecast Processor");

        while (!stoppingToken.IsCancellationRequested)
        {
            var request = new ReceiveMessageRequest()
            {
                QueueUrl = _settings.QueueUrl,
            };
            var response = await sqsClient.ReceiveMessageAsync(request, stoppingToken);
            foreach(var message in response.Messages ?? Enumerable.Empty<Message>())
            {
                Console.WriteLine("New message received");
                Console.WriteLine(message.Body);
                await sqsClient.DeleteMessageAsync(_settings.QueueUrl, message.ReceiptHandle);
            }
            
            await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
        }
    }
}