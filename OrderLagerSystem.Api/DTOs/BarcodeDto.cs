namespace OrderLagerSystem.Api.DTOs;

/// <summary>
/// DTO för sträckkodsgenerering request
/// </summary>
public class BarcodeGenerateRequest
{
    /// <summary>
    /// Text som ska kodas i streckkoden (t.ex. SKU)
    /// </summary>
    public string Text { get; set; } = null!;

    /// <summary>
    /// Format för streckkoden
    /// </summary>
    public BarcodeFormat Format { get; set; } = BarcodeFormat.Code128;

    /// <summary>
    /// Bredd på bilden i pixlar
    /// </summary>
    public int Width { get; set; } = 300;

    /// <summary>
    /// Höjd på bilden i pixlar
    /// </summary>
    public int Height { get; set; } = 100;

    /// <summary>
    /// Om text ska visas under streckkoden
    /// </summary>
    public bool ShowText { get; set; } = true;

    /// <summary>
    /// Marginal runt streckkoden
    /// </summary>
    public int Margin { get; set; } = 10;
}

/// <summary>
/// DTO för sträckkod response
/// </summary>
public class BarcodeResponse
{
    /// <summary>
    /// Base64-kodad bild av streckkoden
    /// </summary>
    public string ImageBase64 { get; set; } = null!;

    /// <summary>
    /// MIME-type för bilden (vanligtvis image/png)
    /// </summary>
    public string ContentType { get; set; } = "image/png";

    /// <summary>
    /// Text som kodades i streckkoden
    /// </summary>
    public string EncodedText { get; set; } = null!;

    /// <summary>
    /// Format som användes
    /// </summary>
    public BarcodeFormat Format { get; set; }

    /// <summary>
    /// Tidstämpel när streckkoden genererades
    /// </summary>
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Streckkods-format som stöds
/// </summary>
public enum BarcodeFormat
{
    /// <summary>
    /// Code 128 - mest flexibel, stöder alfanumeriska tecken
    /// </summary>
    Code128,

    /// <summary>
    /// Code 39 - äldre format, begränsade tecken
    /// </summary>
    Code39,

    /// <summary>
    /// EAN-13 - retail standard, 13 siffror
    /// </summary>
    EAN13,

    /// <summary>
    /// QR Code - 2D kod som kan innehålla mycket data
    /// </summary>
    QRCode
}
