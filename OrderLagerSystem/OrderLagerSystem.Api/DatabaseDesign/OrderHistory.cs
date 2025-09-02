using System;
using System.ComponentModel.DataAnnotations;

namespace OrderLagerSystem.Api.DatabaseDesign;

public class OrderHistory
{
    public int OrderHistoryId { get; set; }

    public int OrderId { get; set; }
    public Order Order { get; set; } = null!;

    [Required, MaxLength(GlobalRules.StatusMaxLen)]
    public string Status { get; set; } = "Created";

    [MaxLength(GlobalRules.DescriptionMaxLen)]
    public string? Comment { get; set; }

    public DateTime ChangedUtc { get; set; } = DateTime.UtcNow;
}