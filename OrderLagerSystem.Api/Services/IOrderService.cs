using OrderLagerSystem.Api.DTOs;

namespace OrderLagerSystem.Api.Services;

public interface IOrderService
{
    Task<OrderResponse?> CreateOrderAsync(OrderCreateRequest request, string userId);
    Task<bool> ValidateStockAvailabilityAsync(List<OrderItemCreateRequest> items);
    Task<string> GenerateOrderNumberAsync();
    Task<DeliveryResponse?> CreateDeliveryAsync(DeliveryCreateRequest request, string userId);
}