using TherapyCenter.DTO_s.Finding;
using TherapyCenter.Entities;

namespace TherapyCenter.Services.Interfaces
{
    public interface IFindingService
    {
        Task<DoctorFinding> UpsertAsync(int doctorId, int appointmentId, UpsertFindingRequest request);
        Task<DoctorFinding?> GetByAppointmentAsync(int appointmentId);
        Task<IEnumerable<DoctorFinding>> GetByPatientAsync(int patientId);
    }
}