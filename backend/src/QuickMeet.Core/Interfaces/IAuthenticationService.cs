using QuickMeet.Core.Entities;

namespace QuickMeet.Core.Interfaces;

public interface IAuthenticationService
{
    Task<(bool Success, string Message, AuthenticationResult? Result)> RegisterAsync(
        string email,
        string username,
        string fullName,
        string password);

    Task<(bool Success, string Message, AuthenticationResult? Result)> LoginAsync(
        string email,
        string password);

    Task<(bool Success, string Message)> VerifyEmailAsync(string token);

    Task<bool> EmailExistsAsync(string email);
    Task<bool> UsernameExistsAsync(string username);
}

public record AuthenticationResult(
    Guid ProviderId,
    string Email,
    string Username,
    string FullName,
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt
);
