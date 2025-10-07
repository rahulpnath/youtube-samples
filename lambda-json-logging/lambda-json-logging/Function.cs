using Amazon.Lambda.Core;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace lambda_json_logging;

public class Function
{

    /// <summary>
    /// A simple function that takes a string and does a ToUpper
    /// </summary>
    /// <param name="input">The event for the Lambda function handler to process.</param>
    /// <param name="context">The ILambdaContext that provides methods for logging and describing the Lambda environment.</param>
    /// <returns></returns>
    public Order FunctionHandler(string input, ILambdaContext context)
    {
        var order = LoadOrder();

        context.Logger.LogInformation("Processing order {OrderId} placed on {OrderDate:yyyy-MM-dd} to {@Address} with Items {@Items}"
            , order.OrderId, order.OrderDate, order.ShippingAddress, order.Items);

        return order;
    }

    private Order LoadOrder()
    => new()
    {
        OrderId = "0195627d-b449-7181-9383-7c343e87414f",
        CustomerId = "CUST12345",
        OrderDate = new DateTime(2025, 2, 26, 10, 30, 0, DateTimeKind.Utc),
        OrderAmount = 199.99m,
        OrderStatus = "Shipped",
        IsPriority = true,
        ShippingAddress = new Address { City = "Brisbane", Country = "Australia", PostalCode = "4000" },
        Items =
            {
                new OrderItem { Sku = "SKU123", ProductName = "Wireless Mouse", Quantity = 1, UnitPrice = 49.99m },
                new OrderItem { Sku = "SKU456", ProductName = "Keyboard", Quantity = 1, UnitPrice = 149.99m }
            }
    };
}
