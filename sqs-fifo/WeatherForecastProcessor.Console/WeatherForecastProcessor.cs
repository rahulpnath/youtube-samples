using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.Extensions.Options;
using System.Text.Json;
using WeatherForecastProcessor.Console.Models;

namespace WeatherForecastProcessor.Console;

public class StandardQueueProcessor(IAmazonSQS sqsClient, IOptions<SqsSettings> settings)
{
    private readonly SqsSettings _settings = settings.Value;

    public async Task ProcessMessagesAsync(CancellationToken cancellationToken)
    {
        System.Console.WriteLine("[Standard Queue] Starting processor...");

        while (!cancellationToken.IsCancellationRequested)
        {
            var request = new ReceiveMessageRequest()
            {
                QueueUrl = _settings.StandardQueueUrl,
                MaxNumberOfMessages = 10,
                WaitTimeSeconds = 20
            };

            var response = await sqsClient.ReceiveMessageAsync(request, cancellationToken);

            foreach (var message in response.Messages ?? Enumerable.Empty<Message>())
            {
                System.Console.WriteLine($"[Standard Queue] New message received: {message.Body}");
                
                try
                {
                    var queueMessage = JsonSerializer.Deserialize<QueueMessage>(message.Body);
                    if (queueMessage != null)
                    {
                        System.Console.WriteLine($"[Standard Queue] LoopNumber: {queueMessage.LoopNumber}, Message: {queueMessage.Message}");
                    }
                }
                catch (JsonException ex)
                {
                    System.Console.WriteLine($"[Standard Queue] Failed to deserialize message: {ex.Message}");
                }
                
                await sqsClient.DeleteMessageAsync(_settings.StandardQueueUrl, message.ReceiptHandle, cancellationToken);
            }

            await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
        }
    }
}

public class FifoQueueProcessor(IAmazonSQS sqsClient, IOptions<SqsSettings> settings)
{
    private readonly SqsSettings _settings = settings.Value;

    public async Task ProcessMessagesAsync(CancellationToken cancellationToken)
    {
        System.Console.WriteLine("[FIFO Queue] Starting processor...");

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var request = new ReceiveMessageRequest()
                {
                    QueueUrl = _settings.FifoQueueUrl,
                    MaxNumberOfMessages = 2,
                    WaitTimeSeconds = 20
                };

                var response = await sqsClient.ReceiveMessageAsync(request, cancellationToken);

                foreach (var message in response.Messages ?? Enumerable.Empty<Message>())
                {
                    System.Console.WriteLine($"[FIFO Queue] New message received: {message.Body}");

                    try
                    {
                        var queueMessage = JsonSerializer.Deserialize<QueueMessage>(message.Body);
                        if (queueMessage != null)
                        {
                            System.Console.WriteLine($"[FIFO Queue] LoopNumber: {queueMessage.LoopNumber}, Message: {queueMessage.Message} - {DateTime.Now}");
                            // SIMULATE EXCEPTION
                            // if (queueMessage.LoopNumber % 2 == 0)
                            // {
                            //    await sqsClient.ChangeMessageVisibilityAsync(
                            //        _settings.FifoQueueUrl, message.ReceiptHandle, 0, cancellationToken);
                            //     throw new Exception("Error In processing message");
                            // }
                        }
                    }
                    catch (JsonException ex)
                    {
                        System.Console.WriteLine($"[FIFO Queue] Failed to deserialize message: {ex.Message}");
                    }

                  // await Task.Delay(TimeSpan.FromSeconds(2));
                    await sqsClient.DeleteMessageAsync(_settings.FifoQueueUrl, message.ReceiptHandle, cancellationToken);
                }

                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.Message);
            }
        }
    }
}

public class WeatherForecastProcessor(IAmazonSQS sqsClient, IOptions<SqsSettings> settings)
{
    private readonly SqsSettings _settings = settings.Value;
    private readonly CancellationTokenSource _cancellationTokenSource = new();

    public async Task StartAsync()
    {
        System.Console.WriteLine("Starting Weather Forecast Processors");
        System.Console.WriteLine("Press Ctrl+C to stop...");

        System.Console.CancelKeyPress += (_, e) =>
        {
            e.Cancel = true;
            _cancellationTokenSource.Cancel();
        };

        try
        {
            var standardProcessor = new StandardQueueProcessor(sqsClient, Microsoft.Extensions.Options.Options.Create(_settings));
            var fifoProcessor = new FifoQueueProcessor(sqsClient, Microsoft.Extensions.Options.Options.Create(_settings));

            var tasks = new[]
            {
                standardProcessor.ProcessMessagesAsync(_cancellationTokenSource.Token),
                fifoProcessor.ProcessMessagesAsync(_cancellationTokenSource.Token)
            };

            await Task.WhenAny(tasks);
        }
        catch (OperationCanceledException)
        {
            System.Console.WriteLine("Processing stopped.");
        }
    }
}