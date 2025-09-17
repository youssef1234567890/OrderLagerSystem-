using System.ComponentModel.DataAnnotations;

namespace OrderLagerSystem.Api.DTOs;

/// <summary>
/// Request model for updating user password
/// </summary>
public class UpdatePasswordRequest
{
    /// <summary>
    /// New password for the user
    /// </summary>
    [Required(ErrorMessage = "Password is required")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be between 6 and 100 characters")]
    public string NewPassword { get; set; } = string.Empty;
}
