using System.ComponentModel.DataAnnotations;
using OrderLagerSystem.Data;

namespace OrderLagerSystem.Models;

/// <summary>
/// Order med status och historik
/// </summary>
public class Order
{
    public int OrderId { get; set; }

    /// <summary>
    /// Koppling till användare som skapat ordern
    /// </summary>
    [Required]
    public string UserId { get; set; } = null!;
    public ApplicationUser User { get; set; } = null!;

    /// <summary>
    /// Externt ordernummer för referens
    /// </summary>
    [MaxLength(GlobalRules.NameMaxLen)]
    public string? ExternalOrderNo { get; set; }

    /// <summary>
    /// Orderstatus (Created, Confirmed, Processing, Shipped, Delivered, Cancelled)
    /// </summary>
    [Required, MaxLength(GlobalRules.StatusMaxLen)]
    public string Status { get; set; } = GlobalRules.OrderStatus.Created;

    /// <summary>
    /// Anteckningar till ordern
    /// </summary>
    [MaxLength(GlobalRules.CommentMaxLen)]
    public string? Notes { get; set; }

    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
    public DateTime? ConfirmedUtc { get; set; }
    public DateTime? ShippedUtc { get; set; }
    public DateTime? DeliveredUtc { get; set; }

    /// <summary>
    /// Beräknat totalpris för ordern
    /// </summary>
    public decimal TotalPrice => Items.Sum(item => item.TotalPrice);

    /// <summary>
    /// Totalt antal artiklar i ordern
    /// </summary>
    public int TotalQuantity => Items.Sum(item => item.Quantity);

    // Navigation properties
    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    public ICollection<OrderHistory> History { get; set; } = new List<OrderHistory>();
    public ICollection<Delivery> Deliveries { get; set; } = new List<Delivery>();
}
