using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.SimpleNotificationService;
using Amazon.SQS;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// DynamoDB – Direct interface-to-class registration
builder.Services.AddSingleton<IAmazonDynamoDB, AmazonDynamoDBClient>();
builder.Services.AddSingleton<IDynamoDBContext, DynamoDBContext>();

// S3 – Explicit region
builder.Services.AddSingleton<IAmazonS3>(sp =>
    new AmazonS3Client(RegionEndpoint.APSouth1));

// SQS – Explicit credentials
builder.Services.AddSingleton<IAmazonSQS>(sp =>
{
    var creds = new BasicAWSCredentials("ACCESS_KEY", "SECRET_KEY");
    return new AmazonSQSClient();
});

// SNS – From configuration
builder.Services.AddSingleton<IAmazonSimpleNotificationService>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var key = config["AWS:AccessKey"];
    var secret = config["AWS:SecretKey"];
    var region = RegionEndpoint.GetBySystemName(config["AWS:Region"]);
    return new AmazonSimpleNotificationServiceClient(key, secret, region);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();


app.MapGet("/movie", async (int year, IDynamoDBContext context) 
        =>
    {
        var movies = await context.QueryAsync<Movie>(year).GetRemainingAsync();
        return movies.Select(a => a.Title);
    })
    .WithName("GetWeatherForecast");

app.MapPost("/movie", async (Movie movie, IDynamoDBContext dbContext, IAmazonS3 s3Client) =>
{
    // Save to DynamoDB
    await dbContext.SaveAsync(movie);

    // Save to S3
    var json = System.Text.Json.JsonSerializer.Serialize(movie);
    var putRequest = new Amazon.S3.Model.PutObjectRequest
    {
        BucketName = "rahul-mumbai-region-bucket",
        Key = $"movies/{movie.Year}-{movie.Title}.json",
        ContentBody = json,
        ContentType = "application/json"
    };
    await s3Client.PutObjectAsync(putRequest);

    return Results.Created($"/movie?year={movie.Year}", movie);
});

app.Run();

public class Movie
{
    public int Year { get; set; }
    public string Title { get; set; }
    public List<string> Cast { get; set; }
    public string Description { get; set; }
    public List<string> Genre { get; set; }
    public bool IsAvailableForStreaming { get; set; }
    public List<string> Languages { get; set; }
    public double Rating { get; set; }
}