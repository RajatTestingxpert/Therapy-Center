using TherapyCenter.DTO_s.Appointment;
using TherapyCenter.Entities;
using TherapyCenter.Repositories.Interfaces;
using TherapyCenter.Services.Interfaces;
using AppointmentEntity = TherapyCenter.Entities.Appointment;

namespace TherapyCenter.Services.Implementations
{
    public class AppointmentService : IAppointmentService
    {
        private readonly IAppointmentRepository _appointmentRepo;
        private readonly ISlotRepository _slotRepo;
        private readonly ITherapyRepository _therapyRepo;
        private readonly IDoctorRepository _doctorRepo;
        private readonly IPaymentRepository _paymentRepo;
        private readonly IUnitOfWork _unitOfWork;

        public AppointmentService(
            IAppointmentRepository appointmentRepo,
            ISlotRepository slotRepo,
            ITherapyRepository therapyRepo,
            IDoctorRepository doctorRepo,
            IPaymentRepository paymentRepo,
            IUnitOfWork unitOfWork)
        {
            _appointmentRepo = appointmentRepo;
            _slotRepo = slotRepo;
            _therapyRepo = therapyRepo;
            _doctorRepo = doctorRepo;
            _paymentRepo = paymentRepo;
            _unitOfWork = unitOfWork;
        }

        public async Task<AppointmentEntity> BookAsync(BookAppointmentRequest request)
        {
            if (request.PatientId <= 0)
                throw new InvalidOperationException("Patient is required.");

            if (request.DoctorId <= 0)
                throw new InvalidOperationException("Doctor is required.");

            if (request.TherapyId <= 0)
                throw new InvalidOperationException("Therapy is required.");

            if (request.SlotId <= 0)
                throw new InvalidOperationException("Slot is required.");

            await _unitOfWork.BeginTransactionAsync();

            var slot = await _slotRepo.GetByIdAsync(request.SlotId)
                       ?? throw new KeyNotFoundException("Slot not found.");

            if (slot.DoctorId != request.DoctorId)
                throw new InvalidOperationException("The selected slot does not belong to the selected doctor.");

            if (slot.IsBooked)
                throw new InvalidOperationException("This slot is already booked.");

            var doctor = await _doctorRepo.GetByIdAsync(request.DoctorId)
                         ?? throw new KeyNotFoundException("Doctor not found.");

            var therapy = await _therapyRepo.GetByIdAsync(request.TherapyId)
                          ?? throw new KeyNotFoundException("Therapy not found.");

            if (!TherapyMatchesDoctor(therapy.Name, doctor.Specialization))
                throw new InvalidOperationException("The selected therapy does not match the doctor's specialization.");

            slot.IsBooked = true;
            await _slotRepo.UpdateAsync(slot);

            var appointment = new AppointmentEntity
            {
                PatientId = request.PatientId,
                DoctorId = request.DoctorId,
                TherapyId = request.TherapyId,
                ReceptionistId = request.ReceptionistId,
                AppointmentDate = slot.Date,
                StartTime = slot.StartTime,
                EndTime = slot.EndTime,
                Status = "Scheduled",
                Notes = request.Notes
            };

            var created = await _appointmentRepo.CreateAsync(appointment);

            await _paymentRepo.CreateAsync(new Payment
            {
                AppointmentId = created.AppointmentId,
                Amount = therapy.Cost,
                PaymentMethod = null,
                TransactionId = null,
                Status = "Pending"
            });

            await _unitOfWork.CommitAsync();
            return created;
        }

        public async Task<AppointmentEntity> UpdateStatusAsync(int appointmentId, UpdateAppointmentStatusRequest request)
        {
            var appointment = await _appointmentRepo.GetByIdAsync(appointmentId)
                              ?? throw new KeyNotFoundException("Appointment not found.");

            appointment.Status = request.Status;
            if (!string.IsNullOrEmpty(request.Notes))
                appointment.Notes = request.Notes;

            return await _appointmentRepo.UpdateAsync(appointment);
        }

        public async Task<IEnumerable<AppointmentEntity>> GetByPatientAsync(int patientId)
            => await _appointmentRepo.GetByPatientIdAsync(patientId);

        public async Task<IEnumerable<AppointmentEntity>> GetByDoctorAsync(int doctorId)
            => await _appointmentRepo.GetByDoctorIdAsync(doctorId);

        public async Task<IEnumerable<AppointmentEntity>> GetByDateAsync(DateOnly date)
            => await _appointmentRepo.GetByDateAsync(date);

        public async Task<AppointmentEntity?> GetByIdAsync(int appointmentId)
            => await _appointmentRepo.GetByIdAsync(appointmentId);

        private static bool TherapyMatchesDoctor(string? therapyName, string? specialization)
        {
            var therapy = Normalize(therapyName);
            var spec = Normalize(specialization);

            if (therapy.Length == 0 || spec.Length == 0)
                return true;

            return therapy.Contains(spec) || spec.Contains(therapy);
        }

        private static string Normalize(string? value)
            => new string((value ?? string.Empty)
                .Trim()
                .ToLowerInvariant()
                .Where(char.IsLetterOrDigit)
                .ToArray());
    }
}
