using System.ComponentModel.DataAnnotations;
using OrderLagerSystem.Data;

namespace OrderLagerSystem.Models;

/// <summary>
/// Historik för orderändringar och statusuppdateringar
/// </summary>
public class OrderHistory
{
    public int OrderHistoryId { get; set; }

    public int OrderId { get; set; }
    public Order Order { get; set; } = null!;

    /// <summary>
    /// Användare som gjorde ändringen
    /// </summary>
    public string? ChangedByUserId { get; set; }
    public ApplicationUser? ChangedByUser { get; set; }

    /// <summary>
    /// Föregående status
    /// </summary>
    [MaxLength(GlobalRules.StatusMaxLen)]
    public string? OldStatus { get; set; }

    /// <summary>
    /// Ny status
    /// </summary>
    [Required, MaxLength(GlobalRules.StatusMaxLen)]
    public string NewStatus { get; set; } = null!;

    /// <summary>
    /// Kommentar till ändringen
    /// </summary>
    [MaxLength(GlobalRules.CommentMaxLen)]
    public string? Comment { get; set; }

    public DateTime ChangedUtc { get; set; } = DateTime.UtcNow;
}
