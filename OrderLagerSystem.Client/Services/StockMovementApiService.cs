// System imports first
using System.Text;
using System.Text.Json;

// Project imports
using OrderLagerSystem.Api.DTOs;
using OrderLagerSystem.Client.Services;

namespace OrderLagerSystem.Client.Services;

/// <summary>
/// Frontend service for communicating with Stock Movement API
/// Handles HTTP requests to backend StockMovementController
/// </summary>
public class StockMovementApiService
{
    private readonly HttpClient _httpClient;
    private readonly AuthService _authService;
    private readonly ILogger<StockMovementApiService> _logger;
    
    // Gemensam JSON-serializer för alla metoder
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public StockMovementApiService(
        HttpClient httpClient, 
        AuthService authService,
        ILogger<StockMovementApiService> logger)
    {
        _httpClient = httpClient;
        _authService = authService;
        _logger = logger;
    }

    /// <summary>
    /// Add JWT token to HTTP request headers
    /// </summary>
    private void AddAuthHeader()
    {
        if (!string.IsNullOrEmpty(_authService.AuthToken))
        {
            _httpClient.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _authService.AuthToken);
            
            _logger.LogInformation("JWT token added to request headers. Token length: {TokenLength}", 
                _authService.AuthToken.Length);
        }
        else
        {
            _logger.LogWarning("No JWT token available in AuthService");
        }
    }

    /// <summary>
    /// Get article information by SKU or ID
    /// </summary>
    public async Task<ArticleInfo?> GetArticleInfoAsync(string identifier)
    {
        try
        {
            AddAuthHeader();
            var response = await _httpClient.GetAsync($"/api/stockmovement/article/{identifier}");
            
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<ArticleInfo>(json, JsonOptions);
            }
            
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting article info for identifier: {Identifier}", identifier);
            return null;
        }
    }

    /// <summary>
    /// Create a new purchase order
    /// </summary>
    public async Task<SimplePurchaseResponse?> CreatePurchaseOrderAsync(SimplePurchaseRequest request)
    {
        try
        {
            AddAuthHeader();
            var json = JsonSerializer.Serialize(request, JsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PostAsync("/api/stockmovement/purchase", content);
            var responseJson = await response.Content.ReadAsStringAsync();
            
            if (response.IsSuccessStatusCode)
            {
                return JsonSerializer.Deserialize<SimplePurchaseResponse>(responseJson, JsonOptions);
            }
            
            return JsonSerializer.Deserialize<SimplePurchaseResponse>(responseJson, JsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating purchase order");
            return new SimplePurchaseResponse 
            { 
                Success = false, 
                Message = "Error creating purchase order",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    /// <summary>
    /// Get all purchase orders (both pending and received)
    /// </summary>
    public async Task<List<PendingPurchaseOrder>?> GetPurchaseOrdersAsync()
    {
        try
        {
            AddAuthHeader();
            var response = await _httpClient.GetAsync("/api/stockmovement/pending-purchases");
            
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };
                
                return JsonSerializer.Deserialize<List<PendingPurchaseOrder>>(json, options);
            }
            
            return new List<PendingPurchaseOrder>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting purchase orders");
            return new List<PendingPurchaseOrder>();
        }
    }

    /// <summary>
    /// Receive goods into warehouse
    /// </summary>
    public async Task<GoodsReceiptResponse?> ReceiveGoodsAsync(GoodsReceiptRequest request)
    {
        try
        {
            AddAuthHeader();
            var json = JsonSerializer.Serialize(request, JsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PostAsync("/api/stockmovement/receipt", content);
            var responseJson = await response.Content.ReadAsStringAsync();
            
            if (response.IsSuccessStatusCode)
            {
                return JsonSerializer.Deserialize<GoodsReceiptResponse>(responseJson, JsonOptions);
            }
            
            return JsonSerializer.Deserialize<GoodsReceiptResponse>(responseJson, JsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error receiving goods");
            return new GoodsReceiptResponse 
            { 
                Success = false, 
                Message = "Error receiving goods",
                Errors = new List<string> { ex.Message }
            };
        }
    }
}