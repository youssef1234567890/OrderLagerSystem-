using System.ComponentModel.DataAnnotations;

namespace OrderLagerSystem.Api.DTOs;

/// <summary>
/// Simple purchase request for ordering articles to stock
/// </summary>
public class SimplePurchaseRequest
{
    /// <summary>
    /// Article SKU or ID to purchase
    /// </summary>
    [Required]
    public string ArticleIdentifier { get; set; } = null!; // SKU eller ArticleId

    /// <summary>
    /// Quantity to order
    /// </summary>
    [Required, Range(1, 10000)]
    public int Quantity { get; set; }

    /// <summary>
    /// Expected unit price (optional)
    /// </summary>
    public decimal? ExpectedUnitPrice { get; set; }

    /// <summary>
    /// Supplier name (optional)
    /// </summary>
    public string? SupplierName { get; set; }

    /// <summary>
    /// Notes for this purchase
    /// </summary>
    public string? Notes { get; set; }
}

/// <summary>
/// Response after creating a purchase order
/// </summary>
public class SimplePurchaseResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = null!;
    public string? OrderNumber { get; set; }
    public ArticleInfo? Article { get; set; }
    public int OrderedQuantity { get; set; }
    public List<string> Errors { get; set; } = new();
}

/// <summary>
/// Basic article information for purchase response
/// </summary>
public class ArticleInfo
{
    public int ArticleId { get; set; }
    public string Sku { get; set; } = null!;
    public string Name { get; set; } = null!;
    public int CurrentStock { get; set; }
    public string? StorageLocation { get; set; }
}