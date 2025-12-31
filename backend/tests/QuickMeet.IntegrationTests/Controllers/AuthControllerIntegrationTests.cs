using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using QuickMeet.API.DTOs.Auth;
using QuickMeet.IntegrationTests.Common;
using QuickMeet.IntegrationTests.Fixtures;
using Xunit;

namespace QuickMeet.IntegrationTests.Controllers;

public class AuthControllerIntegrationTests : IntegrationTestBase
{
    public AuthControllerIntegrationTests(QuickMeetWebApplicationFactory factory) : base(factory) { }

    #region Register Tests - Happy Path

    [Fact]
    public async Task Register_ValidData_ReturnsOkWithTokens()
    {
        // Arrange
        var request = new RegisterRequest(
            Email: "newuser@example.com",
            Username: "newuser",
            FullName: "New User",
            Password: "ValidPassword123!@",
            PasswordConfirmation: "ValidPassword123!@"
        );

        // Act
        var response = await Client.PostAsJsonAsync("/api/auth/register", request);
        var result = await response.Content.ReadFromJsonAsync<AuthResponse>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(result);
        Assert.Equal(request.Email, result.Email);
        Assert.Equal(request.Username, result.Username);
        Assert.NotEmpty(result.AccessToken);
        Assert.NotEmpty(result.RefreshToken);

        var provider = await GetFromDatabase(async db =>
            await db.Providers.FirstOrDefaultAsync(p => p.Email == request.Email));
        Assert.NotNull(provider);
        Assert.Equal(request.Email, provider.Email);
    }

    #endregion

    #region Register Tests - Security

    [Fact]
    public async Task Register_ValidData_PasswordNotStoredInPlainText()
    {
        // Arrange
        var password = "MySecretPass123!@";
        var request = new RegisterRequest(
            Email: "security@example.com",
            Username: "secureuser",
            FullName: "Secure User",
            Password: password,
            PasswordConfirmation: password
        );

        // Act
        var response = await Client.PostAsJsonAsync("/api/auth/register", request);

        // Assert
        var provider = await GetFromDatabase(async db =>
            await db.Providers.FirstOrDefaultAsync(p => p.Email == request.Email));

        Assert.NotNull(provider);
        Assert.NotEqual(password, provider.PasswordHash);
        Assert.NotEmpty(provider.PasswordHash);
        Assert.True(provider.PasswordHash.Length > 20);
        Assert.DoesNotContain(password, provider.PasswordHash);
    }

    #endregion

    #region Register Tests - Errors

    [Fact]
    public async Task Register_DuplicateEmail_ReturnsBadRequest()
    {
        // Arrange
        var request1 = new RegisterRequest(
            Email: "duplicate@example.com",
            Username: "user1",
            FullName: "User One",
            Password: "ValidPassword123!@",
            PasswordConfirmation: "ValidPassword123!@"
        );

        var request2 = new RegisterRequest(
            Email: "duplicate@example.com",
            Username: "user2",
            FullName: "User Two",
            Password: "ValidPassword123!@",
            PasswordConfirmation: "ValidPassword123!@"
        );

        // Act
        await Client.PostAsJsonAsync("/api/auth/register", request1);
        var response = await Client.PostAsJsonAsync("/api/auth/register", request2);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Register_DuplicateUsername_ReturnsBadRequest()
    {
        // Arrange
        var request1 = new RegisterRequest(
            Email: "email1@example.com",
            Username: "samename",
            FullName: "User One",
            Password: "ValidPassword123!@",
            PasswordConfirmation: "ValidPassword123!@"
        );

        var request2 = new RegisterRequest(
            Email: "email2@example.com",
            Username: "samename",
            FullName: "User Two",
            Password: "ValidPassword123!@",
            PasswordConfirmation: "ValidPassword123!@"
        );

        // Act
        await Client.PostAsJsonAsync("/api/auth/register", request1);
        var response = await Client.PostAsJsonAsync("/api/auth/register", request2);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Register_NullEmail_ReturnsBadRequest()
    {
        // Arrange
        var request = new RegisterRequest(
            Email: null!,
            Username: "testuser",
            FullName: "Test User",
            Password: "ValidPassword123!@",
            PasswordConfirmation: "ValidPassword123!@"
        );

        // Act
        var response = await Client.PostAsJsonAsync("/api/auth/register", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Register_EmptyPassword_ReturnsBadRequest()
    {
        // Arrange
        var request = new RegisterRequest(
            Email: "test@example.com",
            Username: "testuser",
            FullName: "Test User",
            Password: "",
            PasswordConfirmation: ""
        );

        // Act
        var response = await Client.PostAsJsonAsync("/api/auth/register", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    #endregion

    #region Login Tests

    [Fact]
    public async Task Login_ValidCredentials_ReturnsOk()
    {
        // Arrange - Primero registrar un usuario
        var registerRequest = new RegisterRequest(
            Email: "login@example.com",
            Username: "loginuser",
            FullName: "Login User",
            Password: "ValidPassword123!@",
            PasswordConfirmation: "ValidPassword123!@"
        );
        await Client.PostAsJsonAsync("/api/auth/register", registerRequest);

        var loginRequest = new LoginRequest(
            Email: "login@example.com",
            Password: "ValidPassword123!@"
        );

        // Act
        var response = await Client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(result);
        Assert.NotEmpty(result.AccessToken);
    }

    [Fact]
    public async Task Login_InvalidPassword_ReturnsUnauthorized()
    {
        // Arrange - Registrar usuario
        var registerRequest = new RegisterRequest(
            Email: "wrong@example.com",
            Username: "wronguser",
            FullName: "Wrong User",
            Password: "CorrectPassword123!@",
            PasswordConfirmation: "CorrectPassword123!@"
        );
        await Client.PostAsJsonAsync("/api/auth/register", registerRequest);

        var loginRequest = new LoginRequest(
            Email: "wrong@example.com",
            Password: "WrongPassword123!@"
        );

        // Act
        var response = await Client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_NonexistentUser_ReturnsUnauthorized()
    {
        // Arrange
        var loginRequest = new LoginRequest(
            Email: "nonexistent@example.com",
            Password: "AnyPassword123!@"
        );

        // Act
        var response = await Client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_SuspendedAccount_ReturnsUnauthorized()
    {
        // Arrange - Crear provider suspendido directamente en BD con password hasheado
        var passwordToHash = "SuspendedPassword123!@";
        await SeedDatabase(db =>
        {
            // Usar PasswordHashingService real para generar hash válido
            var hasher = new QuickMeet.Core.Services.PasswordHashingService();
            var provider = new QuickMeet.Core.Entities.Provider
            {
                Email = "suspended@example.com",
                Username = "suspendeduser",
                FullName = "Suspended User",
                PasswordHash = hasher.HashPassword(passwordToHash), // ← Hash real
                Status = QuickMeet.Core.Entities.ProviderStatus.Suspended,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            db.Providers.Add(provider);
        });

        // Intentar login con password correcto
        var loginRequest = new LoginRequest(
            Email: "suspended@example.com",
            Password: passwordToHash // ← Mismo password que hasheamos
        );

        // Act
        var response = await Client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        // Debería retornar Unauthorized porque la cuenta está suspendida
        // (aunque el password sea correcto)
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    #endregion
}
