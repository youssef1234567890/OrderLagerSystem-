using System.ComponentModel.DataAnnotations;

namespace OrderLagerSystem.Models;

/// <summary>
/// Artikel med lagersaldo och lagerplats
/// </summary>
public class Article
{
    public int ArticleId { get; set; }

    [Required, MaxLength(GlobalRules.SkuMaxLen)]
    public string Sku { get; set; } = null!;

    [Required, MaxLength(GlobalRules.NameMaxLen)]
    public string Name { get; set; } = null!;

    [MaxLength(GlobalRules.DescriptionMaxLen)]
    public string? Description { get; set; }

    /// <summary>
    /// Pris i öre för att undvika decimalstrul i SQLite
    /// </summary>
    public long PriceInCents { get; set; }

    /// <summary>
    /// Aktuellt lagersaldo
    /// </summary>
    public int StockQuantity { get; set; }

    /// <summary>
    /// Minimum lagersaldo innan varning
    /// </summary>
    public int MinimumStock { get; set; } = 0;

    /// <summary>
    /// Lagerplats - var artikeln förvaras
    /// </summary>
    [MaxLength(GlobalRules.LocationMaxLen)]
    public string? StorageLocation { get; set; }

    /// <summary>
    /// Om artikeln är aktiv och kan beställas
    /// </summary>
    public bool IsActive { get; set; } = true;

    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedUtc { get; set; }

    /// <summary>
    /// Beräknat pris i kronor
    /// </summary>
    public decimal Price => PriceInCents / 100.0m;

    /// <summary>
    /// Kontrollerar om lagret är lågt
    /// </summary>
    public bool IsLowStock => StockQuantity <= MinimumStock;

    // Navigation properties
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    public ICollection<StockMovement> StockMovements { get; set; } = new List<StockMovement>();
}
