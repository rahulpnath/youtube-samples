using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<IAmazonS3, AmazonS3Client>();

builder.Services.AddCors();

var app = builder.Build();

app.UseCors(builder => builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapPost("/stream-upload-file", async (
        [FromForm] FileUploadRequest request,
        IAmazonS3 s3Client,
        CancellationToken cancellationToken) =>
    {
        await s3Client.PutObjectAsync(new PutObjectRequest()
        {
            BucketName = "user-service-large-messages",
            Key = request.File.FileName,
            InputStream = request.File.OpenReadStream(),
        }, cancellationToken);

        return Results.Ok();
    })
    .WithName("StreamUpload")
    .DisableAntiforgery()
    .WithOpenApi();

app.MapGet("/get-presigned-url", async (
        [FromQuery] string key,
        IAmazonS3 s3Client,
        CancellationToken cancellationToken) =>
    {
       var presignedUrl = await s3Client.GetPreSignedURLAsync(new GetPreSignedUrlRequest()
        {
            BucketName = "user-service-large-messages",
            Key = key,
            Verb = HttpVerb.GET,
            Expires = DateTime.UtcNow.AddMinutes(1)
        });

        return new {url = presignedUrl};
    })
    .WithName("GetPreSignedUrl")
    .DisableAntiforgery()
    .WithOpenApi();


app.MapGet("/get-presigned-url-for-upload", async (
        [FromQuery] string key,
        IAmazonS3 s3Client,
        CancellationToken cancellationToken) =>
    {
        var presignedUrl = await s3Client.GetPreSignedURLAsync(new GetPreSignedUrlRequest()
        {
            BucketName = "user-service-large-messages",
            Key = @$"Rahul\{key}",
            Verb = HttpVerb.PUT,
            Expires = DateTime.UtcNow.AddMinutes(1)
        });

        return new {url = presignedUrl};
    })
    .WithName("GetPreSignedUrlForUpload")
    .DisableAntiforgery()
    .WithOpenApi();

app.Run();

public record FileUploadRequest(IFormFile File);