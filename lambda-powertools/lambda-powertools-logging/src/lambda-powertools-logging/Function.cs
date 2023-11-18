using Amazon.DynamoDBv2.DataModel;
using Amazon.Lambda.Annotations;
using Amazon.Lambda.Annotations.APIGateway;
using Amazon.Lambda.Core;
using AWS.Lambda.Powertools.Logging;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace lambda_powertools_logging
{

    public class Function
    {
        private readonly IDynamoDBContext _dynamoDBContext;
        private ILogger _logger;

        public Function(IDynamoDBContext dynamodbContext)
        {
            _dynamoDBContext = dynamodbContext;
            _logger = Logger.Create<Function>();
        }


        [LambdaFunction(Role = "@WeatherApiLambdaExecutionRole")]
        [HttpApi(LambdaHttpMethod.Get, "/{cityName}")]
        [Logging(LogEvent = true, Service = "GetWeatherForecast", ClearState = true)]
        public async Task<List<WeatherForecast>> FunctionHandler(string cityName, ILambdaContext context)
        {
            Logger.LogInformation("Getting Weatherdata information for city {CityName}", new[] { cityName });

            Logger.AppendKey("CityName", cityName);
            _logger.LogInformation("Getting Weatherdata inforamtion using the created logger");
            var results = await _dynamoDBContext
                .QueryAsync<WeatherForecast>(cityName)
                .GetRemainingAsync();

            if (results.Count >= 10)
                Logger.AppendKey("SpecialCount", results.Count);

            var extraKeys = new Dictionary<string, string>()
            {
                {"CityName", cityName },
                {"Count", results.Count.ToString() }
            };

            _logger.LogInformation(
                extraKeys,
                "Retrieved weather data for City {CityName} with {Count}", new[] { cityName, results.Count.ToString() });

            return results;
        }
    }
}
