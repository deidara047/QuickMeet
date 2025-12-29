using Microsoft.AspNetCore.Mvc;
using QuickMeet.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace QuickMeet.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HealthController : ControllerBase
    {
        private readonly IQuickMeetDbContext _dbContext;
        private readonly ILogger<HealthController> _logger;

        public HealthController(IQuickMeetDbContext dbContext, ILogger<HealthController> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        [HttpGet("ping")]
        public async Task<IActionResult> Ping()
        {
            try
            {
                _logger.LogInformation("Health check iniciado");
                
                // Intenta conectar a la BD
                var canConnect = await _dbContext.Providers.AnyAsync();
                
                _logger.LogInformation("✅ Conexión a BD exitosa");
                
                return Ok(new
                {
                    status = "healthy",
                    message = "Backend y BD se comunican correctamente",
                    timestamp = DateTime.UtcNow,
                    database = "QuickMeet",
                    environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error en health check: {ex.Message}");
                
                return StatusCode(503, new
                {
                    status = "unhealthy",
                    message = "No se pudo conectar a la BD",
                    error = ex.Message,
                    timestamp = DateTime.UtcNow
                });
            }
        }
    }
}
