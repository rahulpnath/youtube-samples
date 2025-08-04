using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();


builder.Services.AddAWSService<IAmazonDynamoDB>();
builder.Services.AddSingleton<IDynamoDBContext, DynamoDBContext>();

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
    .WithName("GetMoviesByYear");


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