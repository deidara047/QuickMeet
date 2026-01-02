using QuickMeet.Core.Entities;

namespace QuickMeet.Core.Interfaces;

public interface IAvailabilityService
{
    Task ConfigureAvailabilityAsync(int providerId, AvailabilityConfig config);
    
    Task<IEnumerable<TimeSlot>> GenerateTimeSlotsAsync(int providerId, DateTimeOffset startDate, DateTimeOffset endDate);
    
    Task<IEnumerable<TimeSlot>> GetAvailableSlotsForDateAsync(int providerId, DateTimeOffset date);
    
    Task<AvailabilityConfig?> GetProviderAvailabilityAsync(int providerId);
}

public class AvailabilityConfig
{
    public List<DayConfig> Days { get; set; } = [];
    
    public int AppointmentDurationMinutes { get; set; } = 30;
    
    public int BufferMinutes { get; set; } = 0;
}

public class DayConfig
{
    public DayOfWeek Day { get; set; }
    
    public bool IsWorking { get; set; }
    
    public TimeSpan? StartTime { get; set; }
    
    public TimeSpan? EndTime { get; set; }
    
    public List<BreakConfig> Breaks { get; set; } = [];
}

public class BreakConfig
{
    public TimeSpan StartTime { get; set; }
    
    public TimeSpan EndTime { get; set; }
}
