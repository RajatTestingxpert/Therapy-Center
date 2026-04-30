using TherapyCenter.Entities;

namespace TherapyCenter.Repositories.Interfaces
{
    public interface IFindingRepository
    {
        Task<DoctorFinding?> GetByAppointmentIdAsync(int appointmentId);
        Task<IEnumerable<DoctorFinding>> GetByPatientIdAsync(int patientId);
        Task<DoctorFinding> CreateAsync(DoctorFinding finding);
        Task<DoctorFinding> UpdateAsync(DoctorFinding finding);
    }
}
