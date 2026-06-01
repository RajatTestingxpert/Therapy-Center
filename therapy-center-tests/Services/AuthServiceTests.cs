using Moq;
using Xunit;
using TherapyCenter.Services.Implementations;
using TherapyCenter.Services.Interfaces;
using TherapyCenter.Repositories.Interfaces;
using TherapyCenter.Entities;
using TherapyCenter.DTO_s.Auth;
using Microsoft.Extensions.Configuration;

namespace TherapyCenter.Tests.Services
{
    public class AuthServiceTests
    {
        private readonly Mock<IUserRepository> _userRepoMock = new();
        private readonly Mock<IPatientRepository> _patientRepoMock = new();
        private readonly Mock<IConfiguration> _configMock = new();
        private readonly IAuthService _authService;

        public AuthServiceTests()
        {
            var jwtSectionMock = new Mock<IConfigurationSection>();
            jwtSectionMock.Setup(x => x["SecretKey"]).Returns("ThisIsMySuperSecretKey1234567890!");
            jwtSectionMock.Setup(x => x["Issuer"]).Returns("TherapyCenter");
            jwtSectionMock.Setup(x => x["Audience"]).Returns("TherapyCenterUsers");
            jwtSectionMock.Setup(x => x["ExpiryMinutes"]).Returns("1440");
            _configMock.Setup(x => x.GetSection("JwtSettings")).Returns(jwtSectionMock.Object);
            _authService = new AuthService(_userRepoMock.Object, _patientRepoMock.Object, _configMock.Object);
        }

        [Fact]
        public async Task RegisterAsync_WithPatientRole_CreatesUserAndPatient()
        {
            var request = new RegisterRequest { FirstName = "John", LastName = "Doe", Email = "john@example.com", Password = "Password123!", Role = "Patient" };
            _userRepoMock.Setup(x => x.EmailExistsAsync(request.Email)).ReturnsAsync(false);
            _userRepoMock.Setup(x => x.CreateAsync(It.IsAny<User>())).ReturnsAsync((User u) => { u.UserId = 1; return u; });
            _patientRepoMock.Setup(x => x.CreateAsync(It.IsAny<Patient>())).ReturnsAsync((Patient p) => { p.PatientId = 1; return p; });

            var result = await _authService.RegisterAsync(request);

            Assert.NotNull(result);
            Assert.NotEmpty(result.Token);
            Assert.Equal("Patient", result.Role);
            Assert.Equal(1, result.UserId);
            Assert.Equal("John Doe", result.FullName);
            _patientRepoMock.Verify(x => x.CreateAsync(It.Is<Patient>(p => p.UserId == 1)), Times.Once);
        }

        [Fact]
        public async Task RegisterAsync_WithGuardianRole_CreatesUserOnly()
        {
            var request = new RegisterRequest { FirstName = "Jane", LastName = "Smith", Email = "jane@example.com", Password = "Password123!", Role = "Guardian" };
            _userRepoMock.Setup(x => x.EmailExistsAsync(request.Email)).ReturnsAsync(false);
            _userRepoMock.Setup(x => x.CreateAsync(It.IsAny<User>())).ReturnsAsync((User u) => { u.UserId = 2; return u; });

            var result = await _authService.RegisterAsync(request);

            Assert.Equal("Guardian", result.Role);
            _patientRepoMock.Verify(x => x.CreateAsync(It.IsAny<Patient>()), Times.Never);
        }

        [Fact]
        public async Task RegisterAsync_WithNonPatientGuardianRole_ThrowsInvalidOperation()
        {
            var request = new RegisterRequest { FirstName = "Admin", LastName = "User", Email = "admin@test.com", Password = "P@ss1234", Role = "Admin" };
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _authService.RegisterAsync(request));
            Assert.Contains("Only Patient or Guardian", ex.Message);
        }

        [Fact]
        public async Task RegisterAsync_WithDuplicateEmail_ThrowsInvalidOperation()
        {
            var request = new RegisterRequest { Email = "existing@test.com", Role = "Patient", FirstName = "T", LastName = "U", Password = "P@ss1234" };
            _userRepoMock.Setup(x => x.EmailExistsAsync(request.Email)).ReturnsAsync(true);
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _authService.RegisterAsync(request));
            Assert.Contains("already registered", ex.Message);
        }

        [Fact]
        public async Task LoginAsync_WithValidCredentials_ReturnsAuthResponse()
        {
            var request = new LoginRequest { Email = "john@test.com", Password = "Password123!" };
            var user = new User { UserId = 1, FirstName = "John", LastName = "Doe", Email = "john@test.com", Role = "Doctor", PasswordHash = new PasswordHasherStub().HashPassword("Password123!"), IsActive = true };
            _userRepoMock.Setup(x => x.GetByEmailAsync(request.Email)).ReturnsAsync(user);

            var result = await _authService.LoginAsync(request);

            Assert.NotEmpty(result.Token);
            Assert.Equal("Doctor", result.Role);
        }

        [Fact]
        public async Task LoginAsync_WithWrongEmail_ThrowsUnauthorized()
        {
            var request = new LoginRequest { Email = "nonexistent@test.com", Password = "Password123!" };
            _userRepoMock.Setup(x => x.GetByEmailAsync(request.Email)).ReturnsAsync((User?)null);
            var ex = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _authService.LoginAsync(request));
            Assert.Contains("Invalid email or password", ex.Message);
        }

        [Fact]
        public async Task LoginAsync_WithWrongPassword_ThrowsUnauthorized()
        {
            var request = new LoginRequest { Email = "john@test.com", Password = "WrongPassword!" };
            var user = new User { UserId = 1, Email = "john@test.com", Role = "Doctor", PasswordHash = new PasswordHasherStub().HashPassword("CorrectPassword!"), IsActive = true };
            _userRepoMock.Setup(x => x.GetByEmailAsync(request.Email)).ReturnsAsync(user);
            var ex = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _authService.LoginAsync(request));
            Assert.Contains("Invalid email or password", ex.Message);
        }

        [Fact]
        public async Task LoginAsync_WithPatientRole_AutoCreatesPatientRecord()
        {
            var request = new LoginRequest { Email = "patient@test.com", Password = "Password123!" };
            var user = new User { UserId = 5, FirstName = "Peter", LastName = "Patient", Email = "patient@test.com", Role = "Patient", PasswordHash = new PasswordHasherStub().HashPassword("Password123!"), IsActive = true };
            _userRepoMock.Setup(x => x.GetByEmailAsync(request.Email)).ReturnsAsync(user);
            _patientRepoMock.Setup(x => x.GetByUserIdAsync(user.UserId)).ReturnsAsync((Patient?)null);
            _patientRepoMock.Setup(x => x.CreateAsync(It.IsAny<Patient>())).ReturnsAsync((Patient p) => { p.PatientId = 1; return p; });

            var result = await _authService.LoginAsync(request);

            Assert.Equal("Patient", result.Role);
            _patientRepoMock.Verify(x => x.CreateAsync(It.Is<Patient>(p => p.UserId == 5)), Times.Once);
        }

        [Fact]
        public async Task LoginAsync_WithPatientRoleAndExistingRecord_DoesNotCreateDuplicate()
        {
            var request = new LoginRequest { Email = "patient@test.com", Password = "Password123!" };
            var user = new User { UserId = 5, FirstName = "Peter", LastName = "Patient", Email = "patient@test.com", Role = "Patient", PasswordHash = new PasswordHasherStub().HashPassword("Password123!"), IsActive = true };
            var existingPatient = new Patient { PatientId = 10, UserId = 5 };
            _userRepoMock.Setup(x => x.GetByEmailAsync(request.Email)).ReturnsAsync(user);
            _patientRepoMock.Setup(x => x.GetByUserIdAsync(user.UserId)).ReturnsAsync(existingPatient);

            await _authService.LoginAsync(request);
            _patientRepoMock.Verify(x => x.CreateAsync(It.IsAny<Patient>()), Times.Never);
        }

        [Fact]
        public async Task CreateStaffAccountAsync_CreatesAnyRole()
        {
            var request = new RegisterRequest { FirstName = "Staff", LastName = "User", Email = "staff@test.com", Password = "Password123!", Role = "Receptionist" };
            _userRepoMock.Setup(x => x.EmailExistsAsync(request.Email)).ReturnsAsync(false);
            _userRepoMock.Setup(x => x.CreateAsync(It.IsAny<User>())).ReturnsAsync((User u) => { u.UserId = 10; return u; });

            var result = await _authService.CreateStaffAccountAsync(request);

            Assert.Equal("Receptionist", result.Role);
            Assert.Equal(10, result.UserId);
        }

        [Fact]
        public async Task CreateStaffAccountAsync_WithDuplicateEmail_ThrowsInvalidOperation()
        {
            var request = new RegisterRequest { Email = "existing@test.com", Role = "Receptionist", FirstName = "T", LastName = "U", Password = "P@ss1234" };
            _userRepoMock.Setup(x => x.EmailExistsAsync(request.Email)).ReturnsAsync(true);
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _authService.CreateStaffAccountAsync(request));
            Assert.Contains("already registered", ex.Message);
        }

        [Fact]
        public async Task RegisterAsync_ReturnsValidJwtToken()
        {
            var request = new RegisterRequest { FirstName = "Token", LastName = "Test", Email = "token@test.com", Password = "Password123!", Role = "Patient" };
            _userRepoMock.Setup(x => x.EmailExistsAsync(request.Email)).ReturnsAsync(false);
            _userRepoMock.Setup(x => x.CreateAsync(It.IsAny<User>())).ReturnsAsync((User u) => { u.UserId = 42; return u; });
            _patientRepoMock.Setup(x => x.CreateAsync(It.IsAny<Patient>())).ReturnsAsync((Patient p) => { p.PatientId = 1; return p; });

            var result = await _authService.RegisterAsync(request);

            var parts = result.Token.Split('.');
            Assert.Equal(3, parts.Length);
            Assert.NotEmpty(parts[0]);
            Assert.NotEmpty(parts[1]);
            Assert.NotEmpty(parts[2]);
        }
    }

    internal class PasswordHasherStub
    {
        private readonly Microsoft.AspNetCore.Identity.PasswordHasher<User> _hasher = new();
        public string HashPassword(string password) => _hasher.HashPassword(new User(), password);
    }
}