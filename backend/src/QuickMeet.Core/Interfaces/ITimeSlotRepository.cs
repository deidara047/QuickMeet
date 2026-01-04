using QuickMeet.Core.Entities;

namespace QuickMeet.Core.Interfaces;

public interface ITimeSlotRepository
{
    Task<IEnumerable<TimeSlot>> GetByProviderIdAndDateRangeAsync(int providerId, DateTimeOffset startDate, DateTimeOffset endDate);
    
    Task<IEnumerable<TimeSlot>> GetAvailableSlotsByProviderAndDateAsync(int providerId, DateTimeOffset date);
    
    Task<TimeSlot?> GetByIdAsync(int id);
    
    Task AddAsync(TimeSlot timeSlot);
    
    Task AddRangeAsync(IEnumerable<TimeSlot> timeSlots);
    
    Task<int> RemoveByProviderIdAndDateRangeAsync(int providerId, DateTimeOffset startDate);
    
    Task<int> SaveChangesAsync();
}
