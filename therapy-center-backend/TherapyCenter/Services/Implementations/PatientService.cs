using TherapyCenter.DTO_s.Patient;
using TherapyCenter.Entities;
using TherapyCenter.Repositories.Interfaces;
using TherapyCenter.Services.Interfaces;

namespace TherapyCenter.Services.Implementations
{
    public class PatientService : IPatientService
    {
        private readonly IPatientRepository _patientRepo;
        private readonly IUserRepository _userRepo;

        public PatientService(IPatientRepository patientRepo, IUserRepository userRepo)
        {
            _patientRepo = patientRepo;
            _userRepo = userRepo;
        }

        public async Task<Patient> CreateAsync(CreatePatientRequest request)
        {
            var patient = new Patient
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                DateOfBirth = request.DateOfBirth,
                Gender = request.Gender,
                MedicalHistory = request.MedicalHistory,
                GuardianId = request.GuardianId
            };

            return await _patientRepo.CreateAsync(patient);
        }

        public async Task<Patient?> GetByIdAsync(int patientId)
            => await _patientRepo.GetByIdAsync(patientId);

        public async Task<Patient?> GetByUserIdAsync(int userId)
            => await _patientRepo.GetByUserIdAsync(userId);

        public async Task<Patient> GetOrCreateByUserIdAsync(int userId)
        {
            var existing = await _patientRepo.GetByUserIdAsync(userId);
            if (existing != null)
                return existing;

            var user = await _userRepo.GetByIdAsync(userId)
                       ?? throw new KeyNotFoundException("User not found.");

            if (!string.Equals(user.Role?.Trim(), "Patient", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Only patient users can auto-create patient profiles.");

            var patient = new Patient
            {
                UserId = user.UserId,
                FirstName = user.FirstName,
                LastName = user.LastName
            };

            return await _patientRepo.CreateAsync(patient);
        }

        public async Task<IEnumerable<Patient>> GetAllAsync()
            => await _patientRepo.GetAllAsync();

        public async Task<IEnumerable<Patient>> GetByGuardianAsync(int guardianId)
            => await _patientRepo.GetByGuardianIdAsync(guardianId);
    }
}
