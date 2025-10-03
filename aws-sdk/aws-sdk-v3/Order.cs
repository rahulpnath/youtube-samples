namespace aws_sdk_v3;

using Amazon.DynamoDBv2.DataModel;

[DynamoDBTable("OrdersTableTest")]
public class Order
{
    [DynamoDBHashKey]
    public string? PK { get; set; } // CustomerId

    [DynamoDBRangeKey]
    public string? SK { get; set; } // OrderId

    public DateTime? OrderDate { get; set; }
}