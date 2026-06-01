using TherapyCenter.DTO_s.Finding;
using TherapyCenter.Entities;
using TherapyCenter.Repositories.Interfaces;
using TherapyCenter.Services.Interfaces;

namespace TherapyCenter.Services.Implementations
{
    public class FindingService : IFindingService
    {
        private readonly IAppointmentRepository _appointmentRepo;
        private readonly IFindingRepository _findingRepo;

        public FindingService(IAppointmentRepository appointmentRepo, IFindingRepository findingRepo)
        {
            _appointmentRepo = appointmentRepo;
            _findingRepo = findingRepo;
        }

        public async Task<DoctorFinding> UpsertAsync(int doctorId, int appointmentId, UpsertFindingRequest request)
        {
            var appointment = await _appointmentRepo.GetByIdAsync(appointmentId)
                              ?? throw new KeyNotFoundException("Appointment not found.");

            if (appointment.DoctorId != doctorId)
                throw new InvalidOperationException("You can only submit reports for your own appointments.");

            var finding = await _findingRepo.GetByAppointmentIdAsync(appointmentId);

            if (finding == null)
            {
                finding = new DoctorFinding
                {
                    AppointmentId = appointmentId
                };
            }

            finding.Observations = request.Observations;
            finding.Recommendations = request.Recommendations;
            finding.NextSessionDate = request.NextSessionDate;

            return finding.FindingId == 0
                ? await _findingRepo.CreateAsync(finding)
                : await _findingRepo.UpdateAsync(finding);
        }

        public async Task<DoctorFinding?> GetByAppointmentAsync(int appointmentId)
            => await _findingRepo.GetByAppointmentIdAsync(appointmentId);

        public async Task<IEnumerable<DoctorFinding>> GetByPatientAsync(int patientId)
            => await _findingRepo.GetByPatientIdAsync(patientId);
    }
}