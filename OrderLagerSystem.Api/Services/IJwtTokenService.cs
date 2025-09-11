using OrderLagerSystem.Api.Data;

namespace OrderLagerSystem.Api.Services;

public interface IJwtTokenService
{
    Task<string> GenerateTokenAsync(ApplicationUser user);
    Task<DateTime> GetTokenExpirationAsync();
}
