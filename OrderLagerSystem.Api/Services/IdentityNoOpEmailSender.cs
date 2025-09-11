using Microsoft.AspNetCore.Identity;
using OrderLagerSystem.Api.Data;

namespace OrderLagerSystem.Api.Services;

/// <summary>
/// Dummy email sender for development that only logs messages instead of sending real emails
/// </summary>
public class IdentityNoOpEmailSender : IEmailSender<ApplicationUser>
{
    private readonly ILogger<IdentityNoOpEmailSender> _logger;

    public IdentityNoOpEmailSender(ILogger<IdentityNoOpEmailSender> logger)
    {
        _logger = logger;
    }

    public Task SendConfirmationLinkAsync(ApplicationUser user, string email, string confirmationLink)
    {
        _logger.LogInformation("Confirmation link sent to {Email}: {ConfirmationLink}", email, confirmationLink);
        return Task.CompletedTask;
    }

    public Task SendPasswordResetLinkAsync(ApplicationUser user, string email, string resetLink)
    {
        _logger.LogInformation("Password reset link sent to {Email}: {ResetLink}", email, resetLink);
        return Task.CompletedTask;
    }

    public Task SendPasswordResetCodeAsync(ApplicationUser user, string email, string resetCode)
    {
        _logger.LogInformation("Password reset code sent to {Email}: {ResetCode}", email, resetCode);
        return Task.CompletedTask;
    }

    public Task SendEmailAsync(string toEmail, string subject, string message)
    {
        _logger.LogInformation("Email sent to {Email} with subject {Subject}: {Message}", toEmail, subject, message);
        return Task.CompletedTask;
    }
}
