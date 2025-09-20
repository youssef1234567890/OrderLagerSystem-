using OrderLagerSystem.Api.DTOs;
using System.Text;
using System.Text.Json;

namespace OrderLagerSystem.Client.Services;

/// <summary>
/// Service for managing user operations
/// </summary>
public class UserService
{
    private readonly HttpClient _httpClient;
    private readonly AuthService _authService;
    private readonly ILogger<UserService> _logger;

    public UserService(HttpClient httpClient, AuthService authService, ILogger<UserService> logger)
    {
        _httpClient = httpClient;
        _authService = authService;
        _logger = logger;
    }

    /// <summary>
    /// Gets all users from the API
    /// </summary>
    /// <returns>List of all users</returns>
    public async Task<List<UserInfo>?> GetAllUsersAsync()
    {
        try
        {
            if (!_authService.IsAuthenticated)
            {
                _logger.LogWarning("User not authenticated");
                return null;
            }

            _httpClient.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _authService.AuthToken);

            var response = await _httpClient.GetAsync("/api/user");
            
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var users = JsonSerializer.Deserialize<List<UserInfo>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                
                _logger.LogInformation("Retrieved {UserCount} users", users?.Count ?? 0);
                return users;
            }
            else
            {
                _logger.LogWarning("Failed to retrieve users. Status: {StatusCode}", response.StatusCode);
                return null;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving users");
            return null;
        }
    }

    /// <summary>
    /// Gets a specific user by ID
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <returns>User information or null if not found</returns>
    public async Task<UserInfo?> GetUserByIdAsync(string userId)
    {
        try
        {
            if (!_authService.IsAuthenticated)
            {
                _logger.LogWarning("User not authenticated");
                return null;
            }

            _httpClient.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _authService.AuthToken);

            var response = await _httpClient.GetAsync($"/api/user/{userId}");
            
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var user = JsonSerializer.Deserialize<UserInfo>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                
                _logger.LogInformation("Retrieved user: {UserName}", user?.UserName);
                return user;
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogWarning("User not found: {UserId}", userId);
                return null;
            }
            else
            {
                _logger.LogWarning("Failed to retrieve user. Status: {StatusCode}", response.StatusCode);
                return null;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user {UserId}", userId);
            return null;
        }
    }

    /// <summary>
    /// Updates user information
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="request">Update request</param>
    /// <returns>True if successful, false otherwise</returns>
    public async Task<bool> UpdateUserAsync(string userId, UpdateUserRequest request)
    {
        try
        {
            if (!_authService.IsAuthenticated)
            {
                _logger.LogWarning("User not authenticated");
                return false;
            }

            _httpClient.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _authService.AuthToken);

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync($"/api/user/{userId}", content);
            
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Successfully updated user: {UserId}", userId);
                return true;
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("Failed to update user. Status: {StatusCode}, Error: {Error}", 
                    response.StatusCode, errorContent);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user {UserId}", userId);
            return false;
        }
    }

    /// <summary>
    /// Toggles user active status
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <returns>True if successful, false otherwise</returns>
    public async Task<bool> ToggleUserStatusAsync(string userId)
    {
        try
        {
            if (!_authService.IsAuthenticated)
            {
                _logger.LogWarning("User not authenticated");
                return false;
            }

            _httpClient.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _authService.AuthToken);

            var response = await _httpClient.PatchAsync($"/api/user/{userId}/toggle-status", null);
            
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Successfully toggled status for user: {UserId}", userId);
                return true;
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("Failed to toggle user status. Status: {StatusCode}, Error: {Error}", 
                    response.StatusCode, errorContent);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling status for user {UserId}", userId);
            return false;
        }
    }

    /// <summary>
    /// Creates a new user
    /// </summary>
    /// <param name="request">Create user request</param>
    /// <returns>UserInfo of created user or null if failed</returns>
    public async Task<UserInfo?> CreateUserAsync(CreateUserRequest request)
    {
        try
        {
            if (!_authService.IsAuthenticated)
            {
                _logger.LogWarning("User not authenticated");
                return null;
            }

            _httpClient.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _authService.AuthToken);

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("/api/user", content);
            
            if (response.IsSuccessStatusCode)
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                var registerResponse = JsonSerializer.Deserialize<RegisterResponse>(responseJson, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                
                if (registerResponse?.Success == true && registerResponse.User != null)
                {
                    _logger.LogInformation("Successfully created user: {Email}", request.Email);
                    return registerResponse.User;
                }
                else
                {
                    _logger.LogWarning("Failed to create user: {Message}", registerResponse?.Message);
                    return null;
                }
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("Failed to create user. Status: {StatusCode}, Error: {Error}", 
                    response.StatusCode, errorContent);
                return null;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user {Email}", request.Email);
            return null;
        }
    }

    /// <summary>
    /// Deletes a user
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <returns>True if successful, false otherwise</returns>
    public async Task<bool> DeleteUserAsync(string userId)
    {
        try
        {
            if (!_authService.IsAuthenticated)
            {
                _logger.LogWarning("User not authenticated");
                return false;
            }

            _httpClient.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _authService.AuthToken);

            var response = await _httpClient.DeleteAsync($"/api/user/{userId}");
            
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Successfully deleted user: {UserId}", userId);
                return true;
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("Failed to delete user. Status: {StatusCode}, Error: {Error}", 
                    response.StatusCode, errorContent);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user {UserId}", userId);
            return false;
        }
    }

    /// <summary>
    /// Updates user password
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="request">Update password request</param>
    /// <returns>True if successful, false otherwise</returns>
    public async Task<bool> UpdatePasswordAsync(string userId, UpdatePasswordRequest request)
    {
        try
        {
            if (!_authService.IsAuthenticated)
            {
                _logger.LogWarning("User not authenticated");
                return false;
            }

            _httpClient.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _authService.AuthToken);

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PatchAsync($"/api/user/{userId}/password", content);
            
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Successfully updated password for user: {UserId}", userId);
                return true;
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("Failed to update password. Status: {StatusCode}, Error: {Error}", 
                    response.StatusCode, errorContent);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating password for user {UserId}", userId);
            return false;
        }
    }
}
