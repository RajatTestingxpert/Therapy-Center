namespace TherapyCenter.Entities
{
    public class Payment
    {
        public int PaymentId { get; set; }
        public int AppointmentId { get; set; }
        public Appointment Appointment { get; set; } = null!;
        public decimal Amount { get; set; }
        public string? PaymentMethod { get; set; }   // Cash | CreditCard | Insurance
        public string? TransactionId { get; set; }   // from payment gateway, null for cash
        public string Status { get; set; } = "Pending"; // Pending | Paid | Failed | Refunded
        public DateTime? PaidAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
