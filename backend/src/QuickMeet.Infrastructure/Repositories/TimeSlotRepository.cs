using Microsoft.EntityFrameworkCore;
using QuickMeet.Core.Entities;
using QuickMeet.Core.Interfaces;
using QuickMeet.Infrastructure.Data;

namespace QuickMeet.Infrastructure.Repositories;

public class TimeSlotRepository : ITimeSlotRepository
{
    private readonly IQuickMeetDbContext _dbContext;

    public TimeSlotRepository(IQuickMeetDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IEnumerable<TimeSlot>> GetByProviderIdAndDateRangeAsync(
        int providerId,
        DateTimeOffset startDate,
        DateTimeOffset endDate)
    {
        return await _dbContext.TimeSlots
            .Where(ts => ts.ProviderId == providerId &&
                        ts.StartTime >= startDate &&
                        ts.StartTime < endDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<TimeSlot>> GetAvailableSlotsByProviderAndDateAsync(
        int providerId,
        DateTimeOffset date)
    {
        var dateOnly = date.Date;
        var nextDate = dateOnly.AddDays(1);

        return await _dbContext.TimeSlots
            .Where(ts => ts.ProviderId == providerId &&
                        ts.StartTime >= dateOnly &&
                        ts.StartTime < nextDate &&
                        ts.Status == TimeSlotStatus.Available)
            .OrderBy(ts => ts.StartTime)
            .ToListAsync();
    }

    public async Task<TimeSlot?> GetByIdAsync(int id)
    {
        return await _dbContext.TimeSlots.FindAsync(id);
    }

    public async Task AddAsync(TimeSlot timeSlot)
    {
        await _dbContext.TimeSlots.AddAsync(timeSlot);
    }

    public async Task AddRangeAsync(IEnumerable<TimeSlot> timeSlots)
    {
        await _dbContext.TimeSlots.AddRangeAsync(timeSlots);
    }

    public async Task<int> RemoveByProviderIdAndDateRangeAsync(int providerId, DateTimeOffset startDate)
    {
        var timeSlots = await _dbContext.TimeSlots
            .Where(ts => ts.ProviderId == providerId && ts.StartTime >= startDate)
            .ToListAsync();

        _dbContext.TimeSlots.RemoveRange(timeSlots);
        return timeSlots.Count;
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _dbContext.SaveChangesAsync();
    }
}
