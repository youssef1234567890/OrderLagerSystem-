using OrderLagerSystem.Api.DTOs;

namespace OrderLagerSystem.Api.Services;

public interface IAuthenticationService
{
    Task<LoginResponse> LoginAsync(LoginRequest request);
    Task<RegisterResponse> RegisterAsync(RegisterRequest request);
}
