using System.Collections.Generic;
using Amazon.CDK;
using Amazon.CDK.AWS.StepFunctions;
using Constructs;

namespace StepFunctions;

public class StepFunctionDemoStack : Stack
{
  internal StepFunctionDemoStack(Construct scope, string id, IStackProps props = null) : base(scope, id, props)
  {
    // Pass state with JSONPath parameters
    var passState = new Pass(this, "PassWithParameters", new PassProps
    {
      Comment = "Extract user from input using JSONPath",
      Assign = new Dictionary<string, object>
      {
        {"user.$", "$.user"},
        {"timestamp.$", "$$.State.EnteredTime"}
      }
    });

    // Wait state
    var waitState = new Wait(this, "WaitState", new WaitProps
    {
      Comment = "Wait for 5 seconds",
      Time = WaitTime.Duration(Duration.Seconds(5))
    });

    // JSONATA pass state
    var jsonataPass = new Pass(this, "JsonataPassNew", new PassProps
    {
      Comment = "Transform data with JSONATA",
      QueryLanguage = QueryLanguage.JSONATA,
      Outputs = new Dictionary<string, object>
      {
        {"Greeting", "{% 'Hello ' & $user.name %}"},
        {"Id", "{% $user.userId %}"}
      }
    });
      
    // Chain the states together
    var definition = passState
      .Next(waitState)
      .Next(jsonataPass);

    new StateMachine(this, "CDKDemoStateMachine", new StateMachineProps
    {
      StateMachineName = "CDKDemoStateMachine",
      DefinitionBody = DefinitionBody.FromChainable(definition),
      StateMachineType = StateMachineType.STANDARD,
    });
  }
}