using Amazon.Lambda.Annotations;
using Amazon.Lambda.Annotations.APIGateway;
using Amazon.Lambda.Core;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace LambdaAnnotationSample.FromScratch;

public class Function
{
    private readonly IMyDependency _myDependencyCtor;

    public Function(IMyDependency myDependencyCtor)
    {
        _myDependencyCtor = myDependencyCtor;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    [LambdaFunction(Timeout = 100)]
    [HttpApi(LambdaHttpMethod.Get, "/add/{a}/{b}")]
    public List<string> MathPlus(int a, int b, ILambdaContext context, [FromServices] IMyDependency myDependencyFunction)
    {
        return new List<string>()
        {
            _myDependencyCtor.GetValue("Constructor"),
            myDependencyFunction.GetValue("Function"),
            (a + b).ToString()
        };
    }
}
