using System.ComponentModel.DataAnnotations;

namespace OrderLagerSystem.Api.DTOs;

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
