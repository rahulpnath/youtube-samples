using Microsoft.AspNetCore.Authorization;

namespace CognitoApiSample;

public class AdminHandler : IAuthorizationHandler
{
    public Task HandleAsync(AuthorizationHandlerContext context)
    {
        if (context.User.IsInRole("Admin"))
        {
            // Admin succeeds ALL pending requirements
            foreach (var requirement in context.PendingRequirements.ToList())
            {
                context.Succeed(requirement);
            }
        }
        
        return Task.CompletedTask;
    }
}