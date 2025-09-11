namespace OrderLagerSystem.Api.DTOs;

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
