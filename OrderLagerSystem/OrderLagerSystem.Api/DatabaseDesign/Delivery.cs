using System;
using System.ComponentModel.DataAnnotations;

namespace OrderLagerSystem.Api.DatabaseDesign;

public class Delivery
{
    public int DeliveryId { get; set; }

    public int OrderId { get; set; }
    public Order Order { get; set; } = null!;

    [MaxLength(GlobalRules.TrackingMaxLen)]
    public string? TrackingNumber { get; set; }

    [MaxLength(GlobalRules.StatusMaxLen)]
    public string Status { get; set; } = "Pending";

    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
    public DateTime? ShippedUtc { get; set; }
    public DateTime? DeliveredUtc { get; set; }
}
