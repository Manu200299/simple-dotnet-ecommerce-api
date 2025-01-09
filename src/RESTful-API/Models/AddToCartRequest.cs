namespace RESTful_API.Models
{
    public class AddToCartRequest
    {
        public int UserID { get; set; }
        public int ProductID { get; set; }
        public int Quantity { get; set; }
        public bool IsShared { get; set; }
    }
}