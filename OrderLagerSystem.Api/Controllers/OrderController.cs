using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderLagerSystem.Api.Data;
using OrderLagerSystem.Api.DTOs;
using OrderLagerSystem.Api.Models;
using OrderLagerSystem.Api.Services;
using System.Security.Claims;

namespace OrderLagerSystem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrderController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly ApplicationDbContext _db;
    private readonly ILogger<OrderController> _logger;

    public OrderController(IOrderService orderService, ApplicationDbContext db, ILogger<OrderController> logger)
    {
        _orderService = orderService;
        _db = db;
        _logger = logger;
    }

    private string GetUserId() =>
        User.FindFirst(ClaimTypes.NameIdentifier)?.Value
        ?? throw new UnauthorizedAccessException("No user id found.");

    // POST: api/order
    [HttpPost]
    [Authorize(Roles = $"{GlobalRules.Roles.Admin},{GlobalRules.Roles.Orderkoordinator},{GlobalRules.Roles.Employee}")]
    public async Task<ActionResult<OrderResponse>> Create([FromBody] OrderCreateRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            var userId = GetUserId();
            var result = await _orderService.CreateOrderAsync(request, userId);
            if (result == null) return BadRequest(new { message = "Insufficient stock or invalid items." });
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating order");
            return StatusCode(500, "An error occurred while creating the order.");
        }
    }

    // GET: api/order
    [HttpGet]
    public async Task<ActionResult<List<OrderResponse>>> GetAll([FromQuery] string? status = null)
    {
        var q = _db.Orders
            .Include(o => o.Items).ThenInclude(i => i.Article)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(status))
            q = q.Where(o => o.Status == status);

        var list = await q
            .OrderByDescending(o => o.CreatedUtc)
            .Select(o => new OrderResponse
            {
                OrderId = o.OrderId,
                UserId = o.UserId,
                ExternalOrderNo = o.ExternalOrderNo,
                Status = o.Status,
                Notes = o.Notes,
                TotalPrice = o.TotalPrice,
                TotalQuantity = o.TotalQuantity,
                CreatedUtc = o.CreatedUtc,
                Items = o.Items.Select(i => new OrderItemResponse
                {
                    OrderItemId = i.OrderItemId,
                    ArticleId = i.ArticleId,
                    ArticleName = i.Article.Name,
                    ArticleSku = i.Article.Sku,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    TotalPrice = i.TotalPrice
                }).ToList()
            })
            .ToListAsync();

        return Ok(list);
    }

    // GET: api/order/{id}
    [HttpGet("{id:int}")]
    public async Task<ActionResult<OrderResponse>> GetById(int id)
    {
        var o = await _db.Orders
            .Include(x => x.Items).ThenInclude(i => i.Article)
            .FirstOrDefaultAsync(x => x.OrderId == id);

        if (o == null) return NotFound();

        return Ok(new OrderResponse
        {
            OrderId = o.OrderId,
            UserId = o.UserId,
            ExternalOrderNo = o.ExternalOrderNo,
            Status = o.Status,
            Notes = o.Notes,
            TotalPrice = o.TotalPrice,
            TotalQuantity = o.TotalQuantity,
            CreatedUtc = o.CreatedUtc,
            Items = o.Items.Select(i => new OrderItemResponse
            {
                OrderItemId = i.OrderItemId,
                ArticleId = i.ArticleId,
                ArticleName = i.Article.Name,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                TotalPrice = i.TotalPrice
            }).ToList()
        });
    }

    // GET: api/order/history
    [HttpGet("history")]
    public async Task<ActionResult<List<OrderHistoryDto>>> GetHistory([FromQuery] int? orderId = null)
    {
        var q = _db.OrderHistories.AsQueryable();
        if (orderId.HasValue) q = q.Where(h => h.OrderId == orderId.Value);

        var list = await q
            .OrderByDescending(h => h.ChangedUtc)
            .Join(_db.Orders,
                h => h.OrderId,
                o => o.OrderId,
                (h, o) => new OrderHistoryDto
                {
                    OrderHistoryId = h.OrderHistoryId,
                    OrderId = h.OrderId,
                    ExternalOrderNo = o.ExternalOrderNo,
                    ChangedByUserId = h.ChangedByUserId,
                    OldStatus = h.OldStatus,
                    NewStatus = h.NewStatus,
                    Comment = h.Comment,
                    ChangedUtc = h.ChangedUtc
                })
            .ToListAsync();

        return Ok(list);
    }

    // GET: api/order/{orderId}/history
    [HttpGet("{orderId:int}/history")]
    public async Task<ActionResult<List<OrderHistoryDto>>> GetHistoryForOrder(int orderId)
    {
        var list = await _db.OrderHistories
            .Where(h => h.OrderId == orderId)
            .OrderByDescending(h => h.ChangedUtc)
            .Join(_db.Orders,
                h => h.OrderId,
                o => o.OrderId,
                (h, o) => new OrderHistoryDto
                {
                    OrderHistoryId = h.OrderHistoryId,
                    OrderId = h.OrderId,
                    ExternalOrderNo = o.ExternalOrderNo,
                    ChangedByUserId = h.ChangedByUserId,
                    OldStatus = h.OldStatus,
                    NewStatus = h.NewStatus,
                    Comment = h.Comment,
                    ChangedUtc = h.ChangedUtc
                })
            .ToListAsync();

        return Ok(list);
    }

    // POST: api/order/{orderId}/delivery
    [HttpPost("{orderId:int}/delivery")]
    [Authorize(Roles = $"{GlobalRules.Roles.Admin},{GlobalRules.Roles.Orderkoordinator}")]
    public async Task<ActionResult> CreateDelivery(int orderId, [FromBody] DeliveryCreateRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            var userId = GetUserId();
            request.OrderId = orderId;
            var delivery = await _orderService.CreateDeliveryAsync(request, userId);
            if (delivery == null) return BadRequest(new { message = "Could not create delivery for this order." });
            return Ok(new { message = "Delivery created", delivery });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating delivery for order {OrderId}", orderId);
            return StatusCode(500, "An error occurred while creating the delivery.");
        }
    }
    // GET: api/order/pending-delivery
    [HttpGet("pending-delivery")]
    [Authorize(Roles = $"{GlobalRules.Roles.Admin},{GlobalRules.Roles.Orderkoordinator}")]
    public async Task<ActionResult<List<OrderResponse>>> GetPendingDelivery()
    {
        var orders = await _db.Orders
            .Include(o => o.Items).ThenInclude(i => i.Article)
            .Where(o => o.Status != GlobalRules.OrderStatus.Delivered && 
                       o.Status != GlobalRules.OrderStatus.Cancelled)
            .OrderBy(o => o.CreatedUtc)
            .Select(o => new OrderResponse
            {
                OrderId = o.OrderId,
                UserId = o.UserId,
                ExternalOrderNo = o.ExternalOrderNo,
                Status = o.Status,
                Notes = o.Notes,
                TotalPrice = o.TotalPrice,
                TotalQuantity = o.TotalQuantity,
                CreatedUtc = o.CreatedUtc,
                Items = o.Items.Select(i => new OrderItemResponse
                {
                    OrderItemId = i.OrderItemId,
                    ArticleId = i.ArticleId,
                    ArticleName = i.Article.Name,
                    ArticleSku = i.Article.Sku,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    TotalPrice = i.TotalPrice
                }).ToList()
            })
            .ToListAsync();

        return Ok(orders);
    }

    // POST: api/order/{orderId}/deliver
    [HttpPost("{orderId:int}/deliver")]
    [Authorize(Roles = $"{GlobalRules.Roles.Admin},{GlobalRules.Roles.Orderkoordinator}")]
    public async Task<ActionResult> DeliverOrder(int orderId, [FromBody] OrderStatusUpdateRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            var userId = GetUserId();
            var order = await _db.Orders
                .Include(o => o.Items).ThenInclude(i => i.Article)
                .FirstOrDefaultAsync(o => o.OrderId == orderId);

            if (order == null) return NotFound(new { message = "Order not found." });

            if (order.Status == GlobalRules.OrderStatus.Delivered)
            {
                return BadRequest(new { message = "Order is already delivered." });
            }

            if (order.Status == GlobalRules.OrderStatus.Cancelled)
            {
                return BadRequest(new { message = "Cannot deliver a cancelled order." });
            }

            // Check if there's enough stock for all items
            foreach (var item in order.Items)
            {
                if (item.Article.StockQuantity < item.Quantity)
                {
                    return BadRequest(new { 
                        message = $"Insufficient stock for {item.Article.Name}. Available: {item.Article.StockQuantity}, Required: {item.Quantity}" 
                    });
                }
            }

            // Update order status
            var oldStatus = order.Status;
            order.Status = GlobalRules.OrderStatus.Delivered;
            order.DeliveredUtc = DateTime.UtcNow;

            // Create stock movements and update inventory
            foreach (var item in order.Items)
            {
                var article = item.Article;
                article.StockQuantity -= item.Quantity;

                var stockMovement = new StockMovement
                {
                    ArticleId = item.ArticleId,
                    UserId = userId,
                    MovementType = StockMovement.MovementTypes.Outgoing,
                    Quantity = -item.Quantity,
                    StockAfterMovement = article.StockQuantity,
                    OrderId = order.OrderId,
                    Reason = "Order delivered",
                    CreatedUtc = DateTime.UtcNow
                };
                _db.StockMovements.Add(stockMovement);
            }

            // Create order history entry
            var history = new OrderHistory
            {
                OrderId = orderId,
                ChangedByUserId = userId,
                OldStatus = oldStatus,
                NewStatus = GlobalRules.OrderStatus.Delivered,
                Comment = request.Comment ?? "Order delivered",
                ChangedUtc = DateTime.UtcNow
            };
            _db.OrderHistories.Add(history);

            await _db.SaveChangesAsync();

            return Ok(new { 
                message = "Order delivered successfully", 
                orderId = orderId,
                deliveredAt = order.DeliveredUtc 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error delivering order {OrderId}", orderId);
            return StatusCode(500, "An error occurred while delivering the order.");
        }
    }
}