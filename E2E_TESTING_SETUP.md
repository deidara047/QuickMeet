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
- [ ] Crear `TestController.cs` con `#if DEBUG ... #endif`
- [ ] Constructor valida `AllowDangerousOperations` (doble check)
- [ ] Usar `ILogger<TestController>` para structured logging
- [ ] `[ApiExplorerSettings(IgnoreApi = true)]` para no aparecer en Swagger
- [ ] Implementar `POST /api/test/seed-user` completo
- [ ] Implementar `DELETE /api/test/cleanup-user/{email}` completo
- [ ] Crear DTOs necesarios

### Archivos a crear/modificar
- [ ] `backend/src/QuickMeet.API/Controllers/TestController.cs` (nuevo)
- [ ] `backend/src/QuickMeet.API/DTOs/Auth/TestDtos.cs` (nuevo)

### Implementación completa

#### TestController.cs

**Archivo**: `backend/src/QuickMeet.API/Controllers/TestController.cs`

```csharp
#if DEBUG
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuickMeet.Core.Interfaces;
using QuickMeet.Core.Entities;
using QuickMeet.Infrastructure.Data;
using QuickMeet.API.DTOs.Auth;

namespace QuickMeet.API.Controllers
{
    /// <summary>
    /// ⚠️ TESTING ONLY - Este controller SOLO EXISTE EN DEBUG BUILDS
    /// En Release: No se compila, imposible hackear
    /// Permite crear y limpiar usuarios (Providers) de test para E2E testing
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [ApiExplorerSettings(IgnoreApi = true)]  // No aparece en Swagger
    public class TestController : ControllerBase
    {
        private readonly IAuthenticationService _authService;
        private readonly QuickMeetDbContext _dbContext;
        private readonly IConfiguration _config;
        private readonly ILogger<TestController> _logger;

        public TestController(
            IAuthenticationService authService,
            QuickMeetDbContext dbContext,
            IConfiguration config,
            ILogger<TestController> logger)
        {
            _authService = authService;
            _dbContext = dbContext;
            _config = config;
            _logger = logger;

            // DOBLE VALIDACIÓN: Verificar que se cargó config de test
            var allowDangerousOps = _config.GetValue<bool>("AllowDangerousOperations");
            if (!allowDangerousOps)
            {
                _logger.LogError(
                    "❌ TestController instantiated but AllowDangerousOperations=false. " +
                    "Check that ASPNETCORE_ENV=Test and appsettings.Test.json has this flag set to true.");
                throw new InvalidOperationException(
                    "TestController requires AllowDangerousOperations=true. " +
                    "Run with: set ASPNETCORE_ENV=Test");
            }

            _logger.LogWarning(
                "🧪 TestController initialized. Dangerous testing operations enabled. " +
                "This MUST NEVER appear in production logs!");
        }

        /// <summary>
        /// POST /api/test/seed-user
        /// Crea un usuario de test para E2E testing
        /// </summary>
        [HttpPost("seed-user")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<object>> SeedUser([FromBody] SeedUserRequest request)
        {
            try
            {
                _logger.LogWarning(
                    "🧪 TEST: Seeding user {Email} at {Timestamp}",
                    request.Email,
                    DateTime.UtcNow);

                // 1. Validar que email no exista
                var existingUser = await _dbContext.Providers
                    .FirstOrDefaultAsync(p => p.Email == request.Email);

                if (existingUser != null)
                {
                    _logger.LogWarning("🧪 TEST: User {Email} already exists", request.Email);
                    return BadRequest(new { error = "User already exists" });
                }

                // 2. Crear usuario usando el servicio de autenticación
                var username = request.Username ?? $"user_{DateTime.UtcNow.Ticks}";
                var fullName = request.FullName ?? "Test User";
                var password = request.Password ?? "Test@123456";

                var (success, message, authResult) = await _authService.RegisterAsync(
                    request.Email,
                    username,
                    fullName,
                    password);

                if (!success)
                {
                    _logger.LogError("🧪 TEST: Failed to seed user {Email}: {Message}", request.Email, message);
                    return BadRequest(new { error = message });
                }

                _logger.LogWarning("🧪 TEST: User {Email} seeded successfully with ProviderId: {ProviderId}", 
                    request.Email, authResult?.ProviderId);
                
                // Retornar el resultado de autenticación (incluye tokens si aplica)
                return CreatedAtAction(nameof(SeedUser), 
                    new { email = request.Email }, 
                    new { 
                        providerId = authResult?.ProviderId,
                        email = authResult?.Email,
                        username = authResult?.Username,
                        fullName = authResult?.FullName
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "🧪 TEST: Error seeding user {Email}", request.Email);
                return StatusCode(500, new { error = "Internal error seeding user", details = ex.Message });
            }
        }

        /// <summary>
        /// DELETE /api/test/cleanup-user/{email}
        /// Elimina usuario de test después de E2E testing
        /// </summary>
        [HttpDelete("cleanup-user/{email}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CleanupUser(string email)
        {
            try
            {
                _logger.LogWarning(
                    "🧪 TEST: Cleanup user {Email} at {Timestamp}",
                    email,
                    DateTime.UtcNow);

                var user = await _dbContext.Providers
                    .FirstOrDefaultAsync(p => p.Email == email);

                if (user == null)
                {
                    _logger.LogInformation("🧪 TEST: User {Email} not found for cleanup", email);
                    return NotFound(new { error = "User not found" });
                }

                _dbContext.Providers.Remove(user);
                await _dbContext.SaveChangesAsync();

                _logger.LogWarning("🧪 TEST: User {Email} deleted successfully", email);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "🧪 TEST: Error cleaning up user {Email}", email);
                return StatusCode(500, new { error = "Internal error cleaning user", details = ex.Message });
            }
        }

        /// <summary>
        /// GET /api/test/ping
        /// Endpoint simple para verificar que TestController está activo
        /// Útil para debugging en E2E tests
        /// </summary>
        [HttpGet("ping")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult Ping()
        {
            _logger.LogInformation("🧪 TEST: Ping received");
            return Ok(new { 
                message = "TestController is active", 
                environment = _config["ASPNETCORE_ENVIRONMENT"],
                timestamp = DateTime.UtcNow,
                allowDangerousOperations = _config.GetValue<bool>("AllowDangerousOperations")
            });
        }
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

#### Test 2: Verificar doble check de AllowDangerousOperations

```bash
# Editar temporalmente appsettings.Test.json
# Cambiar: "AllowDangerousOperations": false

set ASPNETCORE_ENV=Test
dotnet run --project src/QuickMeet.API
# → Debe lanzar exception: "TestController requires AllowDangerousOperations=true"

# Revertir cambio después del test
```

#### Test 3: Probar endpoints funcionales

```bash
# Seed user
curl -X POST http://localhost:5173/api/test/seed-user \
  -H "Content-Type: application/json" \
  -d '{"email":"test@test.com","password":"Test@123456"}'
# → Respuesta 201 Created

# Cleanup user
curl -X DELETE http://localhost:5173/api/test/cleanup-user/test@test.com
# → Respuesta 204 No Content
```

#### Checklist de validación
- [ ] TestController compila en Debug
- [ ] TestController NO existe en Release build
- [ ] Constructor valida `AllowDangerousOperations`
- [ ] Endpoint `/api/test/ping` responde en Test
- [ ] Endpoint `/api/test/seed-user` crea usuarios
- [ ] Endpoint `/api/test/cleanup-user/{email}` elimina usuarios
- [ ] Logging con warnings aparece en consola
- [ ] En Release: `/api/test/ping` retorna 404

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
