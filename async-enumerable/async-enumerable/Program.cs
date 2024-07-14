// See https://aka.ms/new-console-template for more information

using Amazon.S3;
using Amazon.S3.Model;
using Newtonsoft.Json;

var client = new AmazonS3Client();
const string bucketName = "user-ratings-for-my-app";

var ratings = await GetAllRatings(client);

var filteredRatings = ratings.Select(r => r.Review);
Console.WriteLine(string.Join(Environment.NewLine, filteredRatings));


return;

async Task<List<ProductRating>> GetAllRatings(AmazonS3Client s3Client)
{
    var s3Objects = GetAllFileS3Objects(s3Client);
    return await s3Objects.SelectAwait(async (s3Object, i) =>
        {
            var rating = await GetRating(s3Client, s3Object);
            Console.WriteLine("File {0} read {1}", s3Object.Key, i);
            return rating;
        }).Where(rating => rating.Rating == 5)
        .Take(20)
        .ToListAsync();
}

async IAsyncEnumerable<S3Object> GetAllFileS3Objects(AmazonS3Client amazonS3Client)
{
    ListObjectsV2Response? s3ObjectsResponse = null;
    do
    {
        s3ObjectsResponse = await amazonS3Client.ListObjectsV2Async(new ListObjectsV2Request()
        {
            BucketName = bucketName,
            ContinuationToken = s3ObjectsResponse?.NextContinuationToken,
            StartAfter = s3ObjectsResponse?.StartAfter
        });

        Console.WriteLine("Total Objects retrieved {0}", s3ObjectsResponse.S3Objects.Count);
        foreach (var s3Object in s3ObjectsResponse.S3Objects)
        {
            yield return s3Object;
        }
    } while (s3ObjectsResponse.IsTruncated);
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