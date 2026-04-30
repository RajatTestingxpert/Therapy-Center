using TherapyCenter.Entities;

namespace TherapyCenter.Repositories.Interfaces
{
    public interface IPaymentRepository
    {

        Task<Payment?> GetByAppointmentIdAsync(int appointmentId);
        Task<IEnumerable<Payment>> GetByPatientIdAsync(int patientId);
        Task<Payment> CreateAsync(Payment payment);
        Task<Payment> UpdateAsync(Payment payment);
    }
}
