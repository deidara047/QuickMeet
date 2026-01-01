using Xunit;
using Moq;
using QuickMeet.Core.Services;
using QuickMeet.Core.Interfaces;
using QuickMeet.Core.Entities;

namespace QuickMeet.UnitTests.Services;

/// <summary>
/// Unit Tests para AuthenticationService
/// Prueba la lógica de negocio sin dependencias externas (BD, email, etc)
/// Usa mocks puros para aislar el comportamiento del servicio
/// </summary>
public class AuthenticationServiceTests
{
    #region Test Data Constants
    
    private const string ValidEmail = "test@example.com";
    private const string ValidUsername = "testuser";
    private const string ValidFullName = "Test User";
    private const string ValidPassword = "ValidPassword123!@";
    private const string HashedPassword = "hashed_password";
    private const string AccessToken = "access_token";
    private const string RefreshToken = "refresh_token";
    
    #endregion

    private readonly Mock<IProviderRepository> _mockProviderRepository;
    private readonly Mock<IPasswordHashingService> _mockPasswordHasher;
    private readonly Mock<ITokenService> _mockTokenService;
    private readonly AuthenticationService _service;

    public AuthenticationServiceTests()
    {
        _mockProviderRepository = new Mock<IProviderRepository>();
        _mockPasswordHasher = new Mock<IPasswordHashingService>();
        _mockTokenService = new Mock<ITokenService>();

        _service = new AuthenticationService(
            _mockProviderRepository.Object,
            _mockPasswordHasher.Object,
            _mockTokenService.Object
        );
    }

    #region Register Tests - Happy Path

    [Fact]
    public async Task RegisterAsync_ValidInput_ReturnsSuccess()
    {
        // Arrange
        _mockProviderRepository.Setup(r => r.ExistsByEmailAsync(ValidEmail))
            .ReturnsAsync(false);
        _mockProviderRepository.Setup(r => r.ExistsByUsernameAsync(ValidUsername))
            .ReturnsAsync(false);
        _mockPasswordHasher.Setup(h => h.HashPassword(ValidPassword))
            .Returns(HashedPassword);
        _mockTokenService.Setup(t => t.GenerateAccessToken(It.IsAny<int>(), It.IsAny<string>()))
            .Returns(AccessToken);
        _mockTokenService.Setup(t => t.GenerateRefreshToken())
            .Returns(RefreshToken);

        // Act
        var result = await _service.RegisterAsync(ValidEmail, ValidUsername, ValidFullName, ValidPassword);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Result);
        Assert.Equal(ValidEmail, result.Result.Email);
        Assert.Equal(ValidUsername, result.Result.Username);
        Assert.Equal(ValidFullName, result.Result.FullName);
        Assert.Equal(AccessToken, result.Result.AccessToken);
        Assert.Equal(RefreshToken, result.Result.RefreshToken);
    }

    [Fact]
    public async Task RegisterAsync_ValidInput_HashesPassword()
    {
        // Arrange
        _mockProviderRepository.Setup(r => r.ExistsByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync(false);
        _mockProviderRepository.Setup(r => r.ExistsByUsernameAsync(It.IsAny<string>()))
            .ReturnsAsync(false);
        _mockPasswordHasher.Setup(h => h.HashPassword(ValidPassword))
            .Returns(HashedPassword);
        _mockTokenService.Setup(t => t.GenerateAccessToken(It.IsAny<int>(), It.IsAny<string>()))
            .Returns(AccessToken);
        _mockTokenService.Setup(t => t.GenerateRefreshToken())
            .Returns(RefreshToken);

        // Act
        await _service.RegisterAsync(ValidEmail, ValidUsername, ValidFullName, ValidPassword);

        // Assert
        _mockPasswordHasher.Verify(
            h => h.HashPassword(ValidPassword),
            Times.Once,
            "Password should be hashed before storing"
        );
    }

    [Fact]
    public async Task RegisterAsync_ValidInput_SavesProvider()
    {
        // Arrange
        _mockProviderRepository.Setup(r => r.ExistsByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync(false);
        _mockProviderRepository.Setup(r => r.ExistsByUsernameAsync(It.IsAny<string>()))
            .ReturnsAsync(false);
        _mockPasswordHasher.Setup(h => h.HashPassword(It.IsAny<string>()))
            .Returns(HashedPassword);
        _mockTokenService.Setup(t => t.GenerateAccessToken(It.IsAny<int>(), It.IsAny<string>()))
            .Returns(AccessToken);
        _mockTokenService.Setup(t => t.GenerateRefreshToken())
            .Returns(RefreshToken);

        // Act
        await _service.RegisterAsync(ValidEmail, ValidUsername, ValidFullName, ValidPassword);

        // Assert
        _mockProviderRepository.Verify(
            r => r.AddAsync(It.Is<Provider>(p =>
                p.Email == ValidEmail &&
                p.Username == ValidUsername &&
                p.FullName == ValidFullName &&
                p.Status == ProviderStatus.PendingVerification
            )),
            Times.Once,
            "Provider should be saved with PendingVerification status"
        );
    }

    #endregion

    #region Register Tests - Duplicates

    [Fact]
    public async Task RegisterAsync_DuplicateEmail_ReturnsFailure()
    {
        // Arrange
        _mockProviderRepository.Setup(r => r.ExistsByEmailAsync(ValidEmail))
            .ReturnsAsync(true);

        // Act
        var result = await _service.RegisterAsync(ValidEmail, ValidUsername, ValidFullName, ValidPassword);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Email ya existe", result.Message);
        Assert.Null(result.Result);
    }

    [Fact]
    public async Task RegisterAsync_DuplicateUsername_ReturnsFailure()
    {
        // Arrange
        _mockProviderRepository.Setup(r => r.ExistsByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync(false);
        _mockProviderRepository.Setup(r => r.ExistsByUsernameAsync(ValidUsername))
            .ReturnsAsync(true);

        // Act
        var result = await _service.RegisterAsync(ValidEmail, ValidUsername, ValidFullName, ValidPassword);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Usuario ya existe", result.Message);
        Assert.Null(result.Result);
    }

    #endregion

    #region Register Tests - Edge Cases

    [Theory]
    [InlineData(null, "user", "Name", "Pass123!@")]
    [InlineData("", "user", "Name", "Pass123!@")]
    [InlineData("   ", "user", "Name", "Pass123!@")]
    public async Task RegisterAsync_NullOrEmptyEmail_ReturnsFailure(
        string email, string username, string fullName, string password)
    {
        // Act
        var result = await _service.RegisterAsync(email, username, fullName, password);

        // Assert
        Assert.False(result.Success);
        
        // No debe llamar al repositorio con email inválido
        _mockProviderRepository.Verify(
            r => r.ExistsByEmailAsync(It.IsAny<string>()),
            Times.Never,
            "Should not check repository for null/empty email"
        );
    }

    [Theory]
    [InlineData("email@test.com", null, "Name", "Pass123!@")]
    [InlineData("email@test.com", "", "Name", "Pass123!@")]
    [InlineData("email@test.com", "   ", "Name", "Pass123!@")]
    public async Task RegisterAsync_NullOrEmptyUsername_ReturnsFailure(
        string email, string username, string fullName, string password)
    {
        // Act
        var result = await _service.RegisterAsync(email, username, fullName, password);

        // Assert
        Assert.False(result.Success);
        
        // No debe llamar al repositorio con username inválido
        _mockProviderRepository.Verify(
            r => r.ExistsByUsernameAsync(It.IsAny<string>()),
            Times.Never,
            "Should not check repository for null/empty username"
        );
    }

    #endregion

    #region Register Tests - Operation Order

    [Fact]
    public async Task RegisterAsync_DuplicateEmail_DoesNotHashPassword()
    {
        // Arrange
        _mockProviderRepository.Setup(r => r.ExistsByEmailAsync(ValidEmail))
            .ReturnsAsync(true);

        // Act
        await _service.RegisterAsync(ValidEmail, ValidUsername, ValidFullName, ValidPassword);

        // Assert - NO debe intentar hashear si el email ya existe
        _mockPasswordHasher.Verify(
            h => h.HashPassword(It.IsAny<string>()),
            Times.Never,
            "Should not hash password if email already exists"
        );
    }

    [Fact]
    public async Task RegisterAsync_DuplicateEmail_DoesNotCallRepositoryAdd()
    {
        // Arrange
        _mockProviderRepository.Setup(r => r.ExistsByEmailAsync(ValidEmail))
            .ReturnsAsync(true);

        // Act
        await _service.RegisterAsync(ValidEmail, ValidUsername, ValidFullName, ValidPassword);

        // Assert - NO debe intentar guardar si el email ya existe
        _mockProviderRepository.Verify(
            r => r.AddAsync(It.IsAny<Provider>()),
            Times.Never,
            "Should not save provider if email already exists"
        );
    }

    [Fact]
    public async Task RegisterAsync_DuplicateUsername_DoesNotHashPassword()
    {
        // Arrange
        _mockProviderRepository.Setup(r => r.ExistsByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync(false);
        _mockProviderRepository.Setup(r => r.ExistsByUsernameAsync(ValidUsername))
            .ReturnsAsync(true);

        // Act
        await _service.RegisterAsync(ValidEmail, ValidUsername, ValidFullName, ValidPassword);

        // Assert - NO debe hashear si el username ya existe
        _mockPasswordHasher.Verify(
            h => h.HashPassword(It.IsAny<string>()),
            Times.Never,
            "Should not hash password if username already exists"
        );
    }

    #endregion

    #region Login Tests - Happy Path

    [Fact]
    public async Task LoginAsync_ValidCredentials_ReturnsSuccess()
    {
        // Arrange
        var provider = new Provider
        {
            Id = 1,
            Email = ValidEmail,
            Username = ValidUsername,
            FullName = ValidFullName,
            PasswordHash = HashedPassword,
            Status = ProviderStatus.Active
        };

        _mockProviderRepository.Setup(r => r.GetByEmailAsync(ValidEmail))
            .ReturnsAsync(provider);
        _mockPasswordHasher.Setup(h => h.VerifyPassword(ValidPassword, HashedPassword))
            .Returns(true);
        _mockTokenService.Setup(t => t.GenerateAccessToken(provider.Id, provider.Email))
            .Returns(AccessToken);
        _mockTokenService.Setup(t => t.GenerateRefreshToken())
            .Returns(RefreshToken);

        // Act
        var result = await _service.LoginAsync(ValidEmail, ValidPassword);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Result);
        Assert.Equal(ValidEmail, result.Result.Email);
        Assert.Equal(ValidUsername, result.Result.Username);
        Assert.Equal(AccessToken, result.Result.AccessToken);
        Assert.Equal(RefreshToken, result.Result.RefreshToken);
    }

    [Fact]
    public async Task LoginAsync_ValidCredentials_GeneratesTokens()
    {
        // Arrange
        var provider = new Provider
        {
            Id = 1,
            Email = ValidEmail,
            Username = ValidUsername,
            FullName = ValidFullName,
            PasswordHash = HashedPassword,
            Status = ProviderStatus.Active
        };

        _mockProviderRepository.Setup(r => r.GetByEmailAsync(ValidEmail))
            .ReturnsAsync(provider);
        _mockPasswordHasher.Setup(h => h.VerifyPassword(ValidPassword, HashedPassword))
            .Returns(true);
        _mockTokenService.Setup(t => t.GenerateAccessToken(provider.Id, provider.Email))
            .Returns(AccessToken);
        _mockTokenService.Setup(t => t.GenerateRefreshToken())
            .Returns(RefreshToken);

        // Act
        await _service.LoginAsync(ValidEmail, ValidPassword);

        // Assert
        _mockTokenService.Verify(
            t => t.GenerateAccessToken(provider.Id, provider.Email),
            Times.Once
        );
        _mockTokenService.Verify(
            t => t.GenerateRefreshToken(),
            Times.Once
        );
    }

    #endregion

    #region Login Tests - Failures

    [Fact]
    public async Task LoginAsync_NonExistentUser_ReturnsFailure()
    {
        // Arrange
        _mockProviderRepository.Setup(r => r.GetByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((Provider?)null);

        // Act
        var result = await _service.LoginAsync(ValidEmail, ValidPassword);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Invalid email or password", result.Message);
        Assert.Null(result.Result);
    }

    [Fact]
    public async Task LoginAsync_IncorrectPassword_ReturnsFailure()
    {
        // Arrange
        var provider = new Provider
        {
            Id = 2,
            Email = ValidEmail,
            PasswordHash = HashedPassword,
            Status = ProviderStatus.Active
        };

        _mockProviderRepository.Setup(r => r.GetByEmailAsync(ValidEmail))
            .ReturnsAsync(provider);
        _mockPasswordHasher.Setup(h => h.VerifyPassword("WrongPassword123!@", HashedPassword))
            .Returns(false);

        // Act
        var result = await _service.LoginAsync(ValidEmail, "WrongPassword123!@");

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Invalid email or password", result.Message);
    }

    [Fact]
    public async Task LoginAsync_SuspendedAccount_ReturnsFailure()
    {
        // Arrange
        var provider = new Provider
        {
            Id = 3,
            Email = ValidEmail,
            PasswordHash = HashedPassword,
            Status = ProviderStatus.Suspended
        };

        _mockProviderRepository.Setup(r => r.GetByEmailAsync(ValidEmail))
            .ReturnsAsync(provider);
        _mockPasswordHasher.Setup(h => h.VerifyPassword(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(true);

        // Act
        var result = await _service.LoginAsync(ValidEmail, ValidPassword);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Account is suspended", result.Message);
    }

    [Fact]
    public async Task LoginAsync_PendingVerificationAccount_ReturnsSuccess()
    {
        // Arrange
        var provider = new Provider
        {
            Id = 4,
            Email = ValidEmail,
            Username = ValidUsername,
            FullName = ValidFullName,
            PasswordHash = HashedPassword,
            Status = ProviderStatus.PendingVerification
        };

        _mockProviderRepository.Setup(r => r.GetByEmailAsync(ValidEmail))
            .ReturnsAsync(provider);
        _mockPasswordHasher.Setup(h => h.VerifyPassword(ValidPassword, HashedPassword))
            .Returns(true);
        _mockTokenService.Setup(t => t.GenerateAccessToken(provider.Id, provider.Email))
            .Returns(AccessToken);
        _mockTokenService.Setup(t => t.GenerateRefreshToken())
            .Returns(RefreshToken);

        // Act
        var result = await _service.LoginAsync(ValidEmail, ValidPassword);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Result);
    }

    #endregion

    #region Login Tests - Operation Order

    [Fact]
    public async Task LoginAsync_IncorrectPassword_DoesNotGenerateTokens()
    {
        // Arrange
        var provider = new Provider
        {
            Id = 5,
            Email = ValidEmail,
            PasswordHash = HashedPassword,
            Status = ProviderStatus.Active
        };

        _mockProviderRepository.Setup(r => r.GetByEmailAsync(ValidEmail))
            .ReturnsAsync(provider);
        _mockPasswordHasher.Setup(h => h.VerifyPassword("WrongPassword", HashedPassword))
            .Returns(false);

        // Act
        await _service.LoginAsync(ValidEmail, "WrongPassword");

        // Assert - NO debe generar tokens con password incorrecto
        _mockTokenService.Verify(
            t => t.GenerateAccessToken(It.IsAny<int>(), It.IsAny<string>()),
            Times.Never,
            "Should not generate access token with wrong password"
        );
        _mockTokenService.Verify(
            t => t.GenerateRefreshToken(),
            Times.Never,
            "Should not generate refresh token with wrong password"
        );
    }

    [Fact]
    public async Task LoginAsync_SuspendedAccount_DoesNotGenerateTokens()
    {
        // Arrange
        var provider = new Provider
        {
            Id = 6,
            Email = ValidEmail,
            PasswordHash = HashedPassword,
            Status = ProviderStatus.Suspended
        };

        _mockProviderRepository.Setup(r => r.GetByEmailAsync(ValidEmail))
            .ReturnsAsync(provider);
        _mockPasswordHasher.Setup(h => h.VerifyPassword(ValidPassword, HashedPassword))
            .Returns(true);

        // Act
        await _service.LoginAsync(ValidEmail, ValidPassword);

        // Assert - NO debe generar tokens para cuenta suspendida
        _mockTokenService.Verify(
            t => t.GenerateAccessToken(It.IsAny<int>(), It.IsAny<string>()),
            Times.Never,
            "Should not generate tokens for suspended account"
        );
        _mockTokenService.Verify(
            t => t.GenerateRefreshToken(),
            Times.Never
        );
    }

    #endregion

    #region Helper Methods Tests

    [Fact]
    public async Task EmailExistsAsync_ExistingEmail_ReturnsTrue()
    {
        // Arrange
        _mockProviderRepository.Setup(r => r.ExistsByEmailAsync(ValidEmail))
            .ReturnsAsync(true);

        // Act
        var exists = await _service.EmailExistsAsync(ValidEmail);

        // Assert
        Assert.True(exists);
    }

    [Fact]
    public async Task EmailExistsAsync_NonExistentEmail_ReturnsFalse()
    {
        // Arrange
        _mockProviderRepository.Setup(r => r.ExistsByEmailAsync("new@example.com"))
            .ReturnsAsync(false);

        // Act
        var exists = await _service.EmailExistsAsync("new@example.com");

        // Assert
        Assert.False(exists);
    }

    [Fact]
    public async Task UsernameExistsAsync_ExistingUsername_ReturnsTrue()
    {
        // Arrange
        _mockProviderRepository.Setup(r => r.ExistsByUsernameAsync(ValidUsername))
            .ReturnsAsync(true);

        // Act
        var exists = await _service.UsernameExistsAsync(ValidUsername);

        // Assert
        Assert.True(exists);
    }

    [Fact]
    public async Task UsernameExistsAsync_NonExistentUsername_ReturnsFalse()
    {
        // Arrange
        _mockProviderRepository.Setup(r => r.ExistsByUsernameAsync("newuser"))
            .ReturnsAsync(false);

        // Act
        var exists = await _service.UsernameExistsAsync("newuser");

        // Assert
        Assert.False(exists);
    }

    #endregion
}

