using System.ComponentModel.DataAnnotations;

namespace OrderLagerSystem.Api.DTOs;

/// <summary>
/// Request model for updating user information
/// </summary>
public class UpdateUserRequest
{
    /// <summary>
    /// First name of the user
    /// </summary>
    [Required(ErrorMessage = "First name is required")]
    [StringLength(50, ErrorMessage = "First name cannot exceed 50 characters")]
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Last name of the user
    /// </summary>
    [Required(ErrorMessage = "Last name is required")]
    [StringLength(50, ErrorMessage = "Last name cannot exceed 50 characters")]
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Email address of the user
    /// </summary>
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [DatorlagerEmail(ErrorMessage = "Email must end with @datorlager.se")]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// List of roles to assign to the user
    /// </summary>
    public List<string> Roles { get; set; } = new();
}
