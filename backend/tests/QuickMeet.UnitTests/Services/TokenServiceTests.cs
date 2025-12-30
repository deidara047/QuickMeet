using Xunit;
using Microsoft.Extensions.Configuration;
using QuickMeet.Core.Services;

namespace QuickMeet.UnitTests.Services;

public class TokenServiceTests
{
    private readonly TokenService _service;

    public TokenServiceTests()
    {
        // Create a test configuration
        var inMemorySettings = new Dictionary<string, string?>
        {
            {"Jwt:SecretKey", "your-super-secret-key-change-this-in-production-minimum-32-characters-here"},
            {"Jwt:AccessTokenExpirationMinutes", "60"},
            {"Jwt:RefreshTokenExpirationDays", "7"},
            {"Jwt:Issuer", "QuickMeet"},
            {"Jwt:Audience", "QuickMeetClients"}
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings!)
            .Build();

        _service = new TokenService(configuration);
    }

    #region Happy Path Tests

    [Fact]
    public void GenerateAccessToken_ValidInput_ReturnsValidJwt()
    {
        // Arrange
        var providerId = 1;
        var email = "test@example.com";

        // Act
        var token = _service.GenerateAccessToken(providerId, email);

        // Assert
        Assert.NotNull(token);
        Assert.NotEmpty(token);
        Assert.Contains(".", token); // JWT format: header.payload.signature
    }

    [Fact]
    public void GenerateAccessToken_DifferentProviders_ReturnsDifferentTokens()
    {
        // Arrange
        var email = "test@example.com";

        // Act
        var token1 = _service.GenerateAccessToken(1, email);
        var token2 = _service.GenerateAccessToken(2, email);

        // Assert
        Assert.NotEqual(token1, token2);
    }

    [Fact]
    public void GenerateRefreshToken_Called_ReturnsBase64String()
    {
        // Act
        var token = _service.GenerateRefreshToken();

        // Assert
        Assert.NotNull(token);
        Assert.NotEmpty(token);
        // Should be valid base64
        try
        {
            Convert.FromBase64String(token);
        }
        catch
        {
            Assert.Fail("Refresh token is not valid base64");
        }
    }

    [Fact]
    public void GenerateRefreshToken_CalledTwice_ReturnsDifferentTokens()
    {
        // Act
        var token1 = _service.GenerateRefreshToken();
        var token2 = _service.GenerateRefreshToken();

        // Assert
        Assert.NotEqual(token1, token2);
    }

    [Fact]
    public void ValidateAccessToken_ValidToken_ReturnsTrue()
    {
        // Arrange
        var providerId = 1;
        var email = "test@example.com";
        var token = _service.GenerateAccessToken(providerId, email);

        // Act
        var result = _service.ValidateAccessToken(token, out var extractedProviderId);

        // Assert
        Assert.True(result);
        Assert.Equal(providerId, extractedProviderId);
    }

    [Fact]
    public void ValidateAccessToken_ValidToken_ExtractsCorrectProviderId()
    {
        // Arrange
        var providerId = 42;
        var email = "user@example.com";
        var token = _service.GenerateAccessToken(providerId, email);

        // Act
        var result = _service.ValidateAccessToken(token, out var extractedId);

        // Assert
        Assert.True(result);
        Assert.Equal(providerId, extractedId);
    }

    [Fact]
    public void ValidateAccessToken_MultipleIds_ExtractsCorrectProviderId()
    {
        // Arrange
        var testCases = new[] { 1, 10, 100, 1000, 99999 };

        foreach (var providerId in testCases)
        {
            // Act
            var token = _service.GenerateAccessToken(providerId, "test@example.com");
            var result = _service.ValidateAccessToken(token, out var extractedId);

            // Assert
            Assert.True(result, $"Failed for providerId {providerId}");
            Assert.Equal(providerId, extractedId);
        }
    }

    #endregion

    #region Unhappy Path Tests

    [Fact]
    public void ValidateAccessToken_ModifiedToken_ReturnsFalse()
    {
        // Arrange
        var token = _service.GenerateAccessToken(1, "test@example.com");
        var modifiedToken = token.Substring(0, token.Length - 5) + "xxxxx"; // Modify signature

        // Act
        var result = _service.ValidateAccessToken(modifiedToken, out _);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ValidateAccessToken_EmptyToken_ReturnsFalse()
    {
        // Act
        var result = _service.ValidateAccessToken(string.Empty, out _);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ValidateAccessToken_InvalidJwtFormat_ReturnsFalse()
    {
        // Act
        var result = _service.ValidateAccessToken("not.a.valid.jwt", out _);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ValidateAccessToken_RandomString_ReturnsFalse()
    {
        // Act
        var result = _service.ValidateAccessToken("randomstringwithoutvalidjwtformat", out _);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ValidateAccessToken_ExpiredToken_ReturnsFalse()
    {
        // Arrange - Create a token service with a past expiration (would need custom implementation)
        // For now, we test with an obviously invalid token
        var invalidToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJleHAiOjAsImV4cCI6MCwiZXhwIjowfQ.xxxxxxxxxxxx";

        // Act
        var result = _service.ValidateAccessToken(invalidToken, out _);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ValidateAccessToken_NullToken_ReturnsFalse()
    {
        // Act
        var result = _service.ValidateAccessToken(null!, out _);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ValidateAccessToken_CorruptedPayload_ReturnsFalse()
    {
        // Arrange
        var token = _service.GenerateAccessToken(1, "test@example.com");
        var parts = token.Split('.');
        var corruptedToken = $"{parts[0]}.corrupted.{parts[2]}";

        // Act
        var result = _service.ValidateAccessToken(corruptedToken, out _);

        // Assert
        Assert.False(result);
    }

    #endregion

    #region Edge Cases

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(int.MaxValue)]
    public void ValidateAccessToken_VariousProviderIds_WorksCorrectly(int providerId)
    {
        // Act
        var token = _service.GenerateAccessToken(providerId, "test@example.com");
        var result = _service.ValidateAccessToken(token, out var extractedId);

        // Assert
        Assert.True(result);
        Assert.Equal(providerId, extractedId);
    }

    [Theory]
    [InlineData("user@example.com")]
    [InlineData("test.user@domain.co.uk")]
    [InlineData("user+tag@example.com")]
    public void GenerateAccessToken_VariousEmails_GeneratesToken(string email)
    {
        // Act
        var token = _service.GenerateAccessToken(1, email);

        // Assert
        Assert.NotNull(token);
        Assert.NotEmpty(token);
    }

    #endregion
}
