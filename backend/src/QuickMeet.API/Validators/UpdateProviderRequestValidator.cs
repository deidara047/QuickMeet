using FluentValidation;
using QuickMeet.Core.DTOs.Providers;

namespace QuickMeet.API.Validators;

/// <summary>
/// Validador para UpdateProviderDto
/// Valida SOLO formato e integridad de datos de entrada
/// NO valida reglas de negocio (eso lo hace el Service)
/// </summary>
public class UpdateProviderRequestValidator : AbstractValidator<UpdateProviderDto>
{
    public UpdateProviderRequestValidator()
    {
        // FullName: Validar SOLO si viene en el request
        When(x => x.FullName != null, () =>
        {
            RuleFor(x => x.FullName)
                .NotEmpty()
                .WithMessage("FullName no puede estar vacío")
                .Length(3, 100)
                .WithMessage("FullName debe tener entre 3 y 100 caracteres");
        });

        // Description: Validar longitud máxima
        When(x => x.Description != null, () =>
        {
            RuleFor(x => x.Description)
                .MaximumLength(500)
                .WithMessage("Description no puede exceder 500 caracteres");
        });

        // AppointmentDurationMinutes: Validar rango
        When(x => x.AppointmentDurationMinutes.HasValue, () =>
        {
            RuleFor(x => x.AppointmentDurationMinutes)
                .InclusiveBetween(15, 120)
                .WithMessage("AppointmentDurationMinutes debe estar entre 15 y 120 minutos");
        });
    }
}

/// <summary>
/// Validador para upload de foto
/// Nota: En producción, esto se validaría en el Controller antes de llamar al Service
/// porque IFormFile no se puede pasar a través de FluentValidation directamente
/// </summary>
public class UploadPhotoRequestValidator : AbstractValidator<UploadPhotoRequest>
{
    private const long MaxPhotoSize = 5 * 1024 * 1024; // 5MB
    private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };

    public UploadPhotoRequestValidator()
    {
        RuleFor(x => x.FileName)
            .NotEmpty()
            .WithMessage("Nombre de archivo es requerido")
            .Must(fileName =>
            {
                if (string.IsNullOrEmpty(fileName))
                    return false;
                var extension = Path.GetExtension(fileName).ToLowerInvariant();
                return AllowedExtensions.Contains(extension);
            })
            .WithMessage("Extensión de archivo no permitida. Solo se aceptan: JPG, PNG, GIF, WebP");

        RuleFor(x => x.FileContent)
            .NotEmpty()
            .WithMessage("El archivo está vacío")
            .Must(content => content.Length <= MaxPhotoSize)
            .WithMessage("El archivo no puede exceder 5 MB");
    }
}

/// <summary>
/// DTO auxiliar para validar datos de foto
/// En el Controller se convierte IFormFile a este DTO para validar
/// </summary>
public class UploadPhotoRequest
{
    public byte[] FileContent { get; set; } = Array.Empty<byte>();
    public string FileName { get; set; } = string.Empty;
}
