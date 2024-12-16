namespace RESTful_API.Models
{
    public class CartsModel
    {
        public int CartID { get; set; }
        public int UserID { get; set; }
        public int ProductID { get; set; }
        public int Quantity { get; set; }
        public int IsShared { get; set; }
        public string SharedToken { get; set; }
        public DateTime AddedAt { get; set; }


    }
}
