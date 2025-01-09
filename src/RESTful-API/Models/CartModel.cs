namespace RESTful_API.Models
{
    public class CartModel
    {
        public int CartID { get; set; }
        public int UserID { get; set; }
        public int ProductID { get; set; }
        public int Quantity { get; set; }
        public bool IsShared { get; set; }
        public string SharedToken { get; set; }
        public string ProductName { get; set; }
        public decimal ProductPrice { get; set; }
    }
}