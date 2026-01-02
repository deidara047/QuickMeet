namespace QuickMeet.Core.Entities;

public class Break
{
    public int Id { get; set; }
    
    public int ProviderAvailabilityId { get; set; }
    
    public TimeSpan StartTime { get; set; }
    
    public TimeSpan EndTime { get; set; }
    
    public DateTimeOffset CreatedAt { get; set; }
    
    public ProviderAvailability ProviderAvailability { get; set; } = null!;
}
