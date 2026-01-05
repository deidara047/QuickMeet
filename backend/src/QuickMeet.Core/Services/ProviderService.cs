using QuickMeet.Core.Entities;
using QuickMeet.Core.Interfaces;
using QuickMeet.Core.DTOs.Providers;
using Microsoft.Extensions.Logging;

namespace QuickMeet.Core.Services;

public class ProviderService : IProviderService
{
    private readonly IProviderRepository _providerRepository;
    private readonly ILogger<ProviderService> _logger;
    private const long MaxPhotoSize = 5 * 1024 * 1024; // 5MB

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

        // Validaciones
        if (updateDto.FullName != null)
        {
            if (string.IsNullOrWhiteSpace(updateDto.FullName))
                throw new ArgumentException("FullName no puede estar vacío");
            if (updateDto.FullName.Length < 3)
                throw new ArgumentException("FullName debe tener al menos 3 caracteres");
            if (updateDto.FullName.Length > 100)
                throw new ArgumentException("FullName no puede exceder 100 caracteres");
        }

        if (updateDto.Description != null && updateDto.Description.Length > 500)
            throw new ArgumentException("Description no puede exceder 500 caracteres");

        if (updateDto.AppointmentDurationMinutes.HasValue)
        {
            var duration = updateDto.AppointmentDurationMinutes.Value;
            if (duration < 15 || duration > 120)
                throw new ArgumentException("AppointmentDurationMinutes debe estar entre 15 y 120");
        }

        var provider = await _providerRepository.GetByIdAsync(providerId);

        if (provider == null)
        {
            _logger.LogWarning("Proveedor {ProviderId} no encontrado para actualización", providerId);
            return null;
        }

        // Actualizar solo los campos que vinieron en el DTO
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

        provider.UpdatedAt = DateTime.UtcNow;

        await _providerRepository.UpdateAsync(provider);
        _logger.LogInformation("Perfil del proveedor {ProviderId} actualizado exitosamente", providerId);

        return MapToDto(provider);
    }

    public async Task<string> UploadPhotoAsync(int providerId, byte[] fileContent, string fileName)
    {
        _logger.LogInformation("Subiendo foto para proveedor {ProviderId}", providerId);

        // Validaciones
        if (string.IsNullOrWhiteSpace(fileName))
            throw new ArgumentException("El nombre de archivo no puede estar vacío");

        if (fileContent == null || fileContent.Length == 0)
            throw new ArgumentException("El archivo está vacío");

        if (fileContent.Length > MaxPhotoSize)
            throw new ArgumentException("El archivo no puede exceder 5 MB");

        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        if (!allowedExtensions.Contains(extension))
            throw new ArgumentException("Extensión de archivo no permitida. Solo se aceptan: JPG, PNG, GIF, WebP");

        var provider = await _providerRepository.GetByIdAsync(providerId);

        if (provider == null)
        {
            _logger.LogWarning("Proveedor {ProviderId} no encontrado para subir foto", providerId);
            throw new InvalidOperationException("Provider no encontrado");
        }

        // TODO: Implementar almacenamiento en S3 o similar
        // Por ahora, simular con un URL de prueba
        var photoUrl = $"/photos/providers/{providerId}/{Guid.NewGuid()}{extension}";
        
        provider.PhotoUrl = photoUrl;
        provider.UpdatedAt = DateTime.UtcNow;

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
