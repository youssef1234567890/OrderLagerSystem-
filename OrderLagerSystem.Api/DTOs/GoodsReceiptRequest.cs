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

/// <summary>
/// Pending purchase order awaiting receipt
/// </summary>
public class PendingPurchaseOrder
{
    public string OrderNumber { get; set; } = null!;
    public ArticleInfo Article { get; set; } = null!;
    public int OrderedQuantity { get; set; }
    public DateTime OrderDate { get; set; }
    public string? SupplierName { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// Order pending goods issue (utleverans)
/// </summary>
public class OrderForGoodsIssue
{
    public int OrderId { get; set; }
    public string ExternalOrderNo { get; set; } = null!;
    public string Status { get; set; } = null!;
    public DateTime CreatedUtc { get; set; }
    public List<OrderItemForPicking> Items { get; set; } = new();
    public string? Notes { get; set; }
}

/// <summary>
/// Order item that needs to be picked
/// </summary>
public class OrderItemForPicking
{
    public int OrderItemId { get; set; }
    public ArticleInfo Article { get; set; } = null!;
    public int Quantity { get; set; }
    public bool IsAvailable { get; set; } // Check if enough stock
}

/// <summary>
/// Request for processing goods issue
/// </summary>
public class GoodsIssueRequest
{
    public List<GoodsIssueItem> Items { get; set; } = new();
    public string? Notes { get; set; }
    public string? LocationCode { get; set; }
}

/// <summary>
/// Individual item for goods issue
/// </summary>
public class GoodsIssueItem
{
    public int OrderItemId { get; set; }
    public int IssuedQuantity { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// Response after goods issue
/// </summary>
public class GoodsIssueResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = null!;
    public int OrderId { get; set; }
    public List<string> ProcessedItems { get; set; } = new();
    public List<string> Errors { get; set; } = new();
}