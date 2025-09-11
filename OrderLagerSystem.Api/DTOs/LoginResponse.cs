namespace OrderLagerSystem.Api.DTOs;

/// <summary>
/// Response-modell för login-resultat
/// </summary>
public class LoginResponse
{
    /// <summary>
    /// Om inloggningen lyckades
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Meddelande till användaren (felmeddelande eller välkomsttext)
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// JWT token för autentisering (endast vid lyckad inloggning)
    /// </summary>
    public string? Token { get; set; }

    /// <summary>
    /// När token går ut
    /// </summary>
    public DateTime? ExpiresAt { get; set; }

    /// <summary>
    /// Användarinformation (endast vid lyckad inloggning)
    /// </summary>
    public UserInfo? User { get; set; }
}
