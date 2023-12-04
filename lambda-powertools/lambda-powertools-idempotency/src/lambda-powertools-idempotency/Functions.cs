using Amazon.DynamoDBv2.DataModel;
using Amazon.Lambda.Annotations;
using Amazon.Lambda.Annotations.APIGateway;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using AWS.Lambda.Powertools.Idempotency;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace lambda_powertools_idempotency
{
    public class Function
    {
        private readonly IDynamoDBContext _dynamoDBContext;

        public Function(IDynamoDBContext dynamodbContext)
        {
            _dynamoDBContext = dynamodbContext;
            Idempotency.Configure(builder =>
               builder
               .WithOptions(options =>
                        options.WithEventKeyJmesPath("[CityName, Date]")
                        .WithPayloadValidationJmesPath("[TemperatureC]"))
               .UseDynamoDb("IdempotencyTable"));
        }

        [LambdaFunction(Role = "@WeatherApiLambdaExecutionRole")]
        [HttpApi(LambdaHttpMethod.Post, "/weather-forecast")]
        [Idempotent]
        public async Task AddWeatherData(
            APIGatewayHttpApiV2ProxyRequest request,
            [FromBody, IdempotencyKey] WeatherForecast weatherForecast)
        {
            Console.WriteLine("Running time consuming process");
            await Task.Delay(TimeSpan.FromSeconds(5));
            await _dynamoDBContext.SaveAsync(weatherForecast);
        }
    }

    public class WeatherForecast
    {
        public string CityName { get; set; }
        public DateTime Date { get; set; }
        public int TemperatureC { get; set; }
        public string Summary { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}
