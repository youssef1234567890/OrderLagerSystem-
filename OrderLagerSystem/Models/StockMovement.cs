using System.ComponentModel.DataAnnotations;
using OrderLagerSystem.Data;

namespace OrderLagerSystem.Models;

/// <summary>
/// Lagertransaktioner för att spåra in- och utleveranser
/// </summary>
public class StockMovement
{
    public int StockMovementId { get; set; }

    public int ArticleId { get; set; }
    public Article Article { get; set; } = null!;

    /// <summary>
    /// Användare som registrerade rörelsen
    /// </summary>
    public string? UserId { get; set; }
    public ApplicationUser? User { get; set; }

    /// <summary>
    /// Typ av lagerrörelse
    /// </summary>
    [Required, MaxLength(GlobalRules.StatusMaxLen)]
    public string MovementType { get; set; } = null!; // "In", "Out", "Adjustment", "Initial"

    /// <summary>
    /// Kvantitet - positiv för inleverans, negativ för utleverans
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// Lagersaldo efter denna rörelse
    /// </summary>
    public int StockAfterMovement { get; set; }

    /// <summary>
    /// Referens till order om rörelsen är kopplad till en order
    /// </summary>
    public int? OrderId { get; set; }
    public Order? Order { get; set; }

    /// <summary>
    /// Anledning till lagerrörelsen
    /// </summary>
    [MaxLength(GlobalRules.CommentMaxLen)]
    public string? Reason { get; set; }

    /// <summary>
    /// Kommentarer
    /// </summary>
    [MaxLength(GlobalRules.CommentMaxLen)]
    public string? Notes { get; set; }

    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Lagertransaktionstyper
    /// </summary>
    public static class MovementTypes
    {
        public const string Initial = "Initial";        // Första registrering
        public const string Incoming = "Incoming";      // Inleverans
        public const string Outgoing = "Outgoing";      // Utleverans
        public const string Adjustment = "Adjustment";  // Inventering/justering
        public const string Reserved = "Reserved";      // Reserverat för order
        public const string Released = "Released";      // Frisläppt från reservation
    }
}
