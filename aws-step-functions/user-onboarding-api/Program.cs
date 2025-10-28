using Amazon.StepFunctions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddAWSService<IAmazonStepFunctions>();
builder.Services.AddSwaggerGen();
builder.Services.AddOpenApi();

builder.Services.Configure<StepFunctionsOptions>(
    builder.Configuration.GetSection("StepFunctions"));
var app = builder.Build();

// Enable Swagger middleware
app.UseSwagger();
app.UseSwaggerUI();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.MapPost("/process-user-onboarding-sync", UserOnboardingHandlers.ProcessUserOnboardingSync);

app.MapPost("/process-user-onboarding", UserOnboardingHandlers.ProcessUserOnboarding);
app.MapGet("/onboarding-status/{executionArn}", UserOnboardingHandlers.GetOnboardingStatus);

app.Run();