using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;

namespace lambda_powertools_idempotency
{
    [Amazon.Lambda.Annotations.LambdaStartup]
    public class Startup
    {

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAWSService<IAmazonDynamoDB>();
            services.AddSingleton<IDynamoDBContext, DynamoDBContext>();
        }
    }
}
