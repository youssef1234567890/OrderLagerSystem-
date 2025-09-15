namespace OrderLagerSystem.Api.DTOs;

public class BarcodeScanResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty; 
    public List<string>? Errors { get; set; }
    public ArticleInfo? Article { get; set; }
}