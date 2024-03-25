using Amazon.S3;
using Amazon.S3.Model;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/long-running-request", async (CancellationToken cancellationToken) =>
    {
        var randomId = Guid.NewGuid();
        var results = new List<string>();

        for (int i = 0; i < 100; i++)
        {
            if (cancellationToken.IsCancellationRequested)
                return Results.StatusCode(499);

            await Task.Delay(1000);
            var result = $"{randomId} - Result {i}";
            Console.WriteLine(result);
            results.Add(result);
        }

        return Results.Ok(results);
    })
    .WithName("GetAllData")
    .WithOpenApi();

#region Large Upload

app.MapPost("/upload-large-file", async ([FromForm] FileUploadRequest request, CancellationToken cancellationToken) =>
    {
        try
        {
            var s3Client = new AmazonS3Client();
            await s3Client.PutObjectAsync(new PutObjectRequest()
            {
                BucketName = "user-service-large-messages",
                Key = $"{Guid.NewGuid()} - {request.File.FileName}",
                InputStream = request.File.OpenReadStream()
            }, cancellationToken);

            await PerformAdditionalTasks(CancellationToken.None);
            return Results.NoContent();
        }
        catch (OperationCanceledException e)
        {
            return Results.StatusCode(499);
        }
    })
    .WithName("UploadLargeFile")
    .DisableAntiforgery()
    .WithOpenApi();

async Task PerformAdditionalTasks(CancellationToken cancellationToken)
{
    await Task.Delay(1000, cancellationToken);
    
    var snsClient = new AmazonSimpleNotificationServiceClient();
    await snsClient.PublishAsync(new PublishRequest()
    {
        TopicArn = "<SNS TOPIC ARN>",
        Message = "UserUploadedFileEvent"
    }, cancellationToken);
}

#endregion

app.Run();

record FileUploadRequest(IFormFile File)
{
}