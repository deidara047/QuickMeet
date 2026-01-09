using QuickMeet.Core.Entities;
using QuickMeet.Core.Interfaces;
using QuickMeet.Core.DTOs.Providers;
using Microsoft.Extensions.Logging;

namespace QuickMeet.Core.Services;

public class ProviderService : IProviderService
{
    private readonly IProviderRepository _providerRepository;
    private readonly ILogger<ProviderService> _logger;

    public ProviderService(
        IProviderRepository providerRepository,
        ILogger<ProviderService> logger)
    {
        _providerRepository = providerRepository;
        _logger = logger;
    }

    public async Task<ProviderProfileDto?> GetProviderByIdAsync(int providerId)
    {
        _logger.LogInformation("Obteniendo perfil del proveedor {ProviderId}", providerId);

        var provider = await _providerRepository.GetByIdAsync(providerId);

        if (provider == null)
        {
            _logger.LogWarning("Proveedor {ProviderId} no encontrado", providerId);
            return null;
        }

        return MapToDto(provider);
    }

    public async Task<ProviderProfileDto?> UpdateProviderAsync(int providerId, UpdateProviderDto updateDto)
    {
        _logger.LogInformation("Actualizando perfil del proveedor {ProviderId}", providerId);

        var provider = await _providerRepository.GetByIdAsync(providerId);

        if (provider == null)
        {
            _logger.LogWarning("Proveedor {ProviderId} no encontrado para actualización", providerId);
            return null;
        }

        // ✅ Validación de NEGOCIO: No se puede actualizar un provider suspendido o eliminado
        if (provider.Status == ProviderStatus.Suspended)
        {
            _logger.LogWarning("Intento de actualizar provider {ProviderId} con estado Suspended", providerId);
            throw new InvalidOperationException("No se puede actualizar un proveedor suspendido");
        }

        if (provider.Status == ProviderStatus.Deleted)
        {
            _logger.LogWarning("Intento de actualizar provider {ProviderId} con estado Deleted", providerId);
            throw new InvalidOperationException("No se puede actualizar un proveedor eliminado");
        }

        // ✅ Actualizar solo los campos que vinieron en el DTO
        // Nota: Las validaciones de ENTRADA (formato, longitud) ya fueron validadas en el API por FluentValidation
        if (updateDto.FullName != null)
        {
            provider.FullName = updateDto.FullName;
        }

        if (updateDto.Description != null)
        {
            provider.Description = updateDto.Description;
        }

        if (updateDto.PhoneNumber != null)
        {
            provider.PhoneNumber = updateDto.PhoneNumber;
        }

        if (updateDto.AppointmentDurationMinutes.HasValue)
        {
            provider.AppointmentDurationMinutes = updateDto.AppointmentDurationMinutes.Value;
        }

        provider.UpdatedAt = DateTimeOffset.UtcNow;

        await _providerRepository.UpdateAsync(provider);
        _logger.LogInformation("Perfil del proveedor {ProviderId} actualizado exitosamente", providerId);

        return MapToDto(provider);
    }

    public async Task<string> UploadPhotoAsync(int providerId, byte[] fileContent, string fileName)
    {
        _logger.LogInformation("Subiendo foto para proveedor {ProviderId}", providerId);

        var provider = await _providerRepository.GetByIdAsync(providerId);

        if (provider == null)
        {
            _logger.LogWarning("Proveedor {ProviderId} no encontrado para subir foto", providerId);
            throw new InvalidOperationException("Provider no encontrado");
        }

        // ✅ Validación de NEGOCIO: No se puede subir foto a un provider suspendido o eliminado
        if (provider.Status == ProviderStatus.Suspended)
        {
            _logger.LogWarning("Intento de subir foto a provider {ProviderId} con estado Suspended", providerId);
            throw new InvalidOperationException("No se puede actualizar la foto de un proveedor suspendido");
        }

        if (provider.Status == ProviderStatus.Deleted)
        {
            _logger.LogWarning("Intento de subir foto a provider {ProviderId} con estado Deleted", providerId);
            throw new InvalidOperationException("No se puede actualizar la foto de un proveedor eliminado");
        }

        // ✅ Nota: Las validaciones de ENTRADA (tamaño, extensión) ya fueron validadas en el API por el Controller
        // Este Service recibe datos YA validados

        // TODO: Implementar almacenamiento en S3 o similar
        // Por ahora, simular con un URL de prueba
        var photoUrl = $"/photos/providers/{providerId}/{Guid.NewGuid()}{Path.GetExtension(fileName).ToLowerInvariant()}";
        
        provider.PhotoUrl = photoUrl;
        provider.UpdatedAt = DateTimeOffset.UtcNow;

        await _providerRepository.UpdateAsync(provider);
        _logger.LogInformation("Foto subida exitosamente para proveedor {ProviderId}", providerId);

        return photoUrl;
    }

    private static ProviderProfileDto MapToDto(Provider provider)
    {
        return new ProviderProfileDto(
            Id: provider.Id,
            Email: provider.Email,
            Username: provider.Username,
            FullName: provider.FullName,
            Description: provider.Description,
            PhotoUrl: provider.PhotoUrl,
            PhoneNumber: provider.PhoneNumber,
            AppointmentDurationMinutes: provider.AppointmentDurationMinutes,
            CreatedAt: provider.CreatedAt,
            UpdatedAt: provider.UpdatedAt,
            EmailVerifiedAt: provider.EmailVerifiedAt
        );
    }
}
