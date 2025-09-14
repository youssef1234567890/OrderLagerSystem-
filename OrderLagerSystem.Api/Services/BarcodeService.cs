using OrderLagerSystem.Api.DTOs;
using SkiaSharp;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using ZXing;
using ZXing.Common;
using ZXing.Rendering;
using ApiBarcodeFormat = OrderLagerSystem.Api.DTOs.BarcodeFormat;
using ZXingBarcodeFormat = ZXing.BarcodeFormat;

namespace OrderLagerSystem.Api.Services;

/// <summary>
/// Service för generering av sträckkoder med ZXing.Net
/// </summary>
public class BarcodeService : IBarcodeService
{
    private readonly ILogger<BarcodeService> _logger;

    public BarcodeService(ILogger<BarcodeService> logger)
    {
        _logger = logger;
    }

    public async Task<BarcodeResponse> GenerateBarcodeAsync(BarcodeGenerateRequest request)
    {
        try
        {
            _logger.LogInformation("Generating barcode for text: {Text} with format: {Format}", 
                request.Text, request.Format);

            // Förbättrad validering med tydliga felmeddelanden
            if (string.IsNullOrWhiteSpace(request.Text))
                throw new ArgumentException("Text kan inte vara tom");

            var validationResult = ValidateForFormat(request.Text, request.Format);
            if (!validationResult.IsValid)
                throw new ArgumentException($"Validering misslyckades: {validationResult.Message}");

            // Normalisera texten automatiskt
            var normalizedText = NormalizeTextForBarcode(request.Text, request.Format);
            
            // Varna om format inte är optimalt
            var recommendedFormat = GetRecommendedFormat(request.Text);
            if (recommendedFormat != request.Format)
            {
                _logger.LogInformation("Text '{Text}' skulle fungera bättre med format {RecommendedFormat} istället för {RequestedFormat}",
                    request.Text, recommendedFormat, request.Format);
            }

            // Konvertera vårt format till ZXing format
            var zxingFormat = ConvertToZXingFormat(request.Format);

            // Skapa writer för att få pixel data
            var writer = new BarcodeWriterPixelData
            {
                Format = zxingFormat,
                Options = new EncodingOptions
                {
                    Width = request.Width,
                    Height = request.Height,
                    Margin = request.Margin
                }
            };
            
            // Generera pixel data med normaliserad text
            var pixelData = writer.Write(normalizedText);
            
            // Konvertera pixel data till SKBitmap
            using var bitmap = CreateBitmapFromPixelData(pixelData);
            
            // Lägg till text om det begärs (endast för 1D streckkoder)
            using var finalBitmap = request.ShowText && IsOneDimensional(request.Format) 
                ? AddTextToBitmap(bitmap, normalizedText, request.Width, request.Height)
                : bitmap.Copy();
                
            // Konvertera till PNG bytes
            using var image = SKImage.FromBitmap(finalBitmap);
            using var data = image.Encode(SKEncodedImageFormat.Png, 100);
            var base64 = Convert.ToBase64String(data.ToArray());

            var response = new BarcodeResponse
            {
                ImageBase64 = base64,
                ContentType = "image/png",
                EncodedText = normalizedText,
                Format = request.Format,
                GeneratedAt = DateTime.UtcNow
            };

            _logger.LogInformation("Successfully generated barcode for text: {Text}", normalizedText);
            return await Task.FromResult(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating barcode for text: {Text}", request.Text);
            throw;
        }
    }

    public async Task<BarcodeResponse> GenerateArticleBarcodeAsync(string sku, ApiBarcodeFormat format = ApiBarcodeFormat.Code128)
    {
        if (string.IsNullOrWhiteSpace(sku))
            throw new ArgumentException("SKU kan inte vara tom");

        var request = new BarcodeGenerateRequest
        {
            Text = sku,
            Format = format,
            Width = 300,
            Height = 100,
            ShowText = true,
            Margin = 10
        };

        return await GenerateBarcodeAsync(request);
    }

    public bool IsValidForFormat(string text, ApiBarcodeFormat format)
    {
        if (string.IsNullOrWhiteSpace(text))
            return false;

        return format switch
        {
            ApiBarcodeFormat.Code128 => IsValidCode128(text),
            ApiBarcodeFormat.Code39 => IsValidCode39(text),
            ApiBarcodeFormat.EAN13 => IsValidEAN13(text),
            ApiBarcodeFormat.QRCode => IsValidQRCode(text),
            _ => false
        };
    }

    public ApiBarcodeFormat GetRecommendedFormat(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return ApiBarcodeFormat.Code128;

        // EAN-13 för numeriska koder med rätt längd
        if (IsValidEAN13(text))
            return ApiBarcodeFormat.EAN13;

        // Code39 för enkla alfanumeriska koder
        if (text.Length <= 20 && IsValidCode39(text) && !ContainsLowercase(text))
            return ApiBarcodeFormat.Code39;

        // QR Code för långa texter eller speciella tecken
        if (text.Length > 50 || ContainsSpecialCharacters(text))
            return ApiBarcodeFormat.QRCode;

        // Code128 som default för mest flexibilitet
        return ApiBarcodeFormat.Code128;
    }

    /// <summary>
    /// Validerar text för specifikt format med detaljerat resultat
    /// </summary>
    /// <param name="text">Text att validera</param>
    /// <param name="format">Format att validera mot</param>
    /// <returns>Valideringsresultat med meddelande</returns>
    public ValidationResult ValidateForFormat(string text, ApiBarcodeFormat format)
    {
        if (string.IsNullOrWhiteSpace(text))
            return new ValidationResult(false, "Text kan inte vara tom");

        return format switch
        {
            ApiBarcodeFormat.Code128 => ValidateCode128(text),
            ApiBarcodeFormat.Code39 => ValidateCode39(text),
            ApiBarcodeFormat.EAN13 => ValidateEAN13(text),
            ApiBarcodeFormat.QRCode => ValidateQRCode(text),
            _ => new ValidationResult(false, "Okänt format")
        };
    }

    /// <summary>
    /// Normaliserar text för optimal kompatibilitet med givet format
    /// </summary>
    /// <param name="text">Original text</param>
    /// <param name="format">Målformat</param>
    /// <returns>Normaliserad text</returns>
    public string NormalizeTextForBarcode(string text, ApiBarcodeFormat format)
    {
        if (string.IsNullOrWhiteSpace(text))
            return text;

        return format switch
        {
            ApiBarcodeFormat.Code39 => text.ToUpper().Trim(),
            ApiBarcodeFormat.EAN13 => ExtractDigits(text),
            ApiBarcodeFormat.Code128 => text.Trim(),
            ApiBarcodeFormat.QRCode => text,
            _ => text.Trim()
        };
    }

    // Privata hjälpmetoder för validering

    /// <summary>
    /// Validerar text för Code128 format
    /// </summary>
    private ValidationResult ValidateCode128(string text)
    {
        // Code 128 kan hantera alla ASCII-tecken
        if (text.Any(c => c > 127))
            return new ValidationResult(false, "Code128 stöder endast ASCII-tecken (0-127)");

        if (text.Length > 80)
            return new ValidationResult(false, "Code128: Rekommenderar max 80 tecken för läsbarhet");

        return new ValidationResult(true, "Giltig för Code128");
    }

    /// <summary>
    /// Validerar text för Code39 format
    /// </summary>
    private ValidationResult ValidateCode39(string text)
    {
        // Code 39 har begränsad teckenuppsättning
        var allowedPattern = @"^[A-Z0-9\-\.\$\/\+%\s]*$";
        if (!Regex.IsMatch(text.ToUpper(), allowedPattern))
            return new ValidationResult(false, "Code39 tillåter endast: A-Z, 0-9, -, ., $, /, +, %, mellanslag");

        if (text.Length > 25)
            return new ValidationResult(false, "Code39: Rekommenderar max 25 tecken för läsbarhet");

        return new ValidationResult(true, "Giltig för Code39");
    }

    /// <summary>
    /// Validerar text för EAN13 format
    /// </summary>
    private ValidationResult ValidateEAN13(string text)
    {
        var digits = ExtractDigits(text);
        
        if (digits.Length == 0)
            return new ValidationResult(false, "EAN13 kräver numeriska tecken");

        if (digits.Length < 12)
            return new ValidationResult(false, $"EAN13 kräver minst 12 siffror (har {digits.Length})");

        if (digits.Length > 13)
            return new ValidationResult(false, $"EAN13 får max ha 13 siffror (har {digits.Length})");

        // Om 13 siffror, validera checksumma
        if (digits.Length == 13 && !IsValidEAN13Checksum(digits))
            return new ValidationResult(false, "EAN13 har ogiltig checksumma");

        return new ValidationResult(true, "Giltig för EAN13");
    }

    /// <summary>
    /// Validerar text för QR Code format
    /// </summary>
    private ValidationResult ValidateQRCode(string text)
    {
        if (text.Length > 4000)
            return new ValidationResult(false, $"QR Code: Max 4000 tecken (har {text.Length})");

        return new ValidationResult(true, "Giltig för QR Code");
    }

    // Enkla valideringsmetoder för IsValidForFormat

    private bool IsValidCode128(string text) => text.All(c => c < 128);
    
    private bool IsValidCode39(string text) => 
        Regex.IsMatch(text.ToUpper(), @"^[A-Z0-9\-\.\$\/\+%\s]*$");
    
    private bool IsValidEAN13(string text)
    {
        var digits = ExtractDigits(text);
        return digits.Length >= 12 && digits.Length <= 13 && 
               (digits.Length == 12 || IsValidEAN13Checksum(digits));
    }
    
    private bool IsValidQRCode(string text) => text.Length <= 4000;

    /// <summary>
    /// Extraherar endast siffror från en text
    /// </summary>
    private string ExtractDigits(string text) => 
        new string(text.Where(char.IsDigit).ToArray());

    /// <summary>
    /// Validerar EAN13 checksumma
    /// </summary>
    private bool IsValidEAN13Checksum(string digits)
    {
        if (digits.Length != 13) return false;

        int sum = 0;
        for (int i = 0; i < 12; i++)
        {
            int digit = int.Parse(digits[i].ToString());
            sum += (i % 2 == 0) ? digit : digit * 3;
        }

        int checksum = (10 - (sum % 10)) % 10;
        int providedChecksum = int.Parse(digits[12].ToString());

        return checksum == providedChecksum;
    }

    private bool ContainsLowercase(string text) => text.Any(char.IsLower);

    private bool ContainsSpecialCharacters(string text) => 
        text.Any(c => !char.IsLetterOrDigit(c) && !char.IsWhiteSpace(c) && c != '-' && c != '_');

    private static ZXingBarcodeFormat ConvertToZXingFormat(ApiBarcodeFormat format)
    {
        return format switch
        {
            ApiBarcodeFormat.Code128 => ZXingBarcodeFormat.CODE_128,
            ApiBarcodeFormat.Code39 => ZXingBarcodeFormat.CODE_39,
            ApiBarcodeFormat.EAN13 => ZXingBarcodeFormat.EAN_13,
            ApiBarcodeFormat.QRCode => ZXingBarcodeFormat.QR_CODE,
            _ => ZXingBarcodeFormat.CODE_128
        };
    }

    private static bool IsOneDimensional(ApiBarcodeFormat format)
    {
        return format switch
        {
            ApiBarcodeFormat.Code128 => true,
            ApiBarcodeFormat.Code39 => true,
            ApiBarcodeFormat.EAN13 => true,
            ApiBarcodeFormat.QRCode => false,
            _ => true
        };
    }

    private static SKBitmap CreateBitmapFromPixelData(PixelData pixelData)
    {
        var width = pixelData.Width;
        var height = pixelData.Height;
        var bitmap = new SKBitmap(width, height, SKColorType.Bgra8888, SKAlphaType.Premul);
        
        // Kopiera pixel data till SKBitmap
        var pixels = bitmap.GetPixels();
        Marshal.Copy(pixelData.Pixels, 0, pixels, pixelData.Pixels.Length);
        
        return bitmap;
    }

    private static SKBitmap AddTextToBitmap(SKBitmap originalBitmap, string text, int width, int height)
    {
        // Skapa ny bitmap med extra utrymme för text
        var textHeight = 30;
        var newHeight = height + textHeight;
        var newBitmap = new SKBitmap(width, newHeight);

        using var canvas = new SKCanvas(newBitmap);
        
        // Fyll bakgrund med vitt
        canvas.Clear(SKColors.White);

        // Rita den ursprungliga streckkoden
        var destRect = new SKRect(0, 0, width, height);
        canvas.DrawBitmap(originalBitmap, destRect);

        // Rita texten under streckkoden
        using var paint = new SKPaint
        {
            Color = SKColors.Black,
            TextSize = 16,
            TextAlign = SKTextAlign.Center,
            Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyle.Normal),
            IsAntialias = true
        };

        var textY = height + (textHeight / 2) + 8;
        canvas.DrawText(text, width / 2f, textY, paint);

        return newBitmap;
    }
}
