using Amazon.Lambda.Core;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace user_onboarding_workflow;

public class Function
{
    // Maximum retries to simulate transient errors
    const int MaxTransientRetries = 3;

    public string FunctionHandler(Request request, ILambdaContext context)
    {
        context.Logger.LogInformation($"User onboarding request: {System.Text.Json.JsonSerializer.Serialize(request)}");

        switch (request.Source?.ToLowerInvariant())
        {
            case "invalid":
                // Simulate invalid user data
                throw new InvalidUserDataException("Invalid user data provided.");

            case "transient":
                // Only throw transient error if retry count < maxTransientRetries
                if (request.RetryCount < MaxTransientRetries)
                {
                    context.Logger.LogInformation("Throwing transient error for retry attempt {RetryAttempt}", request.RetryCount);
                    throw new LambdaServiceException("Temporary Lambda service issue. Try again.");
                }
                else
                {
                    context.Logger.LogInformation("Transient error ignored on retry attempt {RetryAttempt}", request.RetryCount);
                    return "Processed after transient retries";
                }

            case "sdkerror":
                // Simulate an SDK client exception
                throw new LambdaSdkClientException("AWS SDK client failure.");

            case "unhandled":
                // Simulate an unexpected, unhandled exception
                throw new Exception("Unexpected system error occurred.");

            default:
                // Normal successful processing
                context.Logger.LogInformation("User onboarding processed successfully.");
                return "Processed";
        }
    }
}


public class Request
{
    public string UserId { get; set; }
    public DateTime Created { get; set; }
    public string Source { get; set; }
    public int RetryCount { get; set; }
}

// --- Custom exception types for simulation ---

public class InvalidUserDataException(string message) : Exception(message);

public class LambdaServiceException(string message) : Exception(message);

public class LambdaSdkClientException(string message) : Exception(message);
