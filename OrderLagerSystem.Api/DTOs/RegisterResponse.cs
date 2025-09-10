namespace OrderLagerSystem.Api.DTOs;

/// <summary>
/// Response-modell för användarregistrering
/// </summary>
public class RegisterResponse
{
    /// <summary>
    /// Om registreringen lyckades
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Meddelande om resultatet
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Information om den skapade användaren (om lyckad)
    /// </summary>
    public UserInfo? User { get; set; }

    /// <summary>
    /// Lista över eventuella fel
    /// </summary>
    public List<string> Errors { get; set; } = new List<string>();

    /// <summary>
    /// Skapar en lyckad registrering
    /// </summary>
    public static RegisterResponse CreateSuccess(UserInfo user, string message = "Användaren skapades framgångsrikt")
    {
        return new RegisterResponse
        {
            Success = true,
            Message = message,
            User = user
        };
    }

    /// <summary>
    /// Skapar en misslyckad registrering
    /// </summary>
    public static RegisterResponse CreateFailure(string message, List<string>? errors = null)
    {
        return new RegisterResponse
        {
            Success = false,
            Message = message,
            Errors = errors ?? new List<string>()
        };
    }
}
