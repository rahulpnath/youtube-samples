using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;

namespace dynamodb_transactions;

public class ECommerceOperations(IAmazonDynamoDB dynamoDbClient)
{
    private readonly IAmazonDynamoDB _dynamoDbClient = dynamoDbClient;

    public async Task SetupDemoTables()
    {
        await CreateOrderTable();
        await CreateCustomerTable();
        await CreateProductCatalogTable();
        
        await PopulateProductCatalog();
        await PopulateCustomerData();
    }

    private async Task CreateOrderTable()
    {
        var orderTableRequest = new CreateTableRequest
        {
            TableName = "Orders",
            AttributeDefinitions = [new AttributeDefinition {AttributeName = "OrderId", AttributeType = "S"}],
            KeySchema = [new KeySchemaElement {AttributeName = "OrderId", KeyType = "HASH"}],
            ProvisionedThroughput = new ProvisionedThroughput {ReadCapacityUnits = 5, WriteCapacityUnits = 5}
        };
        await CreateTableIfNotExistsAsync(orderTableRequest);
    }
    private async Task CreateProductCatalogTable()
    {
        var productTableRequest = new CreateTableRequest
        {
            TableName = "ProductCatalog",
            AttributeDefinitions = [new AttributeDefinition {AttributeName = "ProductId", AttributeType = "S"}],
            KeySchema = [new KeySchemaElement {AttributeName = "ProductId", KeyType = "HASH"}],
            ProvisionedThroughput = new ProvisionedThroughput {ReadCapacityUnits = 5, WriteCapacityUnits = 5}
        };
        await CreateTableIfNotExistsAsync(productTableRequest);
    }
    private async Task CreateCustomerTable()
    {
        var customerTableRequest = new CreateTableRequest
        {
            TableName = "Customers",
            AttributeDefinitions = [new AttributeDefinition {AttributeName = "CustomerId", AttributeType = "S"}],
            KeySchema = [new KeySchemaElement {AttributeName = "CustomerId", KeyType = "HASH"}],
            ProvisionedThroughput = new ProvisionedThroughput {ReadCapacityUnits = 5, WriteCapacityUnits = 5}
        };
        await CreateTableIfNotExistsAsync(customerTableRequest);
    }
    
    private async Task<bool> TableExistsAsync(string tableName)
    {
        try
        {
            var request = new DescribeTableRequest
            {
                TableName = tableName
            };

            var response = await _dynamoDbClient.DescribeTableAsync(request);

            // If we get here, the table exists
            Console.WriteLine($"Table {tableName} exists. Status: {response.Table.TableStatus}");
            return true;
        }
        catch (ResourceNotFoundException)
        {
            // The table doesn't exist
            Console.WriteLine($"Table {tableName} does not exist.");
            return false;
        }
        catch (AmazonDynamoDBException ex)
        {
            // Handle other potential exceptions
            Console.WriteLine($"Error checking table existence: {ex.Message}");
            throw;
        }
    }

    private async Task CreateTableIfNotExistsAsync(CreateTableRequest createTableRequest)
    {
        var tableName = createTableRequest.TableName;
        if (!await TableExistsAsync(tableName))
        {
            await _dynamoDbClient.CreateTableAsync(createTableRequest);

            // Wait for the table to be created
            var describeRequest = new DescribeTableRequest {TableName = tableName};
            while (true)
            {
                var response = await _dynamoDbClient.DescribeTableAsync(describeRequest);
                if (response.Table.TableStatus == TableStatus.ACTIVE)
                    break;
                await Task.Delay(1000);
            }
            
            Console.WriteLine($"Table {tableName} created successfully.");
        }
    }

    public async Task PopulateCustomerData()
    {
        var customers = new[]
        {
            new Dictionary<string, AttributeValue>
            {
                { "CustomerId", new AttributeValue { S = "C1" } },
                { "Name", new AttributeValue { S = "John Doe" } },
                { "Email", new AttributeValue { S = "john.doe@example.com" } }
            },
            new Dictionary<string, AttributeValue>
            {
                { "CustomerId", new AttributeValue { S = "C2" } },
                { "Name", new AttributeValue { S = "Jane Smith" } },
                { "Email", new AttributeValue { S = "jane.smith@example.com" } }
            },
            new Dictionary<string, AttributeValue>
            {
                { "CustomerId", new AttributeValue { S = "C3" } },
                { "Name", new AttributeValue { S = "Bob Johnson" } },
                { "Email", new AttributeValue { S = "bob.johnson@example.com" } }
            }
        };

        foreach (var customer in customers)
        {
            try
            {
                var request = new PutItemRequest
                {
                    TableName = "Customers",
                    Item = customer,
                    ConditionExpression = "attribute_not_exists(CustomerId)"
                };

                await _dynamoDbClient.PutItemAsync(request);
                Console.WriteLine($"Added customer {customer["CustomerId"].S} to the database.");
            }
            catch (ConditionalCheckFailedException)
            {
                Console.WriteLine($"Customer {customer["CustomerId"].S} already exists in the database.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding customer {customer["CustomerId"].S}: {ex.Message}");
            }
        }
    }
    private async Task PopulateProductCatalog()
    {
        var products = new[]
        {
            new Dictionary<string, AttributeValue>
            {
                { "ProductId", new AttributeValue { S = "P1" } },
                { "ProductName", new AttributeValue { S = "Laptop" } },
                { "Price", new AttributeValue { N = "999.99" } },
                { "Quantity", new AttributeValue { N = "10" } }
            },
            new Dictionary<string, AttributeValue>
            {
                { "ProductId", new AttributeValue { S = "P2" } },
                { "ProductName", new AttributeValue { S = "Smartphone" } },
                { "Price", new AttributeValue { N = "599.99" } },
                { "Quantity", new AttributeValue { N = "15" } }
            },
            new Dictionary<string, AttributeValue>
            {
                { "ProductId", new AttributeValue { S = "P3" } },
                { "ProductName", new AttributeValue { S = "Headphones" } },
                { "Price", new AttributeValue { N = "199.99" } },
                { "Quantity", new AttributeValue { N = "20" } }
            }
        };
        foreach (var product in products)
        {
            try
            {
                var request = new PutItemRequest
                {
                    TableName = "ProductCatalog",
                    Item = product,
                    ConditionExpression = "attribute_not_exists(ProductId)"
                };

                await _dynamoDbClient.PutItemAsync(request);
                Console.WriteLine($"Added product {product["ProductId"].S} to the catalog.");
            }
            catch (ConditionalCheckFailedException)
            {
                Console.WriteLine($"Product {product["ProductId"].S} already exists in the catalog.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding product {product["ProductId"].S}: {ex.Message}");
            }
        }
    }
}