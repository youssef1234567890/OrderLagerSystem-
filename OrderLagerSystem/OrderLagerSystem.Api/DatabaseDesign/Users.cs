using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OrderLagerSystem.Api.DatabaseDesign;

public class Users
{
    public int UsersId { get; set; }

    [Required, MaxLength(GlobalRules.NameMaxLen)]
    public string Name { get; set; } = null!;

    [Required, MaxLength(GlobalRules.EmailMaxLen)]
    public string Email { get; set; } = null!;

    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;

    // Relation 1→N: User har många Orders
    public ICollection<Order> Orders { get; set; } = new List<Order>();
}