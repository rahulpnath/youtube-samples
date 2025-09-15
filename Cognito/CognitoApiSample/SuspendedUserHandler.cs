using Microsoft.AspNetCore.Authorization;

namespace CognitoApiSample;

public class SuspendedUserHandler : AuthorizationHandler<SubscriptionTierRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        SubscriptionTierRequirement requirement)
    {
        // Check if user is in the Suspended group
        if (context.User.IsInRole("Suspended"))
        {
            context.Fail();
        }
            
        return Task.CompletedTask;
    }
}