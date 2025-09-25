using Amazon.SQS;
using Amazon.SQS.Model;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using sqs_fifo;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Configure SQS settings with options pattern
builder.Services.Configure<SqsSettings>(builder.Configuration.GetSection("SqsSettings"));

builder.Services.AddAWSService<IAmazonSQS>();
// builder.Services.AddHostedService<WeatherForecastProcessor>();

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
        QueueUrl = sqsOptions.Value.QueueUrl
    };

    var result = await sqsClient.SendMessageAsync(request);
})
.WithName("PostWeatherForecast");

app.Run();

public class SqsSettings
{
    public string QueueUrl { get; set; } = string.Empty;
}

public record WeatherForecast(Guid Id, string City, WeatherDetails Details);

public record WeatherDetails(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}