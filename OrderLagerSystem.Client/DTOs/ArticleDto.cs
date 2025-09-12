using System.ComponentModel.DataAnnotations;

namespace OrderLagerSystem.Client.DTOs;

/// <summary>
/// Globala regler för validering (kopierat från API)
/// </summary>
public static class GlobalRules
{
    public const int SkuMaxLen = 64;
    public const int NameMaxLen = 200;
    public const int DescriptionMaxLen = 2000;
    public const int LocationMaxLen = 100;
}

/// <summary>
/// DTO för att returnera artikel information
/// </summary>
public class ArticleDto
{
    public int ArticleId { get; set; }
    public string Sku { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public int MinimumStock { get; set; }
    public string? StorageLocation { get; set; }
    public bool IsActive { get; set; }
    public bool IsLowStock { get; set; }
    public DateTime CreatedUtc { get; set; }
    public DateTime? UpdatedUtc { get; set; }
}

/// <summary>
/// DTO för att skapa nya artiklar
/// </summary>
public class CreateArticleDto
{
    [Required(ErrorMessage = "SKU är obligatoriskt")]
    [MaxLength(GlobalRules.SkuMaxLen, ErrorMessage = "SKU får max vara {1} tecken")]
    public string Sku { get; set; } = null!;

    [Required(ErrorMessage = "Namn är obligatoriskt")]
    [MaxLength(GlobalRules.NameMaxLen, ErrorMessage = "Namn får max vara {1} tecken")]
    public string Name { get; set; } = null!;

    [MaxLength(GlobalRules.DescriptionMaxLen, ErrorMessage = "Beskrivning får max vara {1} tecken")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Pris är obligatoriskt")]
    [Range(0, 999999.99, ErrorMessage = "Pris måste vara mellan 0 och 999999.99 kr")]
    public decimal Price { get; set; }

    [Required(ErrorMessage = "Lagersaldo är obligatoriskt")]
    [Range(0, int.MaxValue, ErrorMessage = "Lagersaldo måste vara 0 eller högre")]
    public int StockQuantity { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Minimum lagersaldo måste vara 0 eller högre")]
    public int MinimumStock { get; set; } = 0;

    [MaxLength(GlobalRules.LocationMaxLen, ErrorMessage = "Lagerplats får max vara {1} tecken")]
    public string? StorageLocation { get; set; }

    public bool IsActive { get; set; } = true;
}

/// <summary>
/// DTO för att uppdatera artiklar
/// </summary>
public class UpdateArticleDto
{
    [Required(ErrorMessage = "SKU är obligatoriskt")]
    [MaxLength(GlobalRules.SkuMaxLen, ErrorMessage = "SKU får max vara {1} tecken")]
    public string Sku { get; set; } = null!;

    [Required(ErrorMessage = "Namn är obligatoriskt")]
    [MaxLength(GlobalRules.NameMaxLen, ErrorMessage = "Namn får max vara {1} tecken")]
    public string Name { get; set; } = null!;

    [MaxLength(GlobalRules.DescriptionMaxLen, ErrorMessage = "Beskrivning får max vara {1} tecken")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Pris är obligatoriskt")]
    [Range(0, 999999.99, ErrorMessage = "Pris måste vara mellan 0 och 999999.99 kr")]
    public decimal Price { get; set; }

    [Required(ErrorMessage = "Lagersaldo är obligatoriskt")]
    [Range(0, int.MaxValue, ErrorMessage = "Lagersaldo måste vara 0 eller högre")]
    public int StockQuantity { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Minimum lagersaldo måste vara 0 eller högre")]
    public int MinimumStock { get; set; } = 0;

    [MaxLength(GlobalRules.LocationMaxLen, ErrorMessage = "Lagerplats får max vara {1} tecken")]
    public string? StorageLocation { get; set; }

    public bool IsActive { get; set; } = true;
}
