using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.Extensions.Options;

namespace WeatherForecastProcessor.Console;

public class WeatherForecastProcessor(IAmazonSQS sqsClient, IOptions<SqsSettings> settings)
{
    private readonly SqsSettings _settings = settings.Value;
    private readonly CancellationTokenSource _cancellationTokenSource = new();

    public async Task StartAsync()
    {
        System.Console.WriteLine("Starting Weather Forecast Processor");
        System.Console.WriteLine("Press Ctrl+C to stop...");

        System.Console.CancelKeyPress += (_, e) =>
        {
            e.Cancel = true;
            _cancellationTokenSource.Cancel();
        };

        try
        {
            await ProcessMessagesAsync(_cancellationTokenSource.Token);
        }
        catch (OperationCanceledException)
        {
            System.Console.WriteLine("Processing stopped.");
        }
    }

    private async Task ProcessMessagesAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            var request = new ReceiveMessageRequest()
            {
                QueueUrl = _settings.QueueUrl,
            };

            var response = await sqsClient.ReceiveMessageAsync(request, cancellationToken);

            foreach (var message in response.Messages ?? Enumerable.Empty<Message>())
            {
                System.Console.WriteLine("New message received");
                System.Console.WriteLine(message.Body);
                await sqsClient.DeleteMessageAsync(_settings.QueueUrl, message.ReceiptHandle, cancellationToken);
            }

            await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
        }
    }
}