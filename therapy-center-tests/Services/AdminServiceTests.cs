using Moq;
using Xunit;
using TherapyCenter.Services.Implementations;
using TherapyCenter.Services.Interfaces;
using TherapyCenter.Repositories.Interfaces;
using TherapyCenter.Entities;
using TherapyCenter.DTO_s.Admin;

namespace TherapyCenter.Tests.Services
{
    public class AdminServiceTests
    {
        private readonly Mock<ITherapyRepository> _therapyRepoMock = new();
        private readonly Mock<IDoctorRepository> _doctorRepoMock = new();
        private readonly Mock<IUserRepository> _userRepoMock = new();
        private readonly Mock<ISlotRepository> _slotRepoMock = new();
        private readonly Mock<IAppointmentRepository> _appointmentRepoMock = new();
        private readonly IAdminService _adminService;

        public AdminServiceTests()
        {
            _adminService = new AdminService(_therapyRepoMock.Object, _doctorRepoMock.Object, _userRepoMock.Object, _slotRepoMock.Object, _appointmentRepoMock.Object);
        }

        [Fact]
        public async Task CreateTherapyAsync_WithValidRequest_CreatesTherapy()
        {
            var request = new CreateTherapyRequest { Name = "Physiotherapy", Description = "Physical therapy", DurationMinutes = 60, Cost = 150 };
            _therapyRepoMock.Setup(x => x.CreateAsync(It.IsAny<Therapy>())).ReturnsAsync((Therapy t) => { t.TherapyId = 1; return t; });
            var result = await _adminService.CreateTherapyAsync(request);
            Assert.Equal("Physiotherapy", result.Name);
            Assert.Equal(60, result.DurationMinutes);
        }

        [Fact]
        public async Task UpdateTherapyAsync_WithValidRequest_UpdatesTherapy()
        {
            var existing = new Therapy { TherapyId = 1, Name = "Old", DurationMinutes = 30, Cost = 50 };
            var request = new UpdateTherapyRequest { Name = "New", DurationMinutes = 60, Cost = 100 };
            _therapyRepoMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(existing);
            _therapyRepoMock.Setup(x => x.UpdateAsync(It.IsAny<Therapy>())).ReturnsAsync((Therapy t) => t);
            var result = await _adminService.UpdateTherapyAsync(1, request);
            Assert.Equal("New", result.Name);
            Assert.Equal(60, result.DurationMinutes);
        }

        [Fact]
        public async Task UpdateTherapyAsync_WithNonexistentId_ThrowsKeyNotFound()
        {
            _therapyRepoMock.Setup(x => x.GetByIdAsync(999)).ReturnsAsync((Therapy?)null);
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _adminService.UpdateTherapyAsync(999, new UpdateTherapyRequest { Name = "X", DurationMinutes = 30, Cost = 50 }));
        }

        [Fact]
        public async Task DeleteTherapyAsync_Deletes() => await _adminService.DeleteTherapyAsync(1);

        [Fact]
        public async Task GetAllTherapiesAsync_ReturnsAll()
        {
            _therapyRepoMock.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<Therapy> { new() { TherapyId = 1, Name = "A" }, new() { TherapyId = 2, Name = "B" } });
            Assert.Equal(2, (await _adminService.GetAllTherapiesAsync()).Count());
        }

        [Fact]
        public async Task CreateDoctorProfileAsync_WithValidRequest_CreatesProfile()
        {
            var user = new User { UserId = 1, FirstName = "John", LastName = "Doe", Email = "john@clinic.com", Role = "Doctor" };
            _userRepoMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(user);
            _therapyRepoMock.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<Therapy> { new() { TherapyId = 1, Name = "Physiotherapy" } });
            _doctorRepoMock.Setup(x => x.GetByUserIdAsync(1)).ReturnsAsync((Doctor?)null);
            _doctorRepoMock.Setup(x => x.CreateAsync(It.IsAny<Doctor>())).ReturnsAsync((Doctor d) => { d.DoctorId = 1; return d; });

            var result = await _adminService.CreateDoctorProfileAsync(new CreateDoctorProfileRequest { UserId = 1, Specialization = "Physiotherapy" });

            Assert.Equal(1, result.DoctorId);
            Assert.Equal("Physiotherapy", result.Specialization);
        }

        [Fact]
        public async Task CreateDoctorProfileAsync_WithNonDoctorUser_Throws()
        {
            _userRepoMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(new User { UserId = 1, Role = "Patient" });
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _adminService.CreateDoctorProfileAsync(new CreateDoctorProfileRequest { UserId = 1, Specialization = "X" }));
            Assert.Contains("must have the Doctor role", ex.Message);
        }

        [Fact]
        public async Task DeleteDoctorAsync_WithNoAppointments_Deletes()
        {
            _appointmentRepoMock.Setup(x => x.GetByDoctorIdAsync(1)).ReturnsAsync(new List<Appointment>());
            await _adminService.DeleteDoctorAsync(1);
            _doctorRepoMock.Verify(x => x.DeleteAsync(1), Times.Once);
        }

        [Fact]
        public async Task DeleteDoctorAsync_WithAppointments_Throws()
        {
            _appointmentRepoMock.Setup(x => x.GetByDoctorIdAsync(1)).ReturnsAsync(new List<Appointment> { new() { AppointmentId = 1, DoctorId = 1, PatientId = 1, TherapyId = 1 } });
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _adminService.DeleteDoctorAsync(1));
            Assert.Contains("Cannot delete doctor", ex.Message);
        }

        [Fact]
        public async Task GenerateSlotsForDoctorAsync_WithValidRequest_GeneratesSlots()
        {
            var doctor = new Doctor { DoctorId = 1, AvailableDays = "Mon", StartTime = new TimeOnly(9, 0), EndTime = new TimeOnly(12, 0) };
            _doctorRepoMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(doctor);
            _slotRepoMock.Setup(x => x.BulkCreateAsync(It.IsAny<IEnumerable<Slot>>())).Returns(Task.CompletedTask);
            var count = await _adminService.GenerateSlotsForDoctorAsync(new GenerateSlotsRequest { DoctorId = 1, FromDate = new DateOnly(2026, 6, 15), ToDate = new DateOnly(2026, 6, 15) });
            Assert.Equal(3, count);
        }

        [Fact]
        public async Task DeactivateStaffAsync_SetsIsActiveFalse()
        {
            var user = new User { UserId = 1, Role = "Receptionist", IsActive = true };
            _userRepoMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(user);
            await _adminService.DeactivateStaffAsync(1);
            Assert.False(user.IsActive);
        }

        [Fact]
        public async Task DeactivateStaffAsync_WithAdmin_Throws()
        {
            _userRepoMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(new User { UserId = 1, Role = "Admin" });
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _adminService.DeactivateStaffAsync(1));
            Assert.Contains("Admin account cannot be removed", ex.Message);
        }
    }
}