namespace QuickMeet.API.DTOs.Providers;

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
