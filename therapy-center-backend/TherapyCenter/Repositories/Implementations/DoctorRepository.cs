using TherapyCenter.Data;
using TherapyCenter.Entities;
using TherapyCenter.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace TherapyCenter.Repositories.Implementations
{
    public class DoctorRepository : IDoctorRepository
    {
        private readonly AppDbContext _context;
        public DoctorRepository(AppDbContext context) => _context = context;

        public async Task<Doctor?> GetByIdAsync(int doctorId)
            => await _context.Doctors
                             .Include(d => d.User)
                             .FirstOrDefaultAsync(d => d.DoctorId == doctorId);

        public async Task<Doctor?> GetByUserIdAsync(int userId)
            => await _context.Doctors
                             .Include(d => d.User)
                             .FirstOrDefaultAsync(d => d.UserId == userId);

        public async Task<IEnumerable<Doctor>> GetAllAsync()
            => await _context.Doctors.Include(d => d.User).ToListAsync();

        public async Task<Doctor> CreateAsync(Doctor doctor)
        {
            _context.Doctors.Add(doctor);
            await _context.SaveChangesAsync();
            return doctor;
        }

        public async Task<Doctor> UpdateAsync(Doctor doctor)
        {
            _context.Doctors.Update(doctor);
            await _context.SaveChangesAsync();
            return doctor;
        }
    }
}
