using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAWSLambdaHosting(LambdaEventSource.HttpApi);

builder.Services.AddSingleton<IDynamoDBContext>(new DynamoDBContext(new AmazonDynamoDBClient()));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet(
    "/weatherforecast/{cityName}",
    (string cityName, IDynamoDBContext dynamoDBContext) =>
{
    return dynamoDBContext.QueryAsync<WeatherForecast>(cityName).GetRemainingAsync();
})
.WithName("GetAllWeatherForecastForCity")
.WithOpenApi();

app.MapGet(
    "/weatherforecast/{cityName}/{date}",
    (string cityName, string date, IDynamoDBContext dynamoDBContext) =>
{
    return dynamoDBContext.LoadAsync<WeatherForecast>(cityName, date);
})
.WithName("GetWeatherForecastForCityAndDate")
.WithOpenApi();

app.MapPost(
    "/weatherforecast/",
    ([FromBody] WeatherForecast weatherForecast, IDynamoDBContext dynamoDBContext) =>
{
    return dynamoDBContext.SaveAsync(weatherForecast);
})
.WithName("PostWeatherForecast")
.WithOpenApi();

app.MapDelete(
    "/weatherforecast/{cityName}/{date}",
    (string cityName, string date, IDynamoDBContext dynamoDBContext) =>
{
    return dynamoDBContext.DeleteAsync<WeatherForecast>(cityName, date);
})
.WithName("DeleteWeatherForecast")
.WithOpenApi();

app.Run();

public class WeatherForecast()
{
    public string CityName { get; set; }
    public string Date { get; set; }
    public string Summary { get; set; }
    public int TemperatureC { get; set; }
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
