
using TherapyCenter.DTO_s.Auth;
using TherapyCenter.Entities;
using TherapyCenter.Repositories.Interfaces;
using TherapyCenter.Services.Interfaces;


using Microsoft.AspNetCore.Identity;      // For PasswordHasher<>
using System.Security.Claims;               // For Claim
using System.IdentityModel.Tokens.Jwt;      // For JwtSecurityToken, JwtSecurityTokenHandler
using Microsoft.IdentityModel.Tokens;      // For SymmetricSecurityKey, SigningCredentials, SecurityAlgorithms
using System.Text;                          // For Encoding.UTF8

namespace TherapyCenter.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepo;
        private readonly IPatientRepository _patientRepo;
        private readonly IConfiguration _config;
        private readonly PasswordHasher<User> _hasher = new();

        private static readonly HashSet<string> SelfRegisterRoles = new() { "Patient", "Guardian" };

        public AuthService(IUserRepository userRepo, IPatientRepository patientRepo, IConfiguration config)
        {
            _userRepo = userRepo;
            _patientRepo = patientRepo;
            _config = config;
        }

        public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
        {
            if (!SelfRegisterRoles.Contains(request.Role))
                throw new InvalidOperationException("Only Patient or Guardian roles can self-register.");

            return await CreateUserAsync(request);
        }

        public async Task<AuthResponse> CreateStaffAccountAsync(RegisterRequest request)
        {
            return await CreateUserAsync(request);
        }

        public async Task<AuthResponse> LoginAsync(LoginRequest request)
        {
            var user = await _userRepo.GetByEmailAsync(request.Email)
                       ?? throw new UnauthorizedAccessException("Invalid email or password.");

            var result = _hasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);

            if (result == PasswordVerificationResult.Failed)
                throw new UnauthorizedAccessException("Invalid email or password.");

            if (string.Equals(user.Role?.Trim(), "Patient", StringComparison.OrdinalIgnoreCase))
            {
                var patient = await _patientRepo.GetByUserIdAsync(user.UserId);
                if (patient == null)
                {
                    await _patientRepo.CreateAsync(new Patient
                    {
                        UserId = user.UserId,
                        FirstName = user.FirstName,
                        LastName = user.LastName
                    });
                }
            }

            return BuildAuthResponse(user);
        }

        private async Task<AuthResponse> CreateUserAsync(RegisterRequest request)
        {
            if (await _userRepo.EmailExistsAsync(request.Email))
                throw new InvalidOperationException("Email is already registered.");

            var user = new User
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                Role = request.Role,
                PhoneNumber = request.PhoneNumber
            };

            user.PasswordHash = _hasher.HashPassword(user, request.Password);

            await _userRepo.CreateAsync(user);

            if (string.Equals(request.Role, "Patient", StringComparison.OrdinalIgnoreCase))
            {
                await _patientRepo.CreateAsync(new Patient
                {
                    UserId = user.UserId,
                    FirstName = user.FirstName,
                    LastName = user.LastName
                });
            }

            return BuildAuthResponse(user);
        }

        private AuthResponse BuildAuthResponse(User user)
        {
            return new AuthResponse
            {
                Token = GenerateToken(user),
                Role = user.Role,
                UserId = user.UserId,
                FullName = $"{user.FirstName} {user.LastName}"
            };
        }

        private string GenerateToken(User user)
        {
            var jwt = _config.GetSection("JwtSettings");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["SecretKey"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Email,          user.Email),
                new Claim(ClaimTypes.Role,           user.Role),
                new Claim(ClaimTypes.Name,           $"{user.FirstName} {user.LastName}")
            };

            var token = new JwtSecurityToken(
                issuer: jwt["Issuer"],
                audience: jwt["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(double.Parse(jwt["ExpiryMinutes"]!)),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
