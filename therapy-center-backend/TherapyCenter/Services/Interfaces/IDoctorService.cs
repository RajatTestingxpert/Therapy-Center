using TherapyCenter.DTO_s.Doctor;
using TherapyCenter.Entities;

namespace TherapyCenter.Services.Interfaces
{
    public interface IDoctorService
    {
        Task<IEnumerable<DoctorResponse>> GetAllAsync();
        Task<DoctorResponse?> GetByIdAsync(int doctorId);
        Task<DoctorResponse?> GetByUserIdAsync(int userId);
        Task<IEnumerable<Slot>> GetAvailableSlotsAsync(int doctorId, DateOnly date);
    }
}
