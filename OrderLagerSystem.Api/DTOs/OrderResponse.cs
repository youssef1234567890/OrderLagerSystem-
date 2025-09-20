using OrderLagerSystem.Api.Models;
namespace OrderLagerSystem.Api.DTOs;


public class OrderResponse
{
    public int OrderId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string? ExternalOrderNo { get; set; }
    public string Status { get; set; } = GlobalRules.OrderStatus.Created;
    public string? Notes { get; set; }
    public decimal TotalPrice { get; set; }
    public int TotalQuantity { get; set; }
    public DateTime CreatedUtc { get; set; }
    public List<OrderItemResponse> Items { get; set; } = new();
}

public class OrderItemResponse
{
    public int OrderItemId { get; set; }
    public int ArticleId { get; set; }
    public string ArticleName { get; set; } = string.Empty;
    public string ArticleSku { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }

}