namespace QuickMeet.API.DTOs.Availability;

public class DayConfigDto
{
    public DayOfWeek Day { get; set; }
    
    public bool IsWorking { get; set; }
    
    public TimeSpan? StartTime { get; set; }
    
    public TimeSpan? EndTime { get; set; }
    
    public List<BreakDto> Breaks { get; set; } = [];
}
