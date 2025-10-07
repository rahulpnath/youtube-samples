namespace lambda_json_logging;

public class Order
{
    public string OrderId { get; set; } = default!;
    public string CustomerId { get; set; } = default!;
    public DateTime OrderDate { get; set; }
    public decimal OrderAmount { get; set; }
    public string OrderStatus { get; set; } = default!;
    public bool IsPriority { get; set; }

    public List<OrderItem> Items { get; set; } = new();
    public Address ShippingAddress { get; set; } = new();
}

public class OrderItem
{
    public string Sku { get; set; } = default!;
    public string ProductName { get; set; } = default!;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}

public class Address
{
    public string City { get; set; } = default!;
    public string Country { get; set; } = default!;
    public string PostalCode { get; set; }

    public override string ToString() => $"{City},{Country}";

}