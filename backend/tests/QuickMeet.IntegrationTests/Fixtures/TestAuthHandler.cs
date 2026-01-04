using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace QuickMeet.IntegrationTests.Fixtures;

/// <summary>
/// Manejador de autenticación para tests E2E en memoria.
/// Lee el header "X-Test-UserId" para autenticar usuarios en tests.
/// Si el header no está presente, devuelve "NoResult" (sin autenticación).
/// </summary>
public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string SchemeName = "TestScheme";
    public const string UserIdHeader = "X-Test-UserId";
    public const string UserEmailHeader = "X-Test-Email";

    public TestAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // Leer el header X-Test-UserId
        if (!Request.Headers.TryGetValue(UserIdHeader, out var userIdValue))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var userId = userIdValue.ToString();
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
        };

        // Agregar email si está presente
        if (Request.Headers.TryGetValue(UserEmailHeader, out var emailValue))
        {
            claims.Add(new Claim(ClaimTypes.Email, emailValue.ToString()));
            claims.Add(new Claim(ClaimTypes.Name, emailValue.ToString()));
        }

        var identity = new ClaimsIdentity(claims, SchemeName);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, SchemeName);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}

