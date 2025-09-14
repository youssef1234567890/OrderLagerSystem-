using System.ComponentModel.DataAnnotations;
using OrderLagerSystem.Api.Models;

namespace OrderLagerSystem.Api.DTOs;

public class OrderCreateRequest
{
    [MaxLength(GlobalRules.NameMaxLen)]
    public string? ExternalOrderNo { get; set; }

    [MaxLength(GlobalRules.CommentMaxLen)]
    public string? Notes { get; set; }

    [Required(ErrorMessage = "Minst en artikel krävs"), MinLength(1)]
    public List<OrderItemCreateRequest> Items { get; set; } = new();
}

public class OrderItemCreateRequest
{
    [Required, Range(1, int.MaxValue)]
    public int ArticleId { get; set; }

    [Required, Range(1, int.MaxValue, ErrorMessage = "Kvantitet måste vara minst 1")]
    public int Quantity { get; set; }
}