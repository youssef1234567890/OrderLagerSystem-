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

    // GET: api/order/current-status
    [HttpGet("current-status")]
    public async Task<ActionResult<List<OrderHistoryDto>>> GetCurrentStatuses([FromQuery] int? orderId = null)
    {
        var q = _db.Orders.Include(o => o.Items).AsQueryable();
        if (orderId.HasValue) q = q.Where(o => o.OrderId == orderId.Value);

        var list = await q
            .OrderByDescending(o => o.CreatedUtc)
            .Select(o => new OrderHistoryDto
            {
                OrderHistoryId = 0, // Not a history record
                OrderId = o.OrderId,
                ExternalOrderNo = o.ExternalOrderNo,
                ChangedByUserId = o.UserId,
                OldStatus = null,
                NewStatus = o.Status,
                Comment = "Current status",
                ChangedUtc = o.CreatedUtc,
                TotalPrice = o.TotalPrice,
                TotalQuantity = o.TotalQuantity
            })
            .ToListAsync();

        return Ok(list);
    }

    // GET: api/order/history
    [HttpGet("history")]
    public async Task<ActionResult<List<OrderHistoryDto>>> GetHistory([FromQuery] int? orderId = null)
    {
        var q = _db.OrderHistories.AsQueryable();
        if (orderId.HasValue) q = q.Where(h => h.OrderId == orderId.Value);

        var list = await q
            .OrderByDescending(h => h.ChangedUtc)
            .Join(_db.Orders.Include(o => o.Items),
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
                    ChangedUtc = h.ChangedUtc,
                    TotalPrice = o.TotalPrice,
                    TotalQuantity = o.TotalQuantity
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
            .Join(_db.Orders.Include(o => o.Items),
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
                    ChangedUtc = h.ChangedUtc,
                    TotalPrice = o.TotalPrice,
                    TotalQuantity = o.TotalQuantity
                })
            .ToListAsync();

        return Ok(list);
    }

    // DELETE: api/order/{id}
    [HttpDelete("{id:int}")]
    [Authorize(Roles = $"{GlobalRules.Roles.Admin},{GlobalRules.Roles.Orderkoordinator}")]
    public async Task<ActionResult> Delete(int id)
    {
        try
        {
            var userId = GetUserId();
            var order = await _db.Orders
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.OrderId == id);
            if (order == null) return NotFound();

            // Restore stock for each item
            var articles = await _db.Articles
                .Where(a => order.Items.Select(i => i.ArticleId).Contains(a.ArticleId))
                .ToDictionaryAsync(a => a.ArticleId, a => a);

            foreach (var item in order.Items)
            {
                if (articles.TryGetValue(item.ArticleId, out var article))
                {
                    article.StockQuantity += item.Quantity;
                    _db.StockMovements.Add(new StockMovement
                    {
                        ArticleId = item.ArticleId,
                        UserId = userId,
                        MovementType = StockMovement.MovementTypes.Released,
                        Quantity = item.Quantity,
                        StockAfterMovement = article.StockQuantity,
                        OrderId = null, // No longer linked to order since we're deleting it
                        Reason = "Order deleted - stock restored",
                        Notes = $"DELETED_ORDER:{order.ExternalOrderNo ?? order.OrderId.ToString()}"
                    });
                }
            }

            // Remove all order history entries for this order
            var orderHistories = await _db.OrderHistories
                .Where(h => h.OrderId == id)
                .ToListAsync();
            _db.OrderHistories.RemoveRange(orderHistories);

            // Remove all stock movements linked to this order
            var orderStockMovements = await _db.StockMovements
                .Where(sm => sm.OrderId == id)
                .ToListAsync();
            _db.StockMovements.RemoveRange(orderStockMovements);

            // Remove all order items
            _db.OrderItems.RemoveRange(order.Items);

            // Finally remove the order itself
            _db.Orders.Remove(order);

            await _db.SaveChangesAsync();
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting order {OrderId}", id);
            return StatusCode(500, "An error occurred while deleting the order.");
        }
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
            var order = await _db.Orders
                .Include(o => o.Items)
                .ThenInclude(oi => oi.Article)
                .FirstOrDefaultAsync(o => o.OrderId == orderId);

            if (order == null) return NotFound();

            // Check if order can be delivered
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
                        message = $"Not enough stock for {item.Article.Name}. Available: {item.Article.StockQuantity}, Required: {item.Quantity}"
                    });
                }
            }
            // Update stock quantities
            foreach (var item in order.Items)
            {
                item.Article.StockQuantity -= item.Quantity;

                // Create stock movement record
                var stockMovement = new StockMovement
                {
                    ArticleId = item.ArticleId,
                    UserId = GetUserId(),
                    MovementType = "Out",
                    Quantity = -item.Quantity, // Negative for outbound
                    StockAfterMovement = item.Article.StockQuantity,
                    OrderId = orderId,
                    Reason = "Order delivered",
                    Notes = request.Comment ?? "Order delivered"
                };
                _db.StockMovements.Add(stockMovement);
            }

            order.Status = GlobalRules.OrderStatus.Delivered;
            order.DeliveredUtc = DateTime.UtcNow;

            var history = new OrderHistory
            {
                OrderId = orderId,
                NewStatus = GlobalRules.OrderStatus.Delivered,
                ChangedByUserId = GetUserId(),
                ChangedUtc = DateTime.UtcNow,
                Comment = request.Comment ?? "Order delivered"
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

    // POST: api/order/{orderId}/status
    [HttpPost("{orderId:int}/status")]
    [Authorize(Roles = $"{GlobalRules.Roles.Admin},{GlobalRules.Roles.Orderkoordinator}")]
    public async Task<ActionResult<OrderResponse>> UpdateStatus(int orderId, [FromBody] OrderStatusUpdateRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var allowed = new[] { GlobalRules.OrderStatus.Created, GlobalRules.OrderStatus.Confirmed, GlobalRules.OrderStatus.Processing };
        if (string.IsNullOrWhiteSpace(request.NewStatus) || !allowed.Contains(request.NewStatus))
            return BadRequest(new { message = "Invalid status. Allowed: Created, Confirmed, Processing." });


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

            return Ok(new
            {
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


