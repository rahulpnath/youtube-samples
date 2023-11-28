using Amazon.DynamoDBv2.DataModel;
using Amazon.Lambda.Annotations;
using Amazon.Lambda.Annotations.APIGateway;
using Amazon.Lambda.Core;

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

        [LambdaFunction(Role = "@WeatherApiLambdaExecutionRole")]
        [HttpApi(LambdaHttpMethod.Get, "/{cityName}")]
        public async Task<List<WeatherForecast>> GetWeather(string cityName, ILambdaContext context)
        {
            var results = await _dynamoDBContext
                .QueryAsync<WeatherForecast>(cityName)
                .GetRemainingAsync();

            return results;
        }
    }
}
