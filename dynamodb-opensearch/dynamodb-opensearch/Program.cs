using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using OpenSearch.Client;
using OpenSearch.Net.Auth.AwsSigV4;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddAWSService<IAmazonDynamoDB>();
builder.Services.AddSingleton<IDynamoDBContext, DynamoDBContext>();

var settings = new ConnectionSettings(
        new Uri("<OPEN SEARCH URL>"),
        new AwsSigV4HttpConnection())
    .DefaultIndex("<OPEN SEARCH INDEX>")
    .DefaultFieldNameInferrer(p => p);
var client = new OpenSearchClient(settings);
builder.Services.AddSingleton<IOpenSearchClient>(client);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "Movies API "));
}

app.UseHttpsRedirection();

app.MapPost("/movies/", async (Movie movie, IDynamoDBContext dynamoDbContext) =>
    {
        await dynamoDbContext.SaveAsync(movie);
    })
    .WithName("AddMovie");

app.MapPost("/movies/search", async (MoviesSearchRequest request, IOpenSearchClient openSearchClient) =>
{
    var searchDescriptor = new SearchDescriptor<Movie>()
        .Query(q =>
        {
            var mustQueryContainer = new List<QueryContainer>();
            var filterQueryContainer = new List<QueryContainer>();

            // Search Title and Description with the Query
            if (!string.IsNullOrEmpty(request.Query))
            {
                mustQueryContainer.Add(q.MultiMatch(m => m
                    .Fields(f => f.Field(ff => ff.Title).Field(ff => ff.Description))
                    .Query(request.Query)
                ));
            }

            // Filter by IsAvailableForStreaming
            if (request.IsAvailableForStreaming.HasValue)
            {
                filterQueryContainer.Add(q.Term(
                    t => t.Field(f => f.IsAvailableForStreaming)
                        .Value(request.IsAvailableForStreaming.GetValueOrDefault() ? 1L : 0L)));
            }

            // Filter by MinRating
            if (request.MinRating.HasValue)
            {
                filterQueryContainer.Add(
                    q.Range(r => r.Field(f => f.Rating).GreaterThanOrEquals(request.MinRating)));
            }

            // Filter by ReleaseYear
            if (request.ReleaseYear.HasValue)
            {
                filterQueryContainer.Add(q.Term(t => t.Field(f => f.Year).Value(request.ReleaseYear)));
            }

            return q.Bool(b => b
                    .Must(mustQueryContainer.ToArray()) // Scoring conditions
                    .Filter(filterQueryContainer.ToArray()) // Non-scoring conditions
            );
        });
    var response = await openSearchClient.SearchAsync<Movie>(searchDescriptor);
    
    return response.Documents;
});

app.Run();

public class Movie
{
    public int IsAvailableForStreaming { get; set; }
    public string[] Genre { get; set; }
    public string Description { get; set; }
    public string[] Languages { get; set; }
    public double Rating { get; set; }
    public int Year { get; set; }
    public string[] Cast { get; set; }
    public string Title { get; set; }
}

public class MoviesSearchRequest
{
    public string Query { get; set; }
    public bool? IsAvailableForStreaming { get; set; }
    public double? MinRating { get; set; }
    public int? ReleaseYear { get; set; }
}