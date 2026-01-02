namespace QuickMeet.Core.Entities;

public class TimeSlot
{
    public int Id { get; set; }
    
    public int ProviderId { get; set; }
    
    public DateTimeOffset StartTime { get; set; }
    
    public DateTimeOffset EndTime { get; set; }
    
    public TimeSlotStatus Status { get; set; } = TimeSlotStatus.Available;
    
    public DateTimeOffset CreatedAt { get; set; }
    
    public DateTimeOffset? UpdatedAt { get; set; }
    
    public Provider Provider { get; set; } = null!;
}

public enum TimeSlotStatus
{
    Available = 0,
    Reserved = 1,
    Blocked = 2
}
