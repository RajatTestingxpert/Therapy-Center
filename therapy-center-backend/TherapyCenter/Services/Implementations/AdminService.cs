using Microsoft.EntityFrameworkCore;
using TherapyCenter.DTO_s.Admin;
using TherapyCenter.DTO_s.Doctor;
using TherapyCenter.Entities;
using TherapyCenter.Repositories.Interfaces;
using TherapyCenter.Services.Interfaces;

namespace TherapyCenter.Services.Implementations
{
    public class AdminService : IAdminService
    {
        private readonly ITherapyRepository _therapyRepo;
        private readonly IDoctorRepository _doctorRepo;
        private readonly IUserRepository _userRepo;
        private readonly ISlotRepository _slotRepo;
        private readonly IAppointmentRepository _appointmentRepo;

        public AdminService(
            ITherapyRepository therapyRepo,
            IDoctorRepository doctorRepo,
            IUserRepository userRepo,
            ISlotRepository slotRepo,
             IAppointmentRepository appointmentRepo)
        {
            _therapyRepo = therapyRepo;
            _doctorRepo = doctorRepo;
            _userRepo = userRepo;
            _slotRepo = slotRepo;
            _appointmentRepo = appointmentRepo;
        }

        // ── Therapy CRUD ───────────────────────────────────────────────────────

        public async Task<Therapy> CreateTherapyAsync(CreateTherapyRequest request)
        {
            var therapy = new Therapy
            {
                Name = request.Name,
                Description = request.Description,
                DurationMinutes = request.DurationMinutes,
                Cost = request.Cost
            };
            return await _therapyRepo.CreateAsync(therapy);
        }
       

        public async Task<Therapy> UpdateTherapyAsync(int therapyId, UpdateTherapyRequest request)
        {
            var therapy = await _therapyRepo.GetByIdAsync(therapyId)
                          ?? throw new KeyNotFoundException($"Therapy {therapyId} not found.");

            therapy.Name = request.Name;
            therapy.Description = request.Description;
            therapy.DurationMinutes = request.DurationMinutes;
            therapy.Cost = request.Cost;

            return await _therapyRepo.UpdateAsync(therapy);
        }

        public async Task DeleteTherapyAsync(int therapyId)
            => await _therapyRepo.DeleteAsync(therapyId);

        public async Task<IEnumerable<Therapy>> GetAllTherapiesAsync()
            => await _therapyRepo.GetAllAsync();

        // ── Doctor profile ─────────────────────────────────────────────────────

        public async Task<DoctorResponse> CreateDoctorProfileAsync(CreateDoctorProfileRequest request)
        {
            var user = await _userRepo.GetByIdAsync(request.UserId)
                       ?? throw new KeyNotFoundException("User not found.");

            if (!string.Equals(user.Role?.Trim(), "Doctor", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("User must have the Doctor role.");

            if (string.IsNullOrWhiteSpace(request.Specialization))
                throw new InvalidOperationException("Specialization is required.");

            var therapyNames = (await _therapyRepo.GetAllAsync())
                .Select(t => t.Name.Trim())
                .Where(n => !string.IsNullOrWhiteSpace(n))
                .ToList();

            var requestedSpec = request.Specialization.Trim();
            var matchedSpec = therapyNames.FirstOrDefault(n => string.Equals(n, requestedSpec, StringComparison.OrdinalIgnoreCase));
            if (matchedSpec == null)
                throw new InvalidOperationException("Please add this specialization as a therapy first, then select it here.");

            if (await _doctorRepo.GetByUserIdAsync(request.UserId) != null)
                throw new InvalidOperationException("This doctor profile already exists.");

            var doctor = new Doctor
            {
                UserId = request.UserId,
                User = user,
                Specialization = matchedSpec,
                Bio = request.Bio,
                AvailableDays = request.AvailableDays,
                StartTime = request.StartTime,
                EndTime = request.EndTime
            };

            var created = await _doctorRepo.CreateAsync(doctor);
            return ToResponse(created);
        }
        public async Task DeleteDoctorAsync(int doctorId)
        {
            var appointments = await _appointmentRepo.GetByDoctorIdAsync(doctorId);

            if (appointments.Any())
            {
                throw new InvalidOperationException(
                    "Cannot delete doctor with existing appointments."
                );
            }

            await _doctorRepo.DeleteAsync(doctorId);
        }


        public async Task<IEnumerable<User>> GetAllReceptionistsAsync()
            => await _userRepo.GetByRoleAsync("Receptionist");

        // ── Bulk slot generation ───────────────────────────────────────────────

        public async Task<int> GenerateSlotsForDoctorAsync(GenerateSlotsRequest request)
        {
            var doctor = await _doctorRepo.GetByIdAsync(request.DoctorId)
                         ?? throw new KeyNotFoundException("Doctor not found.");

            if (doctor.StartTime == null || doctor.EndTime == null)
                throw new InvalidOperationException("Doctor has no working hours configured.");

            var availableDays = (doctor.AvailableDays ?? "Mon,Tue,Wed,Thu,Fri")
                                .Split(',', StringSplitOptions.RemoveEmptyEntries);

            var slots = new List<Slot>();
            var current = request.FromDate;

            while (current <= request.ToDate)
            {
                var dayAbbr = current.DayOfWeek.ToString()[..3];

                if (availableDays.Contains(dayAbbr, StringComparer.OrdinalIgnoreCase))
                {
                    var slotStart = doctor.StartTime.Value;

                    while (slotStart.AddHours(1) <= doctor.EndTime.Value)
                    {
                        slots.Add(new Slot
                        {
                            DoctorId = request.DoctorId,
                            Date = current,
                            StartTime = slotStart,
                            EndTime = slotStart.AddHours(1),
                            IsBooked = false
                        });

                        slotStart = slotStart.AddHours(1);
                    }
                }

                current = current.AddDays(1);
            }

            await _slotRepo.BulkCreateAsync(slots);
            return slots.Count;
        }
        public async Task DeactivateStaffAsync(int userId)
        {
            var user = await _userRepo.GetByIdAsync(userId)
                       ?? throw new KeyNotFoundException("User not found.");

            if (string.Equals(user.Role, "Admin", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Admin account cannot be removed.");

            user.IsActive = false;
            await _userRepo.UpdateAsync(user);
        }


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
