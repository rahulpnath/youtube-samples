using Amazon.Lambda.Core;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace lambda_schedule;

public class Function
{

    /// <summary>
    /// A simple function that takes a string and does a ToUpper
    /// </summary>
    /// <param name="input"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    //public string FunctionHandler(ScheduledEvent input, ILambdaContext context)
    public string FunctionHandler(string input, ILambdaContext context)
    {
        Console.WriteLine($"Scheduled Task Run with {input}");
        return input.ToUpper();
    }
}
