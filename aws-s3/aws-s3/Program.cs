using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<IAmazonS3, AmazonS3Client>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

var bucketName = "myapp-data-files";
app.MapGet("/get-file", async (string key, IAmazonS3 s3Client, HttpResponse response) =>
    {
        var fileObject = await s3Client.GetObjectAsync(bucketName, key);
        response.ContentType = "application/octet-stream";
        response.Headers.Append($"Content-Disposition", $"attachment; filename={fileObject.Key}");
        var responseStream = response.BodyWriter.AsStream();
        await fileObject.ResponseStream.CopyToAsync(responseStream);

    })
    .WithName("GetFile")
    .WithOpenApi();

app.MapPost("/upload-file", async ([FromForm] FileUploadRequest request, IAmazonS3 s3Client) =>
    {
        await s3Client.PutObjectAsync(new PutObjectRequest()
        {
            BucketName = bucketName,
            Key = request.File.FileName,
            InputStream = request.File.OpenReadStream()
        });

        return Results.NoContent();
    })
    .WithName("UploadFile")
    .DisableAntiforgery()
    .WithOpenApi();
;

app.MapDelete("/delete-file", async (string fileName, IAmazonS3 s3Client) =>
    {
        await s3Client.DeleteObjectAsync(bucketName, fileName);
        return Results.NoContent();
    })
    .WithName("DeleteFile")
    .WithOpenApi();
;

app.Run();


public record FileUploadRequest(IFormFile File);