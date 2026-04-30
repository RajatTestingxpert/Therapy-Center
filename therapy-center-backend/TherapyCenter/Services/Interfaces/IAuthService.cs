using TherapyCenter.DTO_s.Auth;

namespace TherapyCenter.Services.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponse> RegisterAsync(RegisterRequest request);
        Task<AuthResponse> LoginAsync(LoginRequest request);
        Task<AuthResponse> CreateStaffAccountAsync(RegisterRequest request);
    }
}
