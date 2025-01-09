namespace RESTful_API.Models
{
    public class OrderModel
    {
        public int OrderID { get; set; }
        public int UserID { get; set; }
        public decimal Total { get; set; }
        public DateTime OrderDate { get; set; }
        public int StatusID { get; set; } // Refers to the OrderStatus table
    }
}
