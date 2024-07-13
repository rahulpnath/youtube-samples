// See https://aka.ms/new-console-template for more information

using Amazon.S3;
using Amazon.S3.Model;
using Newtonsoft.Json;

var client = new AmazonS3Client();
const string bucketName = "user-ratings-for-my-app";

var ratings = await GetAllRatings(client);

var filteredRatings = ratings.Where(r => r.Rating == 3).Take(50).Select(r => r.Review);
Console.WriteLine(string.Join(Environment.NewLine, filteredRatings));


return;

async Task<List<ProductRating>> GetAllRatings(AmazonS3Client s3Client)
{
    var s3Objects = await GetAllFileS3Objects(s3Client);
    var productRatings = new List<ProductRating>();
    var fileReadCounter = 0;
    foreach (var s3Object in s3Objects)
    {
        var rating = await GetRating(s3Client, s3Object);
        productRatings.Add(rating);
        Console.WriteLine("File {0} read {1}", s3Object.Key, ++fileReadCounter);
    }

    return productRatings;
}

async Task<IEnumerable<S3Object>> GetAllFileS3Objects(AmazonS3Client amazonS3Client)
{
    var s3ObjectsResponse = await amazonS3Client.ListObjectsV2Async(new ListObjectsV2Request()
    {
        BucketName = bucketName,
        MaxKeys = 10
    });

    return s3ObjectsResponse.S3Objects;
}

async Task<ProductRating> GetRating(AmazonS3Client amazonS3Client, S3Object o)
{
    var getObjectResponse = await amazonS3Client.GetObjectAsync(new GetObjectRequest {BucketName = o.BucketName, Key = o.Key});
    using var reader = new StreamReader(getObjectResponse.ResponseStream);
    var fileContents = await reader.ReadToEndAsync();
    var productRating = JsonConvert.DeserializeObject<ProductRating>(fileContents);

    return productRating!;
}

public record ProductRating(Guid Id, string ProductId, int Rating, string Review, DateTime TimeStamp);