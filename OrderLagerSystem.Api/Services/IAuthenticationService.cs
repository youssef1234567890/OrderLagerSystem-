using OrderLagerSystem.Api.DTOs;

namespace OrderLagerSystem.Api.Services;

public interface IAuthenticationService
{
    Task<LoginResponse> LoginAsync(LoginRequest request);
    Task<RegisterResponse> RegisterAsync(RegisterRequest request);
    Task<List<UserInfo>> GetAllUsersAsync();
    Task<UserInfo?> GetUserByIdAsync(string userId);
    Task<bool> UpdateUserAsync(string userId, UpdateUserRequest request);
    Task<bool> ToggleUserStatusAsync(string userId);
    Task<bool> DeleteUserAsync(string userId);
    Task<RegisterResponse> CreateUserAsync(CreateUserRequest request);
    Task<bool> UpdatePasswordAsync(string userId, UpdatePasswordRequest request);
}
