using System.ComponentModel.DataAnnotations;
using OrderLagerSystem.Api.Models;

namespace OrderLagerSystem.Api.DTOs;
/// <summary>
/// Request-modell för att uppdatera orderstatus
/// </summary>
/// <remarks>

public class OrderStatusUpdateRequest
{
    /// <summary>
    /// Valfria anteckningar eller kommentarer om statusuppdateringen
    /// </summary>
    [MaxLength(500, ErrorMessage = "Anteckningar får vara max 500 tecken")]
    public string? Comment { get; set; }
}

namespace OrderLagerSystem.Api.DTOs;

public class OrderStatusUpdateRequest
{
    public string NewStatus { get; set; } = null!;
    public string? Comment { get; set; }
}



