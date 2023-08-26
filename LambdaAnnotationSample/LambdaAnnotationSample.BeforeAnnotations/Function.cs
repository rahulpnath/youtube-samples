using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using System.Net;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace LambdaAnnotationSample.BeforeAnnotations;

public class Function
{
    /// <summary>
    /// A simple function that takes a string and does a ToUpper
    /// </summary>
    /// <param name="input"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public APIGatewayProxyResponse MathPlus(APIGatewayProxyRequest request, ILambdaContext context)
    {
        if (!request.PathParameters.TryGetValue("a", out var aString) ||
            !request.PathParameters.TryGetValue("b", out var bString))
            return BadRequest();

        if (!int.TryParse(aString, out var a) || !int.TryParse(bString, out var b))
            return BadRequest();

        var sum = a + b;
        return OkResult(sum);
    }

    private static APIGatewayProxyResponse OkResult(int sum)
    {
        return new APIGatewayProxyResponse
        {
            StatusCode = (int)HttpStatusCode.OK,
            Body = sum.ToString(),
            Headers = new Dictionary<string, string> { { "Content-Type", "text/plain" } }
        };
    }

    private static APIGatewayProxyResponse BadRequest()
    {
        return new APIGatewayProxyResponse
        {
            StatusCode = (int)HttpStatusCode.BadRequest
        };
    }
}