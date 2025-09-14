using System.ComponentModel.DataAnnotations;
using OrderLagerSystem.Api.Models;

namespace OrderLagerSystem.Api.DTOs;

public class DeliveryCreateRequest
{
    [Required, Range(1, int.MaxValue)]
    public int OrderId { get; set; }

    [MaxLength(GlobalRules.TrackingMaxLen)]
    public string? TrackingNumber { get; set; }

    [MaxLength(GlobalRules.DescriptionMaxLen)]
    public string? DeliveryAddress { get; set; }

    [MaxLength(GlobalRules.NameMaxLen)]
    public string? DeliveryMethod { get; set; }

    [MaxLength(GlobalRules.CommentMaxLen)]
    public string? Notes { get; set; }
}

public class DeliveryResponse
{
    public int DeliveryId { get; set; }
    public int OrderId { get; set; }
    public string? TrackingNumber { get; set; }
    public string Status { get; set; } = GlobalRules.DeliveryStatus.Pending;
    public string? DeliveryAddress { get; set; }
    public string? DeliveryMethod { get; set; }
    public DateTime CreatedUtc { get; set; }
    public DateTime? ShippedUtc { get; set; }
    public DateTime? DeliveredUtc { get; set; }
}