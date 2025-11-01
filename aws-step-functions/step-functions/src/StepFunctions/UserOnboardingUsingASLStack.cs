using System;
using System.IO;
using Amazon.CDK;
using Amazon.CDK.AWS.IAM;
using Amazon.CDK.AWS.StepFunctions;
using Constructs;

namespace StepFunctions;

public class UserOnboardingUsingASLStack : Stack
{
    public UserOnboardingUsingASLStack(Construct scope, string id, IStackProps props = null)
        : base(scope, id, props)
    {
        // Load ASL definition from file
        var aslFilePath = Path.Combine(AppContext.BaseDirectory, "user-onboarding.asl.json");
        var aslDefinition = File.ReadAllText(aslFilePath);

        var stateMachineRole = Role.FromRoleArn(
            this,
            "StateMachineRole",
            "arn:aws:iam::189107071895:role/service-role/StepFunctions-user-onboarding-role-p00atflo4"
        );

        // Create the state machine with the ASL definition
        var stateMachine = new StateMachine(
            this,
            "UserOnboardingCDKASL",
            new StateMachineProps
            {
                StateMachineName = "UserOnboardingCDKASL",
                DefinitionBody = DefinitionBody.FromString(aslDefinition),
                StateMachineType = StateMachineType.STANDARD,
                Role = stateMachineRole,
            }
        );

        // Outputs
        new CfnOutput(
            this,
            "StateMachineArn",
            new CfnOutputProps
            {
                Value = stateMachine.StateMachineArn,
                Description = "User Onboarding State Machine ARN",
            }
        );
    }
}
