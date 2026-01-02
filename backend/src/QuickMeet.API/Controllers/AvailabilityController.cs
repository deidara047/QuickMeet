using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuickMeet.API.DTOs.Availability;
using QuickMeet.Core.Interfaces;
using System.Security.Claims;

namespace QuickMeet.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AvailabilityController : ControllerBase
{
    private readonly IAvailabilityService _availabilityService;
    private readonly ILogger<AvailabilityController> _logger;

    public AvailabilityController(
        IAvailabilityService availabilityService,
        ILogger<AvailabilityController> logger)
    {
        _availabilityService = availabilityService;
        _logger = logger;
    }

    [HttpPost("configure")]
    public async Task<IActionResult> ConfigureAvailability([FromBody] AvailabilityConfigDto configDto)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var providerId))
            {
                _logger.LogWarning("Invalid user ID claim in ConfigureAvailability");
                return Unauthorized(new { error = "Usuario no autenticado correctamente" });
            }

            _logger.LogInformation("Configurando disponibilidad para profesional {ProviderId}", providerId);

            var config = new AvailabilityConfig
            {
                Days = configDto.Days
                    .Select(d => new DayConfig
                    {
                        Day = d.Day,
                        IsWorking = d.IsWorking,
                        StartTime = d.StartTime,
                        EndTime = d.EndTime,
                        Breaks = d.Breaks
                            .Select(b => new BreakConfig
                            {
                                StartTime = b.StartTime,
                                EndTime = b.EndTime
                            })
                            .ToList()
                    })
                    .ToList(),
                AppointmentDurationMinutes = configDto.AppointmentDurationMinutes,
                BufferMinutes = configDto.BufferMinutes
            };

            await _availabilityService.ConfigureAvailabilityAsync(providerId, config);

            var generatedSlots = await _availabilityService.GenerateTimeSlotsAsync(
                providerId,
                DateTimeOffset.UtcNow,
                DateTimeOffset.UtcNow.AddDays(3));

            var response = new AvailabilityResponseDto
            {
                Success = true,
                Message = "Disponibilidad configurada exitosamente",
                GeneratedSlots = generatedSlots
                    .Select(s => new TimeSlotDto
                    {
                        Id = s.Id,
                        StartTime = s.StartTime.ToString("O"),
                        EndTime = s.EndTime.ToString("O"),
                        Status = s.Status.ToString()
                    })
                    .ToList()
            };

            _logger.LogInformation("Disponibilidad configurada exitosamente para profesional {ProviderId}", providerId);
            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Validación fallida en ConfigureAvailability: {Message}", ex.Message);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al configurar disponibilidad");
            return StatusCode(500, new { error = "Error interno del servidor" });
        }
    }

    [HttpGet("{providerId}")]
    public async Task<IActionResult> GetAvailability(int providerId)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var currentUserId))
            {
                _logger.LogWarning("Invalid user ID claim in GetAvailability");
                return Unauthorized(new { error = "Usuario no autenticado correctamente" });
            }

            if (currentUserId != providerId)
            {
                _logger.LogWarning("Acceso no autorizado: Usuario {UserId} intentando acceder a {ProviderId}", currentUserId, providerId);
                return Forbid();
            }

            _logger.LogInformation("Obteniendo disponibilidad para profesional {ProviderId}", providerId);

            var availability = await _availabilityService.GetProviderAvailabilityAsync(providerId);

            if (availability == null)
            {
                _logger.LogInformation("No hay disponibilidad configurada para profesional {ProviderId}", providerId);
                return NotFound(new { error = "No hay disponibilidad configurada" });
            }

            var response = new AvailabilityConfigDto
            {
                Days = availability.Days
                    .Select(d => new DayConfigDto
                    {
                        Day = d.Day,
                        IsWorking = d.IsWorking,
                        StartTime = d.StartTime,
                        EndTime = d.EndTime,
                        Breaks = d.Breaks
                            .Select(b => new BreakDto
                            {
                                StartTime = b.StartTime,
                                EndTime = b.EndTime
                            })
                            .ToList()
                    })
                    .ToList(),
                AppointmentDurationMinutes = availability.AppointmentDurationMinutes,
                BufferMinutes = availability.BufferMinutes
            };

            _logger.LogInformation("Disponibilidad obtenida exitosamente para profesional {ProviderId}", providerId);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener disponibilidad");
            return StatusCode(500, new { error = "Error interno del servidor" });
        }
    }

    [HttpPut("{providerId}")]
    public async Task<IActionResult> UpdateAvailability(int providerId, [FromBody] AvailabilityConfigDto configDto)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var currentUserId))
            {
                _logger.LogWarning("Invalid user ID claim in UpdateAvailability");
                return Unauthorized(new { error = "Usuario no autenticado correctamente" });
            }

            if (currentUserId != providerId)
            {
                _logger.LogWarning("Acceso no autorizado: Usuario {UserId} intentando actualizar {ProviderId}", currentUserId, providerId);
                return Forbid();
            }

            _logger.LogInformation("Actualizando disponibilidad para profesional {ProviderId}", providerId);

            var config = new AvailabilityConfig
            {
                Days = configDto.Days
                    .Select(d => new DayConfig
                    {
                        Day = d.Day,
                        IsWorking = d.IsWorking,
                        StartTime = d.StartTime,
                        EndTime = d.EndTime,
                        Breaks = d.Breaks
                            .Select(b => new BreakConfig
                            {
                                StartTime = b.StartTime,
                                EndTime = b.EndTime
                            })
                            .ToList()
                    })
                    .ToList(),
                AppointmentDurationMinutes = configDto.AppointmentDurationMinutes,
                BufferMinutes = configDto.BufferMinutes
            };

            await _availabilityService.ConfigureAvailabilityAsync(providerId, config);

            var generatedSlots = await _availabilityService.GenerateTimeSlotsAsync(
                providerId,
                DateTimeOffset.UtcNow,
                DateTimeOffset.UtcNow.AddDays(3));

            var response = new AvailabilityResponseDto
            {
                Success = true,
                Message = "Disponibilidad actualizada exitosamente",
                GeneratedSlots = generatedSlots
                    .Select(s => new TimeSlotDto
                    {
                        Id = s.Id,
                        StartTime = s.StartTime.ToString("O"),
                        EndTime = s.EndTime.ToString("O"),
                        Status = s.Status.ToString()
                    })
                    .ToList()
            };

            _logger.LogInformation("Disponibilidad actualizada exitosamente para profesional {ProviderId}", providerId);
            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Validación fallida en UpdateAvailability: {Message}", ex.Message);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar disponibilidad");
            return StatusCode(500, new { error = "Error interno del servidor" });
        }
    }

    [HttpGet("slots/{providerId}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetAvailableSlots(int providerId, [FromQuery] DateTimeOffset date)
    {
        try
        {
            _logger.LogInformation("Obteniendo slots disponibles para profesional {ProviderId} en fecha {Date}", providerId, date);

            var slots = await _availabilityService.GetAvailableSlotsForDateAsync(providerId, date);

            var response = new
            {
                success = true,
                date = date.ToString("O"),
                slots = slots
                    .Select(s => new
                    {
                        id = s.Id,
                        startTime = s.StartTime.ToString("O"),
                        endTime = s.EndTime.ToString("O"),
                        status = s.Status.ToString()
                    })
                    .ToList()
            };

            _logger.LogInformation("Slots disponibles obtenidos para profesional {ProviderId}", providerId);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener slots disponibles");
            return StatusCode(500, new { error = "Error interno del servidor" });
        }
    }
}
