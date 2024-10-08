<Query Kind="Program">
  <NuGetReference>AWSSDK.S3</NuGetReference>
  <NuGetReference>System.Linq.Async</NuGetReference>
  <Namespace>Amazon.S3</Namespace>
  <Namespace>Amazon.S3.Model</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
</Query>

async Task Main()
{
	//var numbers = CreateSimpleIterator();
	//"Printing Numbers".Dump();
	//foreach (var number in numbers)
	//{
	//	number.Dump();
	//}
	
	var client = new AmazonS3Client();
	var lines = FetchAndProcessLogsAsync(client, "Android.log", "Service");
	await foreach (var line in lines.Take(200))
	{
		line.Dump();
	}
}

async IAsyncEnumerable<string> FetchAndProcessLogsAsync(
IAmazonS3 s3Client, string logFileKey, string searchTerm)
{
	// Get the S3 object (log file) from the bucket
	var getObjectRequest = new GetObjectRequest
	{
		BucketName = "user-service-large-messages",
		Key = logFileKey
	};

	using var s3Object = await s3Client.GetObjectAsync(getObjectRequest);
	using var stream = s3Object.ResponseStream;
	using var reader = new StreamReader(stream);
	string line;
	while ((line = await reader.ReadLineAsync()) != null)
	{
		if (line.Contains(searchTerm, StringComparison.InvariantCultureIgnoreCase))
			yield return line;
	}
}
static IEnumerable<int> CreateSimpleIterator()
{
	"Starting the loop".Dump();
	var i = 0;
	while (true)
	{
		var tens = i * 10;
		$"Returning for {i}".Dump();
		yield return tens;
		i++;
	}
	"Ending".Dump();
}