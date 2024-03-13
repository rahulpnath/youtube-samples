var source = new CancellationTokenSource();
Console.WriteLine("Press any key to cancel the operation...");
var longRunningTask = LongRunningOperationAsync(source.Token);

Console.ReadKey();
Console.WriteLine("Key Pressed");
source.Cancel();

await longRunningTask;

async Task LongRunningOperationAsync(CancellationToken cancellationToken)
{
    try
    {
        var i = 0;
        while (i++ < 10)
        {
            cancellationToken.ThrowIfCancellationRequested();
            // Simulate some work
            Console.WriteLine($"Working {i}");
            await Task.Delay(2000, cancellationToken);
            Console.WriteLine($"Completed {i}");
        }
    }
    catch (OperationCanceledException )
    {
        Console.WriteLine("User cancelled the operation");
    }
}