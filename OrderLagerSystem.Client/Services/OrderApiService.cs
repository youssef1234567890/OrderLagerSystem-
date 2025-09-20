using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using OrderLagerSystem.Api.DTOs;

namespace OrderLagerSystem.Client.Services;

public class OrderApiService
{
    private readonly HttpClient _httpClient;
    private readonly AuthService _authService;
    private readonly ILogger<OrderApiService> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public OrderApiService(HttpClient httpClient, AuthService authService, ILogger<OrderApiService> logger)
    {
        _httpClient = httpClient;
        _authService = authService;
        _logger = logger;
    }

    private void AddAuthHeader()
    {
        var token = _authService.AuthToken;
        if (!string.IsNullOrEmpty(token))
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }
        else
        {
            _logger.LogWarning("No auth token present when calling Order API");
        }
    }

    public async Task<OrderResponse?> CreateOrderAsync(OrderCreateRequest request)
    {
        AddAuthHeader();
        var res = await _httpClient.PostAsJsonAsync("/api/order", request, JsonOptions);
        var content = await res.Content.ReadAsStringAsync();

        if (!res.IsSuccessStatusCode)
        {
            _logger.LogWarning("CreateOrder failed: {Status} {Content}", (int)res.StatusCode, content);
            return null;
        }

        if (string.IsNullOrWhiteSpace(content))
            return null;

        return JsonSerializer.Deserialize<OrderResponse>(content, JsonOptions);
    }

    public async Task<ValidationResult?> ValidateOrderAsync(int orderId)
    {
        AddAuthHeader();
        var res = await _httpClient.PostAsync($"/api/order/{orderId}/validate", null);
        var json = await res.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<ValidationResult>(json, JsonOptions);
    }

    public async Task<OrderResponse?> ConfirmOrderAsync(int orderId)
    {
        AddAuthHeader();
        var res = await _httpClient.PostAsync($"/api/order/{orderId}/confirm", null);
        var json = await res.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<OrderResponse>(json, JsonOptions);
    }

    public async Task<List<OrderResponse>?> GetOrdersAsync(string? status = null)
    {
        AddAuthHeader();
        var url = string.IsNullOrWhiteSpace(status) ? "/api/order" : $"/api/order?status={Uri.EscapeDataString(status)}";
        var res = await _httpClient.GetAsync(url);
        var json = await res.Content.ReadAsStringAsync();
        return res.IsSuccessStatusCode
            ? JsonSerializer.Deserialize<List<OrderResponse>>(json, JsonOptions)
            : new List<OrderResponse>();
    }

    public async Task<OrderResponse?> GetOrderAsync(int id)
    {
        AddAuthHeader();
        var res = await _httpClient.GetAsync($"/api/order/{id}");
        var json = await res.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<OrderResponse>(json, JsonOptions);
    }

    public async Task<OrderResponse?> CreateDeliveryAsync(int orderId, DeliveryCreateRequest request)
    {
        AddAuthHeader();
        var res = await _httpClient.PostAsJsonAsync($"/api/order/{orderId}/delivery", request, JsonOptions);
        var json = await res.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<OrderResponse>(json, JsonOptions);
    }

    public async Task<List<OrderHistoryDto>> GetOrderHistoryAsync(int? orderId = null)
    {
        AddAuthHeader();
        var url = orderId.HasValue ? $"/api/order/{orderId.Value}/history" : "/api/order/history";
        var res = await _httpClient.GetAsync(url);
        var json = await res.Content.ReadAsStringAsync();
        return res.IsSuccessStatusCode
            ? JsonSerializer.Deserialize<List<OrderHistoryDto>>(json, JsonOptions) ?? new()
            : new();
    }

    public async Task<List<OrderResponse>?> GetPendingDeliveryOrdersAsync()
    {
        AddAuthHeader();
        var res = await _httpClient.GetAsync("/api/order/pending-delivery");
        var json = await res.Content.ReadAsStringAsync();
        return res.IsSuccessStatusCode
            ? JsonSerializer.Deserialize<List<OrderResponse>>(json, JsonOptions)
            : new List<OrderResponse>();
    }

    public async Task<bool> DeliverOrderAsync(int orderId, string? comment = null)
    {
        AddAuthHeader();
        var request = new OrderStatusUpdateRequest { Comment = comment };
        var res = await _httpClient.PostAsJsonAsync($"/api/order/{orderId}/deliver", request, JsonOptions);
        return res.IsSuccessStatusCode;
    }
}