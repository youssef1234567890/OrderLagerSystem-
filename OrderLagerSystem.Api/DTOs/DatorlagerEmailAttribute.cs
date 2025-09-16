using System.ComponentModel.DataAnnotations;

namespace OrderLagerSystem.Api.DTOs;

/// <summary>
/// Custom validation attribute to ensure email ends with @datorlager.se
/// </summary>
public class DatorlagerEmailAttribute : ValidationAttribute
{
    private const string RequiredDomain = "@datorlager.se";

    public DatorlagerEmailAttribute()
    {
        ErrorMessage = $"Email must end with {RequiredDomain}";
    }

    public override bool IsValid(object? value)
    {
        if (value == null) return true; // Let Required attribute handle null values

        var email = value.ToString();
        if (string.IsNullOrEmpty(email)) return true;

        return email.EndsWith(RequiredDomain, StringComparison.OrdinalIgnoreCase);
    }
}
