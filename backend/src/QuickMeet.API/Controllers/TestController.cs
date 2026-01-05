#if DEBUG
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuickMeet.Core.Interfaces;
using QuickMeet.Infrastructure.Data;
using QuickMeet.Core.DTOs.Auth;
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
