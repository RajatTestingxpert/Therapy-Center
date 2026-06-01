using Moq;
using Xunit;
using TherapyCenter.Services.Implementations;
using TherapyCenter.Services.Interfaces;
using TherapyCenter.Repositories.Interfaces;
using TherapyCenter.Entities;
using TherapyCenter.DTO_s.Finding;

namespace TherapyCenter.Tests.Services
{
    public class FindingServiceTests
    {
        private readonly Mock<IAppointmentRepository> _appointmentRepoMock = new();
        private readonly Mock<IFindingRepository> _findingRepoMock = new();
        private readonly IFindingService _findingService;

        public FindingServiceTests()
        {
            _findingService = new FindingService(_appointmentRepoMock.Object, _findingRepoMock.Object);
        }

        private Appointment CreateTestAppointment(int doctorId)
        {
            return new Appointment
            {
                AppointmentId = 1,
                PatientId = 1,
                DoctorId = doctorId,
                TherapyId = 1,
                Status = "Scheduled"
            };
        }

        private UpsertFindingRequest CreateTestRequest()
        {
            return new UpsertFindingRequest
            {
                Observations = "Patient shows improvement",
                Recommendations = "Continue with current treatment plan",
                NextSessionDate = new DateOnly(2026, 7, 15)
            };
        }

        // ────────────────────────────────────────────────────────────────
        // UpsertAsync — Create (no existing finding)
        // ────────────────────────────────────────────────────────────────

        [Fact]
        public async Task UpsertAsync_WhenNoExistingFinding_CreatesNew()
        {
            // Arrange
            var appointment = CreateTestAppointment(doctorId: 1);
            var request = CreateTestRequest();

            _appointmentRepoMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(appointment);
            _findingRepoMock.Setup(x => x.GetByAppointmentIdAsync(1)).ReturnsAsync((DoctorFinding?)null);
            _findingRepoMock.Setup(x => x.CreateAsync(It.IsAny<DoctorFinding>()))
                .ReturnsAsync((DoctorFinding f) =>
                {
                    f.FindingId = 1;
                    return f;
                });

            // Act
            var result = await _findingService.UpsertAsync(doctorId: 1, appointmentId: 1, request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.FindingId);
            Assert.Equal("Patient shows improvement", result.Observations);
            Assert.Equal("Continue with current treatment plan", result.Recommendations);
            Assert.Equal(new DateOnly(2026, 7, 15), result.NextSessionDate);
            _findingRepoMock.Verify(x => x.CreateAsync(It.Is<DoctorFinding>(f =>
                f.AppointmentId == 1 &&
                f.Observations == "Patient shows improvement")), Times.Once);
            _findingRepoMock.Verify(x => x.UpdateAsync(It.IsAny<DoctorFinding>()), Times.Never);
        }

        // ────────────────────────────────────────────────────────────────
        // UpsertAsync — Update (existing finding)
        // ────────────────────────────────────────────────────────────────

        [Fact]
        public async Task UpsertAsync_WhenExistingFinding_UpdatesExisting()
        {
            // Arrange
            var appointment = CreateTestAppointment(doctorId: 1);
            var request = new UpsertFindingRequest
            {
                Observations = "Updated observations",
                Recommendations = "Updated recommendations",
                NextSessionDate = new DateOnly(2026, 8, 1)
            };

            var existingFinding = new DoctorFinding
            {
                FindingId = 5,
                AppointmentId = 1,
                Observations = "Old observations",
                Recommendations = "Old recommendations",
                NextSessionDate = new DateOnly(2026, 7, 1)
            };

            _appointmentRepoMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(appointment);
            _findingRepoMock.Setup(x => x.GetByAppointmentIdAsync(1)).ReturnsAsync(existingFinding);
            _findingRepoMock.Setup(x => x.UpdateAsync(It.IsAny<DoctorFinding>()))
                .ReturnsAsync((DoctorFinding f) => f);

            // Act
            var result = await _findingService.UpsertAsync(doctorId: 1, appointmentId: 1, request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(5, result.FindingId); // same ID as existing
            Assert.Equal("Updated observations", result.Observations);
            Assert.Equal("Updated recommendations", result.Recommendations);
            Assert.Equal(new DateOnly(2026, 8, 1), result.NextSessionDate);
            _findingRepoMock.Verify(x => x.UpdateAsync(It.Is<DoctorFinding>(f =>
                f.FindingId == 5 &&
                f.Observations == "Updated observations")), Times.Once);
            _findingRepoMock.Verify(x => x.CreateAsync(It.IsAny<DoctorFinding>()), Times.Never);
        }

        // ────────────────────────────────────────────────────────────────
        // UpsertAsync — Doctor ownership validation
        // ────────────────────────────────────────────────────────────────

        [Fact]
        public async Task UpsertAsync_WhenDoctorDoesNotOwnAppointment_ThrowsInvalidOperation()
        {
            // Arrange
            var appointment = CreateTestAppointment(doctorId: 2); // different doctor
            var request = CreateTestRequest();

            _appointmentRepoMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(appointment);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _findingService.UpsertAsync(doctorId: 1, appointmentId: 1, request));
            Assert.Contains("only submit reports for your own appointments", ex.Message);
        }

        // ────────────────────────────────────────────────────────────────
        // UpsertAsync — Nonexistent appointment
        // ────────────────────────────────────────────────────────────────

        [Fact]
        public async Task UpsertAsync_WithNonexistentAppointment_ThrowsKeyNotFound()
        {
            // Arrange
            var request = CreateTestRequest();
            _appointmentRepoMock.Setup(x => x.GetByIdAsync(999)).ReturnsAsync((Appointment?)null);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _findingService.UpsertAsync(doctorId: 1, appointmentId: 999, request));
            Assert.Contains("Appointment not found", ex.Message);
        }

        // ────────────────────────────────────────────────────────────────
        // UpsertAsync — Partial data (only observations)
        // ────────────────────────────────────────────────────────────────

        [Fact]
        public async Task UpsertAsync_WithOnlyObservations_CreatesFinding()
        {
            // Arrange
            var appointment = CreateTestAppointment(doctorId: 1);
            var request = new UpsertFindingRequest
            {
                Observations = "Only observations provided",
                Recommendations = null,
                NextSessionDate = null
            };

            _appointmentRepoMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(appointment);
            _findingRepoMock.Setup(x => x.GetByAppointmentIdAsync(1)).ReturnsAsync((DoctorFinding?)null);
            _findingRepoMock.Setup(x => x.CreateAsync(It.IsAny<DoctorFinding>()))
                .ReturnsAsync((DoctorFinding f) =>
                {
                    f.FindingId = 1;
                    return f;
                });

            // Act
            var result = await _findingService.UpsertAsync(doctorId: 1, appointmentId: 1, request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Only observations provided", result.Observations);
            Assert.Null(result.Recommendations);
            Assert.Null(result.NextSessionDate);
        }

        // ────────────────────────────────────────────────────────────────
        // GetByAppointmentAsync
        // ────────────────────────────────────────────────────────────────

        [Fact]
        public async Task GetByAppointmentAsync_ReturnsFinding_WhenExists()
        {
            // Arrange
            var finding = new DoctorFinding
            {
                FindingId = 1,
                AppointmentId = 1,
                Observations = "Test observations"
            };

            _findingRepoMock.Setup(x => x.GetByAppointmentIdAsync(1)).ReturnsAsync(finding);

            // Act
            var result = await _findingService.GetByAppointmentAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result!.FindingId);
            Assert.Equal("Test observations", result.Observations);
        }

        [Fact]
        public async Task GetByAppointmentAsync_ReturnsNull_WhenNotExists()
        {
            // Arrange
            _findingRepoMock.Setup(x => x.GetByAppointmentIdAsync(999)).ReturnsAsync((DoctorFinding?)null);

            // Act
            var result = await _findingService.GetByAppointmentAsync(999);

            // Assert
            Assert.Null(result);
        }

        // ────────────────────────────────────────────────────────────────
        // GetByPatientAsync
        // ────────────────────────────────────────────────────────────────

        [Fact]
        public async Task GetByPatientAsync_ReturnsFindings()
        {
            // Arrange
            var findings = new List<DoctorFinding>
            {
                new() { FindingId = 1, AppointmentId = 1, Observations = "First" },
                new() { FindingId = 2, AppointmentId = 2, Observations = "Second" }
            };

            _findingRepoMock.Setup(x => x.GetByPatientIdAsync(1)).ReturnsAsync(findings);

            // Act
            var result = await _findingService.GetByPatientAsync(1);

            // Assert
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task GetByPatientAsync_WhenNone_ReturnsEmptyList()
        {
            // Arrange
            _findingRepoMock.Setup(x => x.GetByPatientIdAsync(999)).ReturnsAsync(new List<DoctorFinding>());

            // Act
            var result = await _findingService.GetByPatientAsync(999);

            // Assert
            Assert.Empty(result);
        }
    }
}