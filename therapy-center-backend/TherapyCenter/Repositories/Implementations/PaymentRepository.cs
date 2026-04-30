using TherapyCenter.Data;
using TherapyCenter.Entities;
using TherapyCenter.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace TherapyCenter.Repositories.Implementations
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly AppDbContext _context;
        public PaymentRepository(AppDbContext context) => _context = context;

        public async Task<Payment?> GetByAppointmentIdAsync(int appointmentId)
            => await _context.Payments.FirstOrDefaultAsync(p => p.AppointmentId == appointmentId);

        public async Task<IEnumerable<Payment>> GetByPatientIdAsync(int patientId)
            => await _context.Payments
                             .Include(p => p.Appointment)
                             .Where(p => p.Appointment.PatientId == patientId)
                             .ToListAsync();

        public async Task<Payment> CreateAsync(Payment payment)
        {
            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();
            return payment;
        }

        public async Task<Payment> UpdateAsync(Payment payment)
        {
            _context.Payments.Update(payment);
            await _context.SaveChangesAsync();
            return payment;
        }
    }

}
