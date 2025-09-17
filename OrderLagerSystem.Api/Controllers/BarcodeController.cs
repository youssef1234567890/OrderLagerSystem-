using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderLagerSystem.Api.Data;
using OrderLagerSystem.Api.DTOs;
using OrderLagerSystem.Api.Models;
using OrderLagerSystem.Api.Services;
using ApiBarcodeFormat = OrderLagerSystem.Api.DTOs.BarcodeFormat;

namespace OrderLagerSystem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // Alla endpoints kräver inloggning
public class BarcodeController : ControllerBase
{
    private readonly IBarcodeService _barcodeService;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<BarcodeController> _logger;

    public BarcodeController(
        IBarcodeService barcodeService,
        ApplicationDbContext context,
        ILogger<BarcodeController> logger)
    {
        _barcodeService = barcodeService;
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Genererar en sträckod från given text och parametrar
    /// </summary>
    /// <param name="request">Sträckods-generering parametrar</param>
    /// <returns>Base64-kodad bild av streckkoden</returns>
    [HttpPost("generate")]
    public async Task<ActionResult<BarcodeResponse>> GenerateBarcode([FromBody] BarcodeGenerateRequest request)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Invalid barcode generation request: {Errors}",
                string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
            return BadRequest(ModelState);
        }

        try
        {
            var response = await _barcodeService.GenerateBarcodeAsync(request);

            _logger.LogInformation("User {UserId} generated barcode for text: {Text}",
                User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value,
                request.Text);

            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Invalid barcode request: {Message}", ex.Message);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating barcode for text: {Text}", request.Text);
            return StatusCode(500, "Ett fel inträffade när streckkoden skulle genereras");
        }
    }

    /// <summary>
    /// Genererar en sträckod för en specifik artikel baserat på artikel ID
    /// </summary>
    /// <param name="articleId">Artikel ID</param>
    /// <param name="format">Önskat streckkodsformat (standard: Code128)</param>
    /// <param name="width">Bredd i pixlar (standard: 300)</param>
    /// <param name="height">Höjd i pixlar (standard: 100)</param>
    /// <param name="showText">Om text ska visas under streckkoden (standard: true)</param>
    /// <returns>Base64-kodad bild av streckkoden</returns>
    [HttpGet("article/{articleId}")]
    public async Task<ActionResult<BarcodeResponse>> GenerateArticleBarcode(
        int articleId,
        [FromQuery] ApiBarcodeFormat format = ApiBarcodeFormat.Code128,
        [FromQuery] int width = 300,
        [FromQuery] int height = 100,
        [FromQuery] bool showText = true)
    {
        try
        {
            // Hämta artikeln från databasen
            var article = await _context.Articles.FindAsync(articleId);
            if (article == null)
            {
                _logger.LogWarning("Article with ID {ArticleId} not found for barcode generation", articleId);
                return NotFound($"Artikel med ID {articleId} hittades inte");
            }

            // Endast aktiva artiklar för icke-admin användare
            if (!article.IsActive && !User.IsInRole(GlobalRules.Roles.Admin))
            {
                _logger.LogWarning("Non-admin user {UserId} tried to generate barcode for inactive article {ArticleId}",
                    User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value, articleId);
                return NotFound($"Artikel med ID {articleId} hittades inte");
            }

            var request = new BarcodeGenerateRequest
            {
                Text = article.Sku,
                Format = format,
                Width = width,
                Height = height,
                ShowText = showText,
                Margin = 10
            };

            var response = await _barcodeService.GenerateBarcodeAsync(request);

            _logger.LogInformation("User {UserId} generated barcode for article {ArticleId} (SKU: {Sku})",
                User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value,
                articleId, article.Sku);

            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Invalid barcode request for article {ArticleId}: {Message}", articleId, ex.Message);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating barcode for article {ArticleId}", articleId);
            return StatusCode(500, "Ett fel inträffade när streckkoden skulle genereras");
        }
    }

    /// <summary>
    /// Genererar en sträckod för en artikel baserat på SKU
    /// </summary>
    /// <param name="sku">Artikel SKU</param>
    /// <param name="format">Önskat streckkodsformat (standard: Code128)</param>
    /// <param name="width">Bredd i pixlar (standard: 300)</param>
    /// <param name="height">Höjd i pixlar (standard: 100)</param>
    /// <param name="showText">Om text ska visas under streckkoden (standard: true)</param>
    /// <returns>Base64-kodad bild av streckkoden</returns>
    [HttpGet("article/sku/{sku}")]
    public async Task<ActionResult<BarcodeResponse>> GenerateArticleBarcodeBySku(
        string sku,
        [FromQuery] ApiBarcodeFormat format = ApiBarcodeFormat.Code128,
        [FromQuery] int width = 300,
        [FromQuery] int height = 100,
        [FromQuery] bool showText = true)
    {
        try
        {
            // Hämta artikeln från databasen
            var article = await _context.Articles
                .Where(a => a.Sku == sku)
                .FirstOrDefaultAsync();

            if (article == null)
            {
                _logger.LogWarning("Article with SKU {Sku} not found for barcode generation", sku);
                return NotFound($"Artikel med SKU {sku} hittades inte");
            }

            // Endast aktiva artiklar för icke-admin användare
            if (!article.IsActive && !User.IsInRole(GlobalRules.Roles.Admin))
            {
                _logger.LogWarning("Non-admin user {UserId} tried to generate barcode for inactive article SKU {Sku}",
                    User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value, sku);
                return NotFound($"Artikel med SKU {sku} hittades inte");
            }

            var request = new BarcodeGenerateRequest
            {
                Text = sku,
                Format = format,
                Width = width,
                Height = height,
                ShowText = showText,
                Margin = 10
            };

            var response = await _barcodeService.GenerateBarcodeAsync(request);

            _logger.LogInformation("User {UserId} generated barcode for article SKU: {Sku}",
                User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value, sku);

            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Invalid barcode request for SKU {Sku}: {Message}", sku, ex.Message);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating barcode for SKU {Sku}", sku);
            return StatusCode(500, "Ett fel inträffade när streckkoden skulle genereras");
        }
    }

    /// <summary>
    /// Returnerar direkt en PNG-bild av streckkoden för en artikel
    /// </summary>
    /// <param name="articleId">Artikel ID</param>
    /// <param name="format">Streckkodsformat</param>
    /// <param name="width">Bredd</param>
    /// <param name="height">Höjd</param>
    /// <param name="showText">Visa text</param>
    /// <returns>PNG-bild</returns>
    [HttpGet("article/{articleId}/image")]
    public async Task<IActionResult> GetArticleBarcodeImage(
        int articleId,
        [FromQuery] ApiBarcodeFormat format = ApiBarcodeFormat.Code128,
        [FromQuery] int width = 300,
        [FromQuery] int height = 100,
        [FromQuery] bool showText = true)
    {
        try
        {
            var result = await GenerateArticleBarcode(articleId, format, width, height, showText);

            if (result.Result is OkObjectResult okResult && okResult.Value is BarcodeResponse response)
            {
                var imageBytes = Convert.FromBase64String(response.ImageBase64);
                return File(imageBytes, "image/png", $"article-{articleId}-barcode.png");
            }

            return result.Result ?? NotFound();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error returning barcode image for article {ArticleId}", articleId);
            return StatusCode(500, "Ett fel inträffade");
        }
    }

    /// <summary>
    /// Validerar om en text kan kodas i specificerat format
    /// </summary>
    /// <param name="text">Text att validera</param>
    /// <param name="format">Format att testa mot</param>
    /// <returns>Information om validering</returns>
    [HttpPost("validate")]
    public ActionResult<object> ValidateText([FromBody] object request)
    {
        try
        {
            // Extrahera properties från request object
            if (request is not System.Text.Json.JsonElement jsonElement)
                return BadRequest("Ogiltigt request format");

            if (!jsonElement.TryGetProperty("text", out var textElement) ||
                !jsonElement.TryGetProperty("format", out var formatElement))
                return BadRequest("Text och format krävs");

            var text = textElement.GetString();
            if (!Enum.TryParse<ApiBarcodeFormat>(formatElement.GetString(), true, out var format))
                return BadRequest("Ogiltigt format");

            var isValid = _barcodeService.IsValidForFormat(text!, format);
            var recommended = _barcodeService.GetRecommendedFormat(text!);

            return Ok(new
            {
                text = text,
                format = format.ToString(),
                isValid = isValid,
                recommendedFormat = recommended.ToString(),
                message = isValid
                    ? $"Text är giltig för {format}"
                    : $"Text är inte giltig för {format}. Rekommenderar {recommended}"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating barcode text");
            return StatusCode(500, "Ett fel inträffade vid validering");
        }
    }

    /// <summary>
    /// Hämtar information om tillgängliga streckkodsformat
    /// </summary>
    /// <returns>Lista över format och deras beskrivningar</returns>
    [HttpGet("formats")]
    public ActionResult<object> GetAvailableFormats()
    {
        var formats = new[]
        {
            new {
                name = "Code128",
                value = ApiBarcodeFormat.Code128,
                description = "Mest flexibel - stöder alla ASCII-tecken. Rekommenderas för SKU:er.",
                useCases = new[] { "SKU-koder", "Inventarienummer", "Allmän text" },
                maxLength = "Praktiskt obegränsad",
                allowedCharacters = "Alla ASCII-tecken (0-127)"
            },
            new {
                name = "Code39",
                value = ApiBarcodeFormat.Code39,
                description = "Äldre format med begränsad teckenuppsättning men bred kompatibilitet.",
                useCases = new[] { "Enkla identifierare", "Äldre system" },
                maxLength = "Variabel, men håll kort för läsbarhet",
                allowedCharacters = "A-Z, 0-9, -, ., $, /, +, %, mellanslag"
            },
            new {
                name = "EAN13",
                value = ApiBarcodeFormat.EAN13,
                description = "Retail-standard för produkter. Kräver 12 eller 13 siffror.",
                useCases = new[] { "Produktkoder", "Retail", "Försäljning" },
                maxLength = "12-13 siffror",
                allowedCharacters = "Endast siffror (0-9)"
            },
            new {
                name = "QRCode",
                value = ApiBarcodeFormat.QRCode,
                description = "2D-kod som kan innehålla mycket data och speciella tecken.",
                useCases = new[] { "URLs", "Detaljerad information", "Internationella tecken" },
                maxLength = "Upp till ~4000 tecken",
                allowedCharacters = "Unicode - praktiskt alla tecken"
            }
        };

        return Ok(new { formats = formats, defaultFormat = ApiBarcodeFormat.Code128 });
    }

    /// <summary>
    /// Validerar text för streckkodsformat med detaljerad information
    /// </summary>
    /// <param name="request">Request med text och format att validera</param>
    /// <returns>Valideringsresultat med rekommendationer</returns>
    [HttpPost("validate-simple")]
    public ActionResult<object> ValidateTextSimple([FromBody] object request)
    {
        try
        {
            // Parsea JSON request
            if (request is not System.Text.Json.JsonElement jsonElement)
                return BadRequest("Ogiltigt request format");

            if (!jsonElement.TryGetProperty("text", out var textElement) ||
                !jsonElement.TryGetProperty("format", out var formatElement))
                return BadRequest("Text och format krävs");

            var text = textElement.GetString();
            if (!Enum.TryParse<ApiBarcodeFormat>(formatElement.GetString(), true, out var format))
                return BadRequest("Ogiltigt format");

            // Använd den nya valideringsmetoden
            var validationResult = _barcodeService.ValidateForFormat(text!, format);
            var recommended = _barcodeService.GetRecommendedFormat(text!);
            var normalized = _barcodeService.NormalizeTextForBarcode(text!, format);

            return Ok(new
            {
                text = text,
                normalizedText = normalized != text ? normalized : null,
                format = format.ToString(),
                isValid = validationResult.IsValid,
                message = validationResult.Message,
                recommendedFormat = recommended.ToString(),
                recommendation = recommended != format
                    ? $"Rekommenderar {recommended} för bättre kompatibilitet"
                    : "Valt format är optimalt"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating barcode text");
            return StatusCode(500, "Ett fel inträffade vid validering");
        }
    }
     [HttpPost("scan")] //  Tolka (skanna) streckkod från bild
    public async Task<IActionResult> ScanBarcode([FromForm] IFormFile image)
    {
        if (image == null || image.Length == 0)
            return BadRequest("Ingen bild bifogad.");

        var result = await _barcodeService.DecodeBarcodeAsync(image);
        if (string.IsNullOrEmpty(result))
            return NotFound("Ingen streckkod kunde tolkas.");

        return Ok(new { barcode = result });
    }
    [HttpPost("scan-move")] // Lagerförflyttning via streckkod och kvantitet
    public async Task<IActionResult> ScanAndMove([FromBody] ScanMoveRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Barcode) || request.Quantity == 0)
            return BadRequest("Barcode och quantity krävs.");

        // Exempel: användar-id från claims
        var userId = User.Identity?.Name ?? "unknown";

        var success = await _barcodeService.MoveStockByBarcodeAsync(request.Barcode, request.Quantity, userId);
        if (!success)
            return NotFound("Artikel hittades inte för given streckkod.");

        return Ok(new { message = "Lagerförflyttning skapad." });
    }
    public class ScanMoveRequest // DTO för lagerförflyttning via streckkod
{
    public string Barcode { get; set; } = null!;
    public int Quantity { get; set; }
}
}
