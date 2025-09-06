using System.ComponentModel.DataAnnotations;

namespace OrderLagerSystem.Models;

/// <summary>
/// Användarroller i systemet
/// </summary>
public class Role
{
    public int RoleId { get; set; }

    [Required, MaxLength(GlobalRules.StatusMaxLen)]
    public string Name { get; set; } = null!;

    [MaxLength(GlobalRules.DescriptionMaxLen)]
    public string? Description { get; set; }

    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}
