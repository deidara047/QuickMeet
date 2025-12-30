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

    #region Register Tests

    [Fact]
    public async Task RegisterAsync_ValidInput_ReturnsSuccess()
    {
        // Arrange
        var email = "newuser@example.com";
        var username = "newuser";
        var fullName = "New User";
        var password = "ValidPassword123!@";

        _mockProviderRepository.Setup(r => r.ExistsByEmailAsync(email))
            .ReturnsAsync(false);
        _mockProviderRepository.Setup(r => r.ExistsByUsernameAsync(username))
            .ReturnsAsync(false);
        _mockPasswordHasher.Setup(h => h.HashPassword(password))
            .Returns("hashed_password");
        _mockTokenService.Setup(t => t.GenerateAccessToken(It.IsAny<int>(), It.IsAny<string>()))
            .Returns("access_token");
        _mockTokenService.Setup(t => t.GenerateRefreshToken())
            .Returns("refresh_token");

        // Act
        var result = await _service.RegisterAsync(email, username, fullName, password);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Result);
        Assert.Equal(email, result.Result.Email);
        Assert.Equal(username, result.Result.Username);
        Assert.Equal(fullName, result.Result.FullName);
        Assert.Equal("access_token", result.Result.AccessToken);
        Assert.Equal("refresh_token", result.Result.RefreshToken);
    }

    [Fact]
    public async Task RegisterAsync_DuplicateEmail_ReturnsFailure()
    {
        // Arrange
        var email = "existing@example.com";
        _mockProviderRepository.Setup(r => r.ExistsByEmailAsync(email))
            .ReturnsAsync(true);

        // Act
        var result = await _service.RegisterAsync(email, "newuser", "User", "ValidPassword123!@");

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Email already registered", result.Message);
        Assert.Null(result.Result);
    }

    [Fact]
    public async Task RegisterAsync_DuplicateUsername_ReturnsFailure()
    {
        // Arrange
        var username = "existing";
        _mockProviderRepository.Setup(r => r.ExistsByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync(false);
        _mockProviderRepository.Setup(r => r.ExistsByUsernameAsync(username))
            .ReturnsAsync(true);

        // Act
        var result = await _service.RegisterAsync("new@example.com", username, "User", "ValidPassword123!@");

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Username already taken", result.Message);
        Assert.Null(result.Result);
    }

    [Fact]
    public async Task RegisterAsync_ValidInput_HashesPassword()
    {
        // Arrange
        var password = "MySecurePassword123!@";
        _mockProviderRepository.Setup(r => r.ExistsByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync(false);
        _mockProviderRepository.Setup(r => r.ExistsByUsernameAsync(It.IsAny<string>()))
            .ReturnsAsync(false);
        _mockPasswordHasher.Setup(h => h.HashPassword(password))
            .Returns("hashed_pass_xyz");
        _mockTokenService.Setup(t => t.GenerateAccessToken(It.IsAny<int>(), It.IsAny<string>()))
            .Returns("token");
        _mockTokenService.Setup(t => t.GenerateRefreshToken())
            .Returns("refresh");

        // Act
        await _service.RegisterAsync("test@example.com", "testuser", "Test User", password);

        // Assert
        _mockPasswordHasher.Verify(
            h => h.HashPassword(password),
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
            .Returns("hash");
        _mockTokenService.Setup(t => t.GenerateAccessToken(It.IsAny<int>(), It.IsAny<string>()))
            .Returns("token");
        _mockTokenService.Setup(t => t.GenerateRefreshToken())
            .Returns("refresh");

        // Act
        await _service.RegisterAsync("test@example.com", "testuser", "Test User", "ValidPassword123!@");

        // Assert
        _mockProviderRepository.Verify(
            r => r.AddAsync(It.Is<Provider>(p =>
                p.Email == "test@example.com" &&
                p.Username == "testuser" &&
                p.FullName == "Test User" &&
                p.Status == ProviderStatus.PendingVerification
            )),
            Times.Once,
            "Provider should be saved with PendingVerification status"
        );
    }

    #endregion

    #region Login Tests

    [Fact]
    public async Task LoginAsync_ValidCredentials_ReturnsSuccess()
    {
        // Arrange
        var provider = new Provider
        {
            Id = 1,
            Email = "user@example.com",
            Username = "testuser",
            FullName = "Test User",
            PasswordHash = "hashed_password",
            Status = ProviderStatus.Active
        };

        _mockProviderRepository.Setup(r => r.GetByEmailAsync("user@example.com"))
            .ReturnsAsync(provider);
        _mockPasswordHasher.Setup(h => h.VerifyPassword("ValidPassword123!@", "hashed_password"))
            .Returns(true);
        _mockTokenService.Setup(t => t.GenerateAccessToken(provider.Id, provider.Email))
            .Returns("access_token");
        _mockTokenService.Setup(t => t.GenerateRefreshToken())
            .Returns("refresh_token");

        // Act
        var result = await _service.LoginAsync("user@example.com", "ValidPassword123!@");

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Result);
        Assert.Equal("user@example.com", result.Result.Email);
        Assert.Equal("testuser", result.Result.Username);
        Assert.Equal("access_token", result.Result.AccessToken);
        Assert.Equal("refresh_token", result.Result.RefreshToken);
    }

    [Fact]
    public async Task LoginAsync_NonExistentUser_ReturnsFailure()
    {
        // Arrange
        _mockProviderRepository.Setup(r => r.GetByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((Provider?)null);

        // Act
        var result = await _service.LoginAsync("nonexistent@example.com", "password");

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
            Email = "user@example.com",
            PasswordHash = "hashed_correct_password",
            Status = ProviderStatus.Active
        };

        _mockProviderRepository.Setup(r => r.GetByEmailAsync("user@example.com"))
            .ReturnsAsync(provider);
        _mockPasswordHasher.Setup(h => h.VerifyPassword("WrongPassword123!@", "hashed_correct_password"))
            .Returns(false);

        // Act
        var result = await _service.LoginAsync("user@example.com", "WrongPassword123!@");

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
            Email = "suspended@example.com",
            PasswordHash = "hashed_password",
            Status = ProviderStatus.Suspended
        };

        _mockProviderRepository.Setup(r => r.GetByEmailAsync("suspended@example.com"))
            .ReturnsAsync(provider);
        _mockPasswordHasher.Setup(h => h.VerifyPassword(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(true);

        // Act
        var result = await _service.LoginAsync("suspended@example.com", "password");

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
            Email = "pending@example.com",
            Username = "pendinguser",
            FullName = "Pending User",
            PasswordHash = "hashed_password",
            Status = ProviderStatus.PendingVerification
        };

        _mockProviderRepository.Setup(r => r.GetByEmailAsync("pending@example.com"))
            .ReturnsAsync(provider);
        _mockPasswordHasher.Setup(h => h.VerifyPassword("ValidPassword123!@", "hashed_password"))
            .Returns(true);
        _mockTokenService.Setup(t => t.GenerateAccessToken(provider.Id, provider.Email))
            .Returns("access_token");
        _mockTokenService.Setup(t => t.GenerateRefreshToken())
            .Returns("refresh_token");

        // Act
        var result = await _service.LoginAsync("pending@example.com", "ValidPassword123!@");

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Result);
    }

    [Fact]
    public async Task LoginAsync_ValidCredentials_GeneratesTokens()
    {
        // Arrange
        var provider = new Provider
        {
            Id = 5,
            Email = "user@example.com",
            Username = "user",
            FullName = "User",
            PasswordHash = "hash",
            Status = ProviderStatus.Active
        };

        _mockProviderRepository.Setup(r => r.GetByEmailAsync("user@example.com"))
            .ReturnsAsync(provider);
        _mockPasswordHasher.Setup(h => h.VerifyPassword(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(true);
        _mockTokenService.Setup(t => t.GenerateAccessToken(provider.Id, provider.Email))
            .Returns("new_access_token");
        _mockTokenService.Setup(t => t.GenerateRefreshToken())
            .Returns("new_refresh_token");

        // Act
        var result = await _service.LoginAsync("user@example.com", "password");

        // Assert
        _mockTokenService.Verify(
            t => t.GenerateAccessToken(provider.Id, provider.Email),
            Times.Once
        );
        _mockTokenService.Verify(
            t => t.GenerateRefreshToken(),
            Times.Once
        );
        Assert.Equal("new_access_token", result.Result?.AccessToken);
        Assert.Equal("new_refresh_token", result.Result?.RefreshToken);
    }

    #endregion

    #region Helper Methods Tests

    [Fact]
    public async Task EmailExistsAsync_ExistingEmail_ReturnsTrue()
    {
        // Arrange
        _mockProviderRepository.Setup(r => r.ExistsByEmailAsync("existing@example.com"))
            .ReturnsAsync(true);

        // Act
        var exists = await _service.EmailExistsAsync("existing@example.com");

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
        _mockProviderRepository.Setup(r => r.ExistsByUsernameAsync("existing"))
            .ReturnsAsync(true);

        // Act
        var exists = await _service.UsernameExistsAsync("existing");

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

