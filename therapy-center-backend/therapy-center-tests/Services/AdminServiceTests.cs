using Moq;
using Xunit;
using TherapyCenter.Services.Implementations;
using TherapyCenter.Services.Interfaces;
using TherapyCenter.Repositories.Interfaces;
using TherapyCenter.Entities;
using TherapyCenter.DTO_s.Admin;
using TherapyCenter.DTO_s.Doctor;

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
            _adminService = new AdminService(
                _therapyRepoMock.Object,
                _doctorRepoMock.Object,
                _userRepoMock.Object,
                _slotRepoMock.Object,
                _appointmentRepoMock.Object);
        }

        // ────────────────────────────────────────────────────────────────
        // Therapy CRUD
        // ────────────────────────────────────────────────────────────────

        [Fact]
        public async Task CreateTherapyAsync_WithValidRequest_CreatesTherapy()
        {
            // Arrange
            var request = new CreateTherapyRequest
            {
                Name = "Physiotherapy",
                Description = "Physical therapy sessions",
                DurationMinutes = 60,
                Cost = 150.00m
            };

            _therapyRepoMock.Setup(x => x.CreateAsync(It.IsAny<Therapy>()))
                .ReturnsAsync((Therapy t) =>
                {
                    t.TherapyId = 1;
                    return t;
                });

            // Act
            var result = await _adminService.CreateTherapyAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.TherapyId);
            Assert.Equal("Physiotherapy", result.Name);
            Assert.Equal(60, result.DurationMinutes);
            Assert.Equal(150.00m, result.Cost);
            _therapyRepoMock.Verify(x => x.CreateAsync(It.Is<Therapy>(t =>
                t.Name == "Physiotherapy" && t.Cost == 150.00m)), Times.Once);
        }

        [Fact]
        public async Task UpdateTherapyAsync_WithValidRequest_UpdatesTherapy()
        {
            // Arrange
            var existingTherapy = new Therapy
            {
                TherapyId = 1,
                Name = "Old Name",
                Description = "Old description",
                DurationMinutes = 30,
                Cost = 50.00m
            };

            var request = new UpdateTherapyRequest
            {
                Name = "New Name",
                Description = "New description",
                DurationMinutes = 60,
                Cost = 100.00m
            };

            _therapyRepoMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(existingTherapy);
            _therapyRepoMock.Setup(x => x.UpdateAsync(It.IsAny<Therapy>()))
                .ReturnsAsync((Therapy t) => t);

            // Act
            var result = await _adminService.UpdateTherapyAsync(1, request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("New Name", result.Name);
            Assert.Equal("New description", result.Description);
            Assert.Equal(60, result.DurationMinutes);
            Assert.Equal(100.00m, result.Cost);
        }

        [Fact]
        public async Task UpdateTherapyAsync_WithNonexistentId_ThrowsKeyNotFound()
        {
            // Arrange
            var request = new UpdateTherapyRequest { Name = "Test", DurationMinutes = 30, Cost = 50 };
            _therapyRepoMock.Setup(x => x.GetByIdAsync(999)).ReturnsAsync((Therapy?)null);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _adminService.UpdateTherapyAsync(999, request));
            Assert.Contains("Therapy 999 not found", ex.Message);
        }

        [Fact]
        public async Task DeleteTherapyAsync_DeletesTherapy()
        {
            // Arrange
            _therapyRepoMock.Setup(x => x.DeleteAsync(1)).Returns(Task.CompletedTask);

            // Act
            await _adminService.DeleteTherapyAsync(1);

            // Assert
            _therapyRepoMock.Verify(x => x.DeleteAsync(1), Times.Once);
        }

        [Fact]
        public async Task GetAllTherapiesAsync_ReturnsAllTherapies()
        {
            // Arrange
            var therapies = new List<Therapy>
            {
                new() { TherapyId = 1, Name = "Physiotherapy", Cost = 100 },
                new() { TherapyId = 2, Name = "Speech Therapy", Cost = 200 }
            };

            _therapyRepoMock.Setup(x => x.GetAllAsync()).ReturnsAsync(therapies);

            // Act
            var result = await _adminService.GetAllTherapiesAsync();

            // Assert
            Assert.Equal(2, result.Count());
        }

        // ────────────────────────────────────────────────────────────────
        // Doctor Profile
        // ────────────────────────────────────────────────────────────────

        [Fact]
        public async Task CreateDoctorProfileAsync_WithValidRequest_CreatesProfile()
        {
            // Arrange
            var user = new User
            {
                UserId = 1,
                FirstName = "John",
                LastName = "Doe",
                Email = "john@clinic.com",
                Role = "Doctor",
                PhoneNumber = "1234567890"
            };

            var therapies = new List<Therapy>
            {
                new() { TherapyId = 1, Name = "Physiotherapy" }
            };

            var request = new CreateDoctorProfileRequest
            {
                UserId = 1,
                Specialization = "Physiotherapy",
                Bio = "Experienced physiotherapist",
                AvailableDays = "Mon,Wed,Fri",
                StartTime = new TimeOnly(9, 0),
                EndTime = new TimeOnly(17, 0)
            };

            _userRepoMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(user);
            _therapyRepoMock.Setup(x => x.GetAllAsync()).ReturnsAsync(therapies);
            _doctorRepoMock.Setup(x => x.GetByUserIdAsync(1)).ReturnsAsync((Doctor?)null);
            _doctorRepoMock.Setup(x => x.CreateAsync(It.IsAny<Doctor>()))
                .ReturnsAsync((Doctor d) =>
                {
                    d.DoctorId = 1;
                    return d;
                });

            // Act
            var result = await _adminService.CreateDoctorProfileAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.DoctorId);
            Assert.Equal("Physiotherapy", result.Specialization);
            Assert.Equal("John", result.FirstName);
            Assert.Equal("Doe", result.LastName);
            Assert.Equal("john@clinic.com", result.Email);
        }

        [Fact]
        public async Task CreateDoctorProfileAsync_WithNonDoctorUser_ThrowsInvalidOperation()
        {
            // Arrange
            var user = new User { UserId = 1, Role = "Patient" };
            var request = new CreateDoctorProfileRequest { UserId = 1, Specialization = "Physiotherapy" };

            _userRepoMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(user);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _adminService.CreateDoctorProfileAsync(request));
            Assert.Contains("must have the Doctor role", ex.Message);
        }

        [Fact]
        public async Task CreateDoctorProfileAsync_WithEmptySpecialization_ThrowsInvalidOperation()
        {
            // Arrange
            var user = new User { UserId = 1, Role = "Doctor" };
            var request = new CreateDoctorProfileRequest { UserId = 1, Specialization = "" };

            _userRepoMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(user);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _adminService.CreateDoctorProfileAsync(request));
            Assert.Contains("Specialization is required", ex.Message);
        }

        [Fact]
        public async Task CreateDoctorProfileAsync_WithNonExistentTherapy_ThrowsInvalidOperation()
        {
            // Arrange
            var user = new User { UserId = 1, Role = "Doctor" };
            var therapies = new List<Therapy> { new() { TherapyId = 1, Name = "Physiotherapy" } };
            var request = new CreateDoctorProfileRequest
            {
                UserId = 1,
                Specialization = "Speech Therapy"
            };

            _userRepoMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(user);
            _therapyRepoMock.Setup(x => x.GetAllAsync()).ReturnsAsync(therapies);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _adminService.CreateDoctorProfileAsync(request));
            Assert.Contains("add this specialization as a therapy first", ex.Message);
        }

        [Fact]
        public async Task CreateDoctorProfileAsync_WithExistingProfile_ThrowsInvalidOperation()
        {
            // Arrange
            var user = new User { UserId = 1, Role = "Doctor" };
            var therapies = new List<Therapy> { new() { TherapyId = 1, Name = "Physiotherapy" } };
            var existingDoctor = new Doctor { DoctorId = 1, UserId = 1 };
            var request = new CreateDoctorProfileRequest
            {
                UserId = 1,
                Specialization = "Physiotherapy"
            };

            _userRepoMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(user);
            _therapyRepoMock.Setup(x => x.GetAllAsync()).ReturnsAsync(therapies);
            _doctorRepoMock.Setup(x => x.GetByUserIdAsync(1)).ReturnsAsync(existingDoctor);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _adminService.CreateDoctorProfileAsync(request));
            Assert.Contains("already exists", ex.Message);
        }

        // ────────────────────────────────────────────────────────────────
        // Delete Doctor
        // ────────────────────────────────────────────────────────────────

        [Fact]
        public async Task DeleteDoctorAsync_WithNoAppointments_DeletesDoctor()
        {
            // Arrange
            _appointmentRepoMock.Setup(x => x.GetByDoctorIdAsync(1)).ReturnsAsync(new List<Appointment>());
            _doctorRepoMock.Setup(x => x.DeleteAsync(1)).Returns(Task.CompletedTask);

            // Act
            await _adminService.DeleteDoctorAsync(1);

            // Assert
            _doctorRepoMock.Verify(x => x.DeleteAsync(1), Times.Once);
        }

        [Fact]
        public async Task DeleteDoctorAsync_WithExistingAppointments_ThrowsInvalidOperation()
        {
            // Arrange
            var appointments = new List<Appointment>
            {
                new() { AppointmentId = 1, DoctorId = 1, PatientId = 1, TherapyId = 1 }
            };
            _appointmentRepoMock.Setup(x => x.GetByDoctorIdAsync(1)).ReturnsAsync(appointments);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _adminService.DeleteDoctorAsync(1));
            Assert.Contains("Cannot delete doctor with existing appointments", ex.Message);
            _doctorRepoMock.Verify(x => x.DeleteAsync(It.IsAny<int>()), Times.Never);
        }

        // ────────────────────────────────────────────────────────────────
        // Receptionists
        // ────────────────────────────────────────────────────────────────

        [Fact]
        public async Task GetAllReceptionistsAsync_ReturnsReceptionists()
        {
            // Arrange
            var receptionists = new List<User>
            {
                new() { UserId = 1, FirstName = "Alice", Role = "Receptionist" },
                new() { UserId = 2, FirstName = "Bob", Role = "Receptionist" }
            };

            _userRepoMock.Setup(x => x.GetByRoleAsync("Receptionist")).ReturnsAsync(receptionists);

            // Act
            var result = await _adminService.GetAllReceptionistsAsync();

            // Assert
            Assert.Equal(2, result.Count());
        }

        // ────────────────────────────────────────────────────────────────
        // Slot Generation
        // ────────────────────────────────────────────────────────────────

        [Fact]
        public async Task GenerateSlotsForDoctorAsync_WithValidRequest_GeneratesSlots()
        {
            // Arrange
            var doctor = new Doctor
            {
                DoctorId = 1,
                AvailableDays = "Mon,Wed,Fri",
                StartTime = new TimeOnly(9, 0),
                EndTime = new TimeOnly(12, 0) // 3 hours → 3 slots
            };

            // June 15, 2026 is a Monday
            var request = new GenerateSlotsRequest
            {
                DoctorId = 1,
                FromDate = new DateOnly(2026, 6, 15),
                ToDate = new DateOnly(2026, 6, 15) // single day
            };

            _doctorRepoMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(doctor);
            _slotRepoMock.Setup(x => x.BulkCreateAsync(It.IsAny<IEnumerable<Slot>>())).Returns(Task.CompletedTask);

            // Act
            var count = await _adminService.GenerateSlotsForDoctorAsync(request);

            // Assert
            Assert.Equal(3, count); // 9-10, 10-11, 11-12
            _slotRepoMock.Verify(x => x.BulkCreateAsync(It.IsAny<IEnumerable<Slot>>()), Times.Once);
        }

        [Fact]
        public async Task GenerateSlotsForDoctorAsync_WithNoWorkingHours_ThrowsInvalidOperation()
        {
            // Arrange
            var doctor = new Doctor
            {
                DoctorId = 1,
                AvailableDays = "Mon",
                StartTime = null,
                EndTime = null
            };

            var request = new GenerateSlotsRequest
            {
                DoctorId = 1,
                FromDate = new DateOnly(2026, 6, 15),
                ToDate = new DateOnly(2026, 6, 15)
            };

            _doctorRepoMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(doctor);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _adminService.GenerateSlotsForDoctorAsync(request));
            Assert.Contains("no working hours", ex.Message);
        }

        [Fact]
        public async Task GenerateSlotsForDoctorAsync_SkipsWeekends()
        {
            // Arrange
            var doctor = new Doctor
            {
                DoctorId = 1,
                AvailableDays = "Mon,Tue,Wed,Thu,Fri", // weekdays only
                StartTime = new TimeOnly(9, 0),
                EndTime = new TimeOnly(10, 0) // 1 hour → 1 slot per day
            };

            // June 13-14 (Sat-Sun) → no slots; June 15 (Mon) → 1 slot
            var request = new GenerateSlotsRequest
            {
                DoctorId = 1,
                FromDate = new DateOnly(2026, 6, 13), // Saturday
                ToDate = new DateOnly(2026, 6, 15)    // Monday
            };

            _doctorRepoMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(doctor);
            _slotRepoMock.Setup(x => x.BulkCreateAsync(It.IsAny<IEnumerable<Slot>>())).Returns(Task.CompletedTask);

            // Act
            var count = await _adminService.GenerateSlotsForDoctorAsync(request);

            // Assert
            Assert.Equal(1, count); // only Monday generates a slot
        }

        [Fact]
        public async Task GenerateSlotsForDoctorAsync_WithNoAvailableDays_UsesDefault()
        {
            // Arrange
            var doctor = new Doctor
            {
                DoctorId = 1,
                AvailableDays = null, // should default to Mon-Fri
                StartTime = new TimeOnly(9, 0),
                EndTime = new TimeOnly(11, 0) // 2 hours → 2 slots
            };

            // June 15, 2026 is Monday
            var request = new GenerateSlotsRequest
            {
                DoctorId = 1,
                FromDate = new DateOnly(2026, 6, 15),
                ToDate = new DateOnly(2026, 6, 15)
            };

            _doctorRepoMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(doctor);
            _slotRepoMock.Setup(x => x.BulkCreateAsync(It.IsAny<IEnumerable<Slot>>())).Returns(Task.CompletedTask);

            // Act
            var count = await _adminService.GenerateSlotsForDoctorAsync(request);

            // Assert
            Assert.Equal(2, count);
        }

        // ────────────────────────────────────────────────────────────────
        // Deactivate Staff
        // ────────────────────────────────────────────────────────────────

        [Fact]
        public async Task DeactivateStaffAsync_SetsIsActiveFalse()
        {
            // Arrange
            var user = new User { UserId = 1, Role = "Receptionist", IsActive = true };
            _userRepoMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(user);
            _userRepoMock.Setup(x => x.UpdateAsync(It.IsAny<User>())).ReturnsAsync((User u) => u);

            // Act
            await _adminService.DeactivateStaffAsync(1);

            // Assert
            Assert.False(user.IsActive);
            _userRepoMock.Verify(x => x.UpdateAsync(It.Is<User>(u => u.IsActive == false)), Times.Once);
        }

        [Fact]
        public async Task DeactivateStaffAsync_WithNonexistentUser_ThrowsKeyNotFound()
        {
            // Arrange
            _userRepoMock.Setup(x => x.GetByIdAsync(999)).ReturnsAsync((User?)null);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _adminService.DeactivateStaffAsync(999));
            Assert.Contains("User not found", ex.Message);
        }

        [Fact]
        public async Task DeactivateStaffAsync_WithAdminRole_ThrowsInvalidOperation()
        {
            // Arrange
            var user = new User { UserId = 1, Role = "Admin", IsActive = true };
            _userRepoMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(user);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _adminService.DeactivateStaffAsync(1));
            Assert.Contains("Admin account cannot be removed", ex.Message);
        }
    }
}