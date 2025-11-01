using System.Collections.Generic;
using Amazon.CDK;
using Amazon.CDK.AWS.DynamoDB;
using Amazon.CDK.AWS.IAM;
using Amazon.CDK.AWS.Lambda;
using Amazon.CDK.AWS.Logs;
using Amazon.CDK.AWS.SQS;
using Amazon.CDK.AWS.StepFunctions;
using Amazon.CDK.AWS.StepFunctions.Tasks;
using Constructs;

namespace StepFunctions;

public class UserOnboardingCDKWithResourcesStack : Stack
{
  internal UserOnboardingCDKWithResourcesStack(Construct scope, string id, IStackProps props = null) : base(scope, id, props)
  {
    // Create DynamoDB Table
    var userTable = new Table(this, "UserTable", new TableProps
    {
      TableName = "User-OnboardingWorkflow",
      PartitionKey = new Attribute
      {
        Name = "PK",
        Type = AttributeType.STRING
      },
      SortKey = new Attribute
      {
        Name = "SK",
        Type = AttributeType.STRING
      },
      BillingMode = BillingMode.PAY_PER_REQUEST,
      RemovalPolicy = RemovalPolicy.DESTROY // For demo purposes only
    });

    // Create SQS Queue
    var onboardingQueue = new Queue(this, "OnboardingQueue", new QueueProps
    {
      QueueName = "user-onboarding-workflow-queue",
      VisibilityTimeout = Duration.Seconds(300),
      RetentionPeriod = Duration.Days(14)
    });

    // Create Lambda Function from the user-onboarding-workflow folder
    // CDK will automatically build the .NET project during deployment
    var lambdaLogGroup = new Amazon.CDK.AWS.Logs.LogGroup(this, "OnboardingFunctionLogGroup", new LogGroupProps
    {
      LogGroupName = "/aws/lambda/user-onboarding-workflow",
      Retention = RetentionDays.ONE_WEEK,
      RemovalPolicy = RemovalPolicy.DESTROY
    });

    var onboardingFunction = new Function(this, "OnboardingFunction", new FunctionProps
    {
      Runtime = Runtime.DOTNET_8,
      Handler = "user-onboarding-workflow::user_onboarding_workflow.Function::FunctionHandler",
      Code = Code.FromAsset("../user-onboarding-workflow", new Amazon.CDK.AWS.S3.Assets.AssetOptions
      {
        Bundling = new BundlingOptions
        {
          Image = Runtime.DOTNET_8.BundlingImage,
          Command = new[]
          {
            "bash", "-c", string.Join(" && ", new[]
            {
              "cd /asset-input",
              "export DOTNET_CLI_HOME=\"/tmp/DOTNET_CLI_HOME\"",
              "export XDG_DATA_HOME=\"/tmp/DOTNET_CLI_HOME\"",
              "dotnet publish -c Release -r linux-x64 --self-contained false -o /asset-output"
            })
          }
        }
      }),
      FunctionName = "user-onboarding-workflow",
      Timeout = Duration.Seconds(30),
      MemorySize = 512,
      LogGroup = lambdaLogGroup,
      Environment = new Dictionary<string, string>
      {
        { "TABLE_NAME", userTable.TableName }
      }
    });

    // Grant Lambda permissions to write to CloudWatch Logs (already included by default)

    // Create IAM role for the state machine
    var stateMachineRole = new Role(this, "StateMachineRole", new RoleProps
    {
      AssumedBy = new ServicePrincipal("states.amazonaws.com"),
      Description = "Role for User Onboarding State Machine"
    });

    // Grant permissions to the state machine role
    userTable.GrantReadWriteData(stateMachineRole);
    onboardingFunction.GrantInvoke(stateMachineRole);
    onboardingQueue.GrantSendMessages(stateMachineRole);

    // Add CloudWatch Logs permissions for the state machine
    stateMachineRole.AddToPolicy(new PolicyStatement(new PolicyStatementProps
    {
      Effect = Effect.ALLOW,
      Actions = new[]
      {
        "logs:CreateLogDelivery",
        "logs:GetLogDelivery",
        "logs:UpdateLogDelivery",
        "logs:DeleteLogDelivery",
        "logs:ListLogDeliveries",
        "logs:PutResourcePolicy",
        "logs:DescribeResourcePolicies",
        "logs:DescribeLogGroups"
      },
      Resources = new[] { "*" }
    }));

    // Pass State - Extract user from input
    var passState = new Pass(this, "Pass", new PassProps
    {
      Comment = "Extract user from input",
      QueryLanguage = QueryLanguage.JSONATA,
      Assign = new Dictionary<string, object>
      {
        {"user", "{% $states.input.user %}"}
      }
    });

    // DynamoDB PutItem Task - Save UserDetails
    var saveUserDetails = new CallAwsService(this, "Save UserDetails", new CallAwsServiceProps
    {
      Service = "dynamodb",
      Action = "putItem",
      Parameters = new Dictionary<string, object>
      {
        {"TableName", userTable.TableName},
        {"Item", new Dictionary<string, object>
        {
          {"PK", new Dictionary<string, object> {{"S", "{% $user.userId %}"}}},
          {"SK", new Dictionary<string, object> {{"S", "PROFILE"}}},
          {"userId", new Dictionary<string, object> {{"S", "{% $user.userId %}"}}},
          {"email", new Dictionary<string, object> {{"S", "{% $user.email %}"}}},
          {"name", new Dictionary<string, object> {{"S", "{% $user.name %}"}}},
          {"signupDate", new Dictionary<string, object> {{"S", "{% $user.signupDate %}"}}},
          {"source", new Dictionary<string, object> {{"S", "{% $user.source %}"}}},
          {"metadata", new Dictionary<string, object>
          {
            {"M", new Dictionary<string, object>
            {
              {"referralCode", new Dictionary<string, object> {{"S", "{% $user.metadata.referralCode %}"}}},
              {"country", new Dictionary<string, object> {{"S", "{% $user.metadata.country %}"}}}
            }}
          }}
        }}
      },
      IamResources = new[] { userTable.TableArn },
      QueryLanguage = QueryLanguage.JSONATA
    });

    // Lambda Invoke Task
    var lambdaInvoke = new CallAwsService(this, "Lambda Invoke", new CallAwsServiceProps
    {
      Service = "lambda",
      Action = "invoke",
      Parameters = new Dictionary<string, object>
      {
        {"FunctionName", onboardingFunction.FunctionArn},
        {"Payload", new Dictionary<string, object>
        {
          {"userId", "{% $user.userId %}"},
          {"created", "{% $now() %}"},
          {"source", "{% $user.source %}"},
          {"retryCount", "{% $states.context.State.RetryCount %}"}
        }}
      },
      IamResources = new[] { onboardingFunction.FunctionArn },
      QueryLanguage = QueryLanguage.JSONATA
    });

    // Add retry configuration
    lambdaInvoke.AddRetry(new RetryProps
    {
      Errors = new []
      {
        "Lambda.ServiceException",
        "Lambda.AWSLambdaException",
        "Lambda.SdkClientException",
        "Lambda.TooManyRequestsException",
        "LambdaServiceException"
      },
      Interval = Duration.Seconds(1),
      MaxAttempts = 3,
      BackoffRate = 2,
      JitterStrategy = JitterType.FULL
    });

    // Pass State (2) - Error handling for invalid user data
    var passStateError = new Pass(this, "Pass (2)", new PassProps
    {
      Comment = "cannot process this record",
      QueryLanguage = QueryLanguage.JSONATA
    });

    // Pass State (1) - No referral code
    var passStateNoAction = new Pass(this, "Pass (1)", new PassProps
    {
      Comment = "No Action",
      QueryLanguage = QueryLanguage.JSONATA
    });

    // SQS SendMessage Task
    var sqsSendMessage = new CallAwsService(this, "SQS SendMessage", new CallAwsServiceProps
    {
      Service = "sqs",
      Action = "sendMessage",
      Parameters = new Dictionary<string, object>
      {
        {"QueueUrl", onboardingQueue.QueueUrl},
        {"MessageBody", new Dictionary<string, object>
        {
          {"type", "UserOnboarded"},
          {"userId", "{% $user.userId %}"},
          {"signupDate", "{% $user.signupDate %}"},
          {"createdDate", "{% $now() %}"}
        }}
      },
      IamResources = new[] { onboardingQueue.QueueArn },
      QueryLanguage = QueryLanguage.JSONATA
    });

    // Add catch block to Lambda Invoke
    lambdaInvoke.AddCatch(passStateError, new CatchProps
    {
      Errors = new [] { "InvalidUserDataException" },
    });

    // Choice State - Check if referral code exists
    var choiceState = new Choice(this, "Choice");
    choiceState.When(
      Condition.Jsonata("{% $exists($user.metadata.referralCode) and $length($user.metadata.referralCode) > 0 %}"),
      lambdaInvoke.Next(sqsSendMessage),
      new ChoiceTransitionOptions
      {
        Comment = "User has referral code"
      }
    );
    choiceState.Otherwise(passStateNoAction.Next(sqsSendMessage));

    // Chain the states together
    var definition = passState
      .Next(saveUserDetails)
      .Next(choiceState);

    // Ensure error path also goes to SQS
    passStateError.Next(sqsSendMessage);

    // Create the State Machine
    var stateMachine = new StateMachine(this, "UserOnboardingCDKWithResource", new StateMachineProps
    {
      DefinitionBody = DefinitionBody.FromChainable(definition),
      StateMachineType = StateMachineType.STANDARD,
      QueryLanguage = QueryLanguage.JSONATA,
      Comment = "User onboarding workflow with all resources",
      StateMachineName = "UserOnboardingCDKWithResource",
      Role = stateMachineRole
    });

    // Output the important resource ARNs
    new CfnOutput(this, "UserTableName", new CfnOutputProps
    {
      Value = userTable.TableName,
      Description = "DynamoDB User Table Name"
    });

    new CfnOutput(this, "QueueUrl", new CfnOutputProps
    {
      Value = onboardingQueue.QueueUrl,
      Description = "SQS Queue URL"
    });

    new CfnOutput(this, "LambdaFunctionArn", new CfnOutputProps
    {
      Value = onboardingFunction.FunctionArn,
      Description = "Lambda Function ARN"
    });

    new CfnOutput(this, "StateMachineArn", new CfnOutputProps
    {
      Value = stateMachine.StateMachineArn,
      Description = "State Machine ARN"
    });
  }
}
