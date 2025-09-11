using OrderLagerSystem.Api.Data;

namespace OrderLagerSystem.Api.Models;

/// <summary>
/// Kopplingstabell mellan användare och roller (many-to-many)
/// </summary>
public class UserRole
{
    public string UserId { get; set; } = null!;
    public ApplicationUser User { get; set; } = null!;

    public int RoleId { get; set; }
    public Role Role { get; set; } = null!;

    public DateTime AssignedUtc { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Vem som tilldelade rollen
    /// </summary>
    public string? AssignedByUserId { get; set; }
    public ApplicationUser? AssignedByUser { get; set; }
}
