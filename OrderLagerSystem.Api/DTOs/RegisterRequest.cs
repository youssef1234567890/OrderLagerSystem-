using System.ComponentModel.DataAnnotations;

namespace OrderLagerSystem.Api.DTOs;

/// <summary>
/// Request-modell för registrering av nya användare (endast admin)
/// </summary>
public class RegisterRequest
{
    /// <summary>
    /// Användarnamn för den nya användaren
    /// </summary>
    [Required(ErrorMessage = "Användarnamn är obligatoriskt")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Användarnamn måste vara mellan 3-50 tecken")]
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// E-postadress för den nya användaren
    /// </summary>
    [Required(ErrorMessage = "E-post är obligatorisk")]
    [EmailAddress(ErrorMessage = "Ogiltig e-postadress")]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Lösenord för den nya användaren
    /// </summary>
    [Required(ErrorMessage = "Lösenord är obligatoriskt")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Lösenord måste vara minst 6 tecken")]
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Förnamn
    /// </summary>
    [Required(ErrorMessage = "Förnamn är obligatoriskt")]
    [StringLength(50, ErrorMessage = "Förnamn får vara max 50 tecken")]
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Efternamn
    /// </summary>
    [Required(ErrorMessage = "Efternamn är obligatoriskt")]
    [StringLength(50, ErrorMessage = "Efternamn får vara max 50 tecken")]
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Roller att tilldela användaren (t.ex. "Admin", "User")
    /// </summary>
    public List<string> Roles { get; set; } = new List<string> { "User" };
}
