namespace OrderLagerSystem.Api.DTOs;

/// <summary>
/// Resultat från validering med information om framgång och felmeddelande
/// </summary>
public class ValidationResult
{
    /// <summary>
    /// Om valideringen lyckades
    /// </summary>
    public bool IsValid { get; }

    /// <summary>
    /// Meddelande som förklarar resultatet
    /// </summary>
    public string Message { get; }

    /// <summary>
    /// Skapar ett nytt valideringsresultat
    /// </summary>
    /// <param name="isValid">Om valideringen lyckades</param>
    /// <param name="message">Beskrivande meddelande</param>
    public ValidationResult(bool isValid, string message)
    {
        IsValid = isValid;
        Message = message;
    }
}
