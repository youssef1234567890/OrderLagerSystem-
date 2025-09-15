using OrderLagerSystem.Api.DTOs;
using ApiBarcodeFormat = OrderLagerSystem.Api.DTOs.BarcodeFormat;

namespace OrderLagerSystem.Api.Services;

/// <summary>
/// Interface för sträckkodsgenerering
/// </summary>
public interface IBarcodeService
{
    /// <summary>
    /// Genererar en sträckod baserat på given text och format
    /// </summary>
    /// <param name="request">Sträckods-generering parametrar</param>
    /// <returns>Base64-kodad bild av streckkoden</returns>
    Task<BarcodeResponse> GenerateBarcodeAsync(BarcodeGenerateRequest request);

    /// <summary>
    /// Genererar en sträckod för en artikel baserat på SKU
    /// </summary>
    /// <param name="sku">Artikel SKU</param>
    /// <param name="format">Önskat format (standard: Code128)</param>
    /// <returns>Base64-kodad bild av streckkoden</returns>
    Task<BarcodeResponse> GenerateArticleBarcodeAsync(string sku, ApiBarcodeFormat format = ApiBarcodeFormat.Code128);

    /// <summary>
    /// Validerar att en text kan kodas i specificerat format
    /// </summary>
    /// <param name="text">Text att validera</param>
    /// <param name="format">Format att validera mot</param>
    /// <returns>True om texten är giltig för formatet</returns>
    bool IsValidForFormat(string text, ApiBarcodeFormat format);

    /// <summary>
    /// Hämtar information om vilket format som rekommenderas för given text
    /// </summary>
    /// <param name="text">Text att analysera</param>
    /// <returns>Rekommenderat streckkodsformat</returns>
    ApiBarcodeFormat GetRecommendedFormat(string text);

    /// <summary>
    /// Validerar text för specifikt format med detaljerat resultat
    /// </summary>
    /// <param name="text">Text att validera</param>
    /// <param name="format">Format att validera mot</param>
    /// <returns>Valideringsresultat med meddelande</returns>
    ValidationResult ValidateForFormat(string text, ApiBarcodeFormat format);

    /// <summary>
    /// Normaliserar text för optimal kompatibilitet med givet format
    /// </summary>
    /// <param name="text">Original text</param>
    /// <param name="format">Målformat</param>
    /// <returns>Normaliserad text</returns>
    string NormalizeTextForBarcode(string text, ApiBarcodeFormat format);

    /// <summary>
    /// Skannar en streckkod och returnerar den avkodade informationen
    /// </summary>
    /// <param name="barcode">Streckkoden att skanna</param>
    /// <returns>Avkodad information från streckkoden</returns>
    Task<BarcodeScanResponse> ScanBarcodeAsync(string barcode);
}
