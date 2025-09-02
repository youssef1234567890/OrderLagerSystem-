using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OrderLagerSystem.Api.DatabaseDesign;

public class Article
{
    public int ArticleId { get; set; }

    [Required, MaxLength(GlobalRules.SkuMaxLen)]
    public string Sku { get; set; } = null!;

    [Required, MaxLength(GlobalRules.NameMaxLen)]
    public string Name { get; set; } = null!;

    [MaxLength(GlobalRules.DescriptionMaxLen)]
    public string? Description { get; set; }

    // Pris i öre (SQLite INTEGER)
    public long PriceInCents { get; set; }

    public int StockQuantity { get; set; }
    public bool IsActive { get; set; } = true;

    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedUtc { get; set; }

    // 1→N till OrderItems
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}