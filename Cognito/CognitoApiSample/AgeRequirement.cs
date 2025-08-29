using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace CognitoApiSample;

public class AgeRequirement(int minimumAge) : IAuthorizationRequirement
{
    public int MinimumAge { get; } = minimumAge;
}

public class AgeRequirementHandler : AuthorizationHandler<AgeRequirement>
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public AgeRequirementHandler(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        AgeRequirement requirement)
    {
        var httpContext = context.Resource as DefaultHttpContext;
        if (httpContext == null)
            return;


        var birthdayClaim = context.User.FindFirst(ClaimTypes.DateOfBirth)?.Value;

        //var birthdate = await GetUserBirthdateAsync(httpContext);
        if (!DateTime.TryParse(birthdayClaim, out var birthdate))
            return;

        var age = CalculateAge(birthdate, DateTime.UtcNow.Date);
        if (age >= requirement.MinimumAge)
        {
            context.Succeed(requirement);
        }
    }

    private async Task<DateTime?> GetUserBirthdateAsync(HttpContext httpContext)
    {
        var authorizationHeader = httpContext.Request.Headers["Authorization"].ToString();
        if (string.IsNullOrWhiteSpace(authorizationHeader))
            return null;

        var userInfoEndpoint = $"{_configuration["JwtBearer:UserPoolDomain"]}/oauth2/userInfo";

        var client = _httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", authorizationHeader);

        try
        {
            var userInfo = await client.GetFromJsonAsync<CognitoUserInfo>(userInfoEndpoint);
            if (userInfo?.birthdate == null)
                return null;

            if (DateTime.TryParse(userInfo.birthdate, out var dob))
                return dob;
        }
        catch
        {
            // Optionally log the error here
        }

        return null;
    }

    private int CalculateAge(DateTime dob, DateTime today)
    {
        var age = today.Year - dob.Year;
        if (dob.Date > today.AddYears(-age))
            age--;

        return age;
    }
}

public class CognitoUserInfo
{
    public string sub { get; set; }
    public string email_verified { get; set; }
    public string birthdate { get; set; }
    public string name { get; set; }
    public string locale { get; set; }
    public string email { get; set; }
    public string username { get; set; }
}

