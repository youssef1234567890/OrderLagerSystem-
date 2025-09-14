using OrderLagerSystem.Client.DTOs;

namespace OrderLagerSystem.Client.Services;

public interface IBarcodeService
{
    /// <summary>
    /// Genererar en sträckod baserat på text och parametrar
    /// </summary>
    Task<BarcodeResponse?> GenerateBarcodeAsync(BarcodeGenerateRequest request);

    /// <summary>
    /// Genererar en sträckod för en artikel baserat på artikel ID
    /// </summary>
    Task<BarcodeResponse?> GenerateArticleBarcodeAsync(int articleId, BarcodeFormat format = BarcodeFormat.Code128, int width = 300, int height = 100, bool showText = true);

    /// <summary>
    /// Genererar en sträckod för en artikel baserat på SKU
    /// </summary>
    Task<BarcodeResponse?> GenerateArticleBarcodeBySKUAsync(string sku, BarcodeFormat format = BarcodeFormat.Code128, int width = 300, int height = 100, bool showText = true);

    /// <summary>
    /// Hämtar URL till streckkodsbild för en artikel
    /// </summary>
    string GetArticleBarcodeImageUrl(int articleId, BarcodeFormat format = BarcodeFormat.Code128, int width = 300, int height = 100, bool showText = true);

    /// <summary>
    /// Validerar om text kan kodas i specificerat format
    /// </summary>
    Task<object?> ValidateTextForFormatAsync(string text, BarcodeFormat format);

    /// <summary>
    /// Hämtar information om tillgängliga streckkodsformat
    /// </summary>
    Task<object?> GetAvailableFormatsAsync();
}
