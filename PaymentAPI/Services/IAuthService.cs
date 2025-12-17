using PaymentAPI.DTOs;

namespace PaymentAPI.Services;

public interface IAuthService
{
    Task<LoginResponse?> LoginAsync(LoginRequest request);
    Task<bool> LogoutAsync(string token);
    Task<int?> ValidateTokenAsync(string token);
}