namespace QuickMeet.Core.DTOs.Availability;

/// <summary>
/// DTO que representa la configuración de un día laboral.
/// Utilizado por IAvailabilityService como parte de AvailabilityConfigDto.
/// </summary>
public class DayConfigDto
{
    public DayOfWeek Day { get; set; }
    
    public bool IsWorking { get; set; }
    
    public TimeSpan? StartTime { get; set; }
    
    public TimeSpan? EndTime { get; set; }
    
    public List<BreakDto> Breaks { get; set; } = [];
}
