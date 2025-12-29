using Microsoft.Extensions.Logging;
using QuickMeet.Core.Interfaces;

namespace QuickMeet.Core.Services;

public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;

    public EmailService(ILogger<EmailService> logger)
    {
        _logger = logger;
    }

    public Task SendEmailVerificationAsync(string email, string username, string token)
    {
        var verificationUrl = $"https://quickmeet.app/verify?token={token}";
        _logger.LogInformation("Email verification for {Email}: {Url}", email, verificationUrl);
        
        // TODO: Implement actual SMTP integration
        return Task.CompletedTask;
    }

    public Task SendWelcomeEmailAsync(string email, string fullName)
    {
        _logger.LogInformation("Welcome email for {Email}", email);
        
        // TODO: Implement actual SMTP integration
        return Task.CompletedTask;
    }
}
