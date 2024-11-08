using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.SQS;
using Amazon.SQS.Model;
using System.Text.Json;
using WeatherForecast.Messages;

var builder = WebApplication.CreateBuilder(args);

Console.WriteLine("Starting WeatherForecast.Api");
// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var weatherDataQueue = "weather-data";
var localStackUrl = "http://localhost:4566";
var localStackRegion = "ap-southeast-2";

//if (builder.Environment.IsDevelopment())
//{
// Option2: AWS Options
//var awsOptions = builder.Configuration.GetAWSOptions();
//awsOptions.DefaultClientConfig.ServiceURL = localStackUrl;
//awsOptions.DefaultClientConfig.AuthenticationRegion = localStackRegion;
//builder.Services.AddDefaultAWSOptions(awsOptions);

// Option1:  Explicitly specifying on each of the client
//builder.Services.AddSingleton<IAmazonDynamoDB>(_ =>
//    new AmazonDynamoDBClient(new AmazonDynamoDBConfig()
//    {
//        ServiceURL = localStackUrl,
//        AuthenticationRegion = localStackRegion
//    }));
//}


builder.Services.AddAWSService<IAmazonDynamoDB>();
builder.Services.AddAWSService<IAmazonSQS>();
builder.Services.AddSingleton<IDynamoDBContext, DynamoDBContext>();

builder.Services.AddAWSLambdaHosting(LambdaEventSource.HttpApi);

var app = builder.Build();

// Configure the HTTP request pipeline.
// if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();


app.MapPost("/weatherforecast", async (WeatherForecastData data, IDynamoDBContext dynamoDbContext, IAmazonSQS publisher) =>
    {
        Console.WriteLine($"Received WeatherForecast data for city {data.City}");
        await dynamoDbContext.SaveAsync(data);
        await publisher.SendMessageAsync(
            new SendMessageRequest(weatherDataQueue,
           JsonSerializer.Serialize(new WeatherForecastAddedEvent()
           {
               City = data.City,
               DateTime = data.Date,
               TemperatureC = data.TemperatureC,
               Summary = data.Summary
           })));
    })
    .WithName("PostWeatherForecast")
    .DisableAntiforgery()
    .WithOpenApi();

app.Run();

public class WeatherForecastData
{
    public string City { get; set; }
    public DateTime Date { get; set; }
    public int TemperatureC { get; set; }
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
    public string Summary { get; set; }
}

