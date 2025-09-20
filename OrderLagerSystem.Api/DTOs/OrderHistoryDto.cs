namespace OrderLagerSystem.Api.DTOs;

public class OrderHistoryDto
{
    public int OrderHistoryId { get; set; }
    public int OrderId { get; set; }
    public string? ExternalOrderNo { get; set; }
    public string? ChangedByUserId { get; set; }
    public string? OldStatus { get; set; }
    public string NewStatus { get; set; } = null!;
    public string? Comment { get; set; }
    public DateTime ChangedUtc { get; set; }
    public decimal TotalPrice { get; set; }
    public int TotalQuantity { get; set; }
}