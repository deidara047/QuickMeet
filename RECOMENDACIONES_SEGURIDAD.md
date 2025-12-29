# Recomendaciones de Seguridad - QuickMeet

Plan de implementación de los 7 pilares de seguridad AWS para QuickMeet. Documento técnico con ejemplos de código.

---

## 1. IMPLEMENTAR UNA BASE DE IDENTIDAD SÓLIDA

### 1.1 Autenticación JWT con Refresh Tokens

**Problema actual:** Solo acceso con JWT sin refresh mechanism. Los tokens nunca expiran dinámicamente.

**Solución:**

Crear `src/QuickMeet.Core/Services/JwtService.cs`:

```csharp
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace QuickMeet.Core.Services
{
    public interface IJwtService
    {
        string GenerateAccessToken(string userId, string email, string role);
        string GenerateRefreshToken();
        ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
        bool ValidateToken(string token);
    }

    public class JwtService : IJwtService
    {
        private readonly string _secretKey;
        private readonly string _issuer;
        private readonly string _audience;
        private readonly int _expiryMinutes;

        public JwtService(IConfiguration config)
        {
            _secretKey = config["Jwt:Secret"] 
                ?? throw new ArgumentNullException("Jwt:Secret not configured");
            _issuer = config["Jwt:Issuer"] ?? "quickmeet-api";
            _audience = config["Jwt:Audience"] ?? "quickmeet-client";
            _expiryMinutes = int.Parse(config["Jwt:ExpiryMinutes"] ?? "60");

            if (_secretKey.Length < 32)
                throw new ArgumentException("JWT secret must be at least 32 characters");
        }

        public string GenerateAccessToken(string userId, string email, string role)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_secretKey);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.Role, role),
                new Claim("jti", Guid.NewGuid().ToString())
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(_expiryMinutes),
                Issuer = _issuer,
                Audience = _audience,
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];
            using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
            }
            return Convert.ToBase64String(randomNumber);
        }

        public ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_secretKey)),
                ValidateLifetime = false
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, 
                out SecurityToken securityToken);

            if (!(securityToken is JwtSecurityToken jwtSecurityToken) ||
                !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256,
                    StringComparison.InvariantCultureIgnoreCase))
                throw new SecurityTokenException("Invalid token");

            return principal;
        }

        public bool ValidateToken(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_secretKey);

                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _issuer,
                    ValidateAudience = true,
                    ValidAudience = _audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
```

Crear `src/QuickMeet.Infrastructure/Repositories/RefreshTokenRepository.cs`:

```csharp
using System;
using System.Threading.Tasks;

namespace QuickMeet.Infrastructure.Repositories
{
    public interface IRefreshTokenRepository
    {
        Task<RefreshToken> GetByTokenAsync(string token);
        Task<RefreshToken> GetByUserIdAsync(string userId);
        Task AddAsync(RefreshToken token);
        Task RevokeAsync(string token);
        Task<bool> IsRevokedAsync(string token);
    }

    public class RefreshToken
    {
        public int Id { get; set; }
        public string ProviderId { get; set; }
        public string Token { get; set; }
        public DateTime ExpiresAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? RevokedAt { get; set; }
        public string RevokedReason { get; set; }
    }

    public class RefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly ApplicationDbContext _context;

        public RefreshTokenRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<RefreshToken> GetByTokenAsync(string token)
        {
            return await _context.RefreshTokens
                .FirstOrDefaultAsync(x => x.Token == token && x.RevokedAt == null);
        }

        public async Task<RefreshToken> GetByUserIdAsync(string userId)
        {
            return await _context.RefreshTokens
                .Where(x => x.ProviderId == userId && x.RevokedAt == null)
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefaultAsync();
        }

        public async Task AddAsync(RefreshToken token)
        {
            await _context.RefreshTokens.AddAsync(token);
            await _context.SaveChangesAsync();
        }

        public async Task RevokeAsync(string token)
        {
            var refreshToken = await GetByTokenAsync(token);
            if (refreshToken != null)
            {
                refreshToken.RevokedAt = DateTime.UtcNow;
                refreshToken.RevokedReason = "Manual revocation";
                _context.RefreshTokens.Update(refreshToken);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> IsRevokedAsync(string token)
        {
            var refreshToken = await _context.RefreshTokens
                .FirstOrDefaultAsync(x => x.Token == token);
            return refreshToken?.RevokedAt != null;
        }
    }
}
```

### 1.2 Validación de Contraseñas Fuertes

Crear `src/QuickMeet.Core/Services/PasswordValidator.cs`:

```csharp
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace QuickMeet.Core.Services
{
    public interface IPasswordValidator
    {
        (bool IsValid, string[] Errors) Validate(string password);
    }

    public class PasswordValidator : IPasswordValidator
    {
        private const int MinLength = 12;
        private const int MaxLength = 128;

        public (bool IsValid, string[] Errors) Validate(string password)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(password))
            {
                errors.Add("Password cannot be empty");
                return (false, errors.ToArray());
            }

            if (password.Length < MinLength)
                errors.Add($"Password must be at least {MinLength} characters");

            if (password.Length > MaxLength)
                errors.Add($"Password cannot exceed {MaxLength} characters");

            if (!Regex.IsMatch(password, @"[A-Z]"))
                errors.Add("Password must contain at least one uppercase letter");

            if (!Regex.IsMatch(password, @"[a-z]"))
                errors.Add("Password must contain at least one lowercase letter");

            if (!Regex.IsMatch(password, @"[0-9]"))
                errors.Add("Password must contain at least one digit");

            if (!Regex.IsMatch(password, @"[!@#$%^&*()_+\-=\[\]{};':"",.<>?/\\|`~]"))
                errors.Add("Password must contain at least one special character");

            return (errors.Count == 0, errors.ToArray());
        }
    }
}
```

### 1.3 Implementar AuthController

Crear `src/QuickMeet.API/Controllers/AuthController.cs`:

```csharp
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using QuickMeet.Core.Services;
using QuickMeet.Core.DTOs;

namespace QuickMeet.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IJwtService _jwtService;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IPasswordValidator _passwordValidator;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            IJwtService jwtService,
            IRefreshTokenRepository refreshTokenRepository,
            IPasswordValidator passwordValidator,
            ILogger<AuthController> logger)
        {
            _jwtService = jwtService;
            _refreshTokenRepository = refreshTokenRepository;
            _passwordValidator = passwordValidator;
            _logger = logger;
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request?.RefreshToken))
                    return BadRequest("Refresh token is required");

                var isRevoked = await _refreshTokenRepository.IsRevokedAsync(request.RefreshToken);
                if (isRevoked)
                {
                    _logger.LogWarning("Attempt to use revoked refresh token");
                    return Unauthorized("Refresh token has been revoked");
                }

                var principal = _jwtService.GetPrincipalFromExpiredToken(request.AccessToken);
                var userId = principal.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                var email = principal.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
                var role = principal.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

                var newAccessToken = _jwtService.GenerateAccessToken(userId, email, role);
                var newRefreshToken = _jwtService.GenerateRefreshToken();

                var refreshToken = new RefreshToken
                {
                    ProviderId = userId,
                    Token = newRefreshToken,
                    ExpiresAt = DateTime.UtcNow.AddDays(7),
                    CreatedAt = DateTime.UtcNow
                };

                await _refreshTokenRepository.AddAsync(refreshToken);

                return Ok(new AuthResponse
                {
                    AccessToken = newAccessToken,
                    RefreshToken = newRefreshToken,
                    ExpiresIn = 3600
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing token");
                return Unauthorized("Invalid token");
            }
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] LogoutRequest request)
        {
            try
            {
                await _refreshTokenRepository.RevokeAsync(request.RefreshToken);
                _logger.LogInformation("User logged out successfully");
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout");
                return StatusCode(500, "Error during logout");
            }
        }
    }
}
```

---

## 2. HABILITAR LA TRAZABILIDAD

### 2.1 Implementar Logging Centralizado con Serilog

Actualizar `src/QuickMeet.API/Program.cs`:

```csharp
using Serilog;
using Serilog.Events;
using Microsoft.ApplicationInsights.Extensibility;

var builder = WebApplication.CreateBuilder(args);

// Configurar Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "QuickMeet.API")
    .Enrich.WithProperty("Environment", builder.Environment.EnvironmentName)
    .WriteTo.Console(
        outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}] [{Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.ApplicationInsights(
        new TelemetryClient(new TelemetryConfiguration()),
        TelemetryConverter.Traces)
    .CreateLogger();

builder.Host.UseSerilog(Log.Logger);

var app = builder.Build();

// Middleware de logging de requests/responses
app.UseMiddleware<RequestLoggingMiddleware>();
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.MapOpenApi();
app.UseHttpsRedirection();
app.Run();
```

Crear `src/QuickMeet.API/Middleware/RequestLoggingMiddleware.cs`:

```csharp
using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Serilog;
using Serilog.Context;

namespace QuickMeet.API.Middleware
{
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger _logger;

        public RequestLoggingMiddleware(RequestDelegate next, ILogger logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var correlationId = Guid.NewGuid().ToString();
            using (LogContext.PushProperty("CorrelationId", correlationId))
            using (LogContext.PushProperty("RequestPath", context.Request.Path))
            using (LogContext.PushProperty("RemoteIP", context.Connection.RemoteIpAddress))
            {
                var stopwatch = Stopwatch.StartNew();

                var originalBodyStream = context.Response.Body;
                using (var responseBody = new MemoryStream())
                {
                    context.Response.Body = responseBody;

                    try
                    {
                        _logger.Information(
                            "HTTP {Method} {Path} started",
                            context.Request.Method,
                            context.Request.Path);

                        await _next(context);

                        stopwatch.Stop();
                        _logger.Information(
                            "HTTP {Method} {Path} completed with status {StatusCode} in {ElapsedMilliseconds}ms",
                            context.Request.Method,
                            context.Request.Path,
                            context.Response.StatusCode,
                            stopwatch.ElapsedMilliseconds);
                    }
                    catch (Exception ex)
                    {
                        stopwatch.Stop();
                        _logger.Error(ex,
                            "HTTP {Method} {Path} failed after {ElapsedMilliseconds}ms",
                            context.Request.Method,
                            context.Request.Path,
                            stopwatch.ElapsedMilliseconds);
                        throw;
                    }
                    finally
                    {
                        await responseBody.CopyToAsync(originalBodyStream);
                    }
                }
            }
        }
    }
}
```

### 2.2 Middleware de Auditoría de Acceso

Crear `src/QuickMeet.Infrastructure/Data/AuditLog.cs`:

```csharp
using System;

namespace QuickMeet.Infrastructure.Data
{
    public class AuditLog
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string Action { get; set; }
        public string Resource { get; set; }
        public string Result { get; set; }
        public int? StatusCode { get; set; }
        public string IpAddress { get; set; }
        public string UserAgent { get; set; }
        public DateTime Timestamp { get; set; }
        public string CorrelationId { get; set; }
        public string Changes { get; set; }
    }
}
```

Crear `src/QuickMeet.API/Middleware/AuditingMiddleware.cs`:

```csharp
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Serilog.Context;
using QuickMeet.Infrastructure.Data;
using System.Security.Claims;

namespace QuickMeet.API.Middleware
{
    public class AuditingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<AuditingMiddleware> _logger;

        public AuditingMiddleware(RequestDelegate next, ILogger<AuditingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, ApplicationDbContext dbContext)
        {
            var correlationId = context.Request.Headers["X-Correlation-ID"].ToString() 
                ?? Guid.NewGuid().ToString();

            using (LogContext.PushProperty("CorrelationId", correlationId))
            {
                var userId = context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "Anonymous";
                var method = context.Request.Method;
                var path = context.Request.Path.ToString();

                var auditLog = new AuditLog
                {
                    UserId = userId,
                    Action = $"{method} {path}",
                    Resource = path,
                    IpAddress = context.Connection.RemoteIpAddress?.ToString(),
                    UserAgent = context.Request.Headers["User-Agent"].ToString(),
                    Timestamp = DateTime.UtcNow,
                    CorrelationId = correlationId
                };

                try
                {
                    await _next(context);
                    auditLog.StatusCode = context.Response.StatusCode;
                    auditLog.Result = "Success";
                }
                catch (Exception ex)
                {
                    auditLog.StatusCode = 500;
                    auditLog.Result = $"Error: {ex.Message}";
                    throw;
                }
                finally
                {
                    dbContext.AuditLogs.Add(auditLog);
                    await dbContext.SaveChangesAsync();
                }
            }
        }
    }
}
```

---

## 3. APLICAR SEGURIDAD EN TODAS LAS CAPAS

### 3.1 Validación de Inputs con FluentValidation

Instalar: `dotnet add package FluentValidation`

Crear `src/QuickMeet.Core/DTOs/RegisterProviderDto.cs`:

```csharp
using FluentValidation;

namespace QuickMeet.Core.DTOs
{
    public class RegisterProviderDto
    {
        public string Email { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string DisplayName { get; set; }
    }

    public class RegisterProviderValidator : AbstractValidator<RegisterProviderDto>
    {
        public RegisterProviderValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Email format is invalid")
                .MaximumLength(255).WithMessage("Email must not exceed 255 characters");

            RuleFor(x => x.Username)
                .NotEmpty().WithMessage("Username is required")
                .Length(3, 30).WithMessage("Username must be between 3 and 30 characters")
                .Matches(@"^[a-zA-Z0-9_-]*$")
                .WithMessage("Username can only contain alphanumeric characters, hyphens, and underscores")
                .Must(x => !ContainsBannedWords(x))
                .WithMessage("Username contains inappropriate content");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required")
                .MinimumLength(12).WithMessage("Password must be at least 12 characters")
                .Matches(@"[A-Z]").WithMessage("Password must contain at least one uppercase letter")
                .Matches(@"[a-z]").WithMessage("Password must contain at least one lowercase letter")
                .Matches(@"[0-9]").WithMessage("Password must contain at least one number")
                .Matches(@"[!@#$%^&*()_+\-=\[\]{};':"",.<>?/\\|`~]")
                .WithMessage("Password must contain at least one special character");

            RuleFor(x => x.DisplayName)
                .NotEmpty().WithMessage("Display name is required")
                .Length(2, 100).WithMessage("Display name must be between 2 and 100 characters");
        }

        private bool ContainsBannedWords(string value)
        {
            var bannedWords = new[] { "admin", "test", "system", "root" };
            return bannedWords.Any(word => value.Contains(word, StringComparison.OrdinalIgnoreCase));
        }
    }
}
```

Crear validador global en `src/QuickMeet.API/Filters/ValidationFilter.cs`:

```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace QuickMeet.API.Filters
{
    public class ValidationFilter : IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ModelState.IsValid)
            {
                var errors = context.ModelState
                    .Where(x => x.Value.Errors.Count > 0)
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray());

                context.Result = new BadRequestObjectResult(new
                {
                    message = "Validation failed",
                    errors = errors
                });
            }
        }

        public void OnActionExecuted(ActionExecutedContext context) { }
    }
}
```

Registrar en `Program.cs`:

```csharp
builder.Services.AddFluentValidation(fv =>
    fv.RegisterValidatorsFromAssemblyContaining<Program>());

builder.Services.AddControllers(options =>
    options.Filters.Add<ValidationFilter>());
```

### 3.2 Content Security Policy (CSP) en Frontend

Actualizar `frontend/nginx.conf`:

```nginx
http {
    server {
        listen 80 default_server;
        server_name _;
        root /usr/share/nginx/html;

        # Security Headers
        add_header X-Content-Type-Options "nosniff" always;
        add_header X-Frame-Options "SAMEORIGIN" always;
        add_header X-XSS-Protection "1; mode=block" always;
        add_header Referrer-Policy "strict-origin-when-cross-origin" always;
        
        # Content Security Policy - Muy restrictivo
        add_header Content-Security-Policy "default-src 'self'; script-src 'self'; style-src 'self' 'unsafe-inline'; img-src 'self' data: https:; font-src 'self'; connect-src 'self' https://api.quickmeet.app; frame-ancestors 'none'; base-uri 'self'; form-action 'self';" always;

        # Permissions Policy (Feature Policy)
        add_header Permissions-Policy "geolocation=(), microphone=(), camera=(), payment=()" always;

        # HSTS - Force HTTPS
        add_header Strict-Transport-Security "max-age=31536000; includeSubDomains; preload" always;

        location / {
            try_files $uri $uri/ /index.html;
            add_header Cache-Control "no-cache, no-store, must-revalidate";
        }

        location ~ /\. {
            deny all;
        }

        location ~ ~$ {
            deny all;
        }
    }
}
```

### 3.3 Rate Limiting Middleware

Crear `src/QuickMeet.API/Middleware/RateLimitingMiddleware.cs`:

```csharp
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace QuickMeet.API.Middleware
{
    public class RateLimitingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RateLimitingMiddleware> _logger;
        private static readonly ConcurrentDictionary<string, ClientRequestLog> _clients 
            = new();
        private readonly int _requestsPerMinute;

        public RateLimitingMiddleware(RequestDelegate next, 
            ILogger<RateLimitingMiddleware> logger,
            IConfiguration config)
        {
            _next = next;
            _logger = logger;
            _requestsPerMinute = int.Parse(config["RateLimiting:RequestsPerMinute"] ?? "100");
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var clientIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var key = $"{clientIp}";

            var clientLog = _clients.AddOrUpdate(key,
                new ClientRequestLog { Count = 1, FirstRequest = DateTime.UtcNow },
                (k, existing) =>
                {
                    if ((DateTime.UtcNow - existing.FirstRequest).TotalMinutes > 1)
                    {
                        return new ClientRequestLog { Count = 1, FirstRequest = DateTime.UtcNow };
                    }

                    existing.Count++;
                    return existing;
                });

            if (clientLog.Count > _requestsPerMinute)
            {
                _logger.LogWarning("Rate limit exceeded for IP {ClientIp}", clientIp);
                context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                await context.Response.WriteAsJsonAsync(new
                {
                    message = "Too many requests. Please try again later.",
                    retryAfter = 60
                });
                return;
            }

            await _next(context);
        }

        private class ClientRequestLog
        {
            public int Count { get; set; }
            public DateTime FirstRequest { get; set; }
        }
    }
}
```

---

## 4. AUTOMATIZAR LAS MEJORES PRÁCTICAS DE SEGURIDAD

### 4.1 SAST en CI/CD

Crear `.github/workflows/security-scan.yml`:

```yaml
name: Security Scanning

on:
  push:
    branches: [ main, develop ]
  pull_request:
    branches: [ main, develop ]

jobs:
  sast:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3

      - name: Run Snyk vulnerability scan
        uses: snyk/actions/dotnet@master
        with:
          args: --severity-threshold=high
        env:
          SNYK_TOKEN: ${{ secrets.SNYK_TOKEN }}

      - name: Run SonarQube scan
        uses: sonarsource/sonarqube-scan-action@master
        env:
          SONAR_HOST_URL: ${{ secrets.SONAR_HOST_URL }}
          SONAR_TOKEN: ${{ secrets.SONAR_TOKEN }}

      - name: Dependency check
        run: |
          npm install -g snyk
          snyk test --package-manager=nuget --fail-on=all

  dependency-check:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3

      - name: Run OWASP Dependency-Check
        uses: dependency-check/Dependency-Check_Action@main
        with:
          project: 'QuickMeet'
          path: '.'
          format: 'JSON'
          args: >
            --enableExperimental

      - name: Upload results
        if: always()
        uses: actions/upload-artifact@v3
        with:
          name: dependency-check-report
          path: reports/

  secrets-scan:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
        with:
          fetch-depth: 0

      - name: TruffleHog secret scanning
        uses: trufflesecurity/trufflehog@main
        with:
          path: ./
          base: ${{ github.event.repository.default_branch }}
          head: HEAD
          extra_args: --debug --json
```

### 4.2 Container Image Scanning

Crear `.github/workflows/container-security.yml`:

```yaml
name: Container Security

on:
  push:
    branches: [ main ]
    paths:
      - 'backend/src/QuickMeet.API/Dockerfile'
      - 'frontend/Dockerfile'

jobs:
  container-scan:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3

      - name: Build Docker image
        run: |
          docker build -t quickmeet-api:${{ github.sha }} -f backend/src/QuickMeet.API/Dockerfile backend/

      - name: Run Trivy vulnerability scanner
        uses: aquasecurity/trivy-action@master
        with:
          image-ref: 'quickmeet-api:${{ github.sha }}'
          format: 'sarif'
          output: 'trivy-results.sarif'

      - name: Upload Trivy results to GitHub Security tab
        uses: github/codeql-action/upload-sarif@v2
        with:
          sarif_file: 'trivy-results.sarif'
```

### 4.3 Pre-commit Hook para Secrets Detection

Crear `.pre-commit-config.yaml`:

```yaml
repos:
  - repo: https://github.com/Yelp/detect-secrets
    rev: v1.4.0
    hooks:
      - id: detect-secrets
        args: ['--baseline', '.secrets.baseline']

  - repo: https://github.com/pre-commit/pre-commit-hooks
    rev: v4.4.0
    hooks:
      - id: detect-private-key
      - id: check-json
      - id: check-yaml
      - id: end-of-file-fixer
      - id: trailing-whitespace

  - repo: https://github.com/gitpython-developers/gitpython
    rev: master
    hooks:
      - id: git-secrets
        entry: git-secrets --pre_commit_hook --
        language: system
        stages: [commit]
```

---

## 5. PROTEGER LOS DATOS EN TRÁNSITO Y EN REPOSO

### 5.1 Encriptación de Datos Sensibles

Crear `src/QuickMeet.Infrastructure/Encryption/DataEncryptionService.cs`:

```csharp
using System;
using System.Text;
using System.Security.Cryptography;

namespace QuickMeet.Infrastructure.Encryption
{
    public interface IDataEncryptionService
    {
        string Encrypt(string plainText);
        string Decrypt(string cipherText);
        string HashPassword(string password);
        bool VerifyPassword(string password, string hash);
    }

    public class DataEncryptionService : IDataEncryptionService
    {
        private readonly string _encryptionKey;
        private const int SaltSize = 16;
        private const int HashSize = 20;
        private const int Iterations = 100000;

        public DataEncryptionService(IConfiguration config)
        {
            _encryptionKey = config["Encryption:Key"] 
                ?? throw new ArgumentNullException("Encryption:Key not configured");
        }

        public string Encrypt(string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
                return plainText;

            using (var aes = Aes.Create())
            {
                aes.KeySize = 256;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                var keyBytes = Encoding.UTF8.GetBytes(_encryptionKey.PadRight(32).Substring(0, 32));
                aes.Key = keyBytes;

                using (var iv = RandomNumberGenerator.GetBytes(aes.IV.Length))
                using (var encryptor = aes.CreateEncryptor(aes.Key, iv))
                using (var ms = new MemoryStream())
                {
                    ms.Write(iv, 0, iv.Length);
                    using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    using (var sw = new StreamWriter(cs))
                    {
                        sw.Write(plainText);
                    }
                    return Convert.ToBase64String(ms.ToArray());
                }
            }
        }

        public string Decrypt(string cipherText)
        {
            if (string.IsNullOrEmpty(cipherText))
                return cipherText;

            using (var aes = Aes.Create())
            {
                aes.KeySize = 256;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                var keyBytes = Encoding.UTF8.GetBytes(_encryptionKey.PadRight(32).Substring(0, 32));
                aes.Key = keyBytes;

                var buffer = Convert.FromBase64String(cipherText);
                var iv = new byte[aes.IV.Length];
                Array.Copy(buffer, 0, iv, 0, iv.Length);

                using (var decryptor = aes.CreateDecryptor(aes.Key, iv))
                using (var ms = new MemoryStream(buffer, iv.Length, buffer.Length - iv.Length))
                using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                using (var sr = new StreamReader(cs))
                {
                    return sr.ReadToEnd();
                }
            }
        }

        public string HashPassword(string password)
        {
            using (var algorithm = new Rfc2898DeriveBytes(
                password,
                SaltSize,
                Iterations,
                HashAlgorithmName.SHA256))
            {
                var key = algorithm.GetBytes(HashSize);
                var salt = algorithm.Salt;

                var hashBytes = new byte[SaltSize + HashSize];
                Array.Copy(salt, 0, hashBytes, 0, SaltSize);
                Array.Copy(key, 0, hashBytes, SaltSize, HashSize);

                return Convert.ToBase64String(hashBytes);
            }
        }

        public bool VerifyPassword(string password, string hash)
        {
            var hashBytes = Convert.FromBase64String(hash);
            var salt = new byte[SaltSize];
            Array.Copy(hashBytes, 0, salt, 0, SaltSize);

            using (var algorithm = new Rfc2898DeriveBytes(
                password,
                salt,
                Iterations,
                HashAlgorithmName.SHA256))
            {
                var keyToCheck = algorithm.GetBytes(HashSize);

                return CryptographicOperations.FixedTimeEquals(
                    keyToCheck,
                    hashBytes.Skip(SaltSize).ToArray());
            }
        }
    }
}
```

### 5.2 Configurar HTTPS y TLS 1.3

Actualizar `src/QuickMeet.API/Program.cs`:

```csharp
var app = builder.Build();

// Force HTTPS
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
    app.UseHttpsRedirection();
}

// TLS configuration
var httpsOptions = new HttpsConnectionAdapterOptions
{
    ServerCertificate = LoadCertificate(),
    SslProtocols = System.Security.Authentication.SslProtocols.Tls13 |
                   System.Security.Authentication.SslProtocols.Tls12
};

app.UseHttpsRedirection();
```

Actualizar `docker-compose.prod.yml`:

```yaml
api:
  environment:
    ASPNETCORE_URLS: "https://+:8080;http://+:8000"
    ASPNETCORE_Kestrel__Certificates__Default__Path: "/app/certs/cert.pem"
    ASPNETCORE_Kestrel__Certificates__Default__KeyPath: "/app/certs/key.pem"
  volumes:
    - /etc/letsencrypt/live/quickmeet.app:/app/certs:ro
```

### 5.3 SQL Server - Transparent Data Encryption (TDE)

Crear migration para TDE en `src/QuickMeet.Infrastructure/Data/Migrations/20250101000002_EnableTDE.cs`:

```csharp
using Microsoft.EntityFrameworkCore.Migrations;

namespace QuickMeet.Infrastructure.Data.Migrations
{
    public partial class EnableTDE : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF NOT EXISTS (
                    SELECT * FROM sys.symmetric_keys 
                    WHERE name LIKE '%DEK%'
                )
                BEGIN
                    CREATE MASTER KEY ENCRYPTION BY PASSWORD = 'YourComplexPassword123!@#';
                    CREATE CERTIFICATE TDE_CERT WITH SUBJECT = 'TDE Certificate';
                    CREATE SYMMETRIC KEY TDE_DEK 
                        WITH ALGORITHM = AES_256 
                        ENCRYPTION BY CERTIFICATE TDE_CERT;
                    ALTER DATABASE QuickMeet SET ENCRYPTION ON;
                END
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                ALTER DATABASE QuickMeet SET ENCRYPTION OFF;
            ");
        }
    }
}
```

---

## 6. MANTENER A LAS PERSONAS ALEJADOS DE LOS DATOS

### 6.1 Enmascaramiento Dinámico de Datos

Crear `src/QuickMeet.Infrastructure/Data/DataMasking.cs`:

```csharp
using System;
using System.Text.RegularExpressions;

namespace QuickMeet.Infrastructure.Data
{
    public interface IDataMaskingService
    {
        string MaskEmail(string email);
        string MaskPhoneNumber(string phone);
        string MaskCreditCard(string cardNumber);
        string MaskSSN(string ssn);
    }

    public class DataMaskingService : IDataMaskingService
    {
        public string MaskEmail(string email)
        {
            if (string.IsNullOrEmpty(email))
                return email;

            var parts = email.Split('@');
            if (parts.Length != 2)
                return email;

            var username = parts[0];
            var domain = parts[1];

            if (username.Length <= 2)
                return $"*@{domain}";

            var visibleChars = Math.Max(1, username.Length / 2);
            var masked = username.Substring(0, visibleChars) + 
                         new string('*', username.Length - visibleChars);

            return $"{masked}@{domain}";
        }

        public string MaskPhoneNumber(string phone)
        {
            if (string.IsNullOrEmpty(phone) || phone.Length < 4)
                return phone;

            var lastFour = phone.Substring(phone.Length - 4);
            return $"***-***-{lastFour}";
        }

        public string MaskCreditCard(string cardNumber)
        {
            if (string.IsNullOrEmpty(cardNumber) || cardNumber.Length < 4)
                return cardNumber;

            var lastFour = cardNumber.Substring(cardNumber.Length - 4);
            return $"****-****-****-{lastFour}";
        }

        public string MaskSSN(string ssn)
        {
            if (string.IsNullOrEmpty(ssn) || ssn.Length < 4)
                return ssn;

            return $"***-**-{ssn.Substring(ssn.Length - 4)}";
        }
    }
}
```

### 6.2 Row-Level Security en SQL Server

Crear migration `20250101000003_EnableRLS.cs`:

```csharp
protected override void Up(MigrationBuilder migrationBuilder)
{
    migrationBuilder.Sql(@"
        CREATE SCHEMA rls;
        GO

        CREATE FUNCTION rls.AppointmentAccessPredicate(@UserId NVARCHAR(450))
        RETURNS TABLE
        WITH SCHEMABINDING
        AS
        RETURN (
            SELECT 1 AS AccessResult
            WHERE @UserId = SESSION_CONTEXT(N'UserId')
               OR EXISTS (
                   SELECT 1 FROM Providers p 
                   WHERE p.ProviderId = @UserId
               )
        );
        GO

        CREATE SECURITY POLICY AppointmentSecurityPolicy
            ADD FILTER PREDICATE rls.AppointmentAccessPredicate(ProviderId)
            ON Appointments
            WITH (STATE = ON);
        GO
    ");
}
```

### 6.3 Auditoría a Nivel de Base de Datos

Crear migration `20250101000004_EnableAuditing.cs`:

```csharp
protected override void Up(MigrationBuilder migrationBuilder)
{
    migrationBuilder.Sql(@"
        CREATE TABLE AuditLog (
            AuditLogId BIGINT IDENTITY(1,1) PRIMARY KEY,
            TableName NVARCHAR(128) NOT NULL,
            Operation NVARCHAR(10) NOT NULL,
            RecordId NVARCHAR(450),
            UserId NVARCHAR(450),
            OldValue NVARCHAR(MAX),
            NewValue NVARCHAR(MAX),
            Timestamp DATETIME2 DEFAULT GETUTCDATE(),
            IpAddress NVARCHAR(45),
            INDEX idx_timestamp (Timestamp DESC),
            INDEX idx_user (UserId)
        );

        CREATE TRIGGER AuditAppointmentsUpdate ON Appointments
        AFTER UPDATE
        AS
        BEGIN
            INSERT INTO AuditLog (TableName, Operation, RecordId, OldValue, NewValue, UserId)
            SELECT 
                'Appointments',
                'UPDATE',
                d.AppointmentId,
                (SELECT * FROM deleted d2 WHERE d2.AppointmentId = d.AppointmentId FOR JSON PATH),
                (SELECT * FROM inserted i2 WHERE i2.AppointmentId = i.AppointmentId FOR JSON PATH),
                SESSION_CONTEXT(N'UserId')
            FROM deleted d
            JOIN inserted i ON d.AppointmentId = i.AppointmentId;
        END;
    ");
}
```

---

## 7. PREPARARSE PARA INCIDENTES DE SEGURIDAD

### 7.1 Incident Response Plan

Crear `INCIDENT_RESPONSE_PLAN.md`:

```markdown
# Incident Response Plan - QuickMeet

## Equipo de Respuesta
- Security Lead: [Contact]
- DevOps Lead: [Contact]
- Backend Lead: [Contact]
- Frontend Lead: [Contact]

## Fases

### Phase 1: Detection & Analysis (0-1 hora)
1. Confirmación del incidente
2. Clasificación de severidad
3. Aislamiento del componente afectado
4. Notificar al equipo

### Phase 2: Containment (1-4 horas)
1. Revoke compromised credentials
2. Rotate JWT secrets
3. Kill all active sessions
4. Block malicious IPs
5. Capture logs for forensics

### Phase 3: Eradication (4-24 horas)
1. Patch vulnerabilities
2. Rebuild affected systems
3. Verify clean state
4. Restore from clean backups if needed

### Phase 4: Recovery (24-72 horas)
1. Gradual service restoration
2. Monitor for reinfection
3. Update security controls
4. Document lessons learned

### Phase 5: Post-Incident (1 semana)
1. Root cause analysis
2. Implement preventive controls
3. Update runbooks
4. Team training
```

### 7.2 Backup y Disaster Recovery

Actualizar `docker-compose.prod.yml`:

```yaml
services:
  sql-server:
    environment:
      MSSQL_BACKUP_DIR: /var/opt/mssql/backup
    volumes:
      - sqlserver_backup:/var/opt/mssql/backup
    command: /bin/bash -c '/opt/mssql/bin/sqlservr & sleep 10 && /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P $SA_PASSWORD -Q "BACKUP DATABASE QuickMeet TO DISK = /var/opt/mssql/backup/quickmeet_$(date +%Y%m%d_%H%M%S).bak WITH COMPRESSION" & wait'

volumes:
  sqlserver_backup:
    driver: local
```

### 7.3 Security Monitoring & Alerting

Crear `src/QuickMeet.API/Services/SecurityMonitoringService.cs`:

```csharp
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace QuickMeet.API.Services
{
    public interface ISecurityMonitoringService
    {
        Task LogFailedLoginAttempt(string username, string ipAddress);
        Task LogSuspiciousActivity(string description, string userId, string ipAddress);
        Task<int> GetFailedLoginCount(string username, TimeSpan timeWindow);
        Task LockAccountIfNeeded(string username);
    }

    public class SecurityMonitoringService : ISecurityMonitoringService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<SecurityMonitoringService> _logger;
        private const int MaxFailedAttempts = 5;
        private const int LockoutDurationMinutes = 15;

        public SecurityMonitoringService(ApplicationDbContext context,
            ILogger<SecurityMonitoringService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task LogFailedLoginAttempt(string username, string ipAddress)
        {
            var securityEvent = new SecurityEvent
            {
                EventType = "FailedLogin",
                Username = username,
                IpAddress = ipAddress,
                Timestamp = DateTime.UtcNow,
                Severity = "Medium"
            };

            _context.SecurityEvents.Add(securityEvent);
            await _context.SaveChangesAsync();

            var count = await GetFailedLoginCount(username, TimeSpan.FromMinutes(15));
            if (count >= MaxFailedAttempts)
            {
                await LockAccountIfNeeded(username);
                _logger.LogWarning(
                    "Account locked due to excessive failed login attempts: {Username}",
                    username);
            }
        }

        public async Task LogSuspiciousActivity(string description, string userId,
            string ipAddress)
        {
            var securityEvent = new SecurityEvent
            {
                EventType = "SuspiciousActivity",
                Description = description,
                UserId = userId,
                IpAddress = ipAddress,
                Timestamp = DateTime.UtcNow,
                Severity = "High"
            };

            _context.SecurityEvents.Add(securityEvent);
            await _context.SaveChangesAsync();

            _logger.LogWarning(
                "Suspicious activity detected for user {UserId} from IP {IpAddress}: {Description}",
                userId, ipAddress, description);
        }

        public async Task<int> GetFailedLoginCount(string username, TimeSpan timeWindow)
        {
            var threshold = DateTime.UtcNow.Subtract(timeWindow);
            return await _context.SecurityEvents
                .Where(e => e.Username == username &&
                            e.EventType == "FailedLogin" &&
                            e.Timestamp > threshold)
                .CountAsync();
        }

        public async Task LockAccountIfNeeded(string username)
        {
            // Implement account lockout logic
            await Task.CompletedTask;
        }
    }

    public class SecurityEvent
    {
        public int Id { get; set; }
        public string EventType { get; set; }
        public string Description { get; set; }
        public string Username { get; set; }
        public string UserId { get; set; }
        public string IpAddress { get; set; }
        public DateTime Timestamp { get; set; }
        public string Severity { get; set; }
    }
}
```

---

## Plan de Implementación Recomendado

### Semana 1: Fundación
- Implementar JwtService y RefreshTokenRepository
- Agregar Serilog + RequestLoggingMiddleware
- Crear AuthController con validación de contraseñas
- Configurar CSP headers en Nginx

### Semana 2: Validación y Control de Acceso
- Implementar FluentValidation en todos los DTOs
- Agregar RateLimitingMiddleware
- Crear AuditingMiddleware
- Implementar DataEncryptionService

### Semana 3: Seguridad Avanzada
- Configurar CI/CD con SAST/DAST
- Implementar TDE en SQL Server
- Agregar Data Masking
- Crear Security Monitoring Service

### Semana 4: Preparación y Testing
- Implementar backup strategy
- Crear Incident Response Plan
- Agregar chaos engineering tests
- Realizar security audit

---

## Configuración de Entorno - Variables Requeridas

Crear `.env.production`:

```bash
# ASPNETCORE
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=https://+:8080
ASPNETCORE_Kestrel__Certificates__Default__Path=/app/certs/cert.pem
ASPNETCORE_Kestrel__Certificates__Default__KeyPath=/app/certs/key.pem

# Database
ConnectionStrings__DefaultConnection=Server=sql-server,1433;Database=QuickMeet;User Id=quickmeet_app;Password=[STRONG_PASSWORD];TrustServerCertificate=true;Encrypt=true;

# JWT
JWT_Secret=[GENERATE_32_CHAR_KEY]
JWT_Issuer=quickmeet-api
JWT_Audience=quickmeet-client
JWT_ExpiryMinutes=15

# Encryption
Encryption__Key=[GENERATE_32_CHAR_KEY]

# Rate Limiting
RateLimiting__RequestsPerMinute=100

# CORS
CORS_AllowedOrigins=https://quickmeet.app,https://www.quickmeet.app

# Application Insights
APPINSIGHTS_INSTRUMENTATION_KEY=[YOUR_KEY]

# Security Headers
RequireHttps=true
```

Para generar claves seguras:

```bash
openssl rand -base64 32
```

---

## Verificación de Seguridad

Checklist post-implementación:

- [ ] Todos los endpoints requieren HTTPS
- [ ] JWT tokens tienen tiempo de expiración corto (15 min)
- [ ] Refresh tokens se pueden revocar
- [ ] Contraseñas se hashean con BCrypt/PBKDF2
- [ ] Todos los inputs se validan con FluentValidation
- [ ] Rate limiting está activo en todos los endpoints
- [ ] Logging centralizado con correlation IDs
- [ ] Auditoría de cambios en base de datos
- [ ] CORS está restringido a dominios conocidos
- [ ] CSP headers están presentes
- [ ] HTTPS es forzado (HSTS)
- [ ] Secretos no están en código
- [ ] CI/CD ejecuta SAST y dependency checks
- [ ] Container images se escanean con Trivy
- [ ] TDE está habilitado en SQL Server
- [ ] Backup automático cada 24 horas
- [ ] Incident response plan documentado
- [ ] Security monitoring activo

