using TherapyCenter.DTO_s.Doctor;
using TherapyCenter.Entities;
using TherapyCenter.Repositories.Interfaces;
using TherapyCenter.Services.Interfaces;

namespace TherapyCenter.Services.Implementations
{
    public class DoctorService : IDoctorService
    {
        private readonly IDoctorRepository _doctorRepo;
        private readonly ISlotRepository _slotRepo;

        public DoctorService(IDoctorRepository doctorRepo, ISlotRepository slotRepo)
        {
            _doctorRepo = doctorRepo;
            _slotRepo = slotRepo;
        }

        public async Task<IEnumerable<DoctorResponse>> GetAllAsync()
            => (await _doctorRepo.GetAllAsync()).Select(ToResponse);

        public async Task<DoctorResponse?> GetByIdAsync(int doctorId)
        {
            var doctor = await _doctorRepo.GetByIdAsync(doctorId);
            return doctor == null ? null : ToResponse(doctor);
        }

        public async Task<DoctorResponse?> GetByUserIdAsync(int userId)
        {
            var doctor = await _doctorRepo.GetByUserIdAsync(userId);
            return doctor == null ? null : ToResponse(doctor);
        }

        public async Task<IEnumerable<Slot>> GetAvailableSlotsAsync(int doctorId, DateOnly date)
            => await _slotRepo.GetAvailableSlotsByDoctorAsync(doctorId, date);

        private static DoctorResponse ToResponse(Doctor doctor)
        {
            var firstName = doctor.User?.FirstName ?? string.Empty;
            var lastName = doctor.User?.LastName ?? string.Empty;

            return new DoctorResponse
            {
                DoctorId = doctor.DoctorId,
                UserId = doctor.UserId,
                FirstName = firstName,
                LastName = lastName,
                FullName = string.Join(' ', new[] { firstName, lastName }.Where(s => !string.IsNullOrWhiteSpace(s))).Trim(),
                Email = doctor.User?.Email ?? string.Empty,
                Role = doctor.User?.Role ?? string.Empty,
                PhoneNumber = doctor.User?.PhoneNumber,
                Specialization = doctor.Specialization,
                Bio = doctor.Bio,
                AvailableDays = doctor.AvailableDays,
                StartTime = doctor.StartTime,
                EndTime = doctor.EndTime
            };
        }
    }
}
