using System.Text.Json;
using Amazon.StepFunctions;
using Amazon.StepFunctions.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

public static class UserOnboardingHandlers
{
    private static readonly JsonSerializerOptions CamelCaseOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public static async Task<IResult> ProcessUserOnboardingSync(
        [FromBody] UserOnboardingRequest requestBody,
        IAmazonStepFunctions stepFunctions,
        IOptions<StepFunctionsOptions> stepOptions)
    {
        if (!requestBody.IsValid())
            return Results.BadRequest(new {message = "Missing required user onboarding fields."});

        var input = JsonSerializer.Serialize(requestBody, CamelCaseOptions);

        var request = new StartSyncExecutionRequest
        {
            StateMachineArn = stepOptions.Value.WorkflowArnExpress,
            Input = input
        };

        var response = await stepFunctions.StartSyncExecutionAsync(request);

        return response.Status == SyncExecutionStatus.SUCCEEDED
            ? Results.Ok()
            : Results.BadRequest(new {response.Error});
    }

    public static async Task<IResult> ProcessUserOnboarding(
        [FromBody] UserOnboardingRequest requestBody,
        IAmazonStepFunctions stepFunctions,
        IOptions<StepFunctionsOptions> stepOptions)
    {
        if (!requestBody.IsValid())
            return Results.BadRequest(new {message = "Missing required user onboarding fields."});

        var input = JsonSerializer.Serialize(requestBody, CamelCaseOptions);

        var request = new StartExecutionRequest
        {
            StateMachineArn = stepOptions.Value.WorkflowArn,
            Input = input,
            Name = $"user-onboarding-{requestBody.User.UserId}-{Guid.NewGuid()}"
        };

        var response = await stepFunctions.StartExecutionAsync(request);

        return Results.Accepted(null, new
        {
            Message = "User onboarding started",
            ExecutionArn = response.ExecutionArn,
            TrackingId = requestBody.User.UserId
        });
    }

    public static async Task<IResult> GetOnboardingStatus(
        string executionArn,
        IAmazonStepFunctions stepFunctions)
    {
        var request = new DescribeExecutionRequest
        {
            ExecutionArn = executionArn
        };

        var response = await stepFunctions.DescribeExecutionAsync(request);

        return Results.Ok(new
        {
            status = response.Status.Value,
            output = response.Output
        });
    }

    public class UserOnboardingRequest
    {
        public required UserInfo User { get; set; }

        public bool IsValid()
        {
            return User != null && User.IsValid();
        }
    }

    public class UserInfo
    {
        public required string UserId { get; set; }
        public required string Email { get; set; }
        public required string Name { get; set; }
        public required string SignupDate { get; set; }
        public required string Source { get; set; }
        public required Metadata Metadata { get; set; }

        public bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(UserId)
                   && !string.IsNullOrWhiteSpace(Email)
                   && !string.IsNullOrWhiteSpace(Name)
                   && !string.IsNullOrWhiteSpace(SignupDate)
                   && !string.IsNullOrWhiteSpace(Source)
                   && Metadata != null
                   && Metadata.IsValid();
        }
    }

    public class Metadata
    {
        public required string ReferralCode { get; set; }
        public required string Country { get; set; }

        public bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(ReferralCode)
                   && !string.IsNullOrWhiteSpace(Country);
        }
    }

    public class OnboardingResult
    {
        // Define properties matching your Step Function output
        public required string Status { get; set; }

        public required string Message { get; set; }
        // Add more properties as needed
    }
}