using Amazon.DynamoDBv2.DataModel;

namespace aws_sdk_v4;

[DynamoDBTable("OrdersTableTest")]
public class Order
{
    [DynamoDBHashKey]
    public string? PK { get; set; } // CustomerId

    [DynamoDBRangeKey]
    public string? SK { get; set; } // OrderId

    public DateTime? OrderDate { get; set; }
}