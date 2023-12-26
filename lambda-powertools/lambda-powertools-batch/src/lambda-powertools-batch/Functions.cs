using Amazon.Lambda.Annotations;
using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace lambda_powertools_batch
{

    public class Functions
    {
        [LambdaFunction(Role = "@NotifyCustomersLambdaExecutionRole")]
        public async Task<SQSBatchResponse> NotifyCustomers(SQSEvent evnt, ILambdaContext context)
        {
            var response = new SQSBatchResponse()
            {
                BatchItemFailures = new List<SQSBatchResponse.BatchItemFailure>()
            };

            var watch = System.Diagnostics.Stopwatch.StartNew();
            context.Logger.LogInformation($"Received Message of count {evnt.Records.Count}");
            foreach (var message in evnt.Records)
            {
                try
                {
                    await ProcessMessageAsync(message, context);
                }
                catch (Exception e)
                {

                    context.Logger.LogError(e.Message);
                    response.BatchItemFailures.Add(new SQSBatchResponse.BatchItemFailure()
                    {
                        ItemIdentifier = message.MessageId
                    });
                }
            }
            watch.Stop();
            Console.WriteLine($"Total time in seconds to process {evnt.Records.Count} messages - {watch.Elapsed.TotalSeconds}"); ;

            return response;
        }

        private async Task ProcessMessageAsync(SQSEvent.SQSMessage message, ILambdaContext context)
        {
            await Task.Delay(1000);
            if (message.Body.Contains("Exception"))
                throw new Exception($"Invalid Message - {message.Body}");

            Console.WriteLine($"Processed Message - {message.Body}");
        }
    }
}
