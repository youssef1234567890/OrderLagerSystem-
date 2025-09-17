using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderLagerSystem.Api.Services;
using OrderLagerSystem.Api.DTOs;

namespace OrderLagerSystem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")] // Only admins can manage users
public class UserController : ControllerBase
{
    private readonly IAuthenticationService _authenticationService;
    private readonly ILogger<UserController> _logger;

    public UserController(
        IAuthenticationService authenticationService,
        ILogger<UserController> logger)
    {
        _authenticationService = authenticationService;
        _logger = logger;
    }

    /// <summary>
    /// Gets all users in the system (admin only)
    /// </summary>
    /// <returns>List of all users with their roles</returns>
    [HttpGet]
    public async Task<ActionResult<List<UserInfo>>> GetAllUsers()
    {
        try
        {
            _logger.LogInformation("Admin {AdminId} requested all users", 
                User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);

            var users = await _authenticationService.GetAllUsersAsync();
            return Ok(users);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all users");
            return StatusCode(500, new { message = "An error occurred while retrieving users" });
        }
    }

    /// <summary>
    /// Gets a specific user by ID (admin only)
    /// </summary>
    /// <param name="id">User ID</param>
    /// <returns>User information</returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<UserInfo>> GetUser(string id)
    {
        try
        {
            _logger.LogInformation("Admin {AdminId} requested user {UserId}", 
                User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value, id);

            var user = await _authenticationService.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            return Ok(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user {UserId}", id);
            return StatusCode(500, new { message = "An error occurred while retrieving the user" });
        }
    }

    /// <summary>
    /// Updates user information (admin only)
    /// </summary>
    /// <param name="id">User ID</param>
    /// <param name="request">Updated user information</param>
    /// <returns>Success or failure result</returns>
    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateUser(string id, [FromBody] UpdateUserRequest request)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();
            
            return BadRequest(new { message = "Invalid data", errors });
        }

        try
        {
            _logger.LogInformation("Admin {AdminId} updating user {UserId}", 
                User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value, id);

            var success = await _authenticationService.UpdateUserAsync(id, request);
            if (!success)
            {
                return NotFound(new { message = "User not found or update failed" });
            }

            _logger.LogInformation("Admin {AdminId} successfully updated user {UserId}", 
                User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value, id);

            return Ok(new { message = "User updated successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user {UserId}", id);
            return StatusCode(500, new { message = "An error occurred while updating the user" });
        }
    }

    /// <summary>
    /// Toggles user active status (admin only)
    /// </summary>
    /// <param name="id">User ID</param>
    /// <returns>Success or failure result</returns>
    [HttpPatch("{id}/toggle-status")]
    public async Task<ActionResult> ToggleUserStatus(string id)
    {
        try
        {
            _logger.LogInformation("Admin {AdminId} toggling status for user {UserId}", 
                User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value, id);

            var success = await _authenticationService.ToggleUserStatusAsync(id);
            if (!success)
            {
                return NotFound(new { message = "User not found or status toggle failed" });
            }

            _logger.LogInformation("Admin {AdminId} successfully toggled status for user {UserId}", 
                User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value, id);

            return Ok(new { message = "User status updated successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling status for user {UserId}", id);
            return StatusCode(500, new { message = "An error occurred while updating user status" });
        }
    }

    /// <summary>
    /// Deletes a user (admin only)
    /// </summary>
    /// <param name="id">User ID</param>
    /// <returns>Success or failure result</returns>
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteUser(string id)
    {
        try
        {
            var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            
            // Prevent user from deleting themselves
            if (currentUserId == id)
            {
                _logger.LogWarning("User {UserId} attempted to delete themselves", currentUserId);
                return BadRequest(new { message = "You cannot delete your own account" });
            }

            _logger.LogInformation("Admin {AdminId} deleting user {UserId}", currentUserId, id);

            var success = await _authenticationService.DeleteUserAsync(id);
            if (!success)
            {
                return NotFound(new { message = "User not found or could not be deleted" });
            }

            _logger.LogInformation("Admin {AdminId} successfully deleted user {UserId}", 
                User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value, id);

            return Ok(new { message = "User deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user {UserId}", id);
            return StatusCode(500, new { message = "An error occurred while deleting the user" });
        }
    }

    /// <summary>
    /// Creates a new user (admin only)
    /// </summary>
    /// <param name="request">User creation data</param>
    /// <returns>Result of the user creation</returns>
    [HttpPost]
    public async Task<ActionResult<RegisterResponse>> CreateUser([FromBody] CreateUserRequest request)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();
            
            return BadRequest(RegisterResponse.CreateFailure("Invalid data", errors));
        }

        try
        {
            _logger.LogInformation("Admin {AdminId} creating new user {Email}", 
                User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value, 
                request.Email);

            var result = await _authenticationService.CreateUserAsync(request);
            
            if (result.Success)
            {
                _logger.LogInformation("Admin {AdminId} successfully created new user {Email}", 
                    User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value, 
                    request.Email);
                return Ok(result);
            }
            
            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user {Email}", request.Email);
            return StatusCode(500, new { message = "An error occurred while creating the user" });
        }
    }

    /// <summary>
    /// Updates user password (admin only)
    /// </summary>
    /// <param name="id">User ID</param>
    /// <param name="request">New password data</param>
    /// <returns>Success or failure result</returns>
    [HttpPatch("{id}/password")]
    public async Task<ActionResult> UpdatePassword(string id, [FromBody] UpdatePasswordRequest request)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();
            
            return BadRequest(new { message = "Invalid data", errors });
        }

        try
        {
            _logger.LogInformation("Admin {AdminId} updating password for user {UserId}", 
                User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value, id);

            var success = await _authenticationService.UpdatePasswordAsync(id, request);
            if (!success)
            {
                return NotFound(new { message = "User not found or password update failed" });
            }

            _logger.LogInformation("Admin {AdminId} successfully updated password for user {UserId}", 
                User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value, id);

            return Ok(new { message = "Password updated successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating password for user {UserId}", id);
            return StatusCode(500, new { message = "An error occurred while updating the password" });
        }
    }
}
