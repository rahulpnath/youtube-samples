public class OrderManager
{
    public void ProcessOrder(Order order)
    {
       order.ProcessLargeOrderDiscount();
    }
}

public class Order
{
    public decimal TotalAmount { get; private set; }
    public bool IsDiscountApplied { get; private set; }

    public void ApplyDiscount(decimal discountPercentage)
    {
        if (!IsDiscountApplied)
        {
            TotalAmount -= TotalAmount * 
                           (discountPercentage / 100);
            IsDiscountApplied = true;
        }
    }
    
    public void ProcessLargeOrderDiscount()
    {
        if (TotalAmount > 1000)
        {
            ApplyDiscount(10);
        }
    }
}