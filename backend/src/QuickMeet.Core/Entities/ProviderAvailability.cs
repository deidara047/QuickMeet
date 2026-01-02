namespace QuickMeet.Core.Entities;

public class ProviderAvailability
{
    public int Id { get; set; }
    
    public int ProviderId { get; set; }
    
    public DayOfWeek DayOfWeek { get; set; }
    
    public TimeSpan StartTime { get; set; }
    
    public TimeSpan EndTime { get; set; }
    
    public int AppointmentDurationMinutes { get; set; } = 30;
    
    public int BufferMinutes { get; set; } = 0;
    
    public DateTimeOffset CreatedAt { get; set; }
    
    public DateTimeOffset? UpdatedAt { get; set; }
    
    public Provider Provider { get; set; } = null!;
    
    public ICollection<Break> Breaks { get; set; } = [];
}
