// See https://aka.ms/new-console-template for more information

using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;
using dynamodb_transactions;

Console.WriteLine("Hello, DynamoDB Transactions!");

var client = new AmazonDynamoDBClient();
var eCommerceOperations = new ECommerceOperations(client);

await eCommerceOperations.SetupDemoTables();

await ProcessOrder("C1", "P2", "O4", 1, client);

Console.ReadKey();

async Task ProcessOrder(string customerId, string productId, string orderId, int orderQuantity, IAmazonDynamoDB dynamoDbClient)
{
    var items = new List<TransactWriteItem>()
        {
            new()
            {
                Put = new Put()
                {
                    TableName = "Orders",
                    Item = new Dictionary<string, AttributeValue>()
                    {
                        {"OrderId", new AttributeValue() {S = orderId}},
                        {"CustomerId", new AttributeValue {S = customerId}},
                        {"ProductId", new AttributeValue {S = productId}},
                        {"Quantity", new AttributeValue {N = orderQuantity.ToString()}},
                        {"DateTime", new AttributeValue {S = DateTime.Now.ToString()}},
                    }
                }
            },
            new()
            {
                Update = new Update()
                {
                    TableName = "ProductCatalog",
                    Key = new Dictionary<string, AttributeValue>
                    {
                        {"ProductId", new AttributeValue {S = productId}}
                    },
                    UpdateExpression = "SET Quantity = Quantity - :orderQuantity",
                    ConditionExpression = "Quantity >= :orderQuantity",
                    ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                    {
                        {":orderQuantity", new AttributeValue {N = orderQuantity.ToString()}}
                    }
                }
            },
            new()
            {
                ConditionCheck = new ConditionCheck()
                {
                    TableName = "Customers",
                    Key = new Dictionary<string, AttributeValue>
                    {
                        {"CustomerId", new AttributeValue {S = customerId}}
                    },
                    ConditionExpression = "attribute_exists(CustomerId)"
                }
            }
        };

    var request = new TransactWriteItemsRequest()
    {
        TransactItems = items,
        ReturnConsumedCapacity = ReturnConsumedCapacity.TOTAL,
        ClientRequestToken = orderId
    };

    try
    {
        var response = await dynamoDbClient.TransactWriteItemsAsync(request);
    }
    catch (TransactionCanceledException ex)
    {
        Console.WriteLine("Transaction was cancelled. Reason: " + ex.Message);
    }
    catch (Exception ex)
    {
        Console.WriteLine("An error occurred: " + ex.Message);
    }
}