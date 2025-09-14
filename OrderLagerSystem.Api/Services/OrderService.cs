using Microsoft.EntityFrameworkCore;
using OrderLagerSystem.Api.Data;
using OrderLagerSystem.Api.DTOs;
using OrderLagerSystem.Api.Models;
using Microsoft.AspNetCore.Identity;

namespace OrderLagerSystem.Api.Services;

public class OrderService : IOrderService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<OrderService> _logger;

    public OrderService(ApplicationDbContext context, ILogger<OrderService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<OrderResponse?> CreateOrderAsync(OrderCreateRequest request, string userId)
    {
        if (!await ValidateStockAvailabilityAsync(request.Items))
        {
            _logger.LogWarning("Order creation failed: Insufficient stock for user {UserId}", userId);
            return null;
        }

        var orderNumber = await GenerateOrderNumberAsync();

        var order = new Order
        {
            UserId = userId,
            ExternalOrderNo = request.ExternalOrderNo ?? orderNumber,
            Notes = request.Notes,
            Status = GlobalRules.OrderStatus.Created,
            CreatedUtc = DateTime.UtcNow,
            Items = new List<OrderItem>()
        };

        foreach (var itemRequest in request.Items)
        {
            var article = await _context.Articles.FindAsync(itemRequest.ArticleId);
            if (article == null || !article.IsActive)
            {
                _logger.LogWarning("Invalid or inactive article {ArticleId} for user {UserId}", itemRequest.ArticleId, userId);
                return null;
            }

            var orderItem = new OrderItem
            {
                ArticleId = itemRequest.ArticleId,
                Quantity = itemRequest.Quantity,
                UnitPriceInCents = article.PriceInCents
            };

            order.Items.Add(orderItem);

            var stockMovement = new StockMovement
            {
                ArticleId = itemRequest.ArticleId,
                UserId = userId,
                MovementType = StockMovement.MovementTypes.Reserved,
                Quantity = -itemRequest.Quantity,
                StockAfterMovement = article.StockQuantity - itemRequest.Quantity,
                OrderId = null,
                Reason = "Reserved for order",
                CreatedUtc = DateTime.UtcNow
            };

            _context.StockMovements.Add(stockMovement);
            article.StockQuantity -= itemRequest.Quantity;
        }

        var history = new OrderHistory
        {
            OrderId = 0,
            ChangedByUserId = userId,
            NewStatus = GlobalRules.OrderStatus.Created,
            Comment = "Order created",
            ChangedUtc = DateTime.UtcNow
        };

        _context.Orders.Add(order);
        _context.OrderHistories.Add(history);

        await _context.SaveChangesAsync();

        history.OrderId = order.OrderId;
        foreach (var movement in _context.StockMovements.Where(m => m.OrderId == null && m.UserId == userId && m.MovementType == StockMovement.MovementTypes.Reserved))
        {
            movement.OrderId = order.OrderId;
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation("Order {OrderId} created by user {UserId}", order.OrderId, userId);

        return MapToOrderResponse(order);
    }

    public async Task<bool> ValidateStockAvailabilityAsync(List<OrderItemCreateRequest> items)
    {
        foreach (var item in items)
        {
            var article = await _context.Articles.FindAsync(item.ArticleId);
            if (article == null || !article.IsActive || article.StockQuantity < item.Quantity)
            {
                _logger.LogWarning("Insufficient stock for Article {ArticleId}: Requested {Quantity}, Available {Stock}", 
                    item.ArticleId, item.Quantity, article?.StockQuantity ?? 0);
                return false;
            }
        }
        return true;
    }

    public async Task<string> GenerateOrderNumberAsync()
    {
        var year = DateTime.UtcNow.Year;
        var lastOrder = await _context.Orders
            .Where(o => o.CreatedUtc.Year == year)
            .OrderByDescending(o => o.OrderId)
            .FirstOrDefaultAsync();

        int sequence = lastOrder != null ? (lastOrder.OrderId % 1000) + 1 : 1;
        return $"ORD-{year}-{sequence:D3}";
    }

    public async Task<DeliveryResponse?> CreateDeliveryAsync(DeliveryCreateRequest request, string userId)
    {
        var order = await _context.Orders
            .Include(o => o.Items).ThenInclude(i => i.Article)
            .FirstOrDefaultAsync(o => o.OrderId == request.OrderId);

        if (order == null || (order.UserId != userId && !await IsUserInRole(userId, GlobalRules.Roles.Admin)))
        {
            _logger.LogWarning("User {UserId} not authorized to ship order {OrderId}", userId, request.OrderId);
            return null;
        }

        if (order.Status != GlobalRules.OrderStatus.Confirmed)
        {
            _logger.LogWarning("Order {OrderId} is not in Confirmed status for shipping", request.OrderId);
            return null;
        }

        var delivery = new Delivery
        {
            OrderId = request.OrderId,
            TrackingNumber = request.TrackingNumber,
            DeliveryAddress = request.DeliveryAddress,
            DeliveryMethod = request.DeliveryMethod,
            Notes = request.Notes,
            Status = GlobalRules.DeliveryStatus.Shipped,
            CreatedUtc = DateTime.UtcNow,
            ShippedUtc = DateTime.UtcNow
        };

        order.Status = GlobalRules.OrderStatus.Shipped;
        order.ShippedUtc = DateTime.UtcNow;

        foreach (var item in order.Items)
        {
            var article = item.Article;
            var stockMovement = new StockMovement
            {
                ArticleId = item.ArticleId,
                UserId = userId,
                MovementType = StockMovement.MovementTypes.Outgoing,
                Quantity = -item.Quantity,
                StockAfterMovement = article.StockQuantity,
                OrderId = order.OrderId,
                Reason = "Shipped with delivery",
                CreatedUtc = DateTime.UtcNow
            };
            _context.StockMovements.Add(stockMovement);
        }

        var history = new OrderHistory
        {
            OrderId = order.OrderId,
            ChangedByUserId = userId,
            OldStatus = GlobalRules.OrderStatus.Confirmed,
            NewStatus = GlobalRules.OrderStatus.Shipped,
            Comment = "Order shipped",
            ChangedUtc = DateTime.UtcNow
        };

        _context.Deliveries.Add(delivery);
        _context.OrderHistories.Add(history);

        await _context.SaveChangesAsync();

        _logger.LogInformation("Delivery {DeliveryId} created for order {OrderId} by user {UserId}", delivery.DeliveryId, order.OrderId, userId);

        return MapToDeliveryResponse(delivery);
    }

    private async Task<bool> IsUserInRole(string userId, string role)
    {
    return await _context.UserRoles
        .Join(_context.Roles,
            ur => ur.RoleId,
            r => r.Id,
            (ur, r) => new { ur.UserId, r.Name })
        .AnyAsync(x => x.UserId == userId && x.Name == role);
    }
    private OrderResponse MapToOrderResponse(Order order)
    {
        return new OrderResponse
        {
            OrderId = order.OrderId,
            UserId = order.UserId,
            ExternalOrderNo = order.ExternalOrderNo,
            Status = order.Status,
            Notes = order.Notes,
            TotalPrice = order.TotalPrice,
            TotalQuantity = order.TotalQuantity,
            CreatedUtc = order.CreatedUtc,
            Items = order.Items.Select(i => new OrderItemResponse
            {
                OrderItemId = i.OrderItemId,
                ArticleId = i.ArticleId,
                ArticleName = i.Article.Name,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                TotalPrice = i.TotalPrice
            }).ToList()
        };
    }

    private DeliveryResponse MapToDeliveryResponse(Delivery delivery)
    {
        return new DeliveryResponse
        {
            DeliveryId = delivery.DeliveryId,
            OrderId = delivery.OrderId,
            TrackingNumber = delivery.TrackingNumber,
            Status = delivery.Status,
            DeliveryAddress = delivery.DeliveryAddress,
            DeliveryMethod = delivery.DeliveryMethod,
            CreatedUtc = delivery.CreatedUtc,
            ShippedUtc = delivery.ShippedUtc,
            DeliveredUtc = delivery.DeliveredUtc
        };
    }
}