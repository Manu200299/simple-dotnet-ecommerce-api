namespace RESTful_API.Models
{
    public class CreatePaymentIntentRequest
    {
        public int OrderID { get; set; }
        public int PaymentMethodID { get; set; }

    }
}
