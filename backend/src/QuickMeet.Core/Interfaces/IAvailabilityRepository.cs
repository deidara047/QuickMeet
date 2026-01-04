using QuickMeet.Core.Entities;

namespace QuickMeet.Core.Interfaces;

public interface IAvailabilityRepository
{
    Task<IEnumerable<ProviderAvailability>> GetByProviderIdAsync(int providerId);
    
    Task<ProviderAvailability?> GetByIdAsync(int id);
    
    Task AddAsync(ProviderAvailability availability);
    
    Task AddRangeAsync(IEnumerable<ProviderAvailability> availabilities);
    
    Task<int> RemoveByProviderIdAsync(int providerId);
    
    Task<int> SaveChangesAsync();
}
