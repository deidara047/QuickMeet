using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuickMeet.Core.Interfaces;
using QuickMeet.Core.DTOs.Providers;
using QuickMeet.API.Validators;
using System.Security.Claims;

namespace QuickMeet.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProvidersController : ControllerBase
{
    private readonly IProviderService _providerService;
    private readonly ILogger<ProvidersController> _logger;

    public ProvidersController(
        IProviderService providerService,
        ILogger<ProvidersController> logger)
    {
        _providerService = providerService;
        _logger = logger;
    }

    [HttpGet("{providerId}")]
    public async Task<IActionResult> GetProvider(int providerId)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var currentUserId))
            {
                _logger.LogWarning("Invalid user ID claim in GetProvider");
                return Unauthorized(new { error = "Usuario no autenticado correctamente" });
            }

            if (currentUserId != providerId)
            {
                _logger.LogWarning("Acceso no autorizado: Usuario {UserId} intentando acceder a {ProviderId}", 
                    currentUserId, providerId);
                return Forbid();
            }

            _logger.LogInformation("Obteniendo perfil del proveedor {ProviderId}", providerId);

            var profile = await _providerService.GetProviderByIdAsync(providerId);

            if (profile == null)
            {
                _logger.LogWarning("Proveedor {ProviderId} no encontrado", providerId);
                return NotFound(new { error = "Proveedor no encontrado" });
            }

            _logger.LogInformation("Perfil obtenido exitosamente para proveedor {ProviderId}", providerId);
            return Ok(profile);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener perfil del proveedor");
            return StatusCode(500, new { error = "Error interno del servidor" });
        }
    }

    [HttpPut("{providerId}")]
    public async Task<IActionResult> UpdateProvider(int providerId, [FromBody] UpdateProviderDto updateDto)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var currentUserId))
            {
                _logger.LogWarning("Invalid user ID claim in UpdateProvider");
                return Unauthorized(new { error = "Usuario no autenticado correctamente" });
            }

            if (currentUserId != providerId)
            {
                _logger.LogWarning("Acceso no autorizado: Usuario {UserId} intentando actualizar {ProviderId}", 
                    currentUserId, providerId);
                return Forbid();
            }

            _logger.LogInformation("Actualizando perfil del proveedor {ProviderId}", providerId);

            // Validar con el Validator (FluentValidation se ejecuta automáticamente en ModelState)
            var validator = new UpdateProviderRequestValidator();
            var validationResult = await validator.ValidateAsync(updateDto);
            
            if (!validationResult.IsValid)
            {
                _logger.LogWarning("Validación fallida en UpdateProvider");
                return BadRequest(new { errors = validationResult.Errors.Select(e => e.ErrorMessage) });
            }

            var updatedProfile = await _providerService.UpdateProviderAsync(providerId, updateDto);

            if (updatedProfile == null)
            {
                _logger.LogWarning("Proveedor {ProviderId} no encontrado", providerId);
                return NotFound(new { error = "Proveedor no encontrado" });
            }

            _logger.LogInformation("Perfil del proveedor {ProviderId} actualizado exitosamente", providerId);
            return Ok(updatedProfile);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Validación de negocio fallida en UpdateProvider: {Message}", ex.Message);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar perfil del proveedor");
            return StatusCode(500, new { error = "Error interno del servidor" });
        }
    }

    [HttpPost("{providerId}/photo")]
    public async Task<IActionResult> UploadPhoto(int providerId, IFormFile file)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var currentUserId))
            {
                _logger.LogWarning("Invalid user ID claim in UploadPhoto");
                return Unauthorized(new { error = "Usuario no autenticado correctamente" });
            }

            if (currentUserId != providerId)
            {
                _logger.LogWarning("Acceso no autorizado: Usuario {UserId} intentando subir foto a {ProviderId}", 
                    currentUserId, providerId);
                return Forbid();
            }

            _logger.LogInformation("Subiendo foto para proveedor {ProviderId}", providerId);

            // Validar el archivo con el Validator
            using var stream = file.OpenReadStream();
            using var memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream);
            var fileContent = memoryStream.ToArray();
            
            var uploadRequest = new UploadPhotoRequest
            {
                FileContent = fileContent,
                FileName = file.FileName
            };

            var validator = new UploadPhotoRequestValidator();
            var validationResult = await validator.ValidateAsync(uploadRequest);

            if (!validationResult.IsValid)
            {
                _logger.LogWarning("Validación de archivo fallida en UploadPhoto");
                return BadRequest(new { errors = validationResult.Errors.Select(e => e.ErrorMessage) });
            }

            var photoUrl = await _providerService.UploadPhotoAsync(providerId, fileContent, file.FileName);

            _logger.LogInformation("Foto subida exitosamente para proveedor {ProviderId}", providerId);
            return Ok(new { photoUrl });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Validación de negocio fallida en UploadPhoto: {Message}", ex.Message);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al subir foto");
            return StatusCode(500, new { error = "Error interno del servidor" });
        }
    }
}
