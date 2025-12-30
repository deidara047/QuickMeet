using Microsoft.EntityFrameworkCore;
using QuickMeet.Core.Entities;
using QuickMeet.Core.Interfaces;
using QuickMeet.Infrastructure.Data;

namespace QuickMeet.Infrastructure.Repositories;

/// <summary>
/// Implementación del repositorio para operaciones con Providers
/// Encapsula toda la lógica de acceso a datos
/// </summary>
public class ProviderRepository : IProviderRepository
{
    private readonly QuickMeetDbContext _dbContext;

    public ProviderRepository(QuickMeetDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Provider?> GetByEmailAsync(string email)
    {
        return await _dbContext.Providers
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Email == email);
    }

    public async Task<Provider?> GetByIdAsync(int id)
    {
        return await _dbContext.Providers
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<bool> ExistsByEmailAsync(string email)
    {
        return await _dbContext.Providers
            .AnyAsync(p => p.Email == email);
    }

    public async Task<bool> ExistsByUsernameAsync(string username)
    {
        return await _dbContext.Providers
            .AnyAsync(p => p.Username == username);
    }

    public async Task AddAsync(Provider provider)
    {
        await _dbContext.Providers.AddAsync(provider);
        await _dbContext.SaveChangesAsync();
    }

    public async Task UpdateAsync(Provider provider)
    {
        _dbContext.Providers.Update(provider);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<Provider?> GetByUsernameAsync(string username)
    {
        return await _dbContext.Providers
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Username == username);
    }
}
