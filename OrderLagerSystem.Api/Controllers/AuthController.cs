using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderLagerSystem.Api.Services;
using OrderLagerSystem.Api.DTOs;

namespace OrderLagerSystem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthenticationService _authenticationService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IAuthenticationService authenticationService,
        ILogger<AuthController> logger)
    {
        _authenticationService = authenticationService;
        _logger = logger;
    }

    /// <summary>
    /// Logs in a user with username/email and password
    /// </summary>
    /// <param name="request">Login credentials</param>
    /// <returns>JWT token and user information</returns>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Invalid login request: {Errors}", 
                string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
            
            return BadRequest(new LoginResponse
            {
                Success = false,
                Message = "Invalid login credentials"
            });
        }

        var result = await _authenticationService.LoginAsync(request);

        if (!result.Success)
        {
            return Unauthorized(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Tests if the user is authenticated
    /// </summary>
    /// <returns>Information about the logged in user</returns>
    [HttpGet("me")]
    [Authorize]
    public ActionResult<object> GetCurrentUser()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var username = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;
        var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
        var fullName = User.FindFirst("fullName")?.Value;
        var roles = User.FindAll(System.Security.Claims.ClaimTypes.Role).Select(c => c.Value).ToList();

        return Ok(new
        {
            id = userId,
            username = username,
            email = email,
            fullName = fullName,
            roles = roles,
            isAuthenticated = true
        });
    }

    /// <summary>
    /// Registers a new user (admin only)
    /// </summary>
    /// <param name="request">Registration data for new user</param>
    /// <returns>Result of the registration</returns>
    [HttpPost("register")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<RegisterResponse>> Register([FromBody] RegisterRequest request)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();
            
            return BadRequest(RegisterResponse.CreateFailure("Invalid data", errors));
        }

        var result = await _authenticationService.RegisterAsync(request);
        
        if (result.Success)
        {
            _logger.LogInformation("Admin {AdminId} successfully registered new user {UserName}", 
                User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value, 
                request.UserName);
            return Ok(result);
        }
        
        return BadRequest(result);
    }

    /// <summary>
    /// Logs out the user 
    /// </summary>
    /// <returns>Logout confirmation</returns>
    [HttpPost("logout")]
    [Authorize]
    public ActionResult Logout()
    {
        _logger.LogInformation("User {UserId} logged out", User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);
        
        return Ok(new { message = "Logout successful" });
    }
}
