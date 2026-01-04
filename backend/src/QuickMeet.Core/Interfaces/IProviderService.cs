namespace QuickMeet.Core.Interfaces;

public interface IProviderService
{
    Task<ProviderProfileDto?> GetProviderByIdAsync(int providerId);
    Task<ProviderProfileDto?> UpdateProviderAsync(int providerId, UpdateProviderDto updateDto);
    Task<string> UploadPhotoAsync(int providerId, byte[] fileContent, string fileName);
}

public record ProviderProfileDto(
    int Id,
    string Email,
    string Username,
    string FullName,
    string? Description,
    string? PhotoUrl,
    string? PhoneNumber,
    int AppointmentDurationMinutes,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt,
    DateTimeOffset? EmailVerifiedAt
);

public record UpdateProviderDto(
    string? FullName,
    string? Description,
    string? PhoneNumber,
    int? AppointmentDurationMinutes
);
