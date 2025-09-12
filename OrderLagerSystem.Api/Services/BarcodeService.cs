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

            // Validera input
            if (string.IsNullOrWhiteSpace(request.Text))
                throw new ArgumentException("Text kan inte vara tom");

            if (!IsValidForFormat(request.Text, request.Format))
                throw new ArgumentException($"Texten '{request.Text}' är inte giltig för format {request.Format}");

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
            
            // Generera pixel data
            var pixelData = writer.Write(request.Text);
            
            // Konvertera pixel data till SKBitmap
            using var bitmap = CreateBitmapFromPixelData(pixelData);
            
            // Lägg till text om det begärs (endast för 1D streckkoder)
            using var finalBitmap = request.ShowText && IsOneDimensional(request.Format) 
                ? AddTextToBitmap(bitmap, request.Text, request.Width, request.Height)
                : bitmap.Copy();
                
            // Konvertera till PNG bytes
            using var image = SKImage.FromBitmap(finalBitmap);
            using var data = image.Encode(SKEncodedImageFormat.Png, 100);
            var base64 = Convert.ToBase64String(data.ToArray());

            var response = new BarcodeResponse
            {
                ImageBase64 = base64,
                ContentType = "image/png",
                EncodedText = request.Text,
                Format = request.Format,
                GeneratedAt = DateTime.UtcNow
            };

            _logger.LogInformation("Successfully generated barcode for text: {Text}", request.Text);
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
            ApiBarcodeFormat.Code128 => true, // Code 128 can encode almost any ASCII character
            ApiBarcodeFormat.Code39 => Regex.IsMatch(text, @"^[A-Z0-9\-\.\$\/\+%\s]*$"), // Limited character set
            ApiBarcodeFormat.EAN13 => Regex.IsMatch(text, @"^\d{12,13}$"), // Must be 12 or 13 digits
            ApiBarcodeFormat.QRCode => text.Length <= 4296, // QR Code can handle up to ~4000 characters
            _ => false
        };
    }

    public ApiBarcodeFormat GetRecommendedFormat(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return ApiBarcodeFormat.Code128;

        // EAN-13 för numeriska koder med rätt längd
        if (Regex.IsMatch(text, @"^\d{12,13}$"))
            return ApiBarcodeFormat.EAN13;

        // Code39 för enkla alfanumeriska koder
        if (text.Length <= 20 && Regex.IsMatch(text, @"^[A-Z0-9\-\.\$\/\+%\s]*$"))
            return ApiBarcodeFormat.Code39;

        // QR Code för långa texter eller speciella tecken
        if (text.Length > 50 || Regex.IsMatch(text, @"[^\x20-\x7E]"))
            return ApiBarcodeFormat.QRCode;

        // Code128 som default för mest flexibilitet
        return ApiBarcodeFormat.Code128;
    }

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
