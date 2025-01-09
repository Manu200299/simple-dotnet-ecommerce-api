namespace RESTful_API.Models
{
    public class StripePaymentRequest
    {
        public long Amount { get; set; } // Ammount in cents
        public string Currency { get; set; } = "eur";
        public string PaymentMethodId { get; set; }
    }
}
