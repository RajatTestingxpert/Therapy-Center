using TherapyCenter.Entities;

namespace TherapyCenter.Repositories.Interfaces
{
    public interface IPatientRepository
    {
        Task<Patient?> GetByIdAsync(int patientId);
        Task<Patient?> GetByUserIdAsync(int userId);
        Task<IEnumerable<Patient>> GetAllAsync();
        Task<IEnumerable<Patient>> GetByGuardianIdAsync(int guardianId);
        Task<Patient> CreateAsync(Patient patient);
        Task<Patient> UpdateAsync(Patient patient);
    }
}
