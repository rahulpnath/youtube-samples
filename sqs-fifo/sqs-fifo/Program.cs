using Amazon.SQS;
using Amazon.SQS.Model;
using System.Text.Json;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Configure SQS settings with options pattern
builder.Services.Configure<SqsSettings>(builder.Configuration.GetSection("SqsSettings"));

builder.Services.AddAWSService<IAmazonSQS>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", (string city) =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast(
            Guid.NewGuid(),
            city,
            new WeatherDetails(
                DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                Random.Shared.Next(-20, 55),
                summaries[Random.Shared.Next(summaries.Length)]
            )
        )
    ).ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

app.MapPost("/weatherforecast", async (WeatherForecast data, IAmazonSQS sqsClient, IOptions<SqsSettings> sqsOptions) =>
{
    var request = new SendMessageRequest()
    {
        MessageGroupId = data.City,
        MessageDeduplicationId = data.Id == Guid.Empty ? null : data.Id.ToString(),
        MessageBody = JsonSerializer.Serialize(data.Details),
        QueueUrl = sqsOptions.Value.FifoQueueUrl
    };

    var result = await sqsClient.SendMessageAsync(request);
})
.WithName("PostWeatherForecast");

app.MapPost("/publish-batch-standard", async (IAmazonSQS sqsClient, IOptions<SqsSettings> sqsOptions) =>
{
    var standardEntries = new List<SendMessageBatchRequestEntry>();
    for (int i = 1; i <= 10; i++)
    {
        standardEntries.Add(new SendMessageBatchRequestEntry
        {
            Id = $"standard-{i}",
            MessageBody = JsonSerializer.Serialize(new {
                LoopNumber = i,
                Message = $"Standard queue message #{i}",
                Timestamp = DateTime.UtcNow
            })
        });
    }
    var standardBatchResult = await sqsClient.SendMessageBatchAsync(new SendMessageBatchRequest
    {
        QueueUrl = sqsOptions.Value.StandardQueueUrl,
        Entries = standardEntries
    });
    return Results.Ok(new {
        Message = "Successfully sent 10 messages to the standard queue using batch operation",
        BatchResults = standardBatchResult.Successful.Select(x => new { x.MessageId }).ToList()
    });
})
.WithName("PublishBatchStandardMessages");

app.MapPost("/publish-batch-fifo", async (IAmazonSQS sqsClient, IOptions<SqsSettings> sqsOptions) =>
{
    var fifoEntries = new List<SendMessageBatchRequestEntry>();
    for (int i = 1; i <= 10; i++)
    {
        fifoEntries.Add(new SendMessageBatchRequestEntry
        {
            Id = $"fifo-{i}",
            MessageGroupId = $"message-group-id",
            MessageDeduplicationId = Guid.NewGuid().ToString(),
            MessageBody = JsonSerializer.Serialize(new {
                LoopNumber = i,
                Message = $"FIFO queue message #{i}",
                Timestamp = DateTime.UtcNow
            })
        });
    }
    var fifoBatchResult = await sqsClient.SendMessageBatchAsync(new SendMessageBatchRequest
    {
        QueueUrl = sqsOptions.Value.FifoQueueUrl,
        Entries = fifoEntries
    });
    return Results.Ok(new {
        Message = "Successfully sent 10 messages to the FIFO queue using batch operation",
        BatchResults = fifoBatchResult.Successful.Select(x => new { x.MessageId }).ToList()
    });
})
.WithName("PublishBatchFifoMessages");

app.Run();

public class SqsSettings
{
    public string StandardQueueUrl { get; set; } = string.Empty;
    public string FifoQueueUrl { get; set; } = string.Empty;
}

public record WeatherForecast(Guid Id, string City, WeatherDetails Details);

public record WeatherDetails(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}