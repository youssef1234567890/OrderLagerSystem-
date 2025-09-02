using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OrderLagerSystem.Api.DatabaseDesign;

public class Order
{
    public int OrderId { get; set; }

    // Koppling till Users (ApplicationUser)
    public int UsersId { get; set; }
    public Users User { get; set; } = null!;

    [MaxLength(GlobalRules.NameMaxLen)]
    public string? ExternalOrderNo { get; set; }

    [MaxLength(GlobalRules.StatusMaxLen)]
    public string Status { get; set; } = "Created";

    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
    public DateTime? PaidUtc { get; set; }

    // Relationer
    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    public ICollection<OrderHistory> History { get; set; } = new List<OrderHistory>();
    public ICollection<Delivery> Deliveries { get; set; } = new List<Delivery>();
}