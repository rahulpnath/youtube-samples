using Amazon;
using Amazon.Runtime;
using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace aws_sqs.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger)
        {
            _logger = logger;
        }

        [HttpGet(Name = "GetWeatherForecast")]
        public IEnumerable<WeatherForecast> Get()
        {
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
        }

        [HttpPost]
        public async Task Post(WeatherForecast data)
        {
            var credentials = new BasicAWSCredentials("ACCESS KEY", "SECRET");
            var client = new AmazonSQSClient(credentials, RegionEndpoint.APSoutheast2);

            var request = new SendMessageRequest()
            {
                QueueUrl = "https://sqs.ap-southeast-2.amazonaws.com/189107071895/youtube-demo",
                MessageBody = JsonSerializer.Serialize(data),
                //DelaySeconds = 10
            };

            _ = await client.SendMessageAsync(request);
        }
    }
}