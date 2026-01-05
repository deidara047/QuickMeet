namespace QuickMeet.Core.DTOs.Availability;

/// <summary>
/// DTO que representa un período de descanso en la disponibilidad de un proveedor.
/// Utilizado por IAvailabilityService como parte de DayConfigDto.
/// </summary>
public class BreakDto
{
    public TimeSpan StartTime { get; set; }
    
    public TimeSpan EndTime { get; set; }
}
