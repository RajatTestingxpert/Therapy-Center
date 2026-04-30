namespace TherapyCenter.DTO_s.Payment
{
    public class RecordPaymentRequest
    {
        public int AppointmentId { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;  // Cash | CreditCard | Insurance
        public string? TransactionId { get; set; }                 // from payment gateway, null for cash
    }
}
