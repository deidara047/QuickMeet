# E2E Testing Setup - Sprint 1

**Arquitectura de testing industrial** siguiendo estándares de .NET Core + Angular:
- Compilación condicional (`#if DEBUG`) para endpoints de testing
- Separación de ambientes mediante `appsettings.{Environment}.json`
- Datos dinámicos + Page Objects + Fixtures custom
- Protección en 3 capas: compilación, configuración, runtime

---

## Fase 0: Setup de Ambientes (.NET + Angular)

**Duración**: 60-75 minutos
**Estado**: COMPLETADA

### 0.1 Backend - Configuración de Entornos

#### Objetivos
- [x] Crear base de datos `QuickMeet_Test` en SQL Server
- [x] Crear `appsettings.Test.json` (separado de Development)
- [x] Aplicar migraciones a DB de test
- [x] Validar que `Program.cs` carga archivos por entorno automáticamente
- [x] Revisar logging por entorno

#### Paso 1: Crear base de datos de test

**Ejecutar en SQL Server Management Studio o sqlcmd:**
```sql
USE master;
GO

-- Crear DB de testing (separada de Development y Production)
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'QuickMeet_Test')
BEGIN
    CREATE DATABASE QuickMeet_Test;
    PRINT '✅ Base de datos QuickMeet_Test creada';
END
ELSE
BEGIN
    PRINT '⚠️ QuickMeet_Test ya existe';
END
GO

-- Verificar creación
USE QuickMeet_Test;
GO
SELECT DB_NAME() AS CurrentDatabase;
GO
```

**Validación:**
- [ ] DB `QuickMeet_Test` existe en SQL Server
- [ ] No contiene tablas todavía (las migraciones se aplican después)

#### Paso 2: Crear appsettings.Test.json

**Archivo**: `backend/src/QuickMeet.API/appsettings.Test.json`

```json
{
  "AllowDangerousOperations": true,
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=QuickMeet_Test;Integrated Security=true;Encrypt=false;TrustServerCertificate=true;"
  },
  "Cors": {
    "AllowedOrigins": ["http://localhost:4200", "http://localhost:4201"]
  },
  "Jwt": {
    "SecretKey": "test-secret-key-minimum-32-characters-for-testing-only",
    "Issuer": "QuickMeet.Test",
    "Audience": "QuickMeet.Test.Users",
    "ExpirationMinutes": 60
  },
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.AspNetCore": "Warning",
      "QuickMeet": "Debug"
    }
  }
}
```

**Puntos clave:**
- DB: `QuickMeet_Test` (nunca modifiques Dev o Prod)
- `AllowDangerousOperations: true` será verificado en TestController constructor
- Logging: Debug level para visibilidad en testing
- CORS: Abierto a localhost:4200 y :4201 para E2E testing
- JWT: Secrets simples para testing (no usar en producción)

#### Paso 3: Aplicar migraciones a DB de test

```bash
# Asegurarte de estar en la carpeta raíz del proyecto backend
cd backend

# Aplicar migraciones a QuickMeet_Test
set ASPNETCORE_ENV=Test
dotnet ef database update --project src/QuickMeet.Infrastructure --startup-project src/QuickMeet.API

# Verificar que las tablas se crearon
```

**Validación en SQL Server:**
```sql
USE QuickMeet_Test;
GO

-- Verificar tablas creadas
SELECT TABLE_NAME 
FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_TYPE = 'BASE TABLE'
ORDER BY TABLE_NAME;

-- Deberías ver: Providers, __EFMigrationsHistory, etc.
```

#### Paso 4: Validar carga de ambientes

**Carga automática en .NET:**
```csharp
// Program.cs (ya implementado automáticamente)
// builder.Configuration carga:
// 1. appsettings.json (base)
// 2. appsettings.{ASPNETCORE_ENV}.json (overrides)
// Ejemplo: ASPNETCORE_ENV=Test → carga appsettings.Test.json
```

**Diferencia CRUCIAL: Compile-time vs Runtime**
- `#if DEBUG` (Fase 1) → **Compile-time**: TestController NO EXISTE en binario Release
- `ASPNETCORE_ENV` (Fase 0) → **Runtime**: Cargar configuración diferente
- ✅ Seguridad by design: #if DEBUG (imposible hackear en producción)
- ⚠️ Seguridad by convention: Checks en runtime (necesario para config)

**Comandos para validar:**
```bash
# Desarrollo
set ASPNETCORE_ENV=Development
dotnet run --project src/QuickMeet.API
# → Carga appsettings.Development.json, usa QuickMeet_Dev

# Testing
set ASPNETCORE_ENV=Test
dotnet run --project src/QuickMeet.API
# → Carga appsettings.Test.json, usa QuickMeet_Test
```

#### Validación de Fase 0.1
- [ ] `dotnet run` con `ASPNETCORE_ENV=Test` no genera errores
- [ ] DB de test es diferente a Dev (verificar connection string)
- [ ] `AllowDangerousOperations: true` en appsettings.Test.json
- [ ] En appsettings.Production.json: `AllowDangerousOperations` está **ausente o false**
- [ ] Migraciones aplicadas correctamente (tablas existen en QuickMeet_Test)

---

### 0.2 Frontend - Variables de Entorno

#### Objetivos
- [ ] Definir estrategia de archivos .env
- [ ] Crear/actualizar `.env.test`
- [ ] Verificar que Angular lee las variables con prefijo `NG_APP_`
- [ ] Asegurar que API URL apunta a localhost:5173

#### Estrategia de archivos .env

**Estructura final:**
```
frontend/
├── .env                    # git ignored, desarrollo local (copia manual de .env.example)
├── .env.example            # git tracked, template general
├── .env.test               # git tracked, valores específicos para E2E testing
└── .env.example.prod       # git tracked, template para producción
```

**Scripts en package.json:**
```json
{
  "scripts": {
    "start": "ng serve",
    "start:test": "ng serve --port 4201"
  }
}
```

**Nota**: Angular carga automáticamente `.env` en desarrollo. Para testing, Playwright configurará las variables necesarias.

#### Paso 1: Crear .env.test

**Archivo**: `frontend/.env.test`

```bash
# Ambiente de testing E2E
NG_APP_ENVIRONMENT=test
NG_APP_API_URL=http://localhost:5173/api
NG_APP_API_TIMEOUT=30000
NG_APP_LOG_LEVEL=debug
NG_APP_ENABLE_DEBUG_PANEL=true
```

#### Paso 2: Actualizar .env.example

**Archivo**: `frontend/.env.example`

```bash
# Desarrollo local
NG_APP_ENVIRONMENT=development
NG_APP_API_URL=http://localhost:5173/api
NG_APP_API_TIMEOUT=30000
NG_APP_LOG_LEVEL=info
NG_APP_ENABLE_DEBUG_PANEL=true

# INSTRUCCIONES:
# 1. Copiar este archivo a .env (git ignored)
# 2. Ajustar valores según tu configuración local
# 3. Para testing E2E, usar .env.test
```

#### Validación de Fase 0.2
- [ ] `.env` existe (git ignored, copia de .env.example)
- [ ] `.env.test` existe (git tracked)
- [ ] `.env.example` actualizado con todas las variables
- [ ] `.gitignore` incluye `.env` pero NO `.env.test`
- [ ] Variables tienen prefijo `NG_APP_`

---

## Fase 1: Backend Testing Endpoints (2-3 horas)

### ⚠️ CONCEPTO CLAVE: `#if DEBUG` es Compile-time, No Runtime

**TestController NO EXISTE en binario Release:**
```
Debug build (tu laptop):
  - #if DEBUG evaluado como TRUE
  - TestController SE COMPILA y existe
  - /api/test/* disponibles ✅

Release build (producción):
  - #if DEBUG evaluado como FALSE
  - TestController NO SE COMPILA
  - /api/test/* devuelve 404 automático ❌
  - Imposible de hackear (no existe en el binario)
```

### Objetivos
- [x] Crear `TestController.cs` con `#if DEBUG ... #endif`
- [x] Crear `RequireDangerousOperationsAttribute.cs` como filtro de autorización
- [x] Usar `ILogger<T>` para structured logging
- [x] `[ApiExplorerSettings(IgnoreApi = true)]` para no aparecer en Swagger
- [x] Implementar `POST /api/test/seed-user` completo
- [x] Implementar `DELETE /api/test/cleanup-user/{email}` completo
- [x] Implementar `POST /api/test/reset-database` completo
- [x] Crear DTOs necesarios

### Archivos a crear/modificar
- [x] `backend/src/QuickMeet.API/Controllers/TestController.cs` (nuevo)
- [x] `backend/src/QuickMeet.API/Filters/RequireDangerousOperationsAttribute.cs` (nuevo)
- [x] `backend/src/QuickMeet.API/DTOs/Auth/TestDtos.cs` (nuevo)

### Implementación completa

#### RequireDangerousOperationsAttribute.cs

**Archivo**: `backend/src/QuickMeet.API/Filters/RequireDangerousOperationsAttribute.cs`

```csharp
#if DEBUG
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace QuickMeet.API.Filters
{
    /// <summary>
    /// Filtro de autorización para endpoints de testing.
    /// Solo permite acceso cuando AllowDangerousOperations=true en Development.
    /// En Release, esta clase no se compila gracias a #if DEBUG.
    /// </summary>
    public class RequireDangerousOperationsAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var config = context.HttpContext.RequestServices
                .GetRequiredService<IConfiguration>();
            var env = context.HttpContext.RequestServices
                .GetRequiredService<IWebHostEnvironment>();
            var logger = context.HttpContext.RequestServices
                .GetRequiredService<ILogger<RequireDangerousOperationsAttribute>>();

            var allowDangerous = config.GetValue<bool>("AllowDangerousOperations");
            var path = context.HttpContext.Request.Path;
            var method = context.HttpContext.Request.Method;

            if (!allowDangerous || !env.IsDevelopment())
            {
                logger.LogWarning(
                    "Blocked dangerous operation attempt. AllowDangerous={Allow}, Environment={Env}, " +
                    "Path={Path}, Method={Method}",
                    allowDangerous,
                    env.EnvironmentName,
                    path,
                    method);

                context.Result = new NotFoundResult();
                return;
            }

            logger.LogDebug(
                "Dangerous operation allowed. Path={Path}, Method={Method}",
                path,
                method);

            base.OnActionExecuting(context);
        }
    }
}
#endif
```

**Características:**
- `#if DEBUG`: No se compila en Release
- Doble verificación: `AllowDangerousOperations` + `IsDevelopment()`
- Retorna 404 silenciosamente si está bloqueado (no expone existencia del endpoint)
- Logger tipado para mejor debugging
- Registra Path y Method para auditoría

#### TestController.cs

**Archivo**: `backend/src/QuickMeet.API/Controllers/TestController.cs`

```csharp
#if DEBUG
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuickMeet.Core.Interfaces;
using QuickMeet.Infrastructure.Data;
using QuickMeet.API.DTOs.Auth;
using QuickMeet.API.Filters;

namespace QuickMeet.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [ApiExplorerSettings(IgnoreApi = true)]
    [RequireDangerousOperations]
    public class TestController : ControllerBase
    {
        private readonly IAuthenticationService _authService;
        private readonly QuickMeetDbContext _dbContext;
        private readonly ILogger<TestController> _logger;

        public TestController(
            IAuthenticationService authService,
            QuickMeetDbContext dbContext,
            ILogger<TestController> logger)
        {
            _authService = authService;
            _dbContext = dbContext;
            _logger = logger;
            
            _logger.LogInformation("TestController initialized");
        }

        [HttpPost("seed-user")]
        [ProducesResponseType(typeof(SeedUserResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<SeedUserResponse>> SeedUser([FromBody] SeedUserRequest request)
        {
            try
            {
                _logger.LogInformation("Test: Seeding user {Email}", request.Email);

                var existingUser = await _dbContext.Providers
                    .FirstOrDefaultAsync(p => p.Email == request.Email);

                if (existingUser != null)
                {
                    _logger.LogInformation("Test: User {Email} already exists", request.Email);
                    return BadRequest(new ErrorResponse { Error = "User already exists" });
                }

                var username = request.Username ?? $"testuser_{DateTime.UtcNow.Ticks}";
                var fullName = request.FullName ?? "Test User";
                var password = request.Password ?? "Test@123456";

                var (success, message, authResult) = await _authService.RegisterAsync(
                    request.Email,
                    username,
                    fullName,
                    password);

                if (!success || authResult == null)
                {
                    _logger.LogError("Test: Failed to seed user {Email}: {Message}", request.Email, message);
                    return BadRequest(new ErrorResponse { Error = message });
                }

                _logger.LogInformation("Test: User {Email} seeded successfully", request.Email);
                
                var response = new SeedUserResponse
                {
                    ProviderId = authResult.ProviderId,
                    Email = authResult.Email,
                    Username = authResult.Username,
                    FullName = authResult.FullName,
                    AccessToken = authResult.AccessToken,
                    RefreshToken = authResult.RefreshToken
                };

                return CreatedAtAction(nameof(SeedUser), new { email = request.Email }, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Test: Error seeding user {Email}", request.Email);
                return StatusCode(500, new ErrorResponse 
                { 
                    Error = "Internal error seeding user", 
                    Details = ex.Message 
                });
            }
        }

        [HttpDelete("cleanup-user/{email}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CleanupUser(string email)
        {
            try
            {
                _logger.LogInformation("Test: Cleanup user {Email}", email);

                var user = await _dbContext.Providers
                    .FirstOrDefaultAsync(p => p.Email == email);

                if (user == null)
                {
                    _logger.LogInformation("Test: User {Email} not found for cleanup", email);
                    return NotFound(new ErrorResponse { Error = "User not found" });
                }

                _dbContext.Providers.Remove(user);
                await _dbContext.SaveChangesAsync();

                _logger.LogInformation("Test: User {Email} deleted successfully", email);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Test: Error cleaning up user {Email}", email);
                return StatusCode(500, new ErrorResponse 
                { 
                    Error = "Internal error cleaning user", 
                    Details = ex.Message 
                });
            }
        }

        [HttpGet("ping")]
        [ProducesResponseType(typeof(PingResponse), StatusCodes.Status200OK)]
        public IActionResult Ping()
        {
            _logger.LogInformation("Test: Ping received");
            
            var response = new PingResponse
            {
                Message = "TestController is active",
                Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown",
                Timestamp = DateTime.UtcNow
            };

            return Ok(response);
        }

        [HttpPost("reset-database")]
        [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<MessageResponse>> ResetDatabase()
        {
            try
            {
                _logger.LogWarning("Test: Database reset initiated - DESTRUCTIVE OPERATION");

                await _dbContext.Database.EnsureDeletedAsync();
                await _dbContext.Database.EnsureCreatedAsync();

                _logger.LogWarning("Test: Database reset completed successfully");

                var response = new MessageResponse
                {
                    Message = "Database reset successfully"
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Test: Error resetting database");
                return StatusCode(500, new ErrorResponse 
                { 
                    Error = "Internal error resetting database", 
                    Details = ex.Message 
                });
            }
        }
    }

    public record SeedUserResponse
    {
        public int ProviderId { get; init; }
        public string Email { get; init; } = string.Empty;
        public string Username { get; init; } = string.Empty;
        public string FullName { get; init; } = string.Empty;
        public string AccessToken { get; init; } = string.Empty;
        public string? RefreshToken { get; init; }
    }

    public record ErrorResponse
    {
        public string Error { get; init; } = string.Empty;
        public string? Details { get; init; }
    }

    public record PingResponse
    {
        public string Message { get; init; } = string.Empty;
        public string Environment { get; init; } = string.Empty;
        public DateTime Timestamp { get; init; }
    }

    public record MessageResponse
    {
        public string Message { get; init; } = string.Empty;
    }
}
#endif
```

#### TestDtos.cs

**Archivo**: `backend/src/QuickMeet.API/DTOs/Auth/TestDtos.cs`

```csharp
namespace QuickMeet.API.DTOs.Auth
{
    /// <summary>
    /// DTO para crear usuarios de test via POST /api/test/seed-user
    /// </summary>
    public class SeedUserRequest
    {
        public string Email { get; set; } = string.Empty;
        public string? Username { get; set; }
        public string? FullName { get; set; }
        public string? Password { get; set; }
    }
}
```

**Nota sobre AuthenticationResult**: 
- El `IAuthenticationService.RegisterAsync()` retorna `AuthenticationResult?`
- Este tipo está definido en `QuickMeet.Core.Interfaces` como:
  ```csharp
  public record AuthenticationResult(
      int ProviderId,
      string Email,
      string Username,
      string FullName,
      string AccessToken,
      string RefreshToken,
      DateTimeOffset ExpiresAt
  );
  ```
- En el endpoint de testing, retornamos solo los datos básicos (sin tokens para seguridad)

### Validación de Fase 1 - CRÍTICA

#### Test 1: Verificar que #if DEBUG funciona

```bash
# En tu laptop (Debug)
cd backend
set ASPNETCORE_ENV=Test
dotnet run --project src/QuickMeet.API

# En otra terminal, probar endpoint
curl http://localhost:5173/api/test/ping
# → Debería responder: {"message":"TestController is active",...}
```

```bash
# Compilar Release (simular producción)
dotnet publish -c Release -o ./release
cd release

# Intentar ejecutar
set ASPNETCORE_ENV=Production
dotnet QuickMeet.API.dll

# En otra terminal, probar endpoint
curl http://localhost:5173/api/test/ping
# → Debería responder: 404 Not Found
```

#### Test 2: Verificar protección del filtro

```bash
# Editar temporalmente appsettings.Development.json
# Cambiar: "AllowDangerousOperations": false

# El endpoint DEBE retornar 404 (no 500)
curl http://localhost:5173/api/test/ping
# → Respuesta 404 Not Found

# Revertir cambio después del test
```

#### Test 3: Verificar que no está en Swagger

```bash
# Acceder a Swagger: http://localhost:5173/swagger/index.html
# TestController NO debe aparecer en la lista de endpoints
```

#### Test 4: Probar endpoints funcionales

```bash
# Seed user
curl -X POST http://localhost:5173/api/test/seed-user \
  -H "Content-Type: application/json" \
  -d '{"email":"test@test.com","password":"Test@123456"}'
# → Respuesta 201 Created

# Reset database (destructive)
curl -X POST http://localhost:5173/api/test/reset-database
# → Respuesta 200 OK con {"message":"Database reset successfully"}

# Cleanup user
curl -X DELETE http://localhost:5173/api/test/cleanup-user/test@test.com
# → Respuesta 204 No Content
```

#### Checklist de validación
- [x] TestController compila en Debug
- [x] TestController NO existe en Release build
- [x] Atributo [RequireDangerousOperations] protege los endpoints
- [x] Endpoint `/api/test/ping` responde en Development
- [x] Endpoint `/api/test/seed-user` crea usuarios
- [x] Endpoint `/api/test/cleanup-user/{email}` elimina usuarios
- [x] Endpoint `/api/test/reset-database` reinicia DB
- [x] Logging estructurado con Path y Method
- [x] En Release: `/api/test/*` retorna 404 automático
- [x] AllowDangerousOperations solo en Development y Test

---

## Fase 2: Frontend - Estructura E2E (2-3 horas)

### Objetivos
- [ ] Crear carpeta `e2e/pages/`
- [ ] Crear carpeta `e2e/helpers/`
- [ ] Reorganizar fixtures existentes
- [ ] Crear helpers de datos y API

### Estructura final

```
frontend/e2e/
├── fixtures/
│   ├── page.fixture.ts          (ya existe, mantener)
│   ├── auth.fixture.ts          (nuevo - Fase 3)
│   └── users.json               (ya existe, mantener)
├── pages/                        (nuevo)
│   ├── login.page.ts
│   ├── register.page.ts
│   └── dashboard.page.ts
├── helpers/                      (nuevo)
│   ├── test-api.helper.ts
│   └── test-data.helper.ts
├── tests/
│   ├── auth/
│   │   ├── login.spec.ts        (refactorizar en Fase 4)
│   │   └── register.spec.ts     (refactorizar en Fase 4)
│   └── shared/
│       └── test-helpers.ts      (ya existe, mantener)
└── playwright.config.ts
```

### Archivos a crear (esqueletos en esta fase)

Crear estructura vacía con exports básicos:

```typescript
// e2e/pages/login.page.ts
import { Page } from '@playwright/test';

export class LoginPage {
  constructor(private page: Page) {}
  
  async goto() {
    // Implementar en Fase 3
  }
}
```

```typescript
// e2e/pages/register.page.ts
import { Page } from '@playwright/test';

export class RegisterPage {
  constructor(private page: Page) {}
  
  async goto() {
    // Implementar en Fase 3
  }
}
```

```typescript
// e2e/pages/dashboard.page.ts
import { Page } from '@playwright/test';

export class DashboardPage {
  constructor(private page: Page) {}
  
  async goto() {
    // Implementar en Fase 3
  }
}
```

```typescript
// e2e/helpers/test-api.helper.ts
export async function seedUser(userData: any) {
  // Implementar en Fase 3
}

export async function cleanupUser(email: string) {
  // Implementar en Fase 3
}
```

```typescript
// e2e/helpers/test-data.helper.ts
export function generateUniqueUser(baseUser: any) {
  // Implementar en Fase 3
}
```

### Validación de Fase 2
- [ ] Carpetas `pages/` y `helpers/` creadas
- [ ] Todos los archivos .ts existen (aunque vacíos/esqueletos)
- [ ] `npm run build` no genera errores de compilación
- [ ] Imports se resuelven correctamente

---

## Fase 3: Implementación Page Objects + Helpers (4-5 horas)

### Objetivos
- [ ] Implementar LoginPage completo
- [ ] Implementar RegisterPage completo
- [ ] Implementar DashboardPage completo
- [ ] Implementar test-api.helper.ts
- [ ] Implementar test-data.helper.ts
- [ ] Crear auth.fixture.ts con Page Objects integrados
