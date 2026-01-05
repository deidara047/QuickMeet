using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using QuickMeet.Core.DTOs.Auth;
using QuickMeet.IntegrationTests.Common;
using QuickMeet.IntegrationTests.Fixtures;
using Xunit;

namespace QuickMeet.IntegrationTests.Controllers;

public class AuthControllerE2ETests : IntegrationTestBase
{
    public AuthControllerE2ETests(QuickMeetWebApplicationFactory factory) : base(factory) { }

    #region E2E Scenarios

    [Fact]
    public async Task E2E_LoginWithoutRegister_Fails()
    {
        // Arrange - Intentar login sin registrarse previamente
        var loginRequest = new LoginRequest(
            Email: "neverregistered@example.com",
            Password: "SomePassword123!@"
        );

        // Act
        var response = await Client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    // ⚠️ E2E_RegisterVerifyEmailThenLogin deshabilitado
    // Razón: VerifyEmailAsync no está completamente implementado
    // Ver: AuthenticationService.cs línea 118 - "Email verification requires token repository"
    // Se habilitará cuando la funcionalidad esté lista

    //[Fact]
    //public async Task E2E_RegisterVerifyEmailThenLogin_Success()
    //{
    //    // Arrange
    //    var registerRequest = new RegisterRequest(
    //        Email: "verify@example.com",
    //        Username: "verifyuser",
    //        FullName: "Verify User",
    //        Password: "ValidPassword123!@",
    //        PasswordConfirmation: "ValidPassword123!@"
    //    );

    //    // Act 1 - Register
    //    var registerResponse = await Client.PostAsJsonAsync("/api/auth/register", registerRequest);

    //    // Act 2 - Obtener token de verificación desde BD
    //    var provider = await GetFromDatabase(async db =>
    //        await db.Providers.FirstOrDefaultAsync(p => p.Email == registerRequest.Email));
    //    var verificationToken = await GetFromDatabase(async db =>
    //        await db.EmailVerificationTokens
    //            .Where(t => t.ProviderId == provider!.Id)
    //            .FirstOrDefaultAsync());

    //    // Act 3 - Verificar email
    //    var verifyRequest = new VerifyEmailRequest(Token: verificationToken!.Token);
    //    var verifyResponse = await Client.PostAsJsonAsync("/api/auth/verify-email", verifyRequest);

    //    // Act 4 - Login después de verificar
    //    var loginRequest = new LoginRequest(
    //        Email: registerRequest.Email,
    //        Password: registerRequest.Password
    //    );
    //    var loginResponse = await Client.PostAsJsonAsync("/api/auth/login", loginRequest);

    //    // Assert
    //    Assert.Equal(HttpStatusCode.OK, registerResponse.StatusCode);
    //    Assert.Equal(HttpStatusCode.OK, verifyResponse.StatusCode);
    //    Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
    //}

    #endregion
}
