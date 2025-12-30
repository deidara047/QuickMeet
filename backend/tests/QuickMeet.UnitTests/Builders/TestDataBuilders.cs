using QuickMeet.Core.Entities;

namespace QuickMeet.UnitTests.Builders;

/// <summary>
/// Builder para crear Providers en tests.
/// Simplifica la creación de datos de prueba para usuario logueado.
/// </summary>
public class ProviderBuilder
{
    private int _id = 1;
    private string _email = "test@example.com";
    private string _username = "testuser";
    private string _fullName = "Test User";
    private string _passwordHash = "hashedpassword";
    private ProviderStatus _status = ProviderStatus.Active;
    private DateTime _createdAt = DateTime.UtcNow;
    private DateTime _updatedAt = DateTime.UtcNow;

    public ProviderBuilder WithId(int id)
    {
        _id = id;
        return this;
    }

    public ProviderBuilder WithEmail(string email)
    {
        _email = email;
        return this;
    }

    public ProviderBuilder WithUsername(string username)
    {
        _username = username;
        return this;
    }

    public ProviderBuilder WithFullName(string fullName)
    {
        _fullName = fullName;
        return this;
    }

    public ProviderBuilder WithPasswordHash(string passwordHash)
    {
        _passwordHash = passwordHash;
        return this;
    }

    public ProviderBuilder WithStatus(ProviderStatus status)
    {
        _status = status;
        return this;
    }

    public ProviderBuilder WithCreatedAt(DateTime createdAt)
    {
        _createdAt = createdAt;
        return this;
    }

    public ProviderBuilder WithUpdatedAt(DateTime updatedAt)
    {
        _updatedAt = updatedAt;
        return this;
    }

    public Provider Build()
    {
        return new Provider
        {
            Id = _id,
            Email = _email,
            Username = _username,
            FullName = _fullName,
            PasswordHash = _passwordHash,
            Status = _status,
            CreatedAt = _createdAt,
            UpdatedAt = _updatedAt
        };
    }
}

/// <summary>
/// Builder para crear EmailVerificationTokens en tests.
/// </summary>
public class EmailVerificationTokenBuilder
{
    private int _id = 1;
    private int _providerId = 1;
    private string _token = "test-token-guid";
    private DateTime _expiresAt = DateTime.UtcNow.AddHours(24);
    private bool _isUsed = false;

    public EmailVerificationTokenBuilder WithId(int id)
    {
        _id = id;
        return this;
    }

    public EmailVerificationTokenBuilder WithProviderId(int providerId)
    {
        _providerId = providerId;
        return this;
    }

    public EmailVerificationTokenBuilder WithToken(string token)
    {
        _token = token;
        return this;
    }

    public EmailVerificationTokenBuilder WithExpiresAt(DateTime expiresAt)
    {
        _expiresAt = expiresAt;
        return this;
    }

    public EmailVerificationTokenBuilder WithIsUsed(bool isUsed)
    {
        _isUsed = isUsed;
        return this;
    }

    public EmailVerificationTokenBuilder AsExpired()
    {
        _expiresAt = DateTime.UtcNow.AddHours(-1);
        return this;
    }

    public EmailVerificationTokenBuilder AsUsed()
    {
        _isUsed = true;
        return this;
    }

    public EmailVerificationToken Build()
    {
        return new EmailVerificationToken
        {
            Id = _id,
            ProviderId = _providerId,
            Token = _token,
            ExpiresAt = _expiresAt,
            IsUsed = _isUsed
        };
    }
}

/// <summary>
/// Presets comunes para diferentes escenarios de tests.
/// </summary>
public static class TestDataPresets
{
    /// <summary>
    /// Provider activo típico para usuario logueado.
    /// </summary>
    public static Provider GetActiveProvider()
    {
        return new ProviderBuilder()
            .WithEmail("active@example.com")
            .WithUsername("activeuser")
            .WithFullName("Active User")
            .WithStatus(ProviderStatus.Active)
            .Build();
    }

    /// <summary>
    /// Provider suspendido para pruebas de acceso denegado.
    /// </summary>
    public static Provider GetSuspendedProvider()
    {
        return new ProviderBuilder()
            .WithId(2)
            .WithEmail("suspended@example.com")
            .WithUsername("suspendeduser")
            .WithFullName("Suspended User")
            .WithStatus(ProviderStatus.Suspended)
            .Build();
    }

    /// <summary>
    /// Provider pendiente de verificación.
    /// </summary>
    public static Provider GetPendingProvider()
    {
        return new ProviderBuilder()
            .WithId(3)
            .WithEmail("pending@example.com")
            .WithUsername("pendinguser")
            .WithFullName("Pending User")
            .WithStatus(ProviderStatus.PendingVerification)
            .Build();
    }

    /// <summary>
    /// Token de verificación válido.
    /// </summary>
    public static EmailVerificationToken GetValidToken()
    {
        return new EmailVerificationTokenBuilder()
            .WithToken(Guid.NewGuid().ToString())
            .WithExpiresAt(DateTime.UtcNow.AddHours(24))
            .Build();
    }

    /// <summary>
    /// Token de verificación expirado.
    /// </summary>
    public static EmailVerificationToken GetExpiredToken()
    {
        return new EmailVerificationTokenBuilder()
            .WithToken(Guid.NewGuid().ToString())
            .AsExpired()
            .Build();
    }

    /// <summary>
    /// Token de verificación ya usado.
    /// </summary>
    public static EmailVerificationToken GetUsedToken()
    {
        return new EmailVerificationTokenBuilder()
            .WithToken(Guid.NewGuid().ToString())
            .AsUsed()
            .Build();
    }
}
