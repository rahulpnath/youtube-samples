using Amazon.DynamoDBv2.DataModel;
using Amazon.Lambda.Annotations;
using Amazon.Lambda.Annotations.APIGateway;
using Amazon.Lambda.Core;
using AWS.Lambda.Powertools.Metrics;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace lambda_powertools_metrics
{
    public class Function
    {
        private readonly IDynamoDBContext _dynamoDBContext;

        public Function(IDynamoDBContext dynamodbContext)
        {
            _dynamoDBContext = dynamodbContext;
        }

        private Dictionary<string, string> _defaultDimensions = new Dictionary<string, string>()
        {
            {"Environment", "Prod" }
        };

        [LambdaFunction(Role = "@WeatherApiLambdaExecutionRole")]
        [HttpApi(LambdaHttpMethod.Get, "/{cityName}")]
        [Metrics(Namespace = "WeatherStats", Service = "WeatherService")]
        public async Task<List<WeatherForecast>> GetWeather(string cityName, ILambdaContext context)
        {
            Metrics.SetDefaultDimensions(_defaultDimensions);
            Metrics.AddDimension("CityName", cityName);
            Metrics.AddMetric("GetWeather", 1, MetricUnit.Count, MetricResolution.High);
            var results = await _dynamoDBContext
                .QueryAsync<WeatherForecast>(cityName)
                .GetRemainingAsync();

            if (results.Count > 10)
            {
                Metrics.AddDimension("High", cityName);
                Metrics.AddMetadata("ResultCount", results.Count);
            }

            return results;
        }
    }
}
