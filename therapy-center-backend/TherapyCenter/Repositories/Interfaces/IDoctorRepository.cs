using TherapyCenter.Entities;

namespace TherapyCenter.Repositories.Interfaces
{
    public interface IDoctorRepository
    {
        Task<Doctor?> GetByIdAsync(int doctorId);
        Task<Doctor?> GetByUserIdAsync(int userId);
        Task<IEnumerable<Doctor>> GetAllAsync();
        Task<Doctor> CreateAsync(Doctor doctor);
        Task<Doctor> UpdateAsync(Doctor doctor);
    }
}
