using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using CognitoApiSample;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddAuthorization(configure =>
{
    configure.AddPolicy("AdminOnly", policy =>
    {
        policy.RequireAuthenticatedUser();
        // policy.RequireClaim("cognito:groups", "Admin");
        policy.Requirements.Add(new AdminOnlyRequirement());
    });

    configure.AddPolicy("CanAccessDetailedWeatherData", policy =>
    {
        policy.AddRequirements(
            new SubscriptionTierRequirement(),
            new GeographicAccessRequirement()
        );
    });

    configure.AddPolicy("Over18Only", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.Requirements.Add(new AgeRequirement(18));
    });
});
builder.Services.AddAWSService<IAmazonCognitoIdentityProvider>();
builder.Services.AddSingleton<IAuthorizationHandler, PaidSubscriptionHandler>();
builder.Services.AddSingleton<IAuthorizationHandler, EducationalInstitutionHandler>();
builder.Services.AddSingleton<IAuthorizationHandler, SuspendedUserHandler>();
builder.Services.AddAWSService<IAmazonCognitoIdentityProvider>();
builder.Services.AddSingleton<IAuthorizationHandler, AdminHandler>();

builder.Services.AddSingleton<IAuthorizationHandler, AdminOnlyRequirementHandler>();
builder.Services.AddTransient<IAuthorizationHandler, AgeRequirementHandler>();
builder.Services.AddHttpClient();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => { builder.Configuration.GetSection("JwtBearer").Bind(options); });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
    {
        var forecast = Enumerable.Range(1, 5).Select(index =>
                new WeatherForecast
                (
                    DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    Random.Shared.Next(-20, 55),
                    summaries[Random.Shared.Next(summaries.Length)]
                ))
            .ToArray();
        return forecast;
    })
    .WithName("GetWeatherForecast")
    .RequireAuthorization();

app.MapPost("/user/subscribe", async (IAmazonCognitoIdentityProvider congnitoIdentityProvider, HttpContext httpContext) =>
    {
        var userName = httpContext.User.Claims.FirstOrDefault(c => c.Type == "username")?.Value;
        var attributeUpdateRequest = new AdminUpdateUserAttributesRequest()
        {
            UserPoolId = "ap-southeast-2_TTjHrnktq",
            Username = userName,
            UserAttributes = new List<AttributeType>()
            {
                new AttributeType()
                {
                    Name = "custom:subscription",
                    Value = "Premium"
                },
                new AttributeType()
                {
                    Name = "locale",
                    Value = "en-AU"
                }
            }
        };
        
        await congnitoIdentityProvider.AdminUpdateUserAttributesAsync(attributeUpdateRequest);
        
        var addUserToGroupRequest = new AdminAddUserToGroupRequest()
        {
            UserPoolId = "ap-southeast-2_TTjHrnktq",
            Username = userName,
            GroupName = "PremiumUsers"
        };
        await congnitoIdentityProvider.AdminAddUserToGroupAsync(addUserToGroupRequest);

    })
    .WithName("UserSubscribe")
    .RequireAuthorization();

app.MapPost(
        "/weatherforecast",
        // [Authorize(Roles = "AdminOnly")]
        [Authorize(Policy = "Over18Only")](WeatherForecast forecast, ILoggerFactory loggerFactory) =>
        {
            var logger = loggerFactory.CreateLogger("WeatherForecastLogger");
            logger.LogInformation($"Received weather forecast: {forecast}");
            return Results.Ok();
        })
    .WithName("PostWeatherForecast")
    .RequireAuthorization();

app.MapGet("/weatherforecast/detailed",
        [Authorize(Policy = "CanAccessDetailedWeatherData")]
        () =>
        {
            var detailedForecast = Enumerable.Range(1, 5).Select(index =>
                new DetailedWeatherForecast(
                    DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    Random.Shared.Next(-20, 55),
                    summaries[Random.Shared.Next(summaries.Length)],
                    Random.Shared.Next(20, 100), // Humidity
                    Random.Shared.Next(0, 40), // WindSpeed
                    Random.Shared.Next(0, 20) // Precipitation
                )
            ).ToArray();
            return detailedForecast;
        })
    .WithName("GetDetailedWeatherForecast")
    .RequireAuthorization();

app.Run();


public class AdminOnlyRequirement : IAuthorizationRequirement
{
}

public class AdminOnlyRequirementHandler : AuthorizationHandler<AdminOnlyRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, AdminOnlyRequirement requirement)
    {
        if (context.User.HasClaim(c => c.Type == "cognito:groups" && c.Value == "Admin"))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int) (TemperatureC / 0.5556);
}

record DetailedWeatherForecast(DateOnly Date, int TemperatureC, string? Summary, int Humidity, int WindSpeed, int Precipitation)
{
    public int TemperatureF => 32 + (int) (TemperatureC / 0.5556);
}