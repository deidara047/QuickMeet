using QuickMeet.Core.Entities;
using QuickMeet.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace QuickMeet.Core.Services;

public class AuthenticationService : IAuthenticationService
{
    private readonly IQuickMeetDbContext _dbContext;
    private readonly IPasswordHashingService _passwordHashingService;
    private readonly ITokenService _tokenService;

    public AuthenticationService(
        IQuickMeetDbContext dbContext,
        IPasswordHashingService passwordHashingService,
        ITokenService tokenService)
    {
        _dbContext = dbContext;
        _passwordHashingService = passwordHashingService;
        _tokenService = tokenService;
    }

    public async Task<(bool Success, string Message, AuthenticationResult? Result)> RegisterAsync(
        string email,
        string username,
        string fullName,
        string password)
    {
        if (await EmailExistsAsync(email))
            return (false, "Email already registered", null);

        if (await UsernameExistsAsync(username))
            return (false, "Username already taken", null);

        var passwordHash = _passwordHashingService.HashPassword(password);
        
        var provider = new Provider
        {
            Email = email,
            Username = username,
            FullName = fullName,
            PasswordHash = passwordHash,
            Status = ProviderStatus.PendingVerification
        };

        _dbContext.Providers.Add(provider);
        await _dbContext.SaveChangesAsync();

        var verificationToken = new EmailVerificationToken
        {
            ProviderId = provider.Id,
            Token = _tokenService.GenerateRefreshToken(),
            ExpiresAt = DateTimeOffset.UtcNow.AddHours(24),
            IsUsed = false
        };

        _dbContext.EmailVerificationTokens.Add(verificationToken);
        await _dbContext.SaveChangesAsync();

        var accessToken = _tokenService.GenerateAccessToken(provider.Id, provider.Email);
        var refreshToken = _tokenService.GenerateRefreshToken();

        var result = new AuthenticationResult(
            ProviderId: provider.Id,
            Email: provider.Email,
            Username: provider.Username,
            FullName: provider.FullName,
            AccessToken: accessToken,
            RefreshToken: refreshToken,
            ExpiresAt: DateTimeOffset.UtcNow.AddHours(1)
        );

        return (true, "Registration successful. Please verify your email.", result);
    }

    public async Task<(bool Success, string Message, AuthenticationResult? Result)> LoginAsync(
        string email,
        string password)
    {
        var provider = await _dbContext.Providers.FirstOrDefaultAsync(p => p.Email == email);
        
        if (provider == null)
            return (false, "Invalid email or password", null);

        if (provider.Status != ProviderStatus.Active && provider.Status != ProviderStatus.PendingVerification)
            return (false, "Account is suspended", null);

        if (!_passwordHashingService.VerifyPassword(password, provider.PasswordHash))
            return (false, "Invalid email or password", null);

        var accessToken = _tokenService.GenerateAccessToken(provider.Id, provider.Email);
        var refreshToken = _tokenService.GenerateRefreshToken();

        var result = new AuthenticationResult(
            ProviderId: provider.Id,
            Email: provider.Email,
            Username: provider.Username,
            FullName: provider.FullName,
            AccessToken: accessToken,
            RefreshToken: refreshToken,
            ExpiresAt: DateTimeOffset.UtcNow.AddHours(1)
        );

        return (true, "Login successful", result);
    }

    public async Task<(bool Success, string Message)> VerifyEmailAsync(string token)
    {
        var verificationToken = await _dbContext.EmailVerificationTokens
            .FirstOrDefaultAsync(t => t.Token == token && !t.IsUsed);

        if (verificationToken == null)
            return (false, "Invalid or expired token");

        if (verificationToken.ExpiresAt < DateTimeOffset.UtcNow)
            return (false, "Token expired");

        verificationToken.IsUsed = true;
        verificationToken.UsedAt = DateTimeOffset.UtcNow;
        
        var provider = await _dbContext.Providers.FirstOrDefaultAsync(p => p.Id == verificationToken.ProviderId);
        if (provider == null)
            return (false, "Provider not found");

        provider.Status = ProviderStatus.Active;
        provider.EmailVerifiedAt = DateTimeOffset.UtcNow;
        provider.UpdatedAt = DateTimeOffset.UtcNow;

        await _dbContext.SaveChangesAsync();

        return (true, "Email verified successfully");
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        return await _dbContext.Providers.AnyAsync(p => p.Email == email);
    }

    public async Task<bool> UsernameExistsAsync(string username)
    {
        return await _dbContext.Providers.AnyAsync(p => p.Username == username);
    }
}
