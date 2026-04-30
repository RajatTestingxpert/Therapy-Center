using TherapyCenter.DTO_s.Payment;
using TherapyCenter.Entities;
using TherapyCenter.Repositories.Interfaces;
using TherapyCenter.Services.Interfaces;

namespace TherapyCenter.Services.Implementations
{
    public class PaymentService : IPaymentService
    {
        private readonly IPaymentRepository _paymentRepo;

        public PaymentService(IPaymentRepository paymentRepo)
        {
            _paymentRepo = paymentRepo;
        }

        public async Task<Payment> RecordPaymentAsync(RecordPaymentRequest request)
        {
            var payment = new Payment
            {
                AppointmentId = request.AppointmentId,
                Amount = request.Amount,
                PaymentMethod = request.PaymentMethod,
                TransactionId = request.TransactionId,
                // If a TransactionId came in, the gateway already confirmed it — mark Paid immediately
                Status = request.TransactionId != null ? "Paid" : "Pending",
                PaidAt = request.TransactionId != null ? DateTime.UtcNow : null
            };

            return await _paymentRepo.CreateAsync(payment);
        }

        public async Task<Payment> MarkAsPaidAsync(int appointmentId, string? transactionId)
        {
            var payment = await _paymentRepo.GetByAppointmentIdAsync(appointmentId)
                          ?? throw new KeyNotFoundException("Payment record not found.");

            payment.Status = "Paid";
            payment.PaidAt = DateTime.UtcNow;
            payment.TransactionId = transactionId;

            return await _paymentRepo.UpdateAsync(payment);
        }

        public async Task<Payment?> GetByAppointmentAsync(int appointmentId)
            => await _paymentRepo.GetByAppointmentIdAsync(appointmentId);

        public async Task<IEnumerable<Payment>> GetByPatientAsync(int patientId)
            => await _paymentRepo.GetByPatientIdAsync(patientId);
    }

}
