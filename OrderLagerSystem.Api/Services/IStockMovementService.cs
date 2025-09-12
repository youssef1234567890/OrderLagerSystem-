using OrderLagerSystem.Api.DTOs;

namespace OrderLagerSystem.Api.Services;

/// <summary>
/// Service interface for stock movement operations (In/Out)
/// </summary>
public interface IStockMovementService
{
    /// <summary>
    /// Create a simple purchase order for incoming goods
    /// </summary>
    /// <param name="request">Purchase request details</param>
    /// <returns>Purchase order response with order number</returns>
    Task<SimplePurchaseResponse> CreatePurchaseOrderAsync(SimplePurchaseRequest request);

    /// <summary>
    /// Receive goods into warehouse and update stock levels
    /// </summary>
    /// <param name="request">Goods receipt details</param>
    /// <returns>Receipt confirmation with updated stock levels</returns>
    Task<GoodsReceiptResponse> ReceiveGoodsAsync(GoodsReceiptRequest request);

    /// <summary>
    /// Get all pending purchase orders awaiting receipt
    /// </summary>
    /// <returns>List of pending purchase orders</returns>
    Task<List<PendingPurchaseOrder>> GetPendingPurchaseOrdersAsync();

    /// <summary>
    /// Get article information by SKU or ID for purchase lookup
    /// </summary>
    /// <param name="identifier">SKU or Article ID</param>
    /// <returns>Article information if found</returns>
    Task<ArticleInfo?> GetArticleInfoAsync(string identifier);

    /// <summary>
    /// Get all orders pending for goods issue (utleverans)
    /// </summary>
    /// <returns>List of orders waiting for picking/shipping</returns>
    Task<List<OrderForGoodsIssue>> GetOrdersPendingGoodsIssueAsync();

    /// <summary>
    /// Process goods issue for an order (utleverans)
    /// </summary>
    /// <param name="orderId">Order ID to process</param>
    /// <param name="request">Goods issue details</param>
    /// <returns>Goods issue confirmation</returns>
    Task<GoodsIssueResponse> ProcessGoodsIssueAsync(int orderId, GoodsIssueRequest request);
}