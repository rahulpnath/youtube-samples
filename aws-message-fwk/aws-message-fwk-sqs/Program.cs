using AWS.Messaging;
using AWS.Messaging.Publishers.SQS;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddAWSMessageBus((builder) =>
{
    var weatherDataQueue = "<YOUR QUEUE URL>";
    builder.AddSQSPublisher<WeatherForecastAddedEvent>(
        weatherDataQueue, nameof(WeatherForecastAddedEvent));
    
    // Registering Handler
    builder.AddMessageHandler<WeatherForecastAddedEventHandler, WeatherForecastAddedEvent>(
        nameof(WeatherForecastAddedEvent));
    builder.AddSQSPoller(weatherDataQueue, options =>
    {
        options.VisibilityTimeout = 5;
        options.VisibilityTimeoutExtensionThreshold = 2;
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
    {
        var forecast = Enumerable.Range(1, 5).Select(index =>
                new WeatherForecast
                (
                    DateTime.Now.Date.AddDays(index),
                    Random.Shared.Next(-20, 55),
                    summaries[Random.Shared.Next(summaries.Length)]
                ))
            .ToArray();
        return forecast;
    })
    .WithName("GetWeatherForecast")
    .WithOpenApi();

app.MapPost("/weatherforecast", (WeatherForecast data, ISQSPublisher publisher) =>
    {
        // Save this your database and other processing logic
        publisher.SendAsync(new WeatherForecastAddedEvent()
        {
            DateTime = data.Date,
            TemperatureC = data.TemperatureC,
            Summary = data.Summary
        });

    }).WithName("PostWeatherForecast")
    .WithOpenApi().DisableAntiforgery();

app.Run();

record WeatherForecast(DateTime Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int) (TemperatureC / 0.5556);
}

class WeatherForecastAddedEvent
{
    public DateTime DateTime { get; set; }
    public int TemperatureC { get; set; }
    public string? Summary { get; set; }
}

class WeatherForecastAddedEventHandler: IMessageHandler<WeatherForecastAddedEvent>
{
    public Task<MessageProcessStatus> HandleAsync(
        MessageEnvelope<WeatherForecastAddedEvent> messageEnvelope, CancellationToken token = new CancellationToken())
    {
        if(messageEnvelope.Message.Summary.Contains("Exception"))
            throw new Exception(messageEnvelope.Message.Summary);
        
        Console.WriteLine($"Received WeatherForecastAddedEvent with {messageEnvelope.Message.DateTime} {messageEnvelope.Message.TemperatureC} ");
        return Task.FromResult(MessageProcessStatus.Success());
    }
}