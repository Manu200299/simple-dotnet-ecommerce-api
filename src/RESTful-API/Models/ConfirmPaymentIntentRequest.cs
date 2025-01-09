namespace RESTful_API.Models
{
    public class ConfirmPaymentIntentRequest
    {
        // The ID of the PaymentIntent to confirm
        public string PaymentIntentId { get; set; }

        // The PaymentMethod ID obtained from the frontend (tokenized card info)
        public string PaymentMethodId { get; set; }
    }

}
