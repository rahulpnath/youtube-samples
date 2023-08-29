namespace LambdaAnnotationSample.OrderApi
{
    public class Order
    {
        public string OrderId { get; set; }
        public decimal Total { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
