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

        public FindingServiceTests() => _findingService = new FindingService(_appointmentRepoMock.Object, _findingRepoMock.Object);

        [Fact]
        public async Task UpsertAsync_WhenNoExistingFinding_CreatesNew()
        {
            var appointment = new Appointment { AppointmentId = 1, DoctorId = 1 };
            var request = new UpsertFindingRequest { Observations = "Improving", Recommendations = "Continue", NextSessionDate = new DateOnly(2026, 7, 15) };
            _appointmentRepoMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(appointment);
            _findingRepoMock.Setup(x => x.GetByAppointmentIdAsync(1)).ReturnsAsync((DoctorFinding?)null);
            _findingRepoMock.Setup(x => x.CreateAsync(It.IsAny<DoctorFinding>())).ReturnsAsync((DoctorFinding f) => { f.FindingId = 1; return f; });

            var result = await _findingService.UpsertAsync(1, 1, request);

            Assert.Equal(1, result.FindingId);
            Assert.Equal("Improving", result.Observations);
            Assert.Equal("Continue", result.Recommendations);
            _findingRepoMock.Verify(x => x.CreateAsync(It.IsAny<DoctorFinding>()), Times.Once);
            _findingRepoMock.Verify(x => x.UpdateAsync(It.IsAny<DoctorFinding>()), Times.Never);
        }

        [Fact]
        public async Task UpsertAsync_WhenExistingFinding_UpdatesExisting()
        {
            var appointment = new Appointment { AppointmentId = 1, DoctorId = 1 };
            var existing = new DoctorFinding { FindingId = 5, AppointmentId = 1, Observations = "Old" };
            var request = new UpsertFindingRequest { Observations = "Updated" };
            _appointmentRepoMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(appointment);
            _findingRepoMock.Setup(x => x.GetByAppointmentIdAsync(1)).ReturnsAsync(existing);
            _findingRepoMock.Setup(x => x.UpdateAsync(It.IsAny<DoctorFinding>())).ReturnsAsync((DoctorFinding f) => f);

            var result = await _findingService.UpsertAsync(1, 1, request);

            Assert.Equal(5, result.FindingId);
            Assert.Equal("Updated", result.Observations);
            _findingRepoMock.Verify(x => x.UpdateAsync(It.IsAny<DoctorFinding>()), Times.Once);
            _findingRepoMock.Verify(x => x.CreateAsync(It.IsAny<DoctorFinding>()), Times.Never);
        }

        [Fact]
        public async Task UpsertAsync_WhenDoctorDoesNotOwnAppointment_Throws()
        {
            var appointment = new Appointment { AppointmentId = 1, DoctorId = 2 };
            _appointmentRepoMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(appointment);
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _findingService.UpsertAsync(1, 1, new UpsertFindingRequest()));
            Assert.Contains("only submit reports for your own appointments", ex.Message);
        }

        [Fact]
        public async Task UpsertAsync_WithNonexistentAppointment_Throws()
        {
            _appointmentRepoMock.Setup(x => x.GetByIdAsync(999)).ReturnsAsync((Appointment?)null);
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _findingService.UpsertAsync(1, 999, new UpsertFindingRequest()));
        }

        [Fact]
        public async Task GetByAppointmentAsync_ReturnsFinding()
        {
            _findingRepoMock.Setup(x => x.GetByAppointmentIdAsync(1)).ReturnsAsync(new DoctorFinding { FindingId = 1, Observations = "Test" });
            Assert.NotNull(await _findingService.GetByAppointmentAsync(1));
        }

        [Fact]
        public async Task GetByAppointmentAsync_ReturnsNull()
        {
            _findingRepoMock.Setup(x => x.GetByAppointmentIdAsync(999)).ReturnsAsync((DoctorFinding?)null);
            Assert.Null(await _findingService.GetByAppointmentAsync(999));
        }

        [Fact]
        public async Task GetByPatientAsync_ReturnsFindings()
        {
            _findingRepoMock.Setup(x => x.GetByPatientIdAsync(1)).ReturnsAsync(new List<DoctorFinding> { new() { FindingId = 1 }, new() { FindingId = 2 } });
            Assert.Equal(2, (await _findingService.GetByPatientAsync(1)).Count());
        }
    }
}