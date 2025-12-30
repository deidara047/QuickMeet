using Xunit;
using QuickMeet.Core.Services;

namespace QuickMeet.UnitTests.Services;

public class PasswordHashingServiceTests
{
    private readonly PasswordHashingService _service;

    public PasswordHashingServiceTests()
    {
        _service = new PasswordHashingService();
    }

    #region Happy Path Tests

    [Fact]
    public void HashPassword_ValidPassword_ReturnsNonEmptyHash()
    {
        // Arrange
        var password = "ValidPassword123!@";

        // Act
        var hash = _service.HashPassword(password);

        // Assert
        Assert.NotNull(hash);
        Assert.NotEmpty(hash);
        Assert.NotEqual(password, hash);
    }

    [Fact]
    public void HashPassword_SamePasswordTwice_ReturnsDifferentHashes()
    {
        // Arrange
        var password = "ValidPassword123!@";

        // Act
        var hash1 = _service.HashPassword(password);
        var hash2 = _service.HashPassword(password);

        // Assert
        Assert.NotEqual(hash1, hash2);
    }

    [Fact]
    public void VerifyPassword_CorrectPassword_ReturnsTrue()
    {
        // Arrange
        var password = "MySecurePassword123!@";
        var hash = _service.HashPassword(password);

        // Act
        var result = _service.VerifyPassword(password, hash);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void VerifyPassword_CorrectPasswordWithSpecialChars_ReturnsTrue()
    {
        // Arrange
        var password = "P@ssw0rd!#$%&*(){}[]<>?,./;':\"\\|";
        var hash = _service.HashPassword(password);

        // Act
        var result = _service.VerifyPassword(password, hash);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void VerifyPassword_CorrectPasswordLongString_ReturnsTrue()
    {
        // Arrange
        var password = "VeryLongPasswordWithManyCharacters123!@#$%^&*()";
        var hash = _service.HashPassword(password);

        // Act
        var result = _service.VerifyPassword(password, hash);

        // Assert
        Assert.True(result);
    }

    #endregion

    #region Unhappy Path Tests

    [Fact]
    public void VerifyPassword_IncorrectPassword_ReturnsFalse()
    {
        // Arrange
        var password = "CorrectPassword123!@";
        var wrongPassword = "WrongPassword123!@";
        var hash = _service.HashPassword(password);

        // Act
        var result = _service.VerifyPassword(wrongPassword, hash);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void VerifyPassword_PasswordWithOneCharDifferent_ReturnsFalse()
    {
        // Arrange
        var password = "MyPassword123!@";
        var nearlyCorrectPassword = "MyPassword123!#"; // Only last char different
        var hash = _service.HashPassword(password);

        // Act
        var result = _service.VerifyPassword(nearlyCorrectPassword, hash);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void VerifyPassword_CaseSensitivePassword_ReturnsFalse()
    {
        // Arrange
        var password = "Password123!@";
        var differentCasePassword = "password123!@"; // Different case
        var hash = _service.HashPassword(password);

        // Act
        var result = _service.VerifyPassword(differentCasePassword, hash);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void VerifyPassword_EmptyPassword_ReturnsFalse()
    {
        // Arrange
        var password = "ValidPassword123!@";
        var hash = _service.HashPassword(password);

        // Act
        var result = _service.VerifyPassword(string.Empty, hash);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void VerifyPassword_CorruptedHash_ReturnsFalse()
    {
        // Arrange
        var password = "ValidPassword123!@";
        var hash = _service.HashPassword(password);
        var corruptedHash = hash.Substring(0, hash.Length - 5) + "xxxxx"; // Corrupt last 5 chars

        // Act
        var result = _service.VerifyPassword(password, corruptedHash);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void VerifyPassword_NullPassword_ReturnsFalse()
    {
        // Arrange
        var password = "ValidPassword123!@";
        var hash = _service.HashPassword(password);

        // Act & Assert
        var result = _service.VerifyPassword(null!, hash);
        Assert.False(result);
    }

    [Fact]
    public void VerifyPassword_NullHash_ReturnsFalse()
    {
        // Arrange
        var password = "ValidPassword123!@";

        // Act & Assert
        var result = _service.VerifyPassword(password, null!);
        Assert.False(result);
    }

    #endregion

    #region Edge Cases

    [Theory]
    [InlineData("A1!")]
    [InlineData("ABC123!@#")]
    [InlineData("VeryVeryVeryLongPasswordWith123!@#$%^&*()")]
    public void HashAndVerify_VariousPasswordLengths_WorksCorrectly(string password)
    {
        // Act
        var hash = _service.HashPassword(password);
        var result = _service.VerifyPassword(password, hash);

        // Assert
        Assert.True(result);
    }

    #endregion
}
