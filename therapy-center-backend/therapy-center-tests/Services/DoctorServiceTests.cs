using Moq;
using Xunit;
using TherapyCenter.Services.Implementations;
using TherapyCenter.Services.Interfaces;
using TherapyCenter.Repositories.Interfaces;
using TherapyCenter.Entities;
using TherapyCenter.DTO_s.Doctor;

namespace TherapyCenter.Tests.Services
{
    public class DoctorServiceTests
    {
        private readonly Mock<IDoctorRepository> _doctorRepoMock = new();
        private readonly Mock<ISlotRepository> _slotRepoMock = new();
        private readonly IDoctorService _doctorService;

        public DoctorServiceTests()
        {
            _doctorService = new DoctorService(_doctorRepoMock.Object, _slotRepoMock.Object);
        }

        private Doctor CreateTestDoctor(int id, string firstName, string lastName, string specialization)
        {
            return new Doctor
            {
                DoctorId = id,
                UserId = id,
                Specialization = specialization,
                Bio = "Experienced doctor",
                AvailableDays = "Mon,Wed,Fri",
                StartTime = new TimeOnly(9, 0),
                EndTime = new TimeOnly(17, 0),
                User = new User
                {
                    UserId = id,
                    FirstName = firstName,
                    LastName = lastName,
                    Email = $"{firstName.ToLower()}@clinic.com",
                    Role = "Doctor",
                    PhoneNumber = "1234567890"
                }
            };
        }

        // ────────────────────────────────────────────────────────────────
        // GetAllAsync
        // ────────────────────────────────────────────────────────────────

        [Fact]
        public async Task GetAllAsync_ReturnsAllDoctors()
        {
            // Arrange
            var doctors = new List<Doctor>
            {
                CreateTestDoctor(1, "John", "Doe", "Physiotherapy"),
                CreateTestDoctor(2, "Jane", "Smith", "Speech Therapy")
            };

            _doctorRepoMock.Setup(x => x.GetAllAsync()).ReturnsAsync(doctors);

            // Act
            var result = await _doctorService.GetAllAsync();

            // Assert
            Assert.Equal(2, result.Count());
            var list = result.ToList();
            Assert.Equal("John Doe", list[0].FullName);
            Assert.Equal("Physiotherapy", list[0].Specialization);
            Assert.Equal("Jane Smith", list[1].FullName);
            Assert.Equal("Speech Therapy", list[1].Specialization);
        }

        [Fact]
        public async Task GetAllAsync_WhenEmpty_ReturnsEmptyList()
        {
            // Arrange
            _doctorRepoMock.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<Doctor>());

            // Act
            var result = await _doctorService.GetAllAsync();

            // Assert
            Assert.Empty(result);
        }

        // ────────────────────────────────────────────────────────────────
        // GetByIdAsync
        // ────────────────────────────────────────────────────────────────

        [Fact]
        public async Task GetByIdAsync_WithValidId_ReturnsDoctor()
        {
            // Arrange
            var doctor = CreateTestDoctor(1, "John", "Doe", "Physiotherapy");
            _doctorRepoMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(doctor);

            // Act
            var result = await _doctorService.GetByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result!.DoctorId);
            Assert.Equal("John Doe", result.FullName);
            Assert.Equal("john@clinic.com", result.Email);
            Assert.Equal("Physiotherapy", result.Specialization);
        }

        [Fact]
        public async Task GetByIdAsync_WithInvalidId_ReturnsNull()
        {
            // Arrange
            _doctorRepoMock.Setup(x => x.GetByIdAsync(999)).ReturnsAsync((Doctor?)null);

            // Act
            var result = await _doctorService.GetByIdAsync(999);

            // Assert
            Assert.Null(result);
        }

        // ────────────────────────────────────────────────────────────────
        // GetByUserIdAsync
        // ────────────────────────────────────────────────────────────────

        [Fact]
        public async Task GetByUserIdAsync_WithValidUserId_ReturnsDoctor()
        {
            // Arrange
            var doctor = CreateTestDoctor(1, "John", "Doe", "Physiotherapy");
            _doctorRepoMock.Setup(x => x.GetByUserIdAsync(1)).ReturnsAsync(doctor);

            // Act
            var result = await _doctorService.GetByUserIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result!.DoctorId);
            Assert.Equal("Physiotherapy", result.Specialization);
        }

        [Fact]
        public async Task GetByUserIdAsync_WithInvalidUserId_ReturnsNull()
        {
            // Arrange
            _doctorRepoMock.Setup(x => x.GetByUserIdAsync(999)).ReturnsAsync((Doctor?)null);

            // Act
            var result = await _doctorService.GetByUserIdAsync(999);

            // Assert
            Assert.Null(result);
        }

        // ────────────────────────────────────────────────────────────────
        // GetAvailableSlotsAsync
        // ────────────────────────────────────────────────────────────────

        [Fact]
        public async Task GetAvailableSlotsAsync_ReturnsSlots()
        {
            // Arrange
            var date = new DateOnly(2026, 6, 15);
            var slots = new List<Slot>
            {
                new() { SlotId = 1, DoctorId = 1, Date = date, StartTime = new TimeOnly(9, 0), EndTime = new TimeOnly(10, 0), IsBooked = false },
                new() { SlotId = 2, DoctorId = 1, Date = date, StartTime = new TimeOnly(10, 0), EndTime = new TimeOnly(11, 0), IsBooked = false }
            };

            _slotRepoMock.Setup(x => x.GetAvailableSlotsByDoctorAsync(1, date)).ReturnsAsync(slots);

            // Act
            var result = await _doctorService.GetAvailableSlotsAsync(1, date);

            // Assert
            Assert.Equal(2, result.Count());
            _slotRepoMock.Verify(x => x.GetAvailableSlotsByDoctorAsync(1, date), Times.Once);
        }

        [Fact]
        public async Task GetAvailableSlotsAsync_WhenNone_ReturnsEmptyList()
        {
            // Arrange
            var date = new DateOnly(2026, 6, 15);
            _slotRepoMock.Setup(x => x.GetAvailableSlotsByDoctorAsync(1, date)).ReturnsAsync(new List<Slot>());

            // Act
            var result = await _doctorService.GetAvailableSlotsAsync(1, date);

            // Assert
            Assert.Empty(result);
        }
    }
}