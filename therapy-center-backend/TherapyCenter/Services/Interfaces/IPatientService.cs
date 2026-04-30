using TherapyCenter.DTO_s.Patient;
using TherapyCenter.Entities;

namespace TherapyCenter.Services.Interfaces
{
    public interface IPatientService
    {
        Task<Patient> CreateAsync(CreatePatientRequest request);
        Task<Patient?> GetByIdAsync(int patientId);
        Task<Patient?> GetByUserIdAsync(int userId);
        Task<Patient> GetOrCreateByUserIdAsync(int userId);
        Task<IEnumerable<Patient>> GetAllAsync();
        Task<IEnumerable<Patient>> GetByGuardianAsync(int guardianId);
    }
}
