namespace QuickMeet.Core.DTOs.Providers;

public record UpdateProviderDto(
    string? FullName,
    string? Description,
    string? PhoneNumber,
    int? AppointmentDurationMinutes
);
