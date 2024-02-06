using Azure.Identity;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

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



app.MapGet("/weatherforecast", async () =>
    {
        var blobClient = new BlobClient(
            new Uri("https://youtubedemoapp.blob.core.windows.net/app-files/weather.json"),
           new DefaultAzureCredential());
        var weatherFile = await blobClient.DownloadContentAsync();

        return JsonSerializer.Deserialize<List<WeatherForecast>>(weatherFile.Value.Content.ToString());
    })
.WithName("GetWeatherForecast")
.WithOpenApi();

app.MapPost("/weatherforecast", async ([FromFormAttribute] FileUploadRequest data) =>
    {
        var blobServiceClient = new BlobServiceClient(
            new Uri("https://youtubedemoapp.blob.core.windows.net"), new DefaultAzureCredential());
        var containerClient = blobServiceClient.GetBlobContainerClient(data.containerName);
        await using var stream = data.file.OpenReadStream();
        //await containerClient.UploadBlobAsync(data.file.FileName, stream);
        var blobClient = containerClient.GetBlobClient(data.file.FileName);
        await blobClient.UploadAsync(stream, overwrite: true);
    })
    .WithName("PostWeatherForecast")
    .DisableAntiforgery()
    .WithOpenApi();


app.MapDelete("/weatherforecast", async (string fileName) =>
    {
        var blobClient = new BlobClient(new Uri($"https://youtubedemoapp.blob.core.windows.net/{fileName}"),
            new DefaultAzureCredential());
        await blobClient.DeleteIfExistsAsync();
    }).WithName("DeleteWeatherForecast")
    .WithOpenApi();

app.Run();

record FileUploadRequest(IFormFile file, string containerName) { }
internal record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
