using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using aws_multi_tenant_di;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddScoped<IAWSOptionsFactory, AWSOptionsFactory>();
builder.Services.AddDefaultAWSOptions(sp => sp.GetService<IAWSOptionsFactory>().AWSOptionsBuilder(), ServiceLifetime.Scoped);

builder.Services.AddAWSService<IAmazonDynamoDB>(lifetime: ServiceLifetime.Scoped);
builder.Services.AddScoped<IDynamoDBContext, DynamoDBContext>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseMiddleware<AWSOptionsMiddleware>();


app.MapGet("/movie", async (int year, IDynamoDBContext context) =>
    {
        var movies = await context.QueryAsync<Movie>(year).GetRemainingAsync();
        return movies.Select(a => a.Title);
    })
    .WithName("GetMoviesByYear");

app.MapGet("/movie/by-header-region", async (int year, HttpContext context) =>
{
    if (!context.Request.Headers.TryGetValue("regionEndpoint", out var regionHeader))
    {
        return Results.BadRequest("Missing 'regionEndpoint' header");
    }

    var regionEndpoint = Amazon.RegionEndpoint.GetBySystemName(regionHeader);
    var dynamoDbClient = new AmazonDynamoDBClient(regionEndpoint);
    var dbContext = new DynamoDBContext(dynamoDbClient);

    var movies = await dbContext.QueryAsync<Movie>(year).GetRemainingAsync();
    return Results.Ok(new
    {
        region = regionHeader.ToString(),
        movies = movies.Select(m => m.Title)
    });
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