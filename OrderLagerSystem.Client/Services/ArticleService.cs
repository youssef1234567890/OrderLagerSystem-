using OrderLagerSystem.Api.DTOs;
using System.Net.Http.Json;
using System.Text.Json;

namespace OrderLagerSystem.Client.Services;

public class ArticleService
{
    private readonly HttpClient _httpClient;
    private readonly AuthService _authService;
    private readonly JsonSerializerOptions _jsonOptions;

    public ArticleService(HttpClient httpClient, AuthService authService)
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
            throw new UnauthorizedAccessException("Du måste vara inloggad för att utföra denna åtgärd");

        _httpClient.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _authService.AuthToken);
    }

    public async Task<List<ArticleDto>?> GetArticlesAsync(bool includeInactive = false)
    {
        try
        {
            EnsureAuthToken();
            
            var url = $"api/article?includeInactive={includeInactive}";
            var response = await _httpClient.GetAsync(url);
            
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<List<ArticleDto>>(_jsonOptions);
            }
            
            // Logga felet
            Console.WriteLine($"Error fetching articles: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception in GetArticlesAsync: {ex.Message}");
            return null;
        }
    }

    public async Task<ArticleDto?> GetArticleAsync(int id)
    {
        try
        {
            EnsureAuthToken();
            
            var response = await _httpClient.GetAsync($"api/article/{id}");
            
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<ArticleDto>(_jsonOptions);
            }
            
            Console.WriteLine($"Error fetching article {id}: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception in GetArticleAsync: {ex.Message}");
            return null;
        }
    }

    public async Task<ArticleDto?> CreateArticleAsync(CreateArticleDto createDto)
    {
        try
        {
            EnsureAuthToken();
            
            var response = await _httpClient.PostAsJsonAsync("api/article", createDto, _jsonOptions);
            
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<ArticleDto>(_jsonOptions);
            }
            
            var errorContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Error creating article: {response.StatusCode} - {errorContent}");
            
            // Kan kasta exception med felmeddelandet för UI att fånga
            if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                throw new InvalidOperationException($"Kunde inte skapa artikel: {errorContent}");
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
            Console.WriteLine($"Exception in CreateArticleAsync: {ex.Message}");
            throw new InvalidOperationException($"Ett oväntat fel inträffade: {ex.Message}");
        }
    }

    public async Task<ArticleDto?> UpdateArticleAsync(int id, UpdateArticleDto updateDto)
    {
        try
        {
            EnsureAuthToken();
            
            var response = await _httpClient.PutAsJsonAsync($"api/article/{id}", updateDto, _jsonOptions);
            
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<ArticleDto>(_jsonOptions);
            }
            
            var errorContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Error updating article {id}: {response.StatusCode} - {errorContent}");
            
            if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                throw new InvalidOperationException($"Kunde inte uppdatera artikel: {errorContent}");
            }
            
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                throw new InvalidOperationException("Artikeln hittades inte");
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
            Console.WriteLine($"Exception in UpdateArticleAsync: {ex.Message}");
            throw new InvalidOperationException($"Ett oväntat fel inträffade: {ex.Message}");
        }
    }

    public async Task<bool> DeleteArticleAsync(int id)
    {
        try
        {
            EnsureAuthToken();
            
            var response = await _httpClient.DeleteAsync($"api/article/{id}");
            
            if (response.IsSuccessStatusCode)
            {
                return true;
            }
            
            var errorContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Error deleting article {id}: {response.StatusCode} - {errorContent}");
            
            if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                throw new InvalidOperationException($"Kunde inte ta bort artikel: {errorContent}");
            }
            
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                throw new InvalidOperationException("Artikeln hittades inte");
            }
            
            if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                throw new UnauthorizedAccessException("Du har inte behörighet att ta bort artiklar");
            }
            
            return false;
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
            Console.WriteLine($"Exception in DeleteArticleAsync: {ex.Message}");
            throw new InvalidOperationException($"Ett oväntat fel inträffade: {ex.Message}");
        }
    }

    public async Task<List<ArticleDto>?> GetLowStockArticlesAsync()
    {
        try
        {
            var articles = await GetArticlesAsync();
            return articles?.Where(a => a.IsLowStock && a.IsActive).ToList();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception in GetLowStockArticlesAsync: {ex.Message}");
            return null;
        }
    }

    public async Task<List<ArticleDto>?> SearchArticlesAsync(string searchTerm)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return await GetArticlesAsync();

            var articles = await GetArticlesAsync();
            if (articles == null) return null;

            var searchLower = searchTerm.ToLower();
            return articles.Where(a => 
                a.Name.ToLower().Contains(searchLower) ||
                a.Sku.ToLower().Contains(searchLower) ||
                (!string.IsNullOrEmpty(a.Description) && a.Description.ToLower().Contains(searchLower)) ||
                (!string.IsNullOrEmpty(a.StorageLocation) && a.StorageLocation.ToLower().Contains(searchLower))
            ).ToList();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception in SearchArticlesAsync: {ex.Message}");
            return null;
        }
    }
}
