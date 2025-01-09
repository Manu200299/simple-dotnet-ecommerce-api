public class PaymentRequest
{
    public int OrderID { get; set; }
    public int PaymentStatusID { get; set; }
    public decimal Amount { get; set; }
    public int PaymentMethodID { get; set; }
}
