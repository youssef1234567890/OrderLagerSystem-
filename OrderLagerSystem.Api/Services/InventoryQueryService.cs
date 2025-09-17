using Microsoft.EntityFrameworkCore;
using OrderLagerSystem.Api.Data;
using OrderLagerSystem.Api.Models;

namespace OrderLagerSystem.Api.Services
{
    public class InventoryQueryService
    {
        private readonly ApplicationDbContext _context;

        public InventoryQueryService(ApplicationDbContext context)
        {
            _context = context;
        }

        // Hämta artiklar per lagerplats
        public async Task<List<Article>> GetArticlesByLocationAsync(string storageLocation)
        {
            return await _context.Articles
                .Where(a => a.StorageLocation == storageLocation)
                .ToListAsync();
        }

        // Beräkna lagersaldo för en artikel (summerar alla StockMovements)
        public async Task<int> GetCalculatedStockAsync(int articleId)
        {
            var sum = await _context.StockMovements
                .Where(sm => sm.ArticleId == articleId)
                .SumAsync(sm => sm.Quantity);

            return sum;
        }

        // Sök och filtrera artiklar
        public async Task<List<Article>> SearchArticlesAsync(string? search, string? location)
        {
            var query = _context.Articles.AsQueryable();

            if (!string.IsNullOrEmpty(search))
                query = query.Where(a => a.Name.Contains(search) || a.Sku.Contains(search));

            if (!string.IsNullOrEmpty(location))
                query = query.Where(a => a.StorageLocation != null && 
                         a.StorageLocation.ToLower().Contains(location.ToLower())); //Flexibel sökning


            return await query.ToListAsync();
        }
    }
}
