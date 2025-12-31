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
}
