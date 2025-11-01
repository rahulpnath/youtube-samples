using System.Collections.Generic;
using Amazon.CDK;
using Amazon.CDK.AWS.DynamoDB;
using Amazon.CDK.AWS.IAM;
using Amazon.CDK.AWS.Lambda;
using Amazon.CDK.AWS.SQS;
using Amazon.CDK.AWS.StepFunctions;
using Amazon.CDK.AWS.StepFunctions.Tasks;
using Constructs;

namespace StepFunctions;

public class UserOnboardingCDKStack : Stack
{
  internal UserOnboardingCDKStack(Construct scope, string id, IStackProps props = null) : base(scope, id, props)
  {
    // Reference existing resources (you can replace these with actual resource creation if needed)
    var userTable = Table.FromTableName(this, "UserTable", "User");
    var onboardingFunction = Function.FromFunctionArn(
      this,
      "OnboardingFunction",
      "arn:aws:lambda:ap-southeast-2:189107071895:function:user-onboarding:$LATEST"
    );
    var onboardingQueue = Queue.FromQueueArn(
      this,
      "OnboardingQueue",
      "arn:aws:sqs:ap-southeast-2:189107071895:user-onboarding"
    );

    // Import existing IAM role for the state machine
    // Replace this ARN with your actual role ARN
    var stateMachineRole = Role.FromRoleArn(
      this,
      "StateMachineRole",
      "arn:aws:iam::189107071895:role/service-role/StepFunctions-user-onboarding-role-p00atflo4"
    );

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
        {"TableName", "User"},
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
        {"FunctionName", "arn:aws:lambda:ap-southeast-2:189107071895:function:user-onboarding:$LATEST"},
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
        {"QueueUrl", "https://sqs.ap-southeast-2.amazonaws.com/189107071895/user-onboarding"},
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
    new StateMachine(this, "UserOnboardingCDK", new StateMachineProps
    {
      StateMachineName = "UserOnboardingCDK",
      DefinitionBody = DefinitionBody.FromChainable(definition),
      StateMachineType = StateMachineType.STANDARD,
      QueryLanguage = QueryLanguage.JSONATA,
      Comment = "User onboarding workflow",
      Role = stateMachineRole
    });
  }
}
