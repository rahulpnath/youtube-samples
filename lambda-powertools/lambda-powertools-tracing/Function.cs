using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.Lambda.Annotations;
using Amazon.Lambda.Annotations.APIGateway;
using Amazon.Lambda.Core;
using AWS.Lambda.Powertools.Tracing;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace lambda_powertools_tracing;

public class Function
{
    private DynamoDBContext dynamoDBContext;

    public Function()
    {
        Tracing.RegisterForAllServices();
        //Tracing.Register<IAmazonDynamoDB>();
        dynamoDBContext = new DynamoDBContext(new AmazonDynamoDBClient());
    }

    /// <summary>
    /// A simple function that takes a string and does a ToUpper
    /// </summary>
    /// <param name="input"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    [LambdaFunction(Role = "@WeatherApiLambdaExecutionRole")]
    [HttpApi(LambdaHttpMethod.Get, "/{cityName}")]
    [Tracing(CaptureMode = TracingCaptureMode.Error)]
    public async Task<List<WeatherForecast>> FunctionHandler(string cityName, ILambdaContext context)
    {
        Tracing.AddAnnotation("city", cityName);
        var results = await GetDataFromDatabase(cityName);

        Tracing.AddMetadata("WeatherCount", results.Count);

        return results;
    }

    [Tracing]
    private async Task<List<WeatherForecast>> GetDataFromDatabase(string cityName)
    {
        return await dynamoDBContext
            .QueryAsync<WeatherForecast>(cityName)
            .GetRemainingAsync();
    }

    [Tracing]
    private async Task<List<WeatherForecast>> GenerateDummyData(string cityName)
    {
        Tracing.WithSubsegment("GenerateDummySubSegment", async _ => await Task.Delay(1000));

        return new List<WeatherForecast>
        {
            new WeatherForecast()
            {
                CityName = cityName,
                Date = DateTime.Now,
                TemperatureC = Random.Shared.Next(-10, 40)
            }
        };
    }
}
