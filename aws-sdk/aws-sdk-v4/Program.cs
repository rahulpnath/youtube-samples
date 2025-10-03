using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;
using Amazon.S3;
using aws_sdk_v4;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddAWSService<IAmazonS3>();
builder.Services.AddAWSService<IAmazonDynamoDB>();
builder.Services.AddSingleton<DynamoDBContext>(sp =>
{
    var client = sp.GetRequiredService<IAmazonDynamoDB>();
    var config = new DynamoDBContextConfig
    {
        RetrieveDateTimeInUtc = false
    };
    return new DynamoDBContext(client, config);
});

Amazon.AWSConfigs.InitializeCollections = true;

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapGet("/weatherforecast-document", async ([FromQuery] string key, IAmazonS3 s3Client) =>
    {
        if (string.IsNullOrEmpty(key))
        {
            return Results.BadRequest("Document key is required.");
        }

        var bucketName = "myapp-data-files";
        var s3Object = await s3Client.GetObjectAsync(bucketName, key).ConfigureAwait(false);

        return Results.Ok(new
        {
            s3Object.Key,
            s3Object.BucketName,
            s3Object.BucketKeyEnabled,
        });
    })
    .WithName("GetWeatherForecastDocument");

app.MapGet("/weatherforecast-documents", async ([FromQuery] string prefix, IAmazonS3 s3Client) =>
    {
        var response = await s3Client.ListObjectsV2Async(new Amazon.S3.Model.ListObjectsV2Request
        {
            BucketName = "myapp-data-files",
            Prefix = prefix
        }, CancellationToken.None).ConfigureAwait(false);


        var documents = response.S3Objects.Select(obj => obj.Key).ToList();
        return Results.Ok(documents);
    })
    .WithName("GetWeatherForecastDocuments");

app.MapGet("/movie/by-year", async (int year, IAmazonDynamoDB dynamoDbClient) =>
{
    var response = await dynamoDbClient.QueryAsync(new QueryRequest
    {
        TableName = "Movie",
        KeyConditionExpression = "#Y = :y",
        ExpressionAttributeValues = new Dictionary<string, AttributeValue>
        {
            {":y", new AttributeValue {N = year.ToString()}}
        },
        ExpressionAttributeNames = new Dictionary<string, string>
        {
            {"#Y", "Year"}
        }
    });

    return Results.Ok(new
    {
        movies = response.Items.Select(m => m["Title"].S).ToList()
    });
}).WithName("GetMoviesByYear");

app.MapGet("/orders/by-customer-id", async (string customerId, DynamoDBContext dbContext) =>
{
    var orders = await dbContext.QueryAsync<Order>(customerId).GetRemainingAsync();

    return Results.Ok(new
    {
        movies = orders.Select(o => new { OrderId = o.SK, DateTime = o.OrderDate }).ToList()
    });
}).WithName("GetOrdersByCustomerId");


app.Run();