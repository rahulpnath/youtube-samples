using System.IO.Compression;
using Amazon.S3;
using Amazon.S3.Model;

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

app.MapGet("/download-zip", async (IAmazonS3 amazonS3Client) =>
    {
        var bucketName = "myapp-data-files";
        var listObjects = await amazonS3Client
            .ListObjectsV2Async(new ListObjectsV2Request() {BucketName = bucketName});
        var zipStream = new MemoryStream();
        using var zipArchive = new ZipArchive(zipStream, ZipArchiveMode.Create, leaveOpen: true);
        foreach (var s3Object in listObjects.S3Objects)
        {
            var file = await amazonS3Client.GetObjectAsync(s3Object.BucketName, s3Object.Key);
            var entry = zipArchive.CreateEntry(file.Key);
            await using var entryStream = entry.Open();
            await file.ResponseStream.CopyToAsync(entryStream);
        }
        zipArchive.Dispose();
        zipStream.Seek(0, SeekOrigin.Begin);
        return Results.File(zipStream, contentType: "application/octet-stream", fileDownloadName: "Files.zip");
    })
    .WithName("DownloadZip")
    .WithOpenApi();

app.MapGet("/stream-zip", async (IAmazonS3 amazonS3Client, HttpResponse httpResponse) =>
    {
        var bucketName = "myapp-data-files";
        var listObjects = await amazonS3Client
            .ListObjectsV2Async(new ListObjectsV2Request() {BucketName = bucketName});
        httpResponse.ContentType = "application/octet-stream";
        httpResponse.Headers.Append("Content-Disposition", "attachment; filename=StreamFile.zip");
        var zipStream = httpResponse.BodyWriter.AsStream();
        using var zipArchive = new ZipArchive(zipStream, ZipArchiveMode.Create, leaveOpen: true);
        foreach (var s3Object in listObjects.S3Objects)
        {
            var file = await amazonS3Client.GetObjectAsync(s3Object.BucketName, s3Object.Key);
            var entry = zipArchive.CreateEntry(file.Key);
            await using var entryStream = entry.Open();
            await file.ResponseStream.CopyToAsync(entryStream);
        }
    })
    .WithName("StreamZip")
    .WithOpenApi();

app.Run();