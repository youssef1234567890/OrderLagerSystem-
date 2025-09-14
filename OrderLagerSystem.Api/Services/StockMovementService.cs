using Microsoft.EntityFrameworkCore;
using OrderLagerSystem.Api.Data;
using OrderLagerSystem.Api.DTOs;
using OrderLagerSystem.Api.Models;

namespace OrderLagerSystem.Api.Services;

/// <summary>
/// Service implementation for stock movement operations (Inkommande)
/// </summary>
public class StockMovementService : IStockMovementService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<StockMovementService> _logger;

    public StockMovementService(ApplicationDbContext context, ILogger<StockMovementService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Create a simple purchase order for incoming goods
    /// </summary>
    public async Task<SimplePurchaseResponse> CreatePurchaseOrderAsync(SimplePurchaseRequest request)
    {
        try
        {
            // Find article by SKU or ID
            var article = await GetArticleByIdentifierAsync(request.ArticleIdentifier);
            if (article == null)
            {
                return new SimplePurchaseResponse
                {
                    Success = false,
                    Message = "Article not found",
                    Errors = new List<string> { $"No article found with identifier: {request.ArticleIdentifier}" }
                };
            }

            // Generate unique order number
            var orderNumber = await GenerateOrderNumberAsync();

            // Create stock movement record as "pending incoming" using existing fields
            var stockMovement = new StockMovement
            {
                ArticleId = article.ArticleId,
                MovementType = "PendingPurchase", // Custom type for pending purchases
                Quantity = request.Quantity,
                StockAfterMovement = article.StockQuantity, // Current stock (will update when received)
                Reason = $"Purchase order {orderNumber}",
                Notes = $"ORDER:{orderNumber}|QTY:{request.Quantity}|SUPPLIER:{request.SupplierName ?? "Unknown"}|STATUS:PENDING|{request.Notes}",
                CreatedUtc = DateTime.UtcNow
            };

            _context.StockMovements.Add(stockMovement);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Purchase order {OrderNumber} created for article {ArticleSku} - {Quantity} units", 
                orderNumber, article.Sku, request.Quantity);

            return new SimplePurchaseResponse
            {
                Success = true,
                Message = "Purchase order created successfully",
                OrderNumber = orderNumber,
                Article = new ArticleInfo
                {
                    ArticleId = article.ArticleId,
                    Sku = article.Sku,
                    Name = article.Name,
                    CurrentStock = article.StockQuantity,
                    StorageLocation = article.StorageLocation
                },
                OrderedQuantity = request.Quantity
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating purchase order for article {ArticleIdentifier}", request.ArticleIdentifier);
            return new SimplePurchaseResponse
            {
                Success = false,
                Message = "An error occurred while creating purchase order",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    /// <summary>
    /// Receive goods into warehouse and update stock levels
    /// </summary>
    public async Task<GoodsReceiptResponse> ReceiveGoodsAsync(GoodsReceiptRequest request)
    {
        try
        {
            // Find the pending purchase order using Notes field
            var pendingMovement = await _context.StockMovements
                .Include(sm => sm.Article)
                .FirstOrDefaultAsync(sm => sm.MovementType == "PendingPurchase" 
                    && sm.Notes != null 
                    && sm.Notes.Contains($"ORDER:{request.OrderNumber}|")
                    && sm.Notes.Contains("STATUS:PENDING"));

            if (pendingMovement == null)
            {
                return new GoodsReceiptResponse
                {
                    Success = false,
                    Message = "Purchase order not found or already received",
                    Errors = new List<string> { $"No pending purchase order found with number: {request.OrderNumber}" }
                };
            }

            // Update article stock
            var article = pendingMovement.Article;
            var newStockLevel = article.StockQuantity + request.ReceivedQuantity;
            article.StockQuantity = newStockLevel;

            // Update storage location if provided
            if (!string.IsNullOrEmpty(request.StorageLocation))
            {
                article.StorageLocation = request.StorageLocation;
            }

            // Create actual incoming movement
            var incomingMovement = new StockMovement
            {
                ArticleId = article.ArticleId,
                MovementType = StockMovement.MovementTypes.Incoming,
                Quantity = request.ReceivedQuantity,
                StockAfterMovement = newStockLevel,
                Reason = $"Goods receipt for purchase order {request.OrderNumber}",
                Notes = $"RECEIVED:{request.ReceivedQuantity}|EXTERNAL_REF:{request.ExternalReference}|{request.Notes}",
                CreatedUtc = DateTime.UtcNow
            };

            // Mark pending order as completed by updating its notes
            pendingMovement.Notes = pendingMovement.Notes?.Replace("STATUS:PENDING", "STATUS:RECEIVED");

            _context.StockMovements.Add(incomingMovement);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Goods received for order {OrderNumber} - {ReceivedQuantity} units of {ArticleSku}. New stock: {NewStock}", 
                request.OrderNumber, request.ReceivedQuantity, article.Sku, newStockLevel);

            return new GoodsReceiptResponse
            {
                Success = true,
                Message = "Goods received successfully",
                OrderNumber = request.OrderNumber,
                Article = new ArticleInfo
                {
                    ArticleId = article.ArticleId,
                    Sku = article.Sku,
                    Name = article.Name,
                    CurrentStock = newStockLevel,
                    StorageLocation = article.StorageLocation
                },
                ReceivedQuantity = request.ReceivedQuantity,
                NewStockLevel = newStockLevel
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error receiving goods for order {OrderNumber}", request.OrderNumber);
            return new GoodsReceiptResponse
            {
                Success = false,
                Message = "An error occurred while receiving goods",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    /// <summary>
    /// Get all purchase orders (both pending and received)
    /// </summary>
    public async Task<List<PendingPurchaseOrder>> GetPurchaseOrdersAsync()
    {
        try
        {
            var allOrders = await _context.StockMovements
                .Include(sm => sm.Article)
                .Where(sm => sm.MovementType == "PendingPurchase")
                .ToListAsync();

            var result = new List<PendingPurchaseOrder>();

            foreach (var movement in allOrders)
            {
                var notes = movement.Notes ?? "";
                var orderNumber = ExtractFromNotes(notes, "ORDER:");
                var supplierName = ExtractFromNotes(notes, "SUPPLIER:");
                
                // Determine status based on notes
                var status = notes.Contains("STATUS:RECEIVED") ? "Received" : "Pending";

                if (!string.IsNullOrEmpty(orderNumber))
                {
                    result.Add(new PendingPurchaseOrder
                    {
                        OrderNumber = orderNumber,
                        Article = new ArticleInfo
                        {
                            ArticleId = movement.Article.ArticleId,
                            Sku = movement.Article.Sku,
                            Name = movement.Article.Name,
                            CurrentStock = movement.Article.StockQuantity,
                            StorageLocation = movement.Article.StorageLocation
                        },
                        OrderedQuantity = movement.Quantity,
                        OrderDate = movement.CreatedUtc,
                        SupplierName = supplierName,
                        Notes = movement.Notes, 
                        Status = status
                    });
                }
            }

            return result.OrderByDescending(po => po.OrderDate).ToList(); // Nyaste först
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving purchase orders");
            return new List<PendingPurchaseOrder>();
        }
    }

    /// <summary>
    /// Get article information by SKU or ID
    /// </summary>
    public async Task<ArticleInfo?> GetArticleInfoAsync(string identifier)
    {
        var article = await GetArticleByIdentifierAsync(identifier);
        if (article == null) return null;

        return new ArticleInfo
        {
            ArticleId = article.ArticleId,
            Sku = article.Sku,
            Name = article.Name,
            CurrentStock = article.StockQuantity,
            StorageLocation = article.StorageLocation
        };
    }

    // Helper methods
    private async Task<Article?> GetArticleByIdentifierAsync(string identifier)
    {
        // Try to parse as ArticleId first
        if (int.TryParse(identifier, out int articleId))
        {
            var articleById = await _context.Articles.FindAsync(articleId);
            if (articleById != null) return articleById;
        }

        // Search by SKU
        return await _context.Articles
            .FirstOrDefaultAsync(a => a.Sku == identifier);
    }

    private async Task<string> GenerateOrderNumberAsync()
    {
        var today = DateTime.Now.ToString("yyyyMMdd");
        var prefix = $"PO-{today}-";
        
        // Count existing orders for today in Notes field
        var todayOrdersCount = await _context.StockMovements
            .CountAsync(sm => sm.Notes != null && sm.Notes.Contains($"ORDER:{prefix}"));

        var nextNumber = todayOrdersCount + 1;
        return $"{prefix}{nextNumber:D3}"; // PO-20241201-001
    }

    private string ExtractFromNotes(string notes, string key)
    {
        var startIndex = notes.IndexOf(key);
        if (startIndex == -1) return "";

        startIndex += key.Length;
        var endIndex = notes.IndexOf('|', startIndex);
        if (endIndex == -1) return notes.Substring(startIndex);

        return notes.Substring(startIndex, endIndex - startIndex);
    }

}