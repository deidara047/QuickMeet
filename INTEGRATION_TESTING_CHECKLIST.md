# Integration Testing Implementation Checklist

**Proyecto:** QuickMeet  
**Objetivo:** Implementar integration tests para Auth Controller  
**Duración total:** ~1h 45min  
**Fecha inicio:** 31 Dic 2025  
**Status:** 🟢 READY TO START

---

## FASE 1: SETUP CRÍTICO (20 min)

### PASO 1a: Dependencias + Program.cs público (10 min)

**Objetivo:** Agregar dependencias necesarias para integration testing y hacer Program.cs público

#### 1️⃣ Editar archivo: `backend/tests/QuickMeet.IntegrationTests/QuickMeet.IntegrationTests.csproj`

- [x] Abrir archivo en editor
- [x] Localizar sección `<ItemGroup>` con `<PackageReference>`
- [x] Agregar estas 4 líneas:

```xml
<PackageReference Include="Microsoft.AspNetCore.Mvc.Testing" Version="10.0.1" />
<PackageReference Include="Microsoft.EntityFrameworkCore.InMemory" Version="10.0.1" />
<PackageReference Include="xunit" Version="2.6.6" />
<PackageReference Include="xunit.runner.visualstudio" Version="2.5.6" />
```

- [x] Guardar archivo

#### 2️⃣ Editar archivo: `backend/src/QuickMeet.API/Program.cs`

- [x] Abrir archivo
- [x] Ir a la **MÁS FINAL** del archivo (después de `app.Run();`)
- [x] Agregar esta línea:

```csharp
public partial class Program { }
```

- [x] Guardar archivo

#### ✅ Criterio de éxito PASO 1a:
- [x] QuickMeet.IntegrationTests.csproj tiene 4 nuevas dependencias
- [x] Program.cs tiene `public partial class Program { }` al final
- [x] Ambos archivos guardados

**Red Flags:**
- ⚠️ Si olvidaste `public partial class Program { }` → WebApplicationFactory no compilará
- ⚠️ Si las versiones no coinciden con `net10.0` → posibles conflictos de dependencia

---

### PASO 1b: Validación de compilación (10 min) [BLOQUEANTE ❌]

**Objetivo:** Verificar que los cambios permiten compilación sin errores

#### 📍 Ubicación: Terminal PowerShell

```powershell
cd C:\Users\luisr\Documents\Proyectos\ProyectoCitas\backend
```

#### Comando 1️⃣: Compilar API

```powershell
dotnet build src/QuickMeet.API
```

- [x] Ejecutar comando
- [x] Esperar resultado
- [x] Status esperado: ✅ **BUILD SUCCEEDED**

**Si falla:** 
```
STOP → Revisar error → Probable causa: Program.cs no público
Solución: Asegurar que Program.cs termina con `public partial class Program { }`
```

#### Comando 2️⃣: Compilar Integration Tests

```powershell
dotnet build tests/QuickMeet.IntegrationTests
```

- [x] Ejecutar comando
- [x] Esperar resultado
- [x] Status esperado: ✅ **BUILD SUCCEEDED**

**Si falla:**
```
STOP → Revisar error → Probable causa: dependencias no agregadas o mal formateadas
Solución: Revisar QuickMeet.IntegrationTests.csproj, asegurar XML válido
```

#### Comando 3️⃣: Agregar referencia de proyecto

```powershell
dotnet add tests/QuickMeet.IntegrationTests reference src/QuickMeet.API
```

- [x] Ejecutar comando
- [x] Esperar resultado
- [x] Status esperado: ✅ **SUCCESS**

#### ✅ Criterio de éxito PASO 1b:
- [x] `dotnet build src/QuickMeet.API` → ✅ SUCCESS
- [x] `dotnet build tests/QuickMeet.IntegrationTests` → ✅ SUCCESS
- [x] `dotnet add reference` → ✅ SUCCESS

**Red Flags:**
- ⚠️ `Type 'Program' not found` → Program.cs no es público
- ⚠️ `NuGet packages not found` → Versiones de paquetes incompatibles
- ⚠️ `Project reference not found` → Path incorrecto en reference

**⚠️ SI ALGO FALLA EN PASO 1b: NO CONTINÚES A FASE 2**

---

## FASE 2: INFRAESTRUCTURA (20 min)

### PASO 2: WebApplicationFactory (12 min)

**Objetivo:** Crear factory que use InMemory DB en lugar de SQL Server

#### 1️⃣ Crear carpeta

```
backend/tests/QuickMeet.IntegrationTests/Fixtures/
```

- [x] Click derecho en QuickMeet.IntegrationTests
- [x] New Folder → `Fixtures`

#### 2️⃣ Crear archivo: `QuickMeetWebApplicationFactory.cs`

- [x] Click derecho en carpeta Fixtures
- [x] New File → `QuickMeetWebApplicationFactory.cs`

#### 3️⃣ Copiar contenido (ver bloque de código abajo)

```csharp
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Moq;
using QuickMeet.Core.Interfaces;
using QuickMeet.Infrastructure.Data;

namespace QuickMeet.IntegrationTests.Fixtures;

public class QuickMeetWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.RemoveAll(typeof(DbContextOptions<QuickMeetDbContext>));

            services.AddDbContext<QuickMeetDbContext>(options =>
            {
                options.UseInMemoryDatabase("QuickMeetTestDb");
            });

            services.RemoveAll<IEmailService>();
            services.AddSingleton<IEmailService>(new Mock<IEmailService>().Object);

            var sp = services.BuildServiceProvider();

            using (var scope = sp.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<QuickMeetDbContext>();
                db.Database.EnsureCreated();
            }
        });

        builder.ConfigureLogging(logging =>
        {
            logging.ClearProviders();
            logging.SetMinimumLevel(LogLevel.Warning);
        });
    }
}
```

- [x] Pegar contenido en el archivo
- [x] Guardar

#### ✅ Criterio de éxito PASO 2:
- [x] Archivo `QuickMeetWebApplicationFactory.cs` existe en `Fixtures/`
- [x] Compila sin errores: `dotnet build tests/QuickMeet.IntegrationTests`

**Red Flags:**
- ⚠️ `QuickMeetDbContext not found` → Revisar usings, debe estar en QuickMeet.Infrastructure.Data
- ⚠️ `Mock not available` → Asegurar que Moq está en dependencias
- ⚠️ `InMemoryDatabase not found` → Asegurar que EntityFrameworkCore.InMemory está instalado

---

### PASO 3: IntegrationTestBase (8 min)

**Objetivo:** Crear clase base con helpers para tests

#### 1️⃣ Crear carpeta

```
backend/tests/QuickMeet.IntegrationTests/Common/
```

- [x] Click derecho en QuickMeet.IntegrationTests
- [x] New Folder → `Common`

#### 2️⃣ Crear archivo: `IntegrationTestBase.cs`

- [x] Click derecho en carpeta Common
- [x] New File → `IntegrationTestBase.cs`

#### 3️⃣ Copiar contenido

```csharp
using QuickMeet.Infrastructure.Data;
using QuickMeet.IntegrationTests.Fixtures;

namespace QuickMeet.IntegrationTests.Common;

public abstract class IntegrationTestBase : IClassFixture<QuickMeetWebApplicationFactory>
{
    protected readonly HttpClient Client;
    protected readonly QuickMeetWebApplicationFactory Factory;

    protected IntegrationTestBase(QuickMeetWebApplicationFactory factory)
    {
        Factory = factory;
        Client = factory.CreateClient();
        ResetDatabase();
    }

    protected void ResetDatabase()
    {
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<QuickMeetDbContext>();
        db.Database.EnsureDeleted();
        db.Database.EnsureCreated();
    }

    protected async Task<T> GetFromDatabase<T>(Func<QuickMeetDbContext, Task<T>> query)
    {
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<QuickMeetDbContext>();
        return await query(db);
    }

    protected async Task SeedDatabase(Action<QuickMeetDbContext> seed)
    {
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<QuickMeetDbContext>();
        seed(db);
        await db.SaveChangesAsync();
    }

    protected async Task AuthenticatedRequest(
        HttpMethod method,
        string url,
        object? body = null,
        string? token = null)
    {
        // Phase 2 - Implementar cuando necesites tests con autenticación
        throw new NotImplementedException("Fase 2 - Por implementar");
    }
}
```

- [x] Pegar contenido
- [x] Guardar

#### ✅ Criterio de éxito PASO 3:
- [x] Archivo `IntegrationTestBase.cs` existe en `Common/`
- [x] Compila sin errores: `dotnet build tests/QuickMeet.IntegrationTests`

**Red Flags:**
- ⚠️ `IClassFixture not found` → Asegurar que xunit está en dependencias
- ⚠️ `QuickMeetDbContext not found` → Revisar usings
- ⚠️ `ServiceScope not working` → Asegurar que Factory.Services está disponible

---

## FASE 3: VALIDACIÓN INCREMENTAL (55 min)

### PASO 4a: Test Simple - Register Happy Path (7 min)

**Objetivo:** Crear PRIMER test para validar que el setup funciona

#### 1️⃣ Crear carpeta

```
backend/tests/QuickMeet.IntegrationTests/Controllers/
```

- [ ] Click derecho en QuickMeet.IntegrationTests
- [ ] New Folder → `Controllers`

#### 2️⃣ Crear archivo: `AuthControllerIntegrationTests.cs`

- [ ] Click derecho en carpeta Controllers
- [ ] New File → `AuthControllerIntegrationTests.cs`

#### 3️⃣ Copiar contenido - PRIMER TEST SOLAMENTE

```csharp
using System.Net;
using System.Net.Http.Json;
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
}
```

- [ ] Pegar contenido
- [ ] Guardar

#### 4️⃣ Ejecutar test

```powershell
cd C:\Users\luisr\Documents\Proyectos\ProyectoCitas\backend
dotnet test --filter "Register_ValidData_ReturnsOkWithTokens"
```

- [ ] Ejecutar comando
- [ ] Esperar resultado

#### ✅ Criterio de éxito PASO 4a:
- [ ] Test **PASA** ✅ (Status esperado: `1 passed`)

**Si FALLA:**

| Error | Causa Probable | Acción |
|-------|----------------|--------|
| `Type 'Program' not found` | Program.cs no público | Agregar `public partial class Program { }` |
| `DbContext initialization error` | InMemory DB no se crea | Revisar Factory.ConfigureServices |
| `404 Not Found` | Endpoint no existe | Verificar que AuthController tiene `[HttpPost("register")]` |
| `Null reference exception` | Mock de IEmailService falla | Revisar que Mock está correctamente inyectado |

**Red Flags:**
- ⚠️ Si test falla → NO CONTINÚES a PASO 4b
- ⚠️ Debuggea aquí, el resto de tests dependen de este setup

---

### PASO 4b: Test Crítico - Password Hashing (8 min)

**Objetivo:** Validar que passwords NO se guardan en plaintext (SEGURIDAD 🔒)

#### 1️⃣ Agregar test en `AuthControllerIntegrationTests.cs`

Después de la sección `#region Register Tests - Happy Path`, agregar:

```csharp
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
```

- [ ] Agregar código
- [ ] Guardar archivo

#### 2️⃣ Ejecutar test

```powershell
dotnet test --filter "PasswordNotStoredInPlainText"
```

- [ ] Ejecutar comando
- [ ] Esperar resultado

#### ✅ Criterio de éxito PASO 4b:
- [ ] Test **PASA** ✅ (Status esperado: `1 passed`)

**Si FALLA:**

| Error | Causa Probable | Acción |
|-------|----------------|--------|
| `Assert.NotEqual failed` | Password está en plaintext | Revisar PasswordHashingService en Program.cs |
| `Null reference` | Provider no creado | Revisar que Register funciona (PASO 4a debería pasar) |

**Red Flags:**
- ⚠️ 🔒 **CRÍTICO:** Este test valida seguridad
- ⚠️ Si falla → No pasar a producción hasta arreglar

---

### PASO 5: Tests Auth Completos (30 min)

**Objetivo:** Agregar todos los tests restantes (13 tests en total)

#### 1️⃣ Agregar REGISTER TESTS (4 tests adicionales)

En `AuthControllerIntegrationTests.cs`, después de `#region Register Tests - Security`, agregar:

```csharp
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
```

- [ ] Agregar 4 tests de Register
- [ ] Guardar archivo

#### 2️⃣ Agregar LOGIN TESTS (4 tests)

Agregar al final de la clase:

```csharp
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
    // Arrange - Crear provider suspendido directamente en BD
    await SeedDatabase(db =>
    {
        var provider = new QuickMeet.Core.Entities.Provider
        {
            Email = "suspended@example.com",
            Username = "suspendeduser",
            FullName = "Suspended User",
            PasswordHash = "hashed_password",
            Status = QuickMeet.Core.Entities.ProviderStatus.Suspended,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        db.Providers.Add(provider);
    });

    var loginRequest = new LoginRequest(
        Email: "suspended@example.com",
        Password: "AnyPassword123!@"
    );

    // Act
    var response = await Client.PostAsJsonAsync("/api/auth/login", loginRequest);

    // Assert
    Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
}

#endregion
```

- [ ] Agregar 4 tests de Login
- [ ] Guardar archivo

#### 3️⃣ Agregar VERIFY EMAIL TESTS (3 tests)

Agregar al final de la clase:

```csharp
#region VerifyEmail Tests

[Fact]
public async Task VerifyEmail_ValidToken_ReturnsOk()
{
    // Arrange
    var token = Guid.NewGuid().ToString();
    await SeedDatabase(db =>
    {
        var provider = new QuickMeet.Core.Entities.Provider
        {
            Email: "verify@example.com",
            Username: "verifyuser",
            FullName: "Verify User",
            PasswordHash: "hashed_password",
            Status = QuickMeet.Core.Entities.ProviderStatus.PendingVerification,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        db.Providers.Add(provider);
        
        var verificationToken = new QuickMeet.Core.Entities.EmailVerificationToken
        {
            ProviderId = provider.Id,
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddHours(24),
            IsUsed = false
        };
        db.EmailVerificationTokens.Add(verificationToken);
    });

    var request = new VerifyEmailRequest(Token: token);

    // Act
    var response = await Client.PostAsJsonAsync("/api/auth/verify-email", request);

    // Assert
    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
}

[Fact]
public async Task VerifyEmail_ExpiredToken_ReturnsBadRequest()
{
    // Arrange
    var token = Guid.NewGuid().ToString();
    await SeedDatabase(db =>
    {
        var provider = new QuickMeet.Core.Entities.Provider
        {
            Email = "expired@example.com",
            Username = "expireduser",
            FullName: "Expired User",
            PasswordHash: "hashed_password",
            Status = QuickMeet.Core.Entities.ProviderStatus.PendingVerification,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        db.Providers.Add(provider);
        
        var verificationToken = new QuickMeet.Core.Entities.EmailVerificationToken
        {
            ProviderId = provider.Id,
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddHours(-1), // Expirado
            IsUsed = false
        };
        db.EmailVerificationTokens.Add(verificationToken);
    });

    var request = new VerifyEmailRequest(Token: token);

    // Act
    var response = await Client.PostAsJsonAsync("/api/auth/verify-email", request);

    // Assert
    Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
}

[Fact]
public async Task VerifyEmail_InvalidToken_ReturnsBadRequest()
{
    // Arrange
    var request = new VerifyEmailRequest(Token: "invalid-token-that-does-not-exist");

    // Act
    var response = await Client.PostAsJsonAsync("/api/auth/verify-email", request);

    // Assert
    Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
}

#endregion
```

- [ ] Agregar 3 tests de VerifyEmail
- [ ] Guardar archivo

#### 4️⃣ Ejecutar todos los tests de Auth

```powershell
dotnet test --filter "IntegrationTests.Controllers"
```

- [ ] Ejecutar comando
- [ ] Esperar resultado

#### ✅ Criterio de éxito PASO 5:
- [ ] 13 tests PASAN (6 Register + 4 Login + 3 VerifyEmail)
- [ ] Status esperado: `13 passed`

**Red Flags:**
- ⚠️ Si algún test falla → Revisar el error específico
- ⚠️ `Null reference in SeedDatabase` → Revisar que Provider/EmailVerificationToken existen
- ⚠️ `404 endpoint not found` → Verificar que los endpoints existen en AuthController

---

## FASE 4: E2E TESTS (25 min)

### PASO 6: Escenarios End-to-End (15 min)

**Objetivo:** Probar flujos completos de usuario

#### 1️⃣ Crear archivo: `AuthControllerE2ETests.cs`

- [ ] Click derecho en carpeta Controllers
- [ ] New File → `AuthControllerE2ETests.cs`

#### 2️⃣ Copiar contenido

```csharp
using System.Net;
using System.Net.Http.Json;
using QuickMeet.API.DTOs.Auth;
using QuickMeet.IntegrationTests.Common;
using QuickMeet.IntegrationTests.Fixtures;
using Xunit;

namespace QuickMeet.IntegrationTests.Controllers;

public class AuthControllerE2ETests : IntegrationTestBase
{
    public AuthControllerE2ETests(QuickMeetWebApplicationFactory factory) : base(factory) { }

    #region E2E Scenarios

    [Fact]
    public async Task E2E_RegisterAndLoginImmediately_Success()
    {
        // Arrange
        var registerRequest = new RegisterRequest(
            Email: "e2e1@example.com",
            Username: "e2e1user",
            FullName: "E2E User One",
            Password: "ValidPassword123!@",
            PasswordConfirmation: "ValidPassword123!@"
        );

        // Act 1 - Register
        var registerResponse = await Client.PostAsJsonAsync("/api/auth/register", registerRequest);
        var registerResult = await registerResponse.Content.ReadFromJsonAsync<AuthResponse>();

        // Act 2 - Login inmediatamente
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
        Assert.NotEmpty(loginResult.AccessToken);
    }

    [Fact]
    public async Task E2E_RegisterVerifyEmailThenLogin_Success()
    {
        // Arrange
        var registerRequest = new RegisterRequest(
            Email: "e2e2@example.com",
            Username: "e2e2user",
            FullName: "E2E User Two",
            Password: "ValidPassword123!@",
            PasswordConfirmation: "ValidPassword123!@"
        );

        // Act 1 - Register
        var registerResponse = await Client.PostAsJsonAsync("/api/auth/register", registerRequest);

        // Act 2 - Obtener token de verificación desde BD
        var provider = await GetFromDatabase(async db =>
            await db.Providers.FirstOrDefaultAsync(p => p.Email == registerRequest.Email));
        var verificationToken = await GetFromDatabase(async db =>
            await db.EmailVerificationTokens
                .Where(t => t.ProviderId == provider!.Id)
                .FirstOrDefaultAsync());

        // Act 3 - Verificar email
        var verifyRequest = new VerifyEmailRequest(Token: verificationToken!.Token);
        var verifyResponse = await Client.PostAsJsonAsync("/api/auth/verify-email", verifyRequest);

        // Act 4 - Login después de verificar
        var loginRequest = new LoginRequest(
            Email: registerRequest.Email,
            Password: registerRequest.Password
        );
        var loginResponse = await Client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, registerResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, verifyResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
    }

    [Fact]
    public async Task E2E_LoginWithoutRegister_Fails()
    {
        // Arrange
        var loginRequest = new LoginRequest(
            Email: "neverregistered@example.com",
            Password: "AnyPassword123!@"
        );

        // Act
        var response = await Client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    #endregion
}
```

- [ ] Pegar contenido
- [ ] Guardar archivo

#### 3️⃣ Ejecutar tests E2E

```powershell
dotnet test --filter "E2E"
```

- [ ] Ejecutar comando
- [ ] Esperar resultado

#### ✅ Criterio de éxito PASO 6:
- [ ] 3 tests E2E PASAN
- [ ] Status esperado: `3 passed`

**Red Flags:**
- ⚠️ Si "RegisterVerifyEmailThenLogin" falla → Revisar que EmailVerificationTokens se crean correctamente
- ⚠️ Si navigation entre pasos falla → Revisar que cada paso retorna datos correctos

---

## FASE 5: VALIDACIÓN FINAL (15 min)

### PASO 7: Validación Final + Go/No-Go (15 min)

**Objetivo:** Ejecutar suite completa de tests y validar cobertura

#### 1️⃣ Ejecutar Unit Tests

```powershell
dotnet test --filter "UnitTests"
```

- [ ] Ejecutar comando
- [ ] Esperar resultado

**Criterio:**
```
Expectativa: 100% PASAN
Mínimo: > 90% cobertura
Tests: ~50+ tests
```

**Resultado:**
```
Status: __________ (escribir resultado)
Passed: ____ | Failed: ____ | Warnings: ____
```

- [ ] Anotar resultado

#### 2️⃣ Ejecutar Integration Tests

```powershell
dotnet test --filter "IntegrationTests"
```

- [ ] Ejecutar comando
- [ ] Esperar resultado

**Criterio:**
```
Expectativa: 100% PASAN
Mínimo: > 70% cobertura
Tests: 16 tests (13 Auth + 3 E2E)
```

**Resultado:**
```
Status: __________ (escribir resultado)
Passed: ____ | Failed: ____ | Warnings: ____
```

- [ ] Anotar resultado

#### 3️⃣ Ejecutar TODOS los tests

```powershell
dotnet test
```

- [ ] Ejecutar comando
- [ ] Esperar resultado

**Criterio:**
```
Expectativa: 100% PASAN sin errores
Tests totales: ~66+ tests
Warnings: 0
Skipped: 0
```

**Resultado:**
```
Status: __________ (escribir resultado)
Total Passed: ____ | Failed: ____ | Warnings: ____
```

- [ ] Anotar resultado

#### 4️⃣ Generar reporte de cobertura

```powershell
dotnet test /p:CollectCoverage=true /p:CoverageReportFormat=cobertura
```

- [ ] Ejecutar comando
- [ ] Esperar resultado
- [ ] Revisar archivo `coverage.cobertura.xml` generado

**Criterios de cobertura:**
```
AuthController:          > 80%
AuthenticationService:   > 90%
PasswordHashingService:  > 90%
TokenService:            > 90%
LoginRequestValidator:   > 85%
RegisterRequestValidator: > 85%
```

- [ ] Revisar cobertura individual de cada clase

#### ✅ Checklist Final PASO 7:

**Validación de tests:**
- [ ] Unit Tests: ✅ 100% PASAN
- [ ] Integration Tests: ✅ 100% PASAN
- [ ] E2E Tests: ✅ 100% PASAN
- [ ] Total: ✅ ~66+ tests PASAN

**Validación de calidad:**
- [ ] 0 warnings
- [ ] 0 errores
- [ ] 0 skipped tests
- [ ] Cobertura total > 85%

**Validación de seguridad:**
- [ ] ✅ Test de password hashing PASA
- [ ] ✅ No hay passwords en plaintext
- [ ] ✅ Todos los endpoints validados

---

## 🎯 GO / NO-GO DECISION

### ✅ Si TODO está VERDE:

```
┌─────────────────────────────────────────────┐
│   TODOS LOS TESTS PASAN ✅                  │
│   COBERTURA > 85% ✅                        │
│   0 WARNINGS / 0 ERRORES ✅                 │
│   SEGURIDAD VALIDADA ✅                     │
│                                              │
│   🚀 LISTO PARA PUSH                         │
│   🚀 LISTO PARA NEXT SPRINT                  │
└─────────────────────────────────────────────┘
```

**Próximos pasos:**
```powershell
# Commit y push
git add .
git commit -m "feat: integration testing para auth controller"
git push origin feature/integration-testing-auth
```

- [ ] Crear commit
- [ ] Hacer push a rama
- [ ] Abrir Pull Request

### ❌ Si ALGO está ROJO:

```
┌─────────────────────────────────────────────┐
│   REVISAR ERRORES                           │
│   1. Leer mensaje de error exacto            │
│   2. Identificar qué test falla              │
│   3. Consultar Red Flags correspondiente     │
│   4. Arreglar y rerun                        │
│   5. NO PUSHEAR hasta que TODO sea VERDE     │
└─────────────────────────────────────────────┘
```

---

## 📊 RESUMEN DE EJECUCIÓN

### Tiempo total estimado:
```
Fase 1: 20 min  ✓
Fase 2: 20 min  ✓
Fase 3: 55 min  ✓
Fase 4: 25 min  ✓
Fase 5: 15 min  ✓
────────────────────
TOTAL: 1h 45min ✓
```

### Tests creados:
```
Register tests:    6 tests
Login tests:       4 tests
VerifyEmail tests: 3 tests
E2E tests:         3 tests
────────────────────
TOTAL:            16 tests
```

### Archivos creados:
```
✓ QuickMeetWebApplicationFactory.cs
✓ IntegrationTestBase.cs
✓ AuthControllerIntegrationTests.cs
✓ AuthControllerE2ETests.cs
```

### Archivos editados:
```
✓ QuickMeet.IntegrationTests.csproj (4 dependencias)
✓ Program.cs (1 línea)
```

---

## 🚨 RED FLAGS RÁPIDA (Referencia)

| Error | Solución |
|-------|----------|
| `Type 'Program' not found` | Agregar `public partial class Program { }` en Program.cs |
| `BUILD FAILED` en PASO 1b | Revisar versiones de paquetes, deben ser v10.0.1 |
| `DbContext null` | Asegurar UseInMemoryDatabase en factory |
| `404 endpoint not found` | Verificar que AuthController existe y tiene rutas |
| `IEmailService null` | Mock no se inyectó, revisar factory RemoveAll/AddSingleton |
| `Password en plaintext` | PasswordHashingService no funciona, revisar Program.cs |
| `Test timeout` | BD no se limpia bien, revisar ResetDatabase |

---

## ✅ FINAL CHECKLIST

```
ANTES DE EMPEZAR:
├─ [ ] Entendí todo el plan
├─ [ ] Terminal lista en: C:\Users\luisr\...\backend
├─ [ ] Tengo 2 horas disponibles
└─ [ ] Este documento abierto para referencia

DURANTE EJECUCIÓN:
├─ [ ] PASO 1a → 1b → VERDE
├─ [ ] PASO 2 → 3 → VERDE
├─ [ ] PASO 4a → PASA
├─ [ ] PASO 4b → PASA
├─ [ ] PASO 5 → 13 PASAN
├─ [ ] PASO 6 → 3 PASAN
└─ [ ] PASO 7 → GO/NO-GO

DESPUÉS:
├─ [ ] dotnet test → ✅ TODOS PASAN
├─ [ ] Cobertura > 85%
├─ [ ] Commit + Push
└─ [ ] 🎉 SPRINT COMPLETADO
```

---

**Última actualización:** 31 Dic 2025  
**Status:** 🟢 READY TO EXECUTE  
**Autor:** Integration Testing Plan v1.0