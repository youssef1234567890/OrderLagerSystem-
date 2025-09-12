using System.ComponentModel.DataAnnotations;

namespace OrderLagerSystem.Api.DTOs;

/// <summary>
/// Request for receiving goods into warehouse
/// </summary>
public class GoodsReceiptRequest
{
    /// <summary>
    /// Purchase order number to receive
    /// </summary>
    [Required]
    public string OrderNumber { get; set; } = null!;

    /// <summary>
    /// Actual quantity received
    /// </summary>
    [Required, Range(1, 10000)]
    public int ReceivedQuantity { get; set; }

    /// <summary>
    /// External reference (delivery note, invoice, etc.)
    /// </summary>
    public string? ExternalReference { get; set; }

    /// <summary>
    /// Location where goods are stored
    /// </summary>
    public string? StorageLocation { get; set; }

    /// <summary>
    /// Notes about the receipt
    /// </summary>
    public string? Notes { get; set; }
}

/// <summary>
/// Response after goods receipt
/// </summary>
public class GoodsReceiptResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = null!;
    public string? OrderNumber { get; set; }
    public ArticleInfo? Article { get; set; }
    public int ReceivedQuantity { get; set; }
    public int NewStockLevel { get; set; }
    public List<string> Errors { get; set; } = new();
}