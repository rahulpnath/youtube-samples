using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.Lambda.Core;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace hello_cdk_lambda;

public class Function
{
    /// <summary>
    /// A simple function that takes a string and does a ToUpper
    /// </summary>
    /// <param name="input">The event for the Lambda function handler to process.</param>
    /// <param name="context">The ILambdaContext that provides methods for logging and describing the Lambda environment.</param>
    /// <returns></returns>
    public async Task FunctionHandler(string input, ILambdaContext context)
    {
        var dynamoDbContext = new DynamoDBContext(new AmazonDynamoDBClient());
        await dynamoDbContext.SaveAsync(new User() {Id = Guid.NewGuid().ToString(), Username = input});
    }
}

public class User
{
    public string Username { get; set; }
    public string Id { get; set; }
}