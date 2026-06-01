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
            _unitOfWorkMock.Setup(x => x.BeginTransactionAsync()).Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(x => x.CommitAsync()).Returns(Task.CompletedTask);

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

        [Fact]
        public async Task BookAsync_WithValidRequest_CreatesAppointmentAndPayment()
        {
            var (slot, doctor, therapy) = SetupValidEntities();
            var request = new BookAppointmentRequest
            {
                PatientId = 1, DoctorId = 1, TherapyId = 1, SlotId = 1, Notes = "Test booking"
            };

            _slotRepoMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(slot);
            _doctorRepoMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(doctor);
            _therapyRepoMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(therapy);
            _slotRepoMock.Setup(x => x.UpdateAsync(It.IsAny<Slot>())).ReturnsAsync(slot);
            _appointmentRepoMock.Setup(x => x.CreateAsync(It.IsAny<AppointmentEntity>()))
                .ReturnsAsync((AppointmentEntity a) => { a.AppointmentId = 1; return a; });
            _paymentRepoMock.Setup(x => x.CreateAsync(It.IsAny<Payment>()))
                .ReturnsAsync((Payment p) => { p.PaymentId = 1; return p; });

            var result = await _appointmentService.BookAsync(request);

            Assert.Equal(1, result.AppointmentId);
            Assert.Equal("Scheduled", result.Status);
            Assert.Equal("Test booking", result.Notes);
            _slotRepoMock.Verify(x => x.UpdateAsync(It.Is<Slot>(s => s.IsBooked)), Times.Once);
            _paymentRepoMock.Verify(x => x.CreateAsync(It.Is<Payment>(p =>
                p.Amount == 100.00m && p.Status == "Pending")), Times.Once);
        }

        [Fact]
        public async Task BookAsync_WithZeroPatientId_ThrowsInvalidOperation()
        {
            var request = new BookAppointmentRequest { PatientId = 0, DoctorId = 1, TherapyId = 1, SlotId = 1 };
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _appointmentService.BookAsync(request));
            Assert.Contains("Patient is required", ex.Message);
        }

        [Fact]
        public async Task BookAsync_WithZeroDoctorId_ThrowsInvalidOperation()
        {
            var request = new BookAppointmentRequest { PatientId = 1, DoctorId = 0, TherapyId = 1, SlotId = 1 };
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _appointmentService.BookAsync(request));
            Assert.Contains("Doctor is required", ex.Message);
        }

        [Fact]
        public async Task BookAsync_WithZeroTherapyId_ThrowsInvalidOperation()
        {
            var request = new BookAppointmentRequest { PatientId = 1, DoctorId = 1, TherapyId = 0, SlotId = 1 };
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _appointmentService.BookAsync(request));
            Assert.Contains("Therapy is required", ex.Message);
        }

        [Fact]
        public async Task BookAsync_WithZeroSlotId_ThrowsInvalidOperation()
        {
            var request = new BookAppointmentRequest { PatientId = 1, DoctorId = 1, TherapyId = 1, SlotId = 0 };
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _appointmentService.BookAsync(request));
            Assert.Contains("Slot is required", ex.Message);
        }

        [Fact]
        public async Task BookAsync_WithNonexistentSlot_ThrowsKeyNotFound()
        {
            var request = new BookAppointmentRequest { PatientId = 1, DoctorId = 1, TherapyId = 1, SlotId = 999 };
            _slotRepoMock.Setup(x => x.GetByIdAsync(999)).ReturnsAsync((Slot?)null);
            var ex = await Assert.ThrowsAsync<KeyNotFoundException>(() => _appointmentService.BookAsync(request));
            Assert.Contains("Slot not found", ex.Message);
        }

        [Fact]
        public async Task BookAsync_WithSlotFromDifferentDoctor_ThrowsInvalidOperation()
        {
            var slot = new Slot { SlotId = 1, DoctorId = 2, Date = new DateOnly(2026, 6, 15), StartTime = new TimeOnly(10, 0), EndTime = new TimeOnly(11, 0), IsBooked = false };
            var request = new BookAppointmentRequest { PatientId = 1, DoctorId = 1, TherapyId = 1, SlotId = 1 };
            _slotRepoMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(slot);
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _appointmentService.BookAsync(request));
            Assert.Contains("does not belong", ex.Message);
        }

        [Fact]
        public async Task BookAsync_WithAlreadyBookedSlot_ThrowsInvalidOperation()
        {
            var slot = new Slot { SlotId = 1, DoctorId = 1, Date = new DateOnly(2026, 6, 15), StartTime = new TimeOnly(10, 0), EndTime = new TimeOnly(11, 0), IsBooked = true };
            var request = new BookAppointmentRequest { PatientId = 1, DoctorId = 1, TherapyId = 1, SlotId = 1 };
            _slotRepoMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(slot);
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _appointmentService.BookAsync(request));
            Assert.Contains("already booked", ex.Message);
        }

        [Fact]
        public async Task BookAsync_WithTherapyMismatch_ThrowsInvalidOperation()
        {
            var slot = new Slot { SlotId = 1, DoctorId = 1, Date = new DateOnly(2026, 6, 15), StartTime = new TimeOnly(10, 0), EndTime = new TimeOnly(11, 0), IsBooked = false };
            var doctor = new Doctor { DoctorId = 1, UserId = 1, Specialization = "Speech Therapy" };
            var therapy = new Therapy { TherapyId = 1, Name = "Physiotherapy", DurationMinutes = 60, Cost = 100 };
            var request = new BookAppointmentRequest { PatientId = 1, DoctorId = 1, TherapyId = 1, SlotId = 1 };
            _slotRepoMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(slot);
            _doctorRepoMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(doctor);
            _therapyRepoMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(therapy);
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _appointmentService.BookAsync(request));
            Assert.Contains("does not match", ex.Message);
        }

        [Fact]
        public async Task BookAsync_WithMatchingPartialName_AllowsBooking()
        {
            var slot = new Slot { SlotId = 1, DoctorId = 1, Date = new DateOnly(2026, 6, 15), StartTime = new TimeOnly(10, 0), EndTime = new TimeOnly(11, 0), IsBooked = false };
            var doctor = new Doctor { DoctorId = 1, UserId = 1, Specialization = "Physio" };
            var therapy = new Therapy { TherapyId = 1, Name = "Physiotherapy", DurationMinutes = 60, Cost = 100 };
            var request = new BookAppointmentRequest { PatientId = 1, DoctorId = 1, TherapyId = 1, SlotId = 1 };
            _slotRepoMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(slot);
            _doctorRepoMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(doctor);
            _therapyRepoMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(therapy);
            _slotRepoMock.Setup(x => x.UpdateAsync(It.IsAny<Slot>())).ReturnsAsync(slot);
            _appointmentRepoMock.Setup(x => x.CreateAsync(It.IsAny<AppointmentEntity>()))
                .ReturnsAsync((AppointmentEntity a) => { a.AppointmentId = 1; return a; });
            _paymentRepoMock.Setup(x => x.CreateAsync(It.IsAny<Payment>()))
                .ReturnsAsync((Payment p) => { p.PaymentId = 1; return p; });

            var result = await _appointmentService.BookAsync(request);
            Assert.NotNull(result);
            Assert.Equal(1, result.AppointmentId);
        }

        [Fact]
        public async Task UpdateStatusAsync_WithValidRequest_UpdatesStatus()
        {
            var appointment = new AppointmentEntity { AppointmentId = 1, PatientId = 1, DoctorId = 1, TherapyId = 1, Status = "Scheduled", Notes = "Original" };
            var request = new UpdateAppointmentStatusRequest { Status = "Completed", Notes = "Done" };
            _appointmentRepoMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(appointment);
            _appointmentRepoMock.Setup(x => x.UpdateAsync(It.IsAny<AppointmentEntity>())).ReturnsAsync((AppointmentEntity a) => a);
            var result = await _appointmentService.UpdateStatusAsync(1, request);
            Assert.Equal("Completed", result.Status);
            Assert.Equal("Done", result.Notes);
        }

        [Fact]
        public async Task UpdateStatusAsync_WithNonexistentAppointment_ThrowsKeyNotFound()
        {
            _appointmentRepoMock.Setup(x => x.GetByIdAsync(999)).ReturnsAsync((AppointmentEntity?)null);
            var ex = await Assert.ThrowsAsync<KeyNotFoundException>(() => _appointmentService.UpdateStatusAsync(999, new UpdateAppointmentStatusRequest { Status = "Cancelled" }));
            Assert.Contains("Appointment not found", ex.Message);
        }

        [Fact]
        public async Task UpdateStatusAsync_WithNullNotes_DoesNotOverwriteNotes()
        {
            var appointment = new AppointmentEntity { AppointmentId = 1, PatientId = 1, DoctorId = 1, TherapyId = 1, Status = "Scheduled", Notes = "Existing" };
            var request = new UpdateAppointmentStatusRequest { Status = "Cancelled", Notes = null };
            _appointmentRepoMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(appointment);
            _appointmentRepoMock.Setup(x => x.UpdateAsync(It.IsAny<AppointmentEntity>())).ReturnsAsync((AppointmentEntity a) => a);
            var result = await _appointmentService.UpdateStatusAsync(1, request);
            Assert.Equal("Cancelled", result.Status);
            Assert.Equal("Existing", result.Notes);
        }

        [Fact]
        public async Task GetByPatientAsync_ReturnsAppointments()
        {
            var appointments = new List<AppointmentEntity> { new() { AppointmentId = 1, PatientId = 1, DoctorId = 1, TherapyId = 1 }, new() { AppointmentId = 2, PatientId = 1, DoctorId = 2, TherapyId = 2 } };
            _appointmentRepoMock.Setup(x => x.GetByPatientIdAsync(1)).ReturnsAsync(appointments);
            Assert.Equal(2, (await _appointmentService.GetByPatientAsync(1)).Count());
        }

        [Fact]
        public async Task GetByDoctorAsync_ReturnsAppointments()
        {
            var appointments = new List<AppointmentEntity> { new() { AppointmentId = 1, PatientId = 1, DoctorId = 1, TherapyId = 1 } };
            _appointmentRepoMock.Setup(x => x.GetByDoctorIdAsync(1)).ReturnsAsync(appointments);
            Assert.Single(await _appointmentService.GetByDoctorAsync(1));
        }

        [Fact]
        public async Task GetByDateAsync_ReturnsAppointments()
        {
            var date = new DateOnly(2026, 6, 15);
            var appointments = new List<AppointmentEntity> { new() { AppointmentId = 1, AppointmentDate = date } };
            _appointmentRepoMock.Setup(x => x.GetByDateAsync(date)).ReturnsAsync(appointments);
            Assert.Single(await _appointmentService.GetByDateAsync(date));
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsAppointment_WhenExists()
        {
            var appointment = new AppointmentEntity { AppointmentId = 1, Status = "Scheduled" };
            _appointmentRepoMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(appointment);
            Assert.NotNull(await _appointmentService.GetByIdAsync(1));
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsNull_WhenNotExists()
        {
            _appointmentRepoMock.Setup(x => x.GetByIdAsync(999)).ReturnsAsync((AppointmentEntity?)null);
            Assert.Null(await _appointmentService.GetByIdAsync(999));
        }
    }
}