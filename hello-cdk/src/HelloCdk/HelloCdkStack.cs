using Amazon.CDK;
using Amazon.CDK.AWS.DynamoDB;
using Amazon.CDK.AWS.Lambda;
using Constructs;

namespace HelloCdk
{
    public class HelloCdkStack : Stack
    {
        internal HelloCdkStack(Construct scope, string id, IStackProps props = null) : base(scope, id, props)
        {
            // The code that defines your stack goes here
            var lambda = new Function(this, "hello-cdk-labmda", new FunctionProps()
            {
                FunctionName = "hello-cdk-labmda",
                Runtime = Runtime.DOTNET_8,
                Handler = "hello-cdk-lambda::hello_cdk_lambda.Function::FunctionHandler",
                Code = Code.FromAsset(@".\src\hello-cdk-lambda\src\hello-cdk-lambda\bin\Release\net8.0\hello-cdk-lambda.zip"),
                Timeout = Duration.Seconds(30)
            });

            var userTable = new Table(this, "user-table", new TableProps()
            {
                TableName = "User",
                PartitionKey = new Attribute()
                {
                    Name = "Id",
                    Type = AttributeType.STRING
                },
                RemovalPolicy = RemovalPolicy.DESTROY
            });

            userTable.GrantReadWriteData(lambda);
        }
    }
}
