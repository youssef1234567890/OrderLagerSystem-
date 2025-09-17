namespace OrderLagerSystem.Api.DTOs;

public class OrderStatusUpdateRequest
{
    public string NewStatus { get; set; } = null!;
    public string? Comment { get; set; }
}


