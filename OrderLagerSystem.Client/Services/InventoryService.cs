using OrderLagerSystem.Api.DTOs;
using System.Text;
using System.Text.Json;



namespace OrderLagerSystem.Client.Services;

public class InventoryService
{
    private readonly HttpClient _http;
    private readonly AuthService _authService;
    private readonly ILogger _logger;

    public InventoryService(HttpClient http, AuthService authService, ILogger<InventoryService> logger)

    {
        _http = http;
        _authService = authService;
        _logger = logger;
    }

    public async Task<List<InventoryArticle>> SearchArticlesAsync(string? search = null, string? location = null)
    {
        var url = "api/inventory/search";
        var qp = new List<string>();
        if (!string.IsNullOrWhiteSpace(search)) qp.Add($"search={Uri.EscapeDataString(search)}");
        if (!string.IsNullOrWhiteSpace(location)) qp.Add($"location={Uri.EscapeDataString(location)}");
        if (qp.Count > 0) url += "?" + string.Join("&", qp);

        return await _http.GetFromJsonAsync<List<InventoryArticle>>(url) ?? new List<InventoryArticle>();
    }

    public async Task<List<InventoryArticle>> GetByLocationAsync(string location)
    {
        var url = $"api/inventory/by-location?location={Uri.EscapeDataString(location)}";
        return await _http.GetFromJsonAsync<List<InventoryArticle>>(url) ?? new List<InventoryArticle>();
    }

    public async Task<int> GetCalculatedStockAsync(int articleId) //Hämta lagersaldo
    {
        return await _http.GetFromJsonAsync<int>($"api/inventory/stock/{articleId}");
    }
        // Flytta artikel
    public async Task<bool> MoveArticleAsync(int articleId, string newLocation)
    {
        var response = await _http.PostAsync(
            $"api/inventory/move?articleId={articleId}&newLocation={Uri.EscapeDataString(newLocation)}",
            null);

        return response.IsSuccessStatusCode;
    }
}

public class InventoryArticle
{
    public int ArticleId { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int StockQuantity { get; set; }
    public string? StorageLocation { get; set; }
    public bool IsLowStock { get; set; }
    public string? NewLocation { get; set; }
}
