using QuickMeet.Core.Entities;
using QuickMeet.Core.Interfaces;

namespace QuickMeet.Core.Services;

public class AuthenticationService : IAuthenticationService
{
    private readonly IProviderRepository _providerRepository;
    private readonly IPasswordHashingService _passwordHashingService;
    private readonly ITokenService _tokenService;

    public AuthenticationService(
        IProviderRepository providerRepository,
        IPasswordHashingService passwordHashingService,
        ITokenService tokenService)
    {
        _providerRepository = providerRepository;
        _passwordHashingService = passwordHashingService;
        _tokenService = tokenService;
    }

    public async Task<(bool Success, string Message, AuthenticationResult? Result)> RegisterAsync(
        string email,
        string username,
        string fullName,
        string password)
    {
        if (await _providerRepository.ExistsByEmailAsync(email))
            return (false, "Email already registered", null);

        if (await _providerRepository.ExistsByUsernameAsync(username))
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

        await _providerRepository.AddAsync(provider);

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
        var provider = await _providerRepository.GetByEmailAsync(email);
        
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
        // TODO: Implementar verificación de email
        // Requiere acceso a EmailVerificationTokens - crear IEmailVerificationTokenRepository
        throw new NotImplementedException("Email verification requires token repository");
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        return await _providerRepository.ExistsByEmailAsync(email);
    }

    public async Task<bool> UsernameExistsAsync(string username)
    {
        return await _providerRepository.ExistsByUsernameAsync(username);
    }
}
