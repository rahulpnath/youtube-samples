using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.Lambda.Annotations;
using Amazon.Lambda.Annotations.APIGateway;
using Amazon.Lambda.Core;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace LambdaAnnotationSample.OrderApi;

public class Function
{
    private readonly IDynamoDBContext _dynamodbContext;

    public Function()
    {
        _dynamodbContext = new DynamoDBContext(new AmazonDynamoDBClient());
    }

    [LambdaFunction(Role = "@OrdersApiLambdaExecutionRole")]
    [HttpApi(LambdaHttpMethod.Post, "/order")]
    public async Task PostOrder([FromBody] Order order, ILambdaContext context)
    {
        await _dynamodbContext.SaveAsync(order);
    }

    [LambdaFunction(Role = "@OrdersApiLambdaExecutionRole")]
    [HttpApi(LambdaHttpMethod.Get, "/order/{orderId}")]
    public async Task<Order> GetOrder(string orderId, ILambdaContext context)
    {
        return await _dynamodbContext.LoadAsync<Order>(orderId);
    }

    [LambdaFunction(Role = "@OrdersApiLambdaExecutionRole")]
    [HttpApi(LambdaHttpMethod.Delete, "/order/{orderId}")]
    public async Task DeleteOrder(string orderId, ILambdaContext context)
    {
        await _dynamodbContext.DeleteAsync<Order>(orderId);
    }
}
