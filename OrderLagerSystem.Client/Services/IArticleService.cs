using OrderLagerSystem.Api.DTOs;

namespace OrderLagerSystem.Client.Services;

public interface IArticleService
{
    Task<List<ArticleDto>?> GetArticlesAsync(bool includeInactive = false);
    Task<ArticleDto?> GetArticleAsync(int id);
    Task<ArticleDto?> CreateArticleAsync(CreateArticleDto createDto);
    Task<ArticleDto?> UpdateArticleAsync(int id, UpdateArticleDto updateDto);
    Task<bool> DeleteArticleAsync(int id);
    Task<List<ArticleDto>?> GetLowStockArticlesAsync();
    Task<List<ArticleDto>?> SearchArticlesAsync(string searchTerm);
}
