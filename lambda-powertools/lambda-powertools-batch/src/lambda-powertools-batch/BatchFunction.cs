using Amazon.Lambda.Annotations;
using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using AWS.Lambda.Powertools.BatchProcessing;
using AWS.Lambda.Powertools.BatchProcessing.Sqs;

namespace lambda_powertools_batch
{
    public class BatchFunction
    {
        [LambdaFunction(Role = "@NotifyCustomersLambdaExecutionRole")]
        public BatchItemFailuresResponse NotifyCustomers(SQSEvent evnt, ILambdaContext context)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            context.Logger.LogInformation($"Received Message of count {evnt.Records.Count}");

            var result = NotifyCustomersBatch(evnt, context);

            watch.Stop();
            Console.WriteLine($"Total time in seconds to process {evnt.Records.Count} messages - {watch.Elapsed.TotalSeconds}"); ;

            return result;
        }

        [BatchProcessor(RecordHandler = typeof(NotifyCustomerRecordHandler), BatchParallelProcessingEnabled = true, MaxDegreeOfParallelism = -1)]
        public BatchItemFailuresResponse NotifyCustomersBatch(SQSEvent evnt, ILambdaContext context)
        {
            return SqsBatchProcessor.Result.BatchItemFailuresResponse;
        }
    }

    public class NotifyCustomerRecordHandler : ISqsRecordHandler
    {
        public async Task<RecordHandlerResult> HandleAsync(SQSEvent.SQSMessage message, CancellationToken cancellationToken)
        {
            await Task.Delay(1000);
            if (message.Body.Contains("Exception"))
                throw new Exception($"Invalid Message - {message.Body}");

            Console.WriteLine($"Processed Message - {message.Body}");

            return await Task.FromResult(RecordHandlerResult.None);
        }
    }
}
