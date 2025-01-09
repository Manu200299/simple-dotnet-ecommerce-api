namespace RESTful_API.Models
{
    public class OrderDetailModel
    {
        public int OrderDetailID { get; set; }
        public int OrderID { get; set; }
        public int ProductID { get; set; }
        public int Quantity { get; set; }
        public decimal ProductPrice { get; set; } // Product price at the time of order
    }
}
