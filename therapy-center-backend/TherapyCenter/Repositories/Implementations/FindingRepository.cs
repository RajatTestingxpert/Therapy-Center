using TherapyCenter.Data;
using TherapyCenter.Entities;
using TherapyCenter.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace TherapyCenter.Repositories.Implementations
{
    public class FindingRepository : IFindingRepository
    {
        private readonly AppDbContext _context;
        public FindingRepository(AppDbContext context) => _context = context;

        public async Task<DoctorFinding?> GetByAppointmentIdAsync(int appointmentId)
            => await _context.DoctorFindings
                             .Include(f => f.Appointment)
                             .FirstOrDefaultAsync(f => f.AppointmentId == appointmentId);

        public async Task<IEnumerable<DoctorFinding>> GetByPatientIdAsync(int patientId)
            => await _context.DoctorFindings
                             .Include(f => f.Appointment)
                             .Where(f => f.Appointment.PatientId == patientId)
                             .OrderByDescending(f => f.CreatedAt)
                             .ToListAsync();

        public async Task<DoctorFinding> CreateAsync(DoctorFinding finding)
        {
            _context.DoctorFindings.Add(finding);
            await _context.SaveChangesAsync();
            return finding;
        }

        public async Task<DoctorFinding> UpdateAsync(DoctorFinding finding)
        {
            _context.DoctorFindings.Update(finding);
            await _context.SaveChangesAsync();
            return finding;
        }
    }
}
