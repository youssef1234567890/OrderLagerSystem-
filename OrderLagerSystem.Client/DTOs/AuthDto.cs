using System.ComponentModel.DataAnnotations;

namespace OrderLagerSystem.Client.DTOs;

/// <summary>
/// Request-modell för användarinloggning
/// </summary>
public class LoginRequest
{
    /// <summary>
    /// Användarnamn eller e-postadress för inloggning
    /// </summary>
    [Required(ErrorMessage = "Användarnamn eller e-post är obligatoriskt")]
    public string UsernameOrEmail { get; set; } = string.Empty;

    /// <summary>
    /// Lösenord för autentisering
    /// </summary>
    [Required(ErrorMessage = "Lösenord är obligatoriskt")]
    [MinLength(6, ErrorMessage = "Lösenord måste vara minst 6 tecken")]
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Om användaren vill bli ihågkommen för längre sessioner
    /// </summary>
    public bool RememberMe { get; set; } = false;
}

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

/// <summary>
/// Användarinformation som delas mellan API och Client
/// </summary>
public class UserInfo
{
    /// <summary>
    /// Unikt användar-ID från databasen
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Användarnamn för inloggning
    /// </summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// E-postadress för användaren
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Förnamn
    /// </summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Efternamn
    /// </summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Fullständigt namn för visning
    /// </summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// Om användarkontot är aktivt
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Lista över användarens roller (Admin, Orderkoordinator, Employee)
    /// </summary>
    public List<string> Roles { get; set; } = new();
}
