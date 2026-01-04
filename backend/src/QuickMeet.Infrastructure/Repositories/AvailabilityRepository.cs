using Microsoft.EntityFrameworkCore;
using QuickMeet.Core.Entities;
using QuickMeet.Core.Interfaces;
using QuickMeet.Infrastructure.Data;

namespace QuickMeet.Infrastructure.Repositories;

public class AvailabilityRepository : IAvailabilityRepository
{
    private readonly IQuickMeetDbContext _dbContext;

    public AvailabilityRepository(IQuickMeetDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IEnumerable<ProviderAvailability>> GetByProviderIdAsync(int providerId)
    {
        return await _dbContext.ProviderAvailabilities
            .Where(pa => pa.ProviderId == providerId)
            .Include(pa => pa.Breaks)
            .ToListAsync();
    }

    public async Task<ProviderAvailability?> GetByIdAsync(int id)
    {
        return await _dbContext.ProviderAvailabilities
            .Include(pa => pa.Breaks)
            .FirstOrDefaultAsync(pa => pa.Id == id);
    }

    public async Task AddAsync(ProviderAvailability availability)
    {
        await _dbContext.ProviderAvailabilities.AddAsync(availability);
    }

    public async Task AddRangeAsync(IEnumerable<ProviderAvailability> availabilities)
    {
        await _dbContext.ProviderAvailabilities.AddRangeAsync(availabilities);
    }

    public async Task<int> RemoveByProviderIdAsync(int providerId)
    {
        var availabilities = await _dbContext.ProviderAvailabilities
            .Where(pa => pa.ProviderId == providerId)
            .ToListAsync();

        _dbContext.ProviderAvailabilities.RemoveRange(availabilities);
        return availabilities.Count;
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _dbContext.SaveChangesAsync();
    }
}
