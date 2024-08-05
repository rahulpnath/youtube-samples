using System.Data.SqlClient;
using Dapper;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
string connectionString = builder.Configuration.GetConnectionString("RDSConnectionString");

app.MapGet("/weatherforecast", async () =>
    {
        using var connection = new SqlConnection(connectionString);
        var forecasts = await connection
            .QueryAsync<WeatherForecast>("SELECT Date, TemperatureC, Summary FROM WeatherForecasts");

        return Results.Ok(forecasts);
    })
    .WithName("GetWeatherForecast")
    .WithOpenApi();

app.Run();

public class WeatherForecast
{
    public DateTime Date { get; set; }
    public int TemperatureC { get; set; }
    public int TemperatureF => 32 + (int) (TemperatureC / 0.5556);
    public string? Summary { get; set; }
}