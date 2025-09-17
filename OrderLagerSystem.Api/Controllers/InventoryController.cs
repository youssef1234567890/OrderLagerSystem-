using Microsoft.AspNetCore.Mvc;
using OrderLagerSystem.Api.DTOs;
using OrderLagerSystem.Api.Services;
using OrderLagerSystem.Api.Models;
using OrderLagerSystem.Api.Data;
namespace OrderLagerSystem.Api.Controllers

{
    [ApiController]
    [Route("api/[controller]")]
    public class InventoryController : ControllerBase
    {
        private readonly InventoryQueryService _inventoryService;
        private readonly ApplicationDbContext _context;

        public InventoryController(InventoryQueryService inventoryService, ApplicationDbContext context)
        {
            _inventoryService = inventoryService;
            _context = context;
        }

        [HttpGet("by-location")]
        public async Task<IActionResult> GetArticlesByLocation([FromQuery] string location)
        {
            var articles = await _inventoryService.GetArticlesByLocationAsync(location);
            return Ok(articles);
        }

        [HttpGet("stock/{articleId}")]
        public async Task<IActionResult> GetCalculatedStock(int articleId)
        {
            var stock = await _inventoryService.GetCalculatedStockAsync(articleId);
            return Ok(stock);
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchArticles([FromQuery] string? search, [FromQuery] string? location)
        {
            var articles = await _inventoryService.SearchArticlesAsync(search, location);
            return Ok(articles);
        }
        //  Flytta artikel
        [HttpPost("move")]
        public async Task<IActionResult> MoveArticle([FromQuery] int articleId, [FromQuery] string newLocation)
        {
            if (string.IsNullOrWhiteSpace(newLocation))
                return BadRequest("Ogiltig plats");

            var article = await _context.Articles.FindAsync(articleId);
            if (article == null)
                return NotFound("Artikeln hittades inte");

            var oldLocation = article.StorageLocation;
            article.StorageLocation = newLocation;
            await _context.SaveChangesAsync();

            // Logga flytten
            var movement = new StockMovement
            {
                ArticleId = article.ArticleId,
                MovementType = "Move",
                Quantity = 0,
                StockAfterMovement = article.StockQuantity,
                Reason = $"Flyttad från {oldLocation} till {newLocation}",
                CreatedUtc = DateTime.UtcNow
            };

            _context.StockMovements.Add(movement);
            await _context.SaveChangesAsync();

            return Ok(new { message = $"Artikel {article.Name} flyttad från {oldLocation} till {newLocation}" });
        }
    }
}