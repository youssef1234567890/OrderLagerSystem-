using OrderLagerSystem.Api.DTOs;
using System.Text.Json;
using System.Net.Http.Json;

namespace OrderLagerSystem.Client.Services;

public class BarcodeService
{
    private readonly HttpClient _httpClient;
    private readonly AuthService _authService;
    private readonly JsonSerializerOptions _jsonOptions;

    public BarcodeService(HttpClient httpClient, AuthService authService)
    {
        _httpClient = httpClient;
        _authService = authService;
        
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };
    }

    private void EnsureAuthToken()
    {
        if (!_authService.IsAuthenticated)
            throw new UnauthorizedAccessException("Du måste vara inloggad för att generera streckkoder");

        _httpClient.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _authService.AuthToken);
    }

    public async Task<BarcodeResponse?> GenerateBarcodeAsync(BarcodeGenerateRequest request)
    {
        try
        {
            EnsureAuthToken();
            
            var response = await _httpClient.PostAsJsonAsync("api/barcode/generate", request, _jsonOptions);
            
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<BarcodeResponse>(_jsonOptions);
            }
            
            var errorContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Error generating barcode: {response.StatusCode} - {errorContent}");
            
            if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                throw new InvalidOperationException($"Kunde inte generera sträckod: {errorContent}");
            }
            
            return null;
        }
        catch (UnauthorizedAccessException)
        {
            throw;
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception in GenerateBarcodeAsync: {ex.Message}");
            throw new InvalidOperationException($"Ett oväntat fel inträffade: {ex.Message}");
        }
    }

    public async Task<BarcodeResponse?> GenerateArticleBarcodeAsync(
        int articleId, 
        BarcodeFormat format = BarcodeFormat.Code128, 
        int width = 300, 
        int height = 100, 
        bool showText = true)
    {
        try
        {
            EnsureAuthToken();
            
            var url = $"api/barcode/article/{articleId}?format={format}&width={width}&height={height}&showText={showText}";
            var response = await _httpClient.GetAsync(url);
            
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<BarcodeResponse>(_jsonOptions);
            }
            
            var errorContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Error generating article barcode: {response.StatusCode} - {errorContent}");
            
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                throw new InvalidOperationException("Artikeln hittades inte");
            }
            
            if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                throw new InvalidOperationException($"Kunde inte generera sträckod: {errorContent}");
            }
            
            return null;
        }
        catch (UnauthorizedAccessException)
        {
            throw;
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception in GenerateArticleBarcodeAsync: {ex.Message}");
            throw new InvalidOperationException($"Ett oväntat fel inträffade: {ex.Message}");
        }
    }

    public async Task<BarcodeResponse?> GenerateArticleBarcodeBySKUAsync(
        string sku, 
        BarcodeFormat format = BarcodeFormat.Code128, 
        int width = 300, 
        int height = 100, 
        bool showText = true)
    {
        try
        {
            EnsureAuthToken();
            
            var encodedSku = Uri.EscapeDataString(sku);
            var url = $"api/barcode/article/sku/{encodedSku}?format={format}&width={width}&height={height}&showText={showText}";
            var response = await _httpClient.GetAsync(url);
            
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<BarcodeResponse>(_jsonOptions);
            }
            
            var errorContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Error generating article barcode by SKU: {response.StatusCode} - {errorContent}");
            
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                throw new InvalidOperationException($"Artikel med SKU {sku} hittades inte");
            }
            
            if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                throw new InvalidOperationException($"Kunde inte generera sträckod: {errorContent}");
            }
            
            return null;
        }
        catch (UnauthorizedAccessException)
        {
            throw;
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception in GenerateArticleBarcodeBySKUAsync: {ex.Message}");
            throw new InvalidOperationException($"Ett oväntat fel inträffade: {ex.Message}");
        }
    }

    public string GetArticleBarcodeImageUrl(
        int articleId, 
        BarcodeFormat format = BarcodeFormat.Code128, 
        int width = 300, 
        int height = 100, 
        bool showText = true)
    {
        if (!_authService.IsAuthenticated)
            throw new UnauthorizedAccessException("Du måste vara inloggad för att visa streckkoder");

        var baseUrl = _httpClient.BaseAddress?.ToString().TrimEnd('/') ?? "http://localhost:5265";
        return $"{baseUrl}/api/barcode/article/{articleId}/image?format={format}&width={width}&height={height}&showText={showText}";
    }

    public async Task<object?> ValidateTextForFormatAsync(string text, BarcodeFormat format)
    {
        try
        {
            EnsureAuthToken();
            
            var request = new { text = text, format = format.ToString() };
            var response = await _httpClient.PostAsJsonAsync("api/barcode/validate", request, _jsonOptions);
            
            if (response.IsSuccessStatusCode)
            {
                var jsonString = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<object>(jsonString, _jsonOptions);
            }
            
            var errorContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Error validating barcode text: {response.StatusCode} - {errorContent}");
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception in ValidateTextForFormatAsync: {ex.Message}");
            return null;
        }
    }

    public async Task<object?> GetAvailableFormatsAsync()
    {
        try
        {
            EnsureAuthToken();
            
            var response = await _httpClient.GetAsync("api/barcode/formats");
            
            if (response.IsSuccessStatusCode)
            {
                var jsonString = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<object>(jsonString, _jsonOptions);
            }
            
            var errorContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Error fetching barcode formats: {response.StatusCode} - {errorContent}");
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception in GetAvailableFormatsAsync: {ex.Message}");
            return null;
        }
    }
}
