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
        // Validate inputs
        if (string.IsNullOrWhiteSpace(email))
            return (false, "Email es requerido", null);

        if (string.IsNullOrWhiteSpace(username))
            return (false, "Usuario es requerido", null);

        if (string.IsNullOrWhiteSpace(fullName))
            return (false, "Nombre completo es requerido", null);

        if (string.IsNullOrWhiteSpace(password))
            return (false, "Contraseña es requerida", null);

        if (await _providerRepository.ExistsByEmailAsync(email))
            return (false, "Email ya existe", null);

        if (await _providerRepository.ExistsByUsernameAsync(username))
            return (false, "Usuario ya existe", null);

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

        return (true, "Registro exitoso. Por favor, verifica tu email.", result);
    }

    public async Task<(bool Success, string Message, AuthenticationResult? Result)> LoginAsync(
        string email,
        string password)
    {
        // Validate inputs
        if (string.IsNullOrWhiteSpace(email))
            return (false, "Email es requerido", null);

        if (string.IsNullOrWhiteSpace(password))
            return (false, "Contraseña es requerida", null);

        var provider = await _providerRepository.GetByEmailAsync(email);
        
        if (provider == null)
            return (false, "Email o contraseña inválidos", null);

        if (provider.Status != ProviderStatus.Active && provider.Status != ProviderStatus.PendingVerification)
            return (false, "Cuenta suspendida", null);

        if (!_passwordHashingService.VerifyPassword(password, provider.PasswordHash))
            return (false, "Email o contraseña inválidos", null);

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

        return (true, "Inicio de sesión exitoso", result);
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
