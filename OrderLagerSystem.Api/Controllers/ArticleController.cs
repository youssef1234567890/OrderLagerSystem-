using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderLagerSystem.Api.Data;
using OrderLagerSystem.Api.Models;
using OrderLagerSystem.Api.DTOs;

namespace OrderLagerSystem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // Alla endpoints kräver inloggning
public class ArticleController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ArticleController> _logger;

    public ArticleController(ApplicationDbContext context, ILogger<ArticleController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Hämtar alla artiklar
    /// </summary>
    /// <param name="includeInactive">Om inaktiva artiklar ska inkluderas (endast admin)</param>
    /// <returns>Lista med alla artiklar</returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ArticleDto>>> GetArticles([FromQuery] bool includeInactive = false)
    {
        try
        {
            var query = _context.Articles.AsQueryable();

            // Endast admins kan se inaktiva artiklar
            if (!includeInactive || !User.IsInRole(GlobalRules.Roles.Admin))
            {
                query = query.Where(a => a.IsActive);
            }

            var articles = await query
                .OrderBy(a => a.Name)
                .Select(a => new ArticleDto
                {
                    ArticleId = a.ArticleId,
                    Sku = a.Sku,
                    Name = a.Name,
                    Description = a.Description,
                    Price = a.Price,
                    StockQuantity = a.StockQuantity,
                    MinimumStock = a.MinimumStock,
                    StorageLocation = a.StorageLocation,
                    IsActive = a.IsActive,
                    IsLowStock = a.IsLowStock,
                    CreatedUtc = a.CreatedUtc,
                    UpdatedUtc = a.UpdatedUtc
                })
                .ToListAsync();

            _logger.LogInformation("Retrieved {Count} articles for user {UserId}", 
                articles.Count, User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);

            return Ok(articles);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving articles");
            return StatusCode(500, "Ett fel inträffade när artiklarna skulle hämtas");
        }
    }

    /// <summary>
    /// Hämtar en specifik artikel
    /// </summary>
    /// <param name="id">Artikel ID</param>
    /// <returns>Artikel eller NotFound</returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<ArticleDto>> GetArticle(int id)
    {
        try
        {
            var article = await _context.Articles
                .Where(a => a.ArticleId == id)
                .Select(a => new ArticleDto
                {
                    ArticleId = a.ArticleId,
                    Sku = a.Sku,
                    Name = a.Name,
                    Description = a.Description,
                    Price = a.Price,
                    StockQuantity = a.StockQuantity,
                    MinimumStock = a.MinimumStock,
                    StorageLocation = a.StorageLocation,
                    IsActive = a.IsActive,
                    IsLowStock = a.IsLowStock,
                    CreatedUtc = a.CreatedUtc,
                    UpdatedUtc = a.UpdatedUtc
                })
                .FirstOrDefaultAsync();

            if (article == null)
            {
                _logger.LogWarning("Article with ID {ArticleId} not found", id);
                return NotFound($"Artikel med ID {id} hittades inte");
            }

            // Endast aktiva artiklar för icke-admin användare
            if (!article.IsActive && !User.IsInRole(GlobalRules.Roles.Admin))
            {
                _logger.LogWarning("Non-admin user {UserId} tried to access inactive article {ArticleId}", 
                    User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value, id);
                return NotFound($"Artikel med ID {id} hittades inte");
            }

            return Ok(article);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving article {ArticleId}", id);
            return StatusCode(500, "Ett fel inträffade när artikeln skulle hämtas");
        }
    }

    /// <summary>
    /// Skapar en ny artikel (Admin och Orderkoordinator)
    /// </summary>
    /// <param name="createDto">Artikel data</param>
    /// <returns>Skapad artikel</returns>
    [HttpPost]
    [Authorize(Roles = $"{GlobalRules.Roles.Admin},{GlobalRules.Roles.Orderkoordinator}")]
    public async Task<ActionResult<ArticleDto>> CreateArticle([FromBody] CreateArticleDto createDto)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Invalid article creation request: {Errors}", 
                string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
            return BadRequest(ModelState);
        }

        try
        {
            // Kontrollera om SKU redan finns
            if (await _context.Articles.AnyAsync(a => a.Sku == createDto.Sku))
            {
                _logger.LogWarning("Attempted to create article with existing SKU: {Sku}", createDto.Sku);
                return BadRequest($"En artikel med SKU '{createDto.Sku}' finns redan");
            }

            var article = new Article
            {
                Sku = createDto.Sku,
                Name = createDto.Name,
                Description = createDto.Description,
                PriceInCents = (long)(createDto.Price * 100), // Konvertera till öre
                StockQuantity = createDto.StockQuantity,
                MinimumStock = createDto.MinimumStock,
                StorageLocation = createDto.StorageLocation,
                IsActive = createDto.IsActive,
                CreatedUtc = DateTime.UtcNow
            };

            _context.Articles.Add(article);
            await _context.SaveChangesAsync();

            var articleDto = new ArticleDto
            {
                ArticleId = article.ArticleId,
                Sku = article.Sku,
                Name = article.Name,
                Description = article.Description,
                Price = article.Price,
                StockQuantity = article.StockQuantity,
                MinimumStock = article.MinimumStock,
                StorageLocation = article.StorageLocation,
                IsActive = article.IsActive,
                IsLowStock = article.IsLowStock,
                CreatedUtc = article.CreatedUtc,
                UpdatedUtc = article.UpdatedUtc
            };

            _logger.LogInformation("User {UserId} created new article {ArticleId} with SKU {Sku}", 
                User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value, 
                article.ArticleId, article.Sku);

            return CreatedAtAction(nameof(GetArticle), new { id = article.ArticleId }, articleDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating article with SKU {Sku}", createDto.Sku);
            return StatusCode(500, "Ett fel inträffade när artikeln skulle skapas");
        }
    }

    /// <summary>
    /// Uppdaterar en artikel (Admin och Orderkoordinator)
    /// </summary>
    /// <param name="id">Artikel ID</param>
    /// <param name="updateDto">Uppdaterad artikel data</param>
    /// <returns>Uppdaterad artikel</returns>
    [HttpPut("{id}")]
    [Authorize(Roles = $"{GlobalRules.Roles.Admin},{GlobalRules.Roles.Orderkoordinator}")]
    public async Task<ActionResult<ArticleDto>> UpdateArticle(int id, [FromBody] UpdateArticleDto updateDto)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Invalid article update request for ID {ArticleId}: {Errors}", 
                id, string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
            return BadRequest(ModelState);
        }

        try
        {
            var article = await _context.Articles.FindAsync(id);
            if (article == null)
            {
                _logger.LogWarning("Article with ID {ArticleId} not found for update", id);
                return NotFound($"Artikel med ID {id} hittades inte");
            }

            // Kontrollera om SKU redan finns på annan artikel
            if (await _context.Articles.AnyAsync(a => a.Sku == updateDto.Sku && a.ArticleId != id))
            {
                _logger.LogWarning("Attempted to update article {ArticleId} with existing SKU: {Sku}", id, updateDto.Sku);
                return BadRequest($"En annan artikel med SKU '{updateDto.Sku}' finns redan");
            }

            // Uppdatera artikel
            article.Sku = updateDto.Sku;
            article.Name = updateDto.Name;
            article.Description = updateDto.Description;
            article.PriceInCents = (long)(updateDto.Price * 100); // Konvertera till öre
            article.StockQuantity = updateDto.StockQuantity;
            article.MinimumStock = updateDto.MinimumStock;
            article.StorageLocation = updateDto.StorageLocation;
            article.IsActive = updateDto.IsActive;
            article.UpdatedUtc = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var articleDto = new ArticleDto
            {
                ArticleId = article.ArticleId,
                Sku = article.Sku,
                Name = article.Name,
                Description = article.Description,
                Price = article.Price,
                StockQuantity = article.StockQuantity,
                MinimumStock = article.MinimumStock,
                StorageLocation = article.StorageLocation,
                IsActive = article.IsActive,
                IsLowStock = article.IsLowStock,
                CreatedUtc = article.CreatedUtc,
                UpdatedUtc = article.UpdatedUtc
            };

            _logger.LogInformation("User {UserId} updated article {ArticleId} with SKU {Sku}", 
                User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value, 
                article.ArticleId, article.Sku);

            return Ok(articleDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating article {ArticleId}", id);
            return StatusCode(500, "Ett fel inträffade när artikeln skulle uppdateras");
        }
    }

    /// <summary>
    /// Tar bort en artikel (endast Admin)
    /// </summary>
    /// <param name="id">Artikel ID</param>
    /// <returns>Bekräftelse på borttagning</returns>
    [HttpDelete("{id}")]
    [Authorize(Roles = GlobalRules.Roles.Admin)]
    public async Task<ActionResult> DeleteArticle(int id)
    {
        try
        {
            var article = await _context.Articles.FindAsync(id);
            if (article == null)
            {
                _logger.LogWarning("Article with ID {ArticleId} not found for deletion", id);
                return NotFound($"Artikel med ID {id} hittades inte");
            }

            // Kontrollera om artikeln används i order
            var hasOrderItems = await _context.OrderItems.AnyAsync(oi => oi.ArticleId == id);
            if (hasOrderItems)
            {
                _logger.LogWarning("Attempted to delete article {ArticleId} that has order items", id);
                return BadRequest("Artikeln kan inte tas bort eftersom den används i order. Sätt den som inaktiv istället.");
            }

            _context.Articles.Remove(article);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Admin {UserId} deleted article {ArticleId} with SKU {Sku}", 
                User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value, 
                article.ArticleId, article.Sku);

            return Ok(new { message = $"Artikel '{article.Name}' har tagits bort" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting article {ArticleId}", id);
            return StatusCode(500, "Ett fel inträffade när artikeln skulle tas bort");
        }
    }
}
