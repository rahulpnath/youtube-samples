using Microsoft.AspNetCore.Authorization;

namespace CognitoApiSample;

public class PaidSubscriptionHandler : AuthorizationHandler<SubscriptionTierRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        SubscriptionTierRequirement requirement)
    {
        var subscriptionClaim = context.User.FindFirst("subscription");
        if (subscriptionClaim != null && subscriptionClaim.Value.Equals("Premium", StringComparison.OrdinalIgnoreCase))
        {
            context.Succeed(requirement);
        }
            
        return Task.CompletedTask;
    }
}