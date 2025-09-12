using OrderLagerSystem.Client.DTOs;

namespace OrderLagerSystem.Client.Services;

/// <summary>
/// Service for managing authentication state in Blazor Server
/// Works better than localStorage for server-side rendering
/// </summary>
public class AuthService
{
    private UserInfo? _currentUser;
    private string? _authToken;

    /// <summary>
    /// Event that triggers when authentication state changes
    /// </summary>
    public event Action? OnAuthStateChanged;

    /// <summary>
    /// Current logged in user (null if not logged in)
    /// </summary>
    public UserInfo? CurrentUser => _currentUser;

    /// <summary>
    /// JWT authentication token (null if not logged in)
    /// </summary>
    public string? AuthToken => _authToken;

    /// <summary>
    /// Whether the user is authenticated
    /// </summary>
    public bool IsAuthenticated => _currentUser != null && !string.IsNullOrEmpty(_authToken);

    /// <summary>
    /// Logs in the user and saves authentication data
    /// </summary>
    /// <param name="token">JWT token from API</param>
    /// <param name="user">User information</param>
    public void Login(string token, UserInfo user)
    {
        _authToken = token;
        _currentUser = user;
        OnAuthStateChanged?.Invoke();
    }

    /// <summary>
    /// Logs out the user and clears authentication data
    /// </summary>
    public void Logout()
    {
        _authToken = null;
        _currentUser = null;
        OnAuthStateChanged?.Invoke();
    }
}
