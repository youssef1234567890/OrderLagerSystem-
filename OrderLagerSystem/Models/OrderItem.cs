using System.ComponentModel.DataAnnotations;

namespace OrderLagerSystem.Models;

/// <summary>
/// En rad i en order - artikel med kvantitet och pris
/// </summary>
public class OrderItem
{
    public int OrderItemId { get; set; }

    public int OrderId { get; set; }
    public Order Order { get; set; } = null!;

    public int ArticleId { get; set; }
    public Article Article { get; set; } = null!;

    /// <summary>
    /// Beställd kvantitet
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "Kvantitet måste vara minst 1")]
    public int Quantity { get; set; }

    /// <summary>
    /// Pris per enhet vid ordertillfället (i öre)
    /// </summary>
    public long UnitPriceInCents { get; set; }

    /// <summary>
    /// Beräknat pris per enhet i kronor
    /// </summary>
    public decimal UnitPrice => UnitPriceInCents / 100.0m;

    /// <summary>
    /// Beräknat totalpris för denna rad
    /// </summary>
    public decimal TotalPrice => UnitPrice * Quantity;

    /// <summary>
    /// Totalpris i öre
    /// </summary>
    public long TotalPriceInCents => UnitPriceInCents * Quantity;
}
