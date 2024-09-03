using hello_mass_transit;
using MassTransit;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddMassTransit(mt =>
{
    mt.AddConsumer<SendNewWeatherDataEmail>();
    mt.AddConsumer<SendNewWeatherDataSMS>();
    
    var rabbitConfiguration = builder.Configuration
        .GetSection(nameof(RabbitConfiguration))
        .Get<RabbitConfiguration>();
    mt.UsingRabbitMq((context, cfg) =>
    {
        cfg.ConfigureEndpoints(context);
        cfg.Host(rabbitConfiguration.Host, host =>
        {
            host.Username(rabbitConfiguration.UserName);
            host.Password(rabbitConfiguration.Password);
        });
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
                    "Brisbane",
                    DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    Random.Shared.Next(-20, 55),
                    summaries[Random.Shared.Next(summaries.Length)]
                ))
            .ToArray();
        return forecast;
    })
    .WithName("GetWeatherForecast")
    .WithOpenApi();

app.MapPost("/weatherforecast", async ([FromBody]WeatherForecast data, IBus bus) =>
        {
            Console.WriteLine($"New weather data added for {data.City} on {data.Date} with Temperature {data.TemperatureC}");
            await bus.Publish(new WeatherDataAddedEvent(){ City = data.City, TemperatureC = data.TemperatureC, DateTime = data.Date});
        })
    .WithName("PostWeatherForecast")
    .DisableAntiforgery()
    .WithOpenApi();

app.Run();

record WeatherForecast(string City,DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int) (TemperatureC / 0.5556);
}

public record RabbitConfiguration
{
    public string Host { get; set; }
    public string UserName { get; init; }
    public string Password { get; init; }
}

