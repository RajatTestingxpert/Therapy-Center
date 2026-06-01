using Moq;
using Xunit;
using TherapyCenter.Services.Implementations;
using TherapyCenter.Services.Interfaces;
using TherapyCenter.Repositories.Interfaces;
using TherapyCenter.Entities;
using TherapyCenter.DTO_s.Patient;

namespace TherapyCenter.Tests.Services
{
    public class PatientServiceTests
    {
        private readonly Mock<IPatientRepository> _patientRepoMock = new();
        private readonly Mock<IUserRepository> _userRepoMock = new();
        private readonly IPatientService _patientService;

        public PatientServiceTests()
        {
            _patientService = new PatientService(_patientRepoMock.Object, _userRepoMock.Object);
        }

        // ────────────────────────────────────────────────────────────────
        // CreateAsync
        // ────────────────────────────────────────────────────────────────

        [Fact]
        public async Task CreateAsync_WithValidRequest_CreatesPatient()
        {
            // Arrange
            var request = new CreatePatientRequest
            {
                FirstName = "Peter",
                LastName = "Patient",
                DateOfBirth = new DateTime(2000, 1, 15),
                Gender = "Male",
                MedicalHistory = "No known allergies",
                GuardianId = null
            };

            _patientRepoMock.Setup(x => x.CreateAsync(It.IsAny<Patient>()))
                .ReturnsAsync((Patient p) =>
                {
                    p.PatientId = 1;
                    return p;
                });

            // Act
            var result = await _patientService.CreateAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.PatientId);
            Assert.Equal("Peter", result.FirstName);
            Assert.Equal("Patient", result.LastName);
            Assert.Equal("Male", result.Gender);
            Assert.Equal("No known allergies", result.MedicalHistory);
            _patientRepoMock.Verify(x => x.CreateAsync(It.Is<Patient>(p =>
                p.FirstName == "Peter" && p.LastName == "Patient")), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_WithGuardian_AssignsGuardianId()
        {
            // Arrange
            var request = new CreatePatientRequest
            {
                FirstName = "Child",
                LastName = "Test",
                GuardianId = 5
            };

            _patientRepoMock.Setup(x => x.CreateAsync(It.IsAny<Patient>()))
                .ReturnsAsync((Patient p) =>
                {
                    p.PatientId = 1;
                    return p;
                });

            // Act
            var result = await _patientService.CreateAsync(request);

            // Assert
            Assert.Equal(5, result.GuardianId);
        }

        // ────────────────────────────────────────────────────────────────
        // GetByIdAsync
        // ────────────────────────────────────────────────────────────────

        [Fact]
        public async Task GetByIdAsync_WithValidId_ReturnsPatient()
        {
            // Arrange
            var patient = new Patient { PatientId = 1, FirstName = "Peter", LastName = "Patient" };
            _patientRepoMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(patient);

            // Act
            var result = await _patientService.GetByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result!.PatientId);
            Assert.Equal("Peter", result.FirstName);
        }

        [Fact]
        public async Task GetByIdAsync_WithInvalidId_ReturnsNull()
        {
            // Arrange
            _patientRepoMock.Setup(x => x.GetByIdAsync(999)).ReturnsAsync((Patient?)null);

            // Act
            var result = await _patientService.GetByIdAsync(999);

            // Assert
            Assert.Null(result);
        }

        // ────────────────────────────────────────────────────────────────
        // GetByUserIdAsync
        // ────────────────────────────────────────────────────────────────

        [Fact]
        public async Task GetByUserIdAsync_ReturnsPatient()
        {
            // Arrange
            var patient = new Patient { PatientId = 1, UserId = 1 };
            _patientRepoMock.Setup(x => x.GetByUserIdAsync(1)).ReturnsAsync(patient);

            // Act
            var result = await _patientService.GetByUserIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result!.PatientId);
        }

        // ────────────────────────────────────────────────────────────────
        // GetOrCreateByUserIdAsync
        // ────────────────────────────────────────────────────────────────

        [Fact]
        public async Task GetOrCreateByUserIdAsync_WhenExists_ReturnsExisting()
        {
            // Arrange
            var existing = new Patient { PatientId = 1, UserId = 1, FirstName = "Existing", LastName = "Patient" };
            _patientRepoMock.Setup(x => x.GetByUserIdAsync(1)).ReturnsAsync(existing);

            // Act
            var result = await _patientService.GetOrCreateByUserIdAsync(1);

            // Assert
            Assert.Equal(1, result.PatientId);
            Assert.Equal("Existing", result.FirstName);
            _patientRepoMock.Verify(x => x.CreateAsync(It.IsAny<Patient>()), Times.Never);
        }

        [Fact]
        public async Task GetOrCreateByUserIdAsync_WhenNotExists_CreatesNew()
        {
            // Arrange
            var user = new User { UserId = 2, FirstName = "New", LastName = "User", Role = "Patient" };

            _patientRepoMock.Setup(x => x.GetByUserIdAsync(2)).ReturnsAsync((Patient?)null);
            _userRepoMock.Setup(x => x.GetByIdAsync(2)).ReturnsAsync(user);
            _patientRepoMock.Setup(x => x.CreateAsync(It.IsAny<Patient>()))
                .ReturnsAsync((Patient p) =>
                {
                    p.PatientId = 2;
                    return p;
                });

            // Act
            var result = await _patientService.GetOrCreateByUserIdAsync(2);

            // Assert
            Assert.Equal(2, result.PatientId);
            Assert.Equal("New", result.FirstName);
            Assert.Equal("User", result.LastName);
            _patientRepoMock.Verify(x => x.CreateAsync(It.Is<Patient>(p =>
                p.UserId == 2 && p.FirstName == "New")), Times.Once);
        }

        [Fact]
        public async Task GetOrCreateByUserIdAsync_WithNonPatientUser_ThrowsInvalidOperation()
        {
            // Arrange
            var user = new User { UserId = 3, Role = "Doctor" };

            _patientRepoMock.Setup(x => x.GetByUserIdAsync(3)).ReturnsAsync((Patient?)null);
            _userRepoMock.Setup(x => x.GetByIdAsync(3)).ReturnsAsync(user);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _patientService.GetOrCreateByUserIdAsync(3));
            Assert.Contains("Only patient users", ex.Message);
        }

        [Fact]
        public async Task GetOrCreateByUserIdAsync_WithNonexistentUser_ThrowsKeyNotFound()
        {
            // Arrange
            _patientRepoMock.Setup(x => x.GetByUserIdAsync(999)).ReturnsAsync((Patient?)null);
            _userRepoMock.Setup(x => x.GetByIdAsync(999)).ReturnsAsync((User?)null);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _patientService.GetOrCreateByUserIdAsync(999));
            Assert.Contains("User not found", ex.Message);
        }

        // ────────────────────────────────────────────────────────────────
        // GetAllAsync
        // ────────────────────────────────────────────────────────────────

        [Fact]
        public async Task GetAllAsync_ReturnsAllPatients()
        {
            // Arrange
            var patients = new List<Patient>
            {
                new() { PatientId = 1, FirstName = "Alice" },
                new() { PatientId = 2, FirstName = "Bob" }
            };

            _patientRepoMock.Setup(x => x.GetAllAsync()).ReturnsAsync(patients);

            // Act
            var result = await _patientService.GetAllAsync();

            // Assert
            Assert.Equal(2, result.Count());
        }

        // ────────────────────────────────────────────────────────────────
        // GetByGuardianAsync
        // ────────────────────────────────────────────────────────────────

        [Fact]
        public async Task GetByGuardianAsync_ReturnsPatients()
        {
            // Arrange
            var patients = new List<Patient>
            {
                new() { PatientId = 1, FirstName = "Child1", GuardianId = 5 },
                new() { PatientId = 2, FirstName = "Child2", GuardianId = 5 }
            };

            _patientRepoMock.Setup(x => x.GetByGuardianIdAsync(5)).ReturnsAsync(patients);

            // Act
            var result = await _patientService.GetByGuardianAsync(5);

            // Assert
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task GetByGuardianAsync_WhenNone_ReturnsEmptyList()
        {
            // Arrange
            _patientRepoMock.Setup(x => x.GetByGuardianIdAsync(999)).ReturnsAsync(new List<Patient>());

            // Act
            var result = await _patientService.GetByGuardianAsync(999);

            // Assert
            Assert.Empty(result);
        }
    }
}