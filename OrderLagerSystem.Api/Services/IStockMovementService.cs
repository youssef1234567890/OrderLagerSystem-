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
    /// Get all purchase orders (both pending and received)
    /// </summary>
    /// <returns>List of pending purchase orders</returns>
    Task<List<PendingPurchaseOrder>> GetPurchaseOrdersAsync();

    /// <summary>
    /// Get article information by SKU or ID for purchase lookup
    /// </summary>
    /// <param name="identifier">SKU or Article ID</param>
    /// <returns>Article information if found</returns>
    Task<ArticleInfo?> GetArticleInfoAsync(string identifier);

    /// <summary>
    /// Calculate the stock balance for an article at a specific storage location
    /// </summary>
    /// <param name="articleId">The ID of the article</param>
    /// <param name="storageLocation">The storage location (optional)</param>
    /// <returns>The stock balance</returns>
    Task<int> CalculateStockBalanceAsync(int articleId, string? storageLocation);
}