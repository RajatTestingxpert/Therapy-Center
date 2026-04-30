using TherapyCenter.DTO_s.Payment;
using TherapyCenter.Entities;

namespace TherapyCenter.Services.Interfaces
{
    public interface IPaymentService
    {
        Task<Payment> RecordPaymentAsync(RecordPaymentRequest request);
        Task<Payment> MarkAsPaidAsync(int paymentId, string? transactionId);
        Task<Payment?> GetByAppointmentAsync(int appointmentId);
        Task<IEnumerable<Payment>> GetByPatientAsync(int patientId);
    }
}
