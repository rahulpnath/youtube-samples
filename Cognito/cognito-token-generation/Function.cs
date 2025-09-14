using Amazon.Lambda.Core;
using System.Text.Json.Nodes;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace cognito_token_generation;

public class Function
{

    public JsonNode FunctionHandler(JsonNode evnt, ILambdaContext context)
    {
        try
        {
            context.Logger.LogLine("Incoming event: " + evnt.ToJsonString());
            // Navigate to response -> claimsAndScopeOverrideDetails -> accessTokenGeneration
            var response = evnt["response"] ??= new JsonObject();
            var claimsScope = response["claimsAndScopeOverrideDetails"] ??= new JsonObject();
            var accessTokenGen = claimsScope["accessTokenGeneration"] ??= new JsonObject();
            accessTokenGen["claimsToAddOrOverride"] ??= new JsonObject();
            // Get birthdate from request.userAttributes
            var birthdate = evnt["request"]?["userAttributes"]?["birthdate"]?.ToString();
            if (!string.IsNullOrEmpty(birthdate))
            {
                ((JsonObject)accessTokenGen["claimsToAddOrOverride"])["birthdate"] = birthdate;
                context.Logger.LogLine($"Added birthdate claim: {birthdate}");
            }

            var subscription = evnt["request"]?["userAttributes"]?["custom:subscription"]?.ToString();
            if (!string.IsNullOrEmpty(subscription))
            {
                ((JsonObject)accessTokenGen["claimsToAddOrOverride"])["subscription"] = subscription;
                context.Logger.LogLine($"Added subscription claim: {subscription}");
            }
        }
        catch (Exception ex)
        {
            context.Logger.LogError(ex, "Error processing event");
        }
        return evnt;
    }
}
