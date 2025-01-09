namespace RESTful_API.Models
{
    public class CreateOrderRequest
    {
        public int UserID { get; set; }
        public decimal Total { get; set; }
        public int StatusID { get; set; }
        public List<OrderDetailRequest> OrderDetails { get; set; }
    }

    public class OrderDetailRequest
    {
        public int ProductID { get; set; }
        public int Quantity { get; set; }
        public decimal ProductPrice { get; set; }
    }
}
