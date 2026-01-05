namespace QuickMeet.Core.DTOs.Availability;

public class AvailabilityResponseDto
{
    public bool Success { get; set; }
    
    public string Message { get; set; } = string.Empty;
    
    public List<TimeSlotDto> GeneratedSlots { get; set; } = [];
}
