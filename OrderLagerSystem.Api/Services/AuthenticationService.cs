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
}
