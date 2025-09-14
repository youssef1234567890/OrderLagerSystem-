using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderLagerSystem.Api.DTOs;
using OrderLagerSystem.Api.Services;

namespace OrderLagerSystem.Api.Controllers;

/// <summary>
/// Stock movement operations for warehouse management
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize] // Require authentication
public class StockMovementController : ControllerBase
{
    private readonly IStockMovementService _stockMovementService;
    private readonly ILogger<StockMovementController> _logger;

    public StockMovementController(
        IStockMovementService stockMovementService,
        ILogger<StockMovementController> logger)
    {
        _stockMovementService = stockMovementService;
        _logger = logger;
    }

    /// <summary>
    /// Create a simple purchase order for incoming goods
    /// </summary>
    /// <param name="request">Purchase order details</param>
    /// <returns>Purchase order confirmation with order number</returns>
    [HttpPost("purchase")]
    [Authorize(Roles = "Admin,Orderkoordinator")]
    public async Task<ActionResult<SimplePurchaseResponse>> CreatePurchaseOrder([FromBody] SimplePurchaseRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _stockMovementService.CreatePurchaseOrderAsync(request);
        
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Receive goods into warehouse and update stock levels
    /// </summary>
    /// <param name="request">Goods receipt details</param>
    /// <returns>Receipt confirmation with updated stock levels</returns>
    [HttpPost("receipt")]
    [Authorize(Roles = "Admin,Orderkoordinator,Employee")]
    public async Task<ActionResult<GoodsReceiptResponse>> ReceiveGoods([FromBody] GoodsReceiptRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _stockMovementService.ReceiveGoodsAsync(request);
        
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Get all purchase orders (both pending and received)
    /// </summary>
    /// <returns>List of all purchase orders with status</returns>
    [HttpGet("pending-purchases")]
    [Authorize(Roles = "Admin,Orderkoordinator,Employee")]
    public async Task<ActionResult<List<PendingPurchaseOrder>>> GetPurchaseOrders()
    {
        var result = await _stockMovementService.GetPurchaseOrdersAsync(); 
        return Ok(result);
    }

    /// <summary>
    /// Get article information by SKU or ID for purchase lookup
    /// </summary>
    /// <param name="identifier">Article SKU or ID</param>
    /// <returns>Article information if found</returns>
    [HttpGet("article/{identifier}")]
    [Authorize(Roles = "Admin,Orderkoordinator,Employee")]
    public async Task<ActionResult<ArticleInfo>> GetArticleInfo(string identifier)
    {
        var result = await _stockMovementService.GetArticleInfoAsync(identifier);
        
        if (result == null)
        {
            return NotFound($"Article not found with identifier: {identifier}");
        }

        return Ok(result);
    }
}