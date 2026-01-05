using Xunit;
using FluentValidation;
using QuickMeet.Core.DTOs.Auth;
using QuickMeet.API.Validators;

namespace QuickMeet.UnitTests.Validators;

public class RegisterRequestValidatorTests
{
    private readonly RegisterRequestValidator _validator;

    public RegisterRequestValidatorTests()
    {
        _validator = new RegisterRequestValidator();
    }

    #region Happy Path Tests

    [Fact]
    public void Validate_ValidRequest_IsValid()
    {
        // Arrange
        var request = new RegisterRequest(
            Email: "user@example.com",
            Username: "validuser",
            FullName: "Valid User",
            Password: "ValidPassword123!@",
            PasswordConfirmation: "ValidPassword123!@"
        );

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Validate_AllFieldsCorrect_IsValid()
    {
        // Arrange
        var request = new RegisterRequest(
            Email: "professional@company.com",
            Username: "professional1",
            FullName: "John Doe",
            Password: "SecurePass123!@#",
            PasswordConfirmation: "SecurePass123!@#"
        );

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("user@example.com")]
    [InlineData("test.user@domain.co.uk")]
    [InlineData("user+tag@example.com")]
    [InlineData("name@subdomain.example.com")]
    public void Validate_VariousValidEmails_IsValid(string email)
    {
        // Arrange
        var request = new RegisterRequest(
            Email: email,
            Username: "validuser",
            FullName: "Valid User",
            Password: "ValidPassword123!@",
            PasswordConfirmation: "ValidPassword123!@"
        );

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("user123")]
    [InlineData("valid-user")]
    [InlineData("user-name")]
    [InlineData("abc")]
    public void Validate_VariousValidUsernames_IsValid(string username)
    {
        // Arrange
        var request = new RegisterRequest(
            Email: "user@example.com",
            Username: username,
            FullName: "Valid User",
            Password: "ValidPassword123!@",
            PasswordConfirmation: "ValidPassword123!@"
        );

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("ValidPassword123!@")]
    [InlineData("StrongP@ssw0rd2024")]
    [InlineData("MySecure!Pass#123")]
    [InlineData("Complex@Pass99!!")]
    public void Validate_VariousValidPasswords_IsValid(string password)
    {
        // Arrange
        var request = new RegisterRequest(
            Email: "user@example.com",
            Username: "validuser",
            FullName: "Valid User",
            Password: password,
            PasswordConfirmation: password
        );

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.True(result.IsValid);
    }

    #endregion

    #region Unhappy Path Tests

    [Fact]
    public void Validate_InvalidEmail_IsInvalid()
    {
        // Arrange
        var request = new RegisterRequest(
            Email: "notanemail",
            Username: "validuser",
            FullName: "Valid User",
            Password: "ValidPassword123!@",
            PasswordConfirmation: "ValidPassword123!@"
        );

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Email");
    }

    [Theory]
    [InlineData("")]
    [InlineData("notanemail")]
    [InlineData("@")]
    [InlineData("plaintext")]
    public void Validate_MalformedEmails_IsInvalid(string email)
    {
        // Arrange
        var request = new RegisterRequest(
            Email: email,
            Username: "validuser",
            FullName: "Valid User",
            Password: "ValidPassword123!@",
            PasswordConfirmation: "ValidPassword123!@"
        );

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_EmptyUsername_IsInvalid()
    {
        // Arrange
        var request = new RegisterRequest(
            Email: "user@example.com",
            Username: "",
            FullName: "Valid User",
            Password: "ValidPassword123!@",
            PasswordConfirmation: "ValidPassword123!@"
        );

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Username");
    }

    [Fact]
    public void Validate_EmptyFullName_IsInvalid()
    {
        // Arrange
        var request = new RegisterRequest(
            Email: "user@example.com",
            Username: "validuser",
            FullName: "",
            Password: "ValidPassword123!@",
            PasswordConfirmation: "ValidPassword123!@"
        );

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "FullName");
    }

    [Fact]
    public void Validate_PasswordsDoNotMatch_IsInvalid()
    {
        // Arrange
        var request = new RegisterRequest(
            Email: "user@example.com",
            Username: "validuser",
            FullName: "Valid User",
            Password: "ValidPassword123!@",
            PasswordConfirmation: "DifferentPassword123!@"
        );

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "PasswordConfirmation");
    }

    [Theory]
    [InlineData("short")]
    [InlineData("NoSpecial123")]
    [InlineData("NoNumber!@")]
    [InlineData("nouppercase123!@")]
    public void Validate_WeakPasswords_IsInvalid(string password)
    {
        // Arrange
        var request = new RegisterRequest(
            Email: "user@example.com",
            Username: "validuser",
            FullName: "Valid User",
            Password: password,
            PasswordConfirmation: password
        );

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Password");
    }

    [Fact]
    public void Validate_EmptyPassword_IsInvalid()
    {
        // Arrange
        var request = new RegisterRequest(
            Email: "user@example.com",
            Username: "validuser",
            FullName: "Valid User",
            Password: "",
            PasswordConfirmation: ""
        );

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_NullEmail_IsInvalid()
    {
        // Arrange
        var request = new RegisterRequest(
            Email: null!,
            Username: "validuser",
            FullName: "Valid User",
            Password: "ValidPassword123!@",
            PasswordConfirmation: "ValidPassword123!@"
        );

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
    }

    #endregion

    #region Edge Cases

    [Theory]
    [InlineData("ValidPassword123!@", "ValidPassword123!@")]
    [InlineData("StrongP@ssw0rd2024", "StrongP@ssw0rd2024")]
    [InlineData("MySecure!Pass#123XYZ", "MySecure!Pass#123XYZ")]
    public void Validate_LongPasswordsMatch_IsValid(string password, string confirmation)
    {
        // Arrange
        var request = new RegisterRequest(
            Email: "user@example.com",
            Username: "validuser",
            FullName: "Valid User",
            Password: password,
            PasswordConfirmation: confirmation
        );

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.True(result.IsValid);
    }

    #endregion
}
