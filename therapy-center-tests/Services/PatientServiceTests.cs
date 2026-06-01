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

        public PatientServiceTests() => _patientService = new PatientService(_patientRepoMock.Object, _userRepoMock.Object);

        [Fact] public async Task CreateAsync_WithValidRequest_CreatesPatient()
        {
            var request = new CreatePatientRequest { FirstName = "Peter", LastName = "Patient", Gender = "Male", MedicalHistory = "None" };
            _patientRepoMock.Setup(x => x.CreateAsync(It.IsAny<Patient>())).ReturnsAsync((Patient p) => { p.PatientId = 1; return p; });
            var result = await _patientService.CreateAsync(request);
            Assert.Equal(1, result.PatientId);
            Assert.Equal("Peter", result.FirstName);
            Assert.Equal("Male", result.Gender);
        }

        [Fact] public async Task CreateAsync_WithGuardian_AssignsGuardianId()
        {
            var request = new CreatePatientRequest { FirstName = "Child", LastName = "Test", GuardianId = 5 };
            _patientRepoMock.Setup(x => x.CreateAsync(It.IsAny<Patient>())).ReturnsAsync((Patient p) => { p.PatientId = 1; return p; });
            Assert.Equal(5, (await _patientService.CreateAsync(request)).GuardianId);
        }

        [Fact] public async Task GetByIdAsync_ReturnsPatient()
        {
            _patientRepoMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(new Patient { PatientId = 1, FirstName = "Peter" });
            Assert.NotNull(await _patientService.GetByIdAsync(1));
        }

        [Fact] public async Task GetByIdAsync_WithInvalidId_ReturnsNull()
        {
            _patientRepoMock.Setup(x => x.GetByIdAsync(999)).ReturnsAsync((Patient?)null);
            Assert.Null(await _patientService.GetByIdAsync(999));
        }

        [Fact] public async Task GetOrCreateByUserIdAsync_WhenExists_ReturnsExisting()
        {
            var existing = new Patient { PatientId = 1, UserId = 1, FirstName = "Existing" };
            _patientRepoMock.Setup(x => x.GetByUserIdAsync(1)).ReturnsAsync(existing);
            Assert.Equal("Existing", (await _patientService.GetOrCreateByUserIdAsync(1)).FirstName);
            _patientRepoMock.Verify(x => x.CreateAsync(It.IsAny<Patient>()), Times.Never);
        }

        [Fact] public async Task GetOrCreateByUserIdAsync_WhenNotExists_CreatesNew()
        {
            var user = new User { UserId = 2, FirstName = "New", LastName = "User", Role = "Patient" };
            _patientRepoMock.Setup(x => x.GetByUserIdAsync(2)).ReturnsAsync((Patient?)null);
            _userRepoMock.Setup(x => x.GetByIdAsync(2)).ReturnsAsync(user);
            _patientRepoMock.Setup(x => x.CreateAsync(It.IsAny<Patient>())).ReturnsAsync((Patient p) => { p.PatientId = 2; return p; });
            var result = await _patientService.GetOrCreateByUserIdAsync(2);
            Assert.Equal("New", result.FirstName);
        }

        [Fact] public async Task GetOrCreateByUserIdAsync_WithNonPatientUser_Throws()
        {
            _patientRepoMock.Setup(x => x.GetByUserIdAsync(3)).ReturnsAsync((Patient?)null);
            _userRepoMock.Setup(x => x.GetByIdAsync(3)).ReturnsAsync(new User { UserId = 3, Role = "Doctor" });
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _patientService.GetOrCreateByUserIdAsync(3));
            Assert.Contains("Only patient users", ex.Message);
        }

        [Fact] public async Task GetAllAsync_ReturnsAll()
        {
            _patientRepoMock.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<Patient> { new() { PatientId = 1 }, new() { PatientId = 2 } });
            Assert.Equal(2, (await _patientService.GetAllAsync()).Count());
        }

        [Fact] public async Task GetByGuardianAsync_ReturnsPatients()
        {
            _patientRepoMock.Setup(x => x.GetByGuardianIdAsync(5)).ReturnsAsync(new List<Patient> { new() { PatientId = 1, GuardianId = 5 }, new() { PatientId = 2, GuardianId = 5 } });
            Assert.Equal(2, (await _patientService.GetByGuardianAsync(5)).Count());
        }

        [Fact] public async Task GetByGuardianAsync_WhenNone_ReturnsEmpty()
        {
            _patientRepoMock.Setup(x => x.GetByGuardianIdAsync(999)).ReturnsAsync(new List<Patient>());
            Assert.Empty(await _patientService.GetByGuardianAsync(999));
        }
    }
}