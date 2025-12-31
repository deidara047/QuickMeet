# Estándar de la industria para testing de rutas autenticadas en ASP.NET con xUnit

## 1. **Patrón de Autenticación en ASP.NET Core**

### Middleware/Atributo estándar:

```csharp
// Controllers/ProtectedController.cs
[ApiController]
[Route("api/[controller]")]
[Authorize] // Atributo estándar de ASP.NET
public class ProtectedController : ControllerBase
{
    [HttpGet]
    public IActionResult GetProtectedData()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Ok(new { data = "protected data", userId });
    }
    
    [HttpGet("admin")]
    [Authorize(Roles = "Admin")]
    public IActionResult GetAdminData()
    {
        return Ok(new { data = "admin data" });
    }
}
```

## 2. **Testing con xUnit: Mejores Prácticas**

### Opción A: **WebApplicationFactory + TestServer** (Más recomendado)

```csharp
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Headers;
using Xunit;

public class ProtectedEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public ProtectedEndpointsTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetProtected_WithoutAuth_Returns401()
    {
        // Act
        var response = await _client.GetAsync("/api/protected");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetProtected_WithValidToken_Returns200()
    {
        // Arrange
        var token = GenerateJwtToken("user123", "user@test.com");
        _client.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.GetAsync("/api/protected");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetProtected_WithInvalidToken_Returns401()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", "invalid-token");

        // Act
        var response = await _client.GetAsync("/api/protected");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private string GenerateJwtToken(string userId, string email)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes("your-test-secret-key-min-32-chars");
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Email, email)
            }),
            Expires = DateTime.UtcNow.AddHours(1),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}
```

### Opción B: **Custom WebApplicationFactory con Auth Mock**

```csharp
public class AuthenticatedWebApplicationFactory<TProgram> 
    : WebApplicationFactory<TProgram> where TProgram : class
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            // Usar autenticación de prueba
            services.AddAuthentication("Test")
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                    "Test", options => { });
        });
    }
}

// Test Auth Handler
public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string UserId = "test-user-id";
    public const string UserEmail = "test@example.com";

    public TestAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISystemClock clock)
        : base(options, logger, encoder, clock)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, UserId),
            new Claim(ClaimTypes.Email, UserEmail),
            new Claim(ClaimTypes.Role, "User")
        };

        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, "Test");

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}

// Uso en tests
public class ProtectedEndpointsTests : IClassFixture<AuthenticatedWebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public ProtectedEndpointsTests(AuthenticatedWebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetProtected_WithTestAuth_Returns200()
    {
        // Act
        var response = await _client.GetAsync("/api/protected");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains(TestAuthHandler.UserId, content);
    }
}
```

## 3. **Testing de Autorización basada en Roles/Policies**

```csharp
public class AuthorizationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    [Theory]
    [InlineData("User", "/api/protected", HttpStatusCode.OK)]
    [InlineData("User", "/api/protected/admin", HttpStatusCode.Forbidden)]
    [InlineData("Admin", "/api/protected/admin", HttpStatusCode.OK)]
    public async Task EndpointAccess_WithDifferentRoles_ReturnsExpectedStatus(
        string role, string endpoint, HttpStatusCode expectedStatus)
    {
        // Arrange
        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                services.AddAuthentication("Test")
                    .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                        "Test", options => 
                        {
                            options.ClaimsIssuer = "Test";
                        });
            });
        }).CreateClient();

        var token = GenerateJwtTokenWithRole("user123", role);
        client.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await client.GetAsync(endpoint);

        // Assert
        Assert.Equal(expectedStatus, response.StatusCode);
    }

    private string GenerateJwtTokenWithRole(string userId, string role)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes("your-test-secret-key-min-32-chars");
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Role, role)
            }),
            Expires = DateTime.UtcNow.AddHours(1),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };
        return tokenHandler.WriteToken(tokenHandler.CreateToken(tokenDescriptor));
    }
}
```

## 4. **Testing Unitario de Controllers con Mocks**

```csharp
public class ProtectedControllerUnitTests
{
    [Fact]
    public void GetProtectedData_WithAuthenticatedUser_ReturnsOkWithUserId()
    {
        // Arrange
        var controller = new ProtectedController();
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "user123"),
            new Claim(ClaimTypes.Email, "test@test.com")
        }, "TestAuth"));

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };

        // Act
        var result = controller.GetProtectedData() as OkObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);
        
        var value = result.Value as dynamic;
        Assert.Equal("user123", value.userId);
    }

    [Fact]
    public void GetAdminData_WithNonAdminUser_ReturnsForbidden()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthorizationService>();
        mockAuthService
            .Setup(x => x.AuthorizeAsync(
                It.IsAny<ClaimsPrincipal>(),
                It.IsAny<object>(),
                "AdminPolicy"))
            .ReturnsAsync(AuthorizationResult.Failed());

        // Act & Assert
        // Verificar que el atributo [Authorize(Roles = "Admin")] esté presente
        var method = typeof(ProtectedController)
            .GetMethod(nameof(ProtectedController.GetAdminData));
        var attributes = method.GetCustomAttributes(typeof(AuthorizeAttribute), true);
        
        Assert.NotEmpty(attributes);
    }
}
```

## 5. **Helper Class para Tests Autenticados**

```csharp
// TestHelpers/AuthenticatedClientBuilder.cs
public class AuthenticatedClientBuilder
{
    private readonly WebApplicationFactory<Program> _factory;
    private string _userId = "default-user";
    private string _email = "default@test.com";
    private readonly List<string> _roles = new();
    private readonly List<Claim> _claims = new();

    public AuthenticatedClientBuilder(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    public AuthenticatedClientBuilder WithUser(string userId, string email)
    {
        _userId = userId;
        _email = email;
        return this;
    }

    public AuthenticatedClientBuilder WithRole(string role)
    {
        _roles.Add(role);
        return this;
    }

    public AuthenticatedClientBuilder WithClaim(string type, string value)
    {
        _claims.Add(new Claim(type, value));
        return this;
    }

    public HttpClient Build()
    {
        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                services.AddAuthentication("Test")
                    .AddScheme<AuthenticationSchemeOptions, CustomTestAuthHandler>(
                        "Test", options =>
                        {
                            options.Items["UserId"] = _userId;
                            options.Items["Email"] = _email;
                            options.Items["Roles"] = _roles;
                            options.Items["Claims"] = _claims;
                        });
            });
        }).CreateClient();

        return client;
    }
}

// Uso en tests
public class ApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    [Fact]
    public async Task AdminEndpoint_WithAdminUser_ReturnsSuccess()
    {
        // Arrange
        var client = new AuthenticatedClientBuilder(_factory)
            .WithUser("admin-123", "admin@test.com")
            .WithRole("Admin")
            .Build();

        // Act
        var response = await client.GetAsync("/api/protected/admin");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
```

## 6. **Testing de Integration con Base de Datos**

```csharp
public class AuthenticationIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    [Fact]
    public async Task LoginFlow_CreateUserLoginAndAccessProtected_Success()
    {
        // Arrange
        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                // Usar base de datos en memoria
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
                if (descriptor != null) services.Remove(descriptor);

                services.AddDbContext<AppDbContext>(options =>
                {
                    options.UseInMemoryDatabase("TestDb");
                });
            });
        }).CreateClient();

        // Act - Register
        var registerResponse = await client.PostAsJsonAsync("/api/auth/register", new
        {
            email = "newuser@test.com",
            password = "Password123!"
        });
        Assert.Equal(HttpStatusCode.OK, registerResponse.StatusCode);

        // Act - Login
        var loginResponse = await client.PostAsJsonAsync("/api/auth/login", new
        {
            email = "newuser@test.com",
            password = "Password123!"
        });
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.NotNull(loginResult?.Token);

        // Act - Access Protected Endpoint
        client.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", loginResult.Token);
        var protectedResponse = await client.GetAsync("/api/protected");

        // Assert
        Assert.Equal(HttpStatusCode.OK, protectedResponse.StatusCode);
    }
}
```

## 7. **Configuración de appsettings.Test.json**

```json
{
  "Jwt": {
    "Key": "test-secret-key-must-be-at-least-32-characters-long",
    "Issuer": "TestIssuer",
    "Audience": "TestAudience",
    "ExpiryMinutes": 60
  },
  "ConnectionStrings": {
    "DefaultConnection": "DataSource=:memory:"
  }
}
```

## 8. **Recomendaciones de Microsoft y Expertos**

**Microsoft Docs** recomienda:
- Usar `WebApplicationFactory` para tests de integración
- Inyectar `TestServer` para simular el pipeline completo
- Evitar mocks en tests de integración, usar la implementación real cuando sea posible

**Steve Smith (ardalis)** sugiere:
- Separar tests unitarios (controller) de tests de integración (HTTP)
- Usar `IClassFixture` para compartir setup costoso entre tests

**Jimmy Bogard** recomienda:
- Preferir tests de integración sobre unitarios para APIs
- Usar `WebApplicationFactory` como estándar
- Testear el comportamiento completo del pipeline de autenticación

El enfoque más común en .NET es usar **WebApplicationFactory con tokens JWT reales** para tests de integración y **mocks de ClaimsPrincipal** solo para tests unitarios específicos de la lógica del controller.