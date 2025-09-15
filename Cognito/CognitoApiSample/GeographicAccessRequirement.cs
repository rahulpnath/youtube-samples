using Microsoft.AspNetCore.Authorization;

namespace CognitoApiSample;

public class GeographicAccessRequirement : AuthorizationHandler<GeographicAccessRequirement>, IAuthorizationRequirement
{
    public string[] LicensedCountries { get; } = ["en-AU", "en-IN", "en-GB", "en-US"];

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        GeographicAccessRequirement requirement)
    {
        var localeClaim = context.User.FindFirst("locale");

        if (localeClaim != null &&
            requirement.LicensedCountries.Contains(localeClaim.Value, StringComparer.OrdinalIgnoreCase))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}