using Amazon.Lambda.Core;
using Amazon.S3;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace lambda_snap_start;

public class Function
{
    private Guid _constructorId;
    private readonly string _configuration;

    public Function()
    {
        _configuration = ReadConfigFromS3();

        SnapshotRestore.RegisterAfterRestore(() =>
        {
            _constructorId = Guid.NewGuid();
            return ValueTask.CompletedTask;
        });
    }

    public async Task<string> FunctionHandler(string input, ILambdaContext context)
    {
        await SimulateWork();

        var message = $"Constructor Id - {_constructorId}. Function Id - {Guid.NewGuid()}. {_configuration}. Function Input {input}";
        context.Logger.LogInformation(message);

        return message;
    }

    private string ReadConfigFromS3()
    {
        var s3Client = new AmazonS3Client();
        var configurationObject = s3Client.GetObjectAsync("myapp-data-files", "configuration.txt").Result;
        using var reader = new StreamReader(configurationObject.ResponseStream);
        return reader.ReadToEnd();
    }

    private async Task SimulateWork()
    {
        await Task.Delay(TimeSpan.FromSeconds(2));
    }
}
