using System.ComponentModel.DataAnnotations;

namespace OrderLagerSystem.Models;

/// <summary>
/// Leverans kopplad till en order
/// </summary>
public class Delivery
{
    public int DeliveryId { get; set; }

    public int OrderId { get; set; }
    public Order Order { get; set; } = null!;

    /// <summary>
    /// Spårningsnummer från leverantör
    /// </summary>
    [MaxLength(GlobalRules.TrackingMaxLen)]
    public string? TrackingNumber { get; set; }

    /// <summary>
    /// Leveransstatus
    /// </summary>
    [Required, MaxLength(GlobalRules.StatusMaxLen)]
    public string Status { get; set; } = GlobalRules.DeliveryStatus.Pending;

    /// <summary>
    /// Leveransadress
    /// </summary>
    [MaxLength(GlobalRules.DescriptionMaxLen)]
    public string? DeliveryAddress { get; set; }

    /// <summary>
    /// Leveransmetod (hemleverans, upphämtning, etc.)
    /// </summary>
    [MaxLength(GlobalRules.NameMaxLen)]
    public string? DeliveryMethod { get; set; }

    /// <summary>
    /// Kommentarer till leveransen
    /// </summary>
    [MaxLength(GlobalRules.CommentMaxLen)]
    public string? Notes { get; set; }

    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
    public DateTime? ShippedUtc { get; set; }
    public DateTime? DeliveredUtc { get; set; }
    public DateTime? EstimatedDeliveryUtc { get; set; }
}
