using Microsoft.AspNetCore.Authorization;

namespace CognitoApiSample;

public class EducationalInstitutionHandler : AuthorizationHandler<SubscriptionTierRequirement>
{
    public string[] AllowedDepartments { get; } = ["Education", "Research"];
        
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        SubscriptionTierRequirement requirement)
    {
        var departmentClaim = context.User.FindFirst("cognito:groups");
            
        if (departmentClaim != null && 
            AllowedDepartments.Contains(departmentClaim.Value, StringComparer.OrdinalIgnoreCase))
        {
            context.Succeed(requirement);
        }
            
        return Task.CompletedTask;
    }
}