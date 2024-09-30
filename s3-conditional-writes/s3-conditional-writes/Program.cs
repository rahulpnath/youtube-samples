using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAWSService<IAmazonS3>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/get-file", async (
        string fileName,
        string etag,
        IAmazonS3 s3Client) =>
    {
        var file = await s3Client.GetObjectAsync(new GetObjectRequest()
        {
            BucketName = "user-service-large-messages",
            Key = fileName,
            EtagToNotMatch = etag
        });
       
        return file.Key;
       
    })
    .WithName("GetFile")
    .WithOpenApi();

app.MapPost("/upload-file", async (
        [FromForm] FileUploadRequest request,
        IAmazonS3 s3Client,
        CancellationToken cancellationToken) =>
    {
        await s3Client.PutObjectAsync(new PutObjectRequest()
        {
            BucketName = "user-service-large-messages",
            Key = request.File.FileName,
            InputStream = request.File.OpenReadStream(),
            IfNoneMatch = "*"
        }, cancellationToken);

        return Results.Ok();
    })
    .WithName("UploadFile")
    .DisableAntiforgery()
    .WithOpenApi();


app.Run();

record FileUploadRequest(IFormFile File);