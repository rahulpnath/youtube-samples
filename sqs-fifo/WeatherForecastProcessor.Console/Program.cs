using Amazon.SQS;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using WeatherForecastProcessor.Console;

var builder = Host.CreateApplicationBuilder(args);

builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

builder.Services.Configure<SqsSettings>(builder.Configuration.GetSection("SqsSettings"));
builder.Services.AddAWSService<IAmazonSQS>();
builder.Services.AddTransient<WeatherForecastProcessor.Console.WeatherForecastProcessor>();

var host = builder.Build();

var processor = host.Services.GetRequiredService<WeatherForecastProcessor.Console.WeatherForecastProcessor>();
await processor.StartAsync();