using System;
using System.IO;
using Amazon.CDK;
using Amazon.CDK.AWS.DynamoDB;
using Amazon.CDK.AWS.IAM;
using Amazon.CDK.AWS.Lambda;
using Amazon.CDK.AWS.SQS;
using Amazon.CDK.AWS.StepFunctions;
using Constructs;

namespace StepFunctions;

public class UserOnboardingUsingASLWithResourcesStack : Stack
{
    public UserOnboardingUsingASLWithResourcesStack(Construct scope, string id, IStackProps props = null)
        : base(scope, id, props)
    {
        // Option 1: Define resources in CDK
        var userTable = new Table(
            this,
            "UserTable",
            new TableProps
            {
                TableName = "ASL-User-OnboardingWorkflow",
                PartitionKey = new Amazon.CDK.AWS.DynamoDB.Attribute { Name = "PK", Type = AttributeType.STRING },
                SortKey = new Amazon.CDK.AWS.DynamoDB.Attribute { Name = "SK", Type = AttributeType.STRING },
                BillingMode = BillingMode.PAY_PER_REQUEST,
                RemovalPolicy = RemovalPolicy.DESTROY
            }
        );

        var onboardingQueue = new Queue(
            this,
            "OnboardingQueue",
            new QueueProps
            {
                QueueName = "ASL-user-onboarding-workflow-queue",
                VisibilityTimeout = Duration.Seconds(300)
            }
        );

        // Option 2: Reference existing Lambda function (or create new one)
        // For existing function - use FromFunctionAttributes with skipPermissions
        var lambdaFunctionArn = "arn:aws:lambda:ap-southeast-2:189107071895:function:user-onboarding";
        var lambdaFunction = Function.FromFunctionAttributes(
            this,
            "UserOnboardingFunction",
            new FunctionAttributes
            {
                FunctionArn = lambdaFunctionArn,
                SameEnvironment = true  // Set to true if Lambda is in the same account/region
            }
        );

        // Option 2a: Or create a new Lambda function
        // var lambdaFunction = new Function(this, "UserOnboardingFunction", new FunctionProps
        // {
        //     Runtime = Runtime.NODEJS_20_X,
        //     Handler = "index.handler",
        //     Code = Code.FromAsset("lambda"),
        //     FunctionName = "user-onboarding"
        // });

        // Load ASL definition from file
        var aslFilePath = Path.Combine(AppContext.BaseDirectory, "user-onboarding-cdk.asl.json");
        var aslDefinition = File.ReadAllText(aslFilePath);

        // Replace placeholders with actual resource values
        aslDefinition = aslDefinition
            .Replace("${TableName}", userTable.TableName)
            .Replace("${LambdaFunctionArn}", lambdaFunction.FunctionArn)
            .Replace("${QueueUrl}", onboardingQueue.QueueUrl);

        // Create IAM role for State Machine with necessary permissions
        var stateMachineRole = new Role(
            this,
            "StateMachineRole",
            new RoleProps
            {
                AssumedBy = new ServicePrincipal("states.amazonaws.com"),
                Description = "Role for User Onboarding State Machine"
            }
        );

        // Grant permissions to the state machine role
        userTable.GrantReadWriteData(stateMachineRole);
        onboardingQueue.GrantSendMessages(stateMachineRole);

        // Manually grant Lambda invoke permissions (for imported functions)
        stateMachineRole.AddToPolicy(new PolicyStatement(new PolicyStatementProps
        {
            Effect = Effect.ALLOW,
            Actions = new[] { "lambda:InvokeFunction" },
            Resources = new[] { lambdaFunction.FunctionArn }
        }));

        // Create the state machine with the ASL definition
        var stateMachine = new StateMachine(
            this,
            "UserOnboardingASL",
            new StateMachineProps
            {
                StateMachineName = "ASL-UserOnboardingWithResources",
                DefinitionBody = DefinitionBody.FromString(aslDefinition),
                StateMachineType = StateMachineType.STANDARD,
                Role = stateMachineRole
            }
        );

        // Outputs
        new CfnOutput(
            this,
            "StateMachineArn",
            new CfnOutputProps
            {
                Value = stateMachine.StateMachineArn,
                Description = "User Onboarding State Machine ARN"
            }
        );

        new CfnOutput(
            this,
            "TableName",
            new CfnOutputProps
            {
                Value = userTable.TableName,
                Description = "DynamoDB Table Name"
            }
        );

        new CfnOutput(
            this,
            "QueueUrl",
            new CfnOutputProps
            {
                Value = onboardingQueue.QueueUrl,
                Description = "SQS Queue URL"
            }
        );
    }
}
