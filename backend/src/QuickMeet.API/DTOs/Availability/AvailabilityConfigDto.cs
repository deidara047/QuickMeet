namespace QuickMeet.API.DTOs.Availability;

public class AvailabilityConfigDto
{
    public List<DayConfigDto> Days { get; set; } = [];
    
    public int AppointmentDurationMinutes { get; set; } = 30;
    
    public int BufferMinutes { get; set; } = 0;
}
