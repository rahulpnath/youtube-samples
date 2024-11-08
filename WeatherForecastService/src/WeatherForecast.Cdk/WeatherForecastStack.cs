using System;
using System.Collections.Generic;
using Amazon.CDK;
using Amazon.CDK.AWS.DynamoDB;
using Amazon.CDK.AWS.Lambda;
using Amazon.CDK.AWS.Lambda.EventSources;
using Amazon.CDK.AWS.SQS;
using Constructs;
using Attribute = Amazon.CDK.AWS.DynamoDB.Attribute;

namespace WeatherForecast.Cdk
{
    public class WeatherForecastStack : Stack
    {
        internal WeatherForecastStack(Construct scope, string id, IStackProps props = null) : base(scope, id, props)
        {
            // DynamoDB 
            var weatherForecastTable = new Table(this, "WeatherForecastData", new TableProps
            {
                TableName = "WeatherForecastData",
                PartitionKey = new Attribute
                {
                    Name = "City",
                    Type = AttributeType.STRING
                },
                SortKey = new Attribute
                {
                    Name = "Date",
                    Type = AttributeType.STRING
                },
                BillingMode = BillingMode.PAY_PER_REQUEST
            });

            // SQS
            var weatherDataQueue = new Queue(this, "WeatherDataQueue", new QueueProps
            {
                QueueName = "weather-data",
                VisibilityTimeout = Duration.Seconds(30)
            });

            // Lambda Function To Process SQS
            var weatherLambdaFunction = new Function(this, "WeatherDataProcessor", new FunctionProps
            {
                FunctionName = "weather-data-processor",
                Runtime = Runtime.DOTNET_8,
                Handler = "WeatherForecast.Service::WeatherForecast.Service.Function::FunctionHandler",
                Code = Code.FromAsset("./artifacts/service.zip"),
                Timeout = Duration.Seconds(30),
                Environment = new Dictionary<string, string>
                {
                    {"DYNAMODB_TABLE_NAME", weatherForecastTable.TableName},
                    {"SQS_QUEUE_URL", weatherDataQueue.QueueUrl}
                }
            });
            // Grant permissions for Lambda to access SQS
            weatherForecastTable.GrantReadWriteData(weatherLambdaFunction);
            // Add SQS as an event source for the Lambda function
            weatherLambdaFunction.AddEventSource(new SqsEventSource(weatherDataQueue));

            // Lambda Function to Host API
            var environmentVariables = new Dictionary<string, string>();
            var environment = this.Node.TryGetContext("ASPNETCORE_ENVIRONMENT")?.ToString();
            if (!string.IsNullOrEmpty(environment))
            {
                Console.WriteLine("Setting ASPNETCORE_ENVIRONMENT variable");
                environmentVariables.Add("ASPNETCORE_ENVIRONMENT", environment);
            }
            
            var apiLambdaFunction = new Function(this, "WeatherForecastApi", new FunctionProps
            {
                FunctionName = "weather-forecast-api",
                Runtime = Runtime.DOTNET_8,
                Code = Code.FromAsset("./artifacts/api.zip"),
                Handler = "WeatherForecast.Api",
                MemorySize = 512,
                Timeout = Duration.Seconds(30),
                Environment = environmentVariables
            });

            // Add a Function URL to the Lambda
            var functionUrl = apiLambdaFunction.AddFunctionUrl(new FunctionUrlOptions
            {
                AuthType = FunctionUrlAuthType.NONE
            }); 

            // Output the Function URL
            new CfnOutput(this, "API FunctionUrl", new CfnOutputProps
            {
                Value = functionUrl.Url
            });

            weatherForecastTable.GrantReadWriteData(apiLambdaFunction);
            weatherDataQueue.GrantSendMessages(apiLambdaFunction);
        }
    }
}