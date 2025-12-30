using Xunit;
using FluentValidation;
using QuickMeet.API.DTOs.Auth;
using QuickMeet.API.Validators;

namespace QuickMeet.UnitTests.Validators;

public class LoginRequestValidatorTests
{
    private readonly LoginRequestValidator _validator;

    public LoginRequestValidatorTests()
    {
        _validator = new LoginRequestValidator();
    }

    #region Happy Path Tests

    [Fact]
    public void Validate_ValidRequest_IsValid()
    {
        // Arrange
        var request = new LoginRequest(
            Email: "user@example.com",
            Password: "ValidPassword123!@"
        );

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Validate_ValidEmailAndPassword_IsValid()
    {
        // Arrange
        var request = new LoginRequest(
            Email: "professional@company.com",
            Password: "SecurePass123!@#"
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
        var request = new LoginRequest(
            Email: email,
            Password: "ValidPassword123!@"
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
        var request = new LoginRequest(
            Email: "user@example.com",
            Password: password
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
        var request = new LoginRequest(
            Email: "notanemail",
            Password: "ValidPassword123!@"
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
    public void Validate_MalformedEmails_IsInvalid(string email)
    {
        // Arrange
        var request = new LoginRequest(
            Email: email,
            Password: "ValidPassword123!@"
        );

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_EmptyEmail_IsInvalid()
    {
        // Arrange
        var request = new LoginRequest(
            Email: "",
            Password: "ValidPassword123!@"
        );

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Email");
    }

    [Fact]
    public void Validate_NullEmail_IsInvalid()
    {
        // Arrange
        var request = new LoginRequest(
            Email: null!,
            Password: "ValidPassword123!@"
        );

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_EmptyPassword_IsInvalid()
    {
        // Arrange
        var request = new LoginRequest(
            Email: "user@example.com",
            Password: ""
        );

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Password");
    }

    [Fact]
    public void Validate_NullPassword_IsInvalid()
    {
        // Arrange
        var request = new LoginRequest(
            Email: "user@example.com",
            Password: null!
        );

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_BothFieldsEmpty_IsInvalid()
    {
        // Arrange
        var request = new LoginRequest(
            Email: "",
            Password: ""
        );

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.True(result.Errors.Count >= 2);
    }

    [Fact]
    public void Validate_WeakPassword_IsValid()
    {
        // Note: Login validator typically only validates format, not strength
        // Strength is checked during registration
        var request = new LoginRequest(
            Email: "user@example.com",
            Password: "anypassword"
        );

        // Act
        var result = _validator.Validate(request);

        // Assert
        // Login should accept any non-empty password (validation is in auth service)
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WhitespacePassword_IsInvalid()
    {
        // Arrange
        var request = new LoginRequest(
            Email: "user@example.com",
            Password: "   "
        );

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void Validate_LongEmail_IsValid()
    {
        // Arrange
        var longEmail = "very.long.email.address.with.many.parts@subdomain.example.co.uk";
        var request = new LoginRequest(
            Email: longEmail,
            Password: "ValidPassword123!@"
        );

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_LongPassword_IsValid()
    {
        // Arrange
        var longPassword = "VeryLongPasswordWith123!@#$%^&*(){}[]<>?,./;':\"\\|";
        var request = new LoginRequest(
            Email: "user@example.com",
            Password: longPassword
        );

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("user+1@example.com")]
    [InlineData("user.name@example.com")]
    [InlineData("user_name@example.com")]
    [InlineData("user-name@example.com")]
    public void Validate_VariousEmailFormats_IsValid(string email)
    {
        // Arrange
        var request = new LoginRequest(
            Email: email,
            Password: "ValidPassword123!@"
        );

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.True(result.IsValid);
    }

    #endregion
}
