namespace QuickMeet.API.DTOs.Availability;

public class TimeSlotDto
{
    public int Id { get; set; }
    
    public string StartTime { get; set; } = string.Empty;
    
    public string EndTime { get; set; } = string.Empty;
    
    public string Status { get; set; } = string.Empty;
}
