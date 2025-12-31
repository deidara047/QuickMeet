Basándome en tu código y estructura de tests unitarios, te recomiendo este enfoque para integration testing:

## Estrategia de Integration Testing

### 1. **WebApplicationFactory con Base de Datos en Memoria**

```csharp
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace QuickMeet.IntegrationTests;

public class QuickMeetWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remover DbContext real
            services.RemoveAll(typeof(DbContextOptions<ApplicationDbContext>));
            
            // Agregar DbContext en memoria
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseInMemoryDatabase("TestDatabase");
            });

            // Construir ServiceProvider
            var sp = services.BuildServiceProvider();

            // Crear scope y obtener DbContext
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            
            // Asegurar que la BD está creada
            db.Database.EnsureCreated();
        });
    }
}
```

### 2. **Clase Base para Integration Tests**

```csharp
namespace QuickMeet.IntegrationTests;

public abstract class IntegrationTestBase : IClassFixture<QuickMeetWebApplicationFactory>
{
    protected readonly HttpClient Client;
    protected readonly QuickMeetWebApplicationFactory Factory;

    protected IntegrationTestBase(QuickMeetWebApplicationFactory factory)
    {
        Factory = factory;
        Client = factory.CreateClient();
        
        // Limpiar la BD antes de cada test
        ResetDatabase();
    }

    protected void ResetDatabase()
    {
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        db.Database.EnsureDeleted();
        db.Database.EnsureCreated();
    }

    protected async Task<T> GetFromDatabase<T>(Func<ApplicationDbContext, Task<T>> query)
    {
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        return await query(db);
    }

    protected async Task SeedDatabase(Action<ApplicationDbContext> seed)
    {
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        seed(db);
        await db.SaveChangesAsync();
    }
}
```

### 3. **Tests de Integration para AuthController**

```csharp
using System.Net;
using System.Net.Http.Json;
using QuickMeet.API.DTOs.Auth;
using Xunit;

namespace QuickMeet.IntegrationTests.Controllers;

public class AuthControllerTests : IntegrationTestBase
{
    public AuthControllerTests(QuickMeetWebApplicationFactory factory) : base(factory) { }

    #region Register Integration Tests

    [Fact]
    public async Task Register_ValidData_ReturnsOkWithTokens()
    {
        // Arrange
        var request = new RegisterRequest(
            Email: "newuser@example.com",
            Username: "newuser",
            FullName: "New User",
            Password: "SecurePass123!@",
            PasswordConfirmation: "SecurePass123!@"
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

        // Verificar que el provider está en la BD
        var providerExists = await GetFromDatabase(async db =>
            await db.Providers.AnyAsync(p => p.Email == request.Email));
        Assert.True(providerExists);
    }

    [Fact]
    public async Task Register_DuplicateEmail_ReturnsBadRequest()
    {
        // Arrange - Crear usuario existente
        await SeedDatabase(db =>
        {
            db.Providers.Add(new Provider
            {
                Email = "existing@example.com",
                Username = "existinguser",
                FullName = "Existing User",
                PasswordHash = "hashed",
                Status = ProviderStatus.Active
            });
        });

        var request = new RegisterRequest(
            Email: "existing@example.com",
            Username: "newuser",
            FullName: "New User",
            Password: "SecurePass123!@",
            PasswordConfirmation: "SecurePass123!@"
        );

        // Act
        var response = await Client.PostAsJsonAsync("/api/auth/register", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Register_DuplicateUsername_ReturnsBadRequest()
    {
        // Arrange
        await SeedDatabase(db =>
        {
            db.Providers.Add(new Provider
            {
                Email = "existing@example.com",
                Username: "existinguser",
                FullName = "Existing User",
                PasswordHash = "hashed",
                Status = ProviderStatus.Active
            });
        });

        var request = new RegisterRequest(
            Email: "newemail@example.com",
            Username: "existinguser",
            FullName: "New User",
            Password: "SecurePass123!@",
            PasswordConfirmation: "SecurePass123!@"
        );

        // Act
        var response = await Client.PostAsJsonAsync("/api/auth/register", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    #endregion

    #region Login Integration Tests

    [Fact]
    public async Task Login_ValidCredentials_ReturnsOkWithTokens()
    {
        // Arrange - Primero registrar un usuario
        var registerRequest = new RegisterRequest(
            Email: "logintest@example.com",
            Username: "loginuser",
            FullName: "Login User",
            Password: "SecurePass123!@",
            PasswordConfirmation: "SecurePass123!@"
        );
        await Client.PostAsJsonAsync("/api/auth/register", registerRequest);

        var loginRequest = new LoginRequest(
            Email: "logintest@example.com",
            Password: "SecurePass123!@"
        );

        // Act
        var response = await Client.PostAsJsonAsync("/api/auth/login", loginRequest);
        var result = await response.Content.ReadFromJsonAsync<AuthResponse>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(result);
        Assert.Equal(loginRequest.Email, result.Email);
        Assert.NotEmpty(result.AccessToken);
        Assert.NotEmpty(result.RefreshToken);
    }

    [Fact]
    public async Task Login_InvalidCredentials_ReturnsUnauthorized()
    {
        // Arrange
        var loginRequest = new LoginRequest(
            Email: "nonexistent@example.com",
            Password: "WrongPassword123!@"
        );

        // Act
        var response = await Client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_SuspendedAccount_ReturnsUnauthorized()
    {
        // Arrange - Crear usuario suspendido directamente en BD
        await SeedDatabase(db =>
        {
            var passwordHasher = new PasswordHashingService(); // Necesitarás instanciar el real
            db.Providers.Add(new Provider
            {
                Email = "suspended@example.com",
                Username = "suspendeduser",
                FullName = "Suspended User",
                PasswordHash = passwordHasher.HashPassword("SecurePass123!@"),
                Status = ProviderStatus.Suspended
            });
        });

        var loginRequest = new LoginRequest(
            Email: "suspended@example.com",
            Password: "SecurePass123!@"
        );

        // Act
        var response = await Client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    #endregion

    #region Email Verification Integration Tests

    [Fact]
    public async Task VerifyEmail_ValidToken_ReturnsOk()
    {
        // Arrange - Crear provider y token
        var token = Guid.NewGuid().ToString();
        await SeedDatabase(db =>
        {
            var provider = new Provider
            {
                Email = "verify@example.com",
                Username = "verifyuser",
                FullName = "Verify User",
                PasswordHash = "hashed",
                Status = ProviderStatus.PendingVerification
            };
            db.Providers.Add(provider);
            db.SaveChanges();

            db.EmailVerificationTokens.Add(new EmailVerificationToken
            {
                ProviderId = provider.Id,
                Token = token,
                ExpiresAt = DateTime.UtcNow.AddHours(24),
                IsUsed = false
            });
        });

        var request = new VerifyEmailRequest(Token: token);

        // Act
        var response = await Client.PostAsJsonAsync("/api/auth/verify-email", request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // Verificar que el status cambió a Active
        var provider = await GetFromDatabase(async db =>
            await db.Providers.FirstOrDefaultAsync(p => p.Email == "verify@example.com"));
        Assert.Equal(ProviderStatus.Active, provider?.Status);
    }

    [Fact]
    public async Task VerifyEmail_ExpiredToken_ReturnsBadRequest()
    {
        // Arrange
        var token = Guid.NewGuid().ToString();
        await SeedDatabase(db =>
        {
            var provider = new Provider
            {
                Email = "expired@example.com",
                Username = "expireduser",
                FullName = "Expired User",
                PasswordHash = "hashed",
                Status = ProviderStatus.PendingVerification
            };
            db.Providers.Add(provider);
            db.SaveChanges();

            db.EmailVerificationTokens.Add(new EmailVerificationToken
            {
                ProviderId = provider.Id,
                Token = token,
                ExpiresAt = DateTime.UtcNow.AddHours(-1), // Token expirado
                IsUsed = false
            });
        });

        var request = new VerifyEmailRequest(Token: token);

        // Act
        var response = await Client.PostAsJsonAsync("/api/auth/verify-email", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    #endregion

    #region End-to-End Scenarios

    [Fact]
    public async Task Scenario_RegisterAndLoginImmediately_Success()
    {
        // Arrange
        var registerRequest = new RegisterRequest(
            Email: "scenario@example.com",
            Username: "scenariouser",
            FullName: "Scenario User",
            Password: "SecurePass123!@",
            PasswordConfirmation: "SecurePass123!@"
        );

        // Act 1 - Register
        var registerResponse = await Client.PostAsJsonAsync("/api/auth/register", registerRequest);
        var registerResult = await registerResponse.Content.ReadFromJsonAsync<AuthResponse>();

        // Act 2 - Login
        var loginRequest = new LoginRequest(
            Email: registerRequest.Email,
            Password: registerRequest.Password
        );
        var loginResponse = await Client.PostAsJsonAsync("/api/auth/login", loginRequest);
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<AuthResponse>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, registerResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
        Assert.NotNull(registerResult);
        Assert.NotNull(loginResult);
        Assert.Equal(registerResult.Email, loginResult.Email);
        // Los tokens deberían ser diferentes
        Assert.NotEqual(registerResult.AccessToken, loginResult.AccessToken);
    }

    #endregion
}
```

### 4. **Configuración en el proyecto de tests**

```xml
<!-- QuickMeet.IntegrationTests.csproj -->
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <IsPackable>false</IsPackable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.AspNetCore.Mvc.Testing" Version="8.0.0" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.InMemory" Version="8.0.0" />
    <PackageReference Include="xunit" Version="2.6.2" />
    <PackageReference Include="xunit.runner.visualstudio" Version="2.5.4" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\QuickMeet.API\QuickMeet.API.csproj" />
  </ItemGroup>
</Project>
```

## Diferencias clave: Unit vs Integration Testing

**Unit Tests** (lo que ya tienes):
- Usan mocks puros (Moq)
- Prueban lógica de negocio aislada
- Son rápidos y no tocan infraestructura
- Se enfocan en un solo componente

**Integration Tests** (lo que te recomiendo):
- Usan BD en memoria (InMemory o Testcontainers)
- Prueban el flujo completo HTTP → Controller → Service → Repository → BD
- Verifican que los componentes funcionan juntos
- Prueban escenarios end-to-end

Esta estructura te permite mantener tus unit tests rápidos y enfocados, mientras que los integration tests validan que todo el sistema funciona correctamente integrado.