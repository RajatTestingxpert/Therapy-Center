using Moq;
using Xunit;
using TherapyCenter.Services.Implementations;
using TherapyCenter.Services.Interfaces;
using TherapyCenter.Repositories.Interfaces;
using TherapyCenter.Entities;
using TherapyCenter.DTO_s.Appointment;
using AppointmentEntity = TherapyCenter.Entities.Appointment;

namespace TherapyCenter.Tests.Services
{
    public class AppointmentServiceTests
    {
        private readonly Mock<IAppointmentRepository> _appointmentRepoMock = new();
        private readonly Mock<ISlotRepository> _slotRepoMock = new();
        private readonly Mock<ITherapyRepository> _therapyRepoMock = new();
        private readonly Mock<IDoctorRepository> _doctorRepoMock = new();
        private readonly Mock<IPaymentRepository> _paymentRepoMock = new();
        private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
        private readonly IAppointmentService _appointmentService;

        public AppointmentServiceTests()
        {
            _appointmentService = new AppointmentService(
                _appointmentRepoMock.Object,
                _slotRepoMock.Object,
                _therapyRepoMock.Object,
                _doctorRepoMock.Object,
                _paymentRepoMock.Object,
                _unitOfWorkMock.Object);
        }

        private (Slot slot, Doctor doctor, Therapy therapy) SetupValidEntities()
        {
            var slot = new Slot
            {
                SlotId = 1,
                DoctorId = 1,
                Date = new DateOnly(2026, 6, 15),
                StartTime = new TimeOnly(10, 0),
                EndTime = new TimeOnly(11, 0),
                IsBooked = false
            };

            var doctor = new Doctor
            {
                DoctorId = 1,
                UserId = 1,
                Specialization = "Physiotherapy"
            };

            var therapy = new Therapy
            {
                TherapyId = 1,
                Name = "Physiotherapy",
                DurationMinutes = 60,
                Cost = 100.00m
            };

            return (slot, doctor, therapy);
        }
    }
}