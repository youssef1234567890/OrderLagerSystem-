using Microsoft.AspNetCore.Identity;
using OrderLagerSystem.Api.Data;
using OrderLagerSystem.Api.Models;
using OrderLagerSystem.Api.DTOs;

namespace OrderLagerSystem.Api.Services;

public class AuthenticationService : IAuthenticationService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly ILogger<AuthenticationService> _logger;

    public AuthenticationService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IJwtTokenService jwtTokenService,
        ILogger<AuthenticationService> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _jwtTokenService = jwtTokenService;
        _logger = logger;
    }

    /// <summary>
    /// Logs in a user
    /// </summary>
    /// <param name="request">Login request</param>
    /// <returns>Login response</returns>
    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        try
        {
            _logger.LogInformation("Login attempt for user: {UsernameOrEmail}", request.UsernameOrEmail);

            // Find user by either username or email
            ApplicationUser? user = null;
            
            // First, try to find by username
            user = await _userManager.FindByNameAsync(request.UsernameOrEmail);
            
            // If not found, try with email
            if (user == null && request.UsernameOrEmail.Contains('@'))
            {
                user = await _userManager.FindByEmailAsync(request.UsernameOrEmail);
            }

            if (user == null)
            {
                _logger.LogWarning("Login failed: User not found for {UsernameOrEmail}", request.UsernameOrEmail);
                return new LoginResponse
                {
                    Success = false,
                    Message = "Invalid username/email or password"
                };
            }

            // Check password
            var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);

            if (!result.Succeeded)
            {
                _logger.LogWarning("Login failed: Invalid password for user {UserId}", user.Id);
                
                var message = result.IsLockedOut 
                    ? "Account is locked due to too many failed login attempts"
                    : "Invalid username/email or password";
                
                return new LoginResponse
                {
                    Success = false,
                    Message = message
                };
            }

            // Generate JWT token
            var token = await _jwtTokenService.GenerateTokenAsync(user);
            var expiresAt = await _jwtTokenService.GetTokenExpirationAsync();
            
            // Get user roles
            var roles = await _userManager.GetRolesAsync(user);

            _logger.LogInformation("Login successful for user {UserId}", user.Id);

            return new LoginResponse
            {
                Success = true,
                Message = "Login successful",
                Token = token,
                ExpiresAt = expiresAt,
                User = new UserInfo
                {
                    Id = user.Id,
                    UserName = user.UserName ?? string.Empty,
                    Email = user.Email ?? string.Empty,
                    FullName = user.FullName ?? string.Empty,
                    FirstName = user.FirstName ?? string.Empty,
                    LastName = user.LastName ?? string.Empty,
                    IsActive = user.IsActive,
                    Roles = roles.ToList()
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for user: {UsernameOrEmail}", request.UsernameOrEmail);
            return new LoginResponse
            {
                Success = false,
                Message = "An error occurred during login. Please try again later."
            };
        }
    }

    /// <summary>
    /// Registers a new user
    /// </summary>
    /// <param name="request">Register request</param>
    /// <returns>Register response</returns>
    public async Task<RegisterResponse> RegisterAsync(RegisterRequest request)
    {
        try
        {
            _logger.LogInformation("Registration attempt for user: {UserName} ({Email})", request.UserName, request.Email);

            // Check if username already exists
            var existingUserByUsername = await _userManager.FindByNameAsync(request.UserName);
            if (existingUserByUsername != null)
            {
                _logger.LogWarning("Registration failed: Username {UserName} already exists", request.UserName);
                return RegisterResponse.CreateFailure("Username already exists", new List<string> { "Username is already taken" });
            }

            // Check if email already exists
            var existingUserByEmail = await _userManager.FindByEmailAsync(request.Email);
            if (existingUserByEmail != null)
            {
                _logger.LogWarning("Registration failed: Email {Email} already exists", request.Email);
                return RegisterResponse.CreateFailure("Email address already exists", new List<string> { "Email address is already registered" });
            }

            // Create new user
            var newUser = new ApplicationUser
            {
                UserName = request.UserName,
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                FullName = $"{request.FirstName} {request.LastName}",
                EmailConfirmed = true, // Admin-created users are automatically confirmed
                IsActive = true,
                CreatedUtc = DateTime.UtcNow,
                UpdatedUtc = DateTime.UtcNow
            };

            // Create user with password
            var createResult = await _userManager.CreateAsync(newUser, request.Password);
            
            if (!createResult.Succeeded)
            {
                var errors = createResult.Errors.Select(e => e.Description).ToList();
                _logger.LogWarning("Registration failed for user {UserName}: {Errors}", request.UserName, string.Join(", ", errors));
                return RegisterResponse.CreateFailure("Could not create user", errors);
            }

            // Add roles
            if (request.Roles.Any())
            {
                foreach (var role in request.Roles)
                {
                    // Check that the role exists through RoleManager
                    try
                    {
                        var roleResult = await _userManager.AddToRoleAsync(newUser, role);
                        if (roleResult.Succeeded)
                        {
                            _logger.LogInformation("Added role {Role} to user {UserId}", role, newUser.Id);
                        }
                        else
                        {
                            _logger.LogWarning("Failed to add role {Role} to user {UserId}: {Errors}", 
                                role, newUser.Id, string.Join(", ", roleResult.Errors.Select(e => e.Description)));
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error adding role {Role} to user {UserId}", role, newUser.Id);
                    }
                }
            }

            // Get user roles for response
            var assignedRoles = await _userManager.GetRolesAsync(newUser);

            _logger.LogInformation("User {UserId} registered successfully by admin", newUser.Id);

            return RegisterResponse.CreateSuccess(new UserInfo
            {
                Id = newUser.Id,
                UserName = newUser.UserName!,
                Email = newUser.Email!,
                FullName = newUser.FullName,
                FirstName = newUser.FirstName,
                LastName = newUser.LastName,
                Roles = assignedRoles.ToList(),
                IsActive = newUser.IsActive
            }, "User created successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration for user: {UserName}", request.UserName);
            return RegisterResponse.CreateFailure("An error occurred during registration. Please try again later.");
        }
    }

    /// <summary>
    /// Gets all users
    /// </summary>
    /// <returns>List of all users</returns>
    public async Task<List<UserInfo>> GetAllUsersAsync()
    {
        try
        {
            _logger.LogInformation("Retrieving all users");

            var users = _userManager.Users.ToList();
            var userInfoList = new List<UserInfo>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userInfoList.Add(new UserInfo
                {
                    Id = user.Id,
                    UserName = user.UserName ?? string.Empty,
                    Email = user.Email ?? string.Empty,
                    FullName = user.FullName ?? string.Empty,
                    FirstName = user.FirstName ?? string.Empty,
                    LastName = user.LastName ?? string.Empty,
                    IsActive = user.IsActive,
                    Roles = roles.ToList()
                });
            }

            _logger.LogInformation("Retrieved {UserCount} users", userInfoList.Count);
            return userInfoList;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all users");
            return new List<UserInfo>();
        }
    }

    /// <summary>
    /// Gets a user by ID
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <returns>User information</returns>
    public async Task<UserInfo?> GetUserByIdAsync(string userId)
    {
        try
        {
            _logger.LogInformation("Retrieving user with ID: {UserId}", userId);

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("User not found with ID: {UserId}", userId);
                return null;
            }

            var roles = await _userManager.GetRolesAsync(user);
            
            _logger.LogInformation("Retrieved user: {UserName}", user.UserName);

            return new UserInfo
            {
                Id = user.Id,
                UserName = user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                FullName = user.FullName ?? string.Empty,
                FirstName = user.FirstName ?? string.Empty,
                LastName = user.LastName ?? string.Empty,
                IsActive = user.IsActive,
                Roles = roles.ToList()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user with ID: {UserId}", userId);
            return null;
        }
    }

    /// <summary>
    /// Updates a user
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="request">Update request</param>
    /// <returns>True if successful, false otherwise</returns>
    public async Task<bool> UpdateUserAsync(string userId, UpdateUserRequest request)
    {
        try
        {
            _logger.LogInformation("Updating user with ID: {UserId}", userId);

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("User not found with ID: {UserId}", userId);
                return false;
            }

            // Update user properties
            user.FirstName = request.FirstName;
            user.LastName = request.LastName;
            user.FullName = $"{request.FirstName} {request.LastName}";
            user.Email = request.Email;
            user.UserName = request.Email; // Use email as username
            user.UpdatedUtc = DateTime.UtcNow;

            // Update user
            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                _logger.LogWarning("Failed to update user {UserId}: {Errors}", 
                    userId, string.Join(", ", updateResult.Errors.Select(e => e.Description)));
                return false;
            }

            // Update roles
            var currentRoles = await _userManager.GetRolesAsync(user);
            
            // Remove all current roles
            if (currentRoles.Any())
            {
                var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
                if (!removeResult.Succeeded)
                {
                    _logger.LogWarning("Failed to remove roles from user {UserId}: {Errors}", 
                        userId, string.Join(", ", removeResult.Errors.Select(e => e.Description)));
                }
            }

            // Add new roles
            if (request.Roles.Any())
            {
                var addResult = await _userManager.AddToRolesAsync(user, request.Roles);
                if (!addResult.Succeeded)
                {
                    _logger.LogWarning("Failed to add roles to user {UserId}: {Errors}", 
                        userId, string.Join(", ", addResult.Errors.Select(e => e.Description)));
                }
            }

            _logger.LogInformation("Successfully updated user: {UserId}", userId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user with ID: {UserId}", userId);
            return false;
        }
    }

    /// <summary>
    /// Toggles a user's active status
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <returns>True if successful, false otherwise</returns>
    public async Task<bool> ToggleUserStatusAsync(string userId)
    {
        try
        {
            _logger.LogInformation("Toggling user status for ID: {UserId}", userId);

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("User not found with ID: {UserId}", userId);
                return false;
            }

            user.IsActive = !user.IsActive;
            user.UpdatedUtc = DateTime.UtcNow;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                _logger.LogWarning("Failed to toggle user status for {UserId}: {Errors}", 
                    userId, string.Join(", ", result.Errors.Select(e => e.Description)));
                return false;
            }

            _logger.LogInformation("Successfully toggled user status for {UserId} to {IsActive}", 
                userId, user.IsActive);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling user status for ID: {UserId}", userId);
            return false;
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
            _logger.LogInformation("Deleting user with ID: {UserId}", userId);

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("User not found with ID: {UserId}", userId);
                return false;
            }

            // Note: We can't easily check if user is deleting themselves without HttpContext
            // This check would need to be done at the controller level

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                _logger.LogWarning("Failed to delete user {UserId}: {Errors}", 
                    userId, string.Join(", ", result.Errors.Select(e => e.Description)));
                return false;
            }

            _logger.LogInformation("Successfully deleted user: {UserId}", userId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user with ID: {UserId}", userId);
            return false;
        }
    }

    /// <summary>
    /// Creates a new user
    /// </summary>
    /// <param name="request">Create user request</param>
    /// <returns>UserInfo of created user or null if failed</returns>
    public async Task<RegisterResponse> CreateUserAsync(CreateUserRequest request)
    {
        try
        {
            _logger.LogInformation("Creating new user: {Email}", request.Email);

            // Check if email already exists
            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser != null)
            {
                _logger.LogWarning("User creation failed: Email {Email} already exists", request.Email);
                return RegisterResponse.CreateFailure("Email address already exists", new List<string> { "Email address is already registered" });
            }

            // Create new user
            var newUser = new ApplicationUser
            {
                UserName = request.Email,
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                FullName = $"{request.FirstName} {request.LastName}",
                EmailConfirmed = true, // Admin-created users are automatically confirmed
                IsActive = true,
                CreatedUtc = DateTime.UtcNow,
                UpdatedUtc = DateTime.UtcNow
            };

            // Create user with password
            var createResult = await _userManager.CreateAsync(newUser, request.Password);
            
            if (!createResult.Succeeded)
            {
                var errors = createResult.Errors.Select(e => e.Description).ToList();
                _logger.LogWarning("User creation failed for {Email}: {Errors}", request.Email, string.Join(", ", errors));
                return RegisterResponse.CreateFailure("Could not create user", errors);
            }

            // Add roles
            if (request.Roles.Any())
            {
                foreach (var role in request.Roles)
                {
                    try
                    {
                        var roleResult = await _userManager.AddToRoleAsync(newUser, role);
                        if (roleResult.Succeeded)
                        {
                            _logger.LogInformation("Added role {Role} to user {UserId}", role, newUser.Id);
                        }
                        else
                        {
                            _logger.LogWarning("Failed to add role {Role} to user {UserId}: {Errors}", 
                                role, newUser.Id, string.Join(", ", roleResult.Errors.Select(e => e.Description)));
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error adding role {Role} to user {UserId}", role, newUser.Id);
                    }
                }
            }

            // Get user roles for response
            var assignedRoles = await _userManager.GetRolesAsync(newUser);

            _logger.LogInformation("User {UserId} created successfully", newUser.Id);

            return RegisterResponse.CreateSuccess(new UserInfo
            {
                Id = newUser.Id,
                UserName = newUser.UserName!,
                Email = newUser.Email!,
                FullName = newUser.FullName,
                FirstName = newUser.FirstName,
                LastName = newUser.LastName,
                Roles = assignedRoles.ToList(),
                IsActive = newUser.IsActive
            }, "User created successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user: {Email}", request.Email);
            return RegisterResponse.CreateFailure("An error occurred during user creation. Please try again later.");
        }
    }

    /// <summary>
    /// Updates a user's password
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="request">Update password request</param>
    /// <returns>True if successful, false otherwise</returns>
    public async Task<bool> UpdatePasswordAsync(string userId, UpdatePasswordRequest request)
    {
        try
        {
            _logger.LogInformation("Updating password for user ID: {UserId}", userId);

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("User not found with ID: {UserId}", userId);
                return false;
            }

            // Remove current password
            var removePasswordResult = await _userManager.RemovePasswordAsync(user);
            if (!removePasswordResult.Succeeded)
            {
                _logger.LogWarning("Failed to remove current password for user {UserId}: {Errors}", 
                    userId, string.Join(", ", removePasswordResult.Errors.Select(e => e.Description)));
                return false;
            }

            // Add new password
            var addPasswordResult = await _userManager.AddPasswordAsync(user, request.NewPassword);
            if (!addPasswordResult.Succeeded)
            {
                _logger.LogWarning("Failed to add new password for user {UserId}: {Errors}", 
                    userId, string.Join(", ", addPasswordResult.Errors.Select(e => e.Description)));
                return false;
            }

            _logger.LogInformation("Successfully updated password for user: {UserId}", userId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating password for user ID: {UserId}", userId);
            return false;
        }
    }
}
