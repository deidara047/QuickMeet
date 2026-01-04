using Microsoft.Extensions.DependencyInjection;
using QuickMeet.Core.Entities;
using QuickMeet.Infrastructure.Data;
using QuickMeet.IntegrationTests.Fixtures;

namespace QuickMeet.IntegrationTests.Common;

public abstract class IntegrationTestBase : IClassFixture<QuickMeetWebApplicationFactory>
{
    protected readonly HttpClient Client;
    protected readonly QuickMeetWebApplicationFactory Factory;

    protected IntegrationTestBase(QuickMeetWebApplicationFactory factory)
    {
        Factory = factory;
        Client = factory.CreateClient();
        // Resetear BD antes de cada test para aislamiento
        ResetDatabase();
    }

    protected void ResetDatabase()
    {
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<QuickMeetDbContext>();
        db.Database.EnsureDeleted();
        db.Database.EnsureCreated();
    }

    protected async Task<T> GetFromDatabase<T>(Func<QuickMeetDbContext, Task<T>> query)
    {
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<QuickMeetDbContext>();
        return await query(db);
    }

    protected async Task SeedDatabase(Action<QuickMeetDbContext> seed)
    {
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<QuickMeetDbContext>();
        seed(db);
        await db.SaveChangesAsync();
    }

    /// <summary>
    /// Establece los headers de autenticación para que el TestAuthHandler autentique la siguiente request.
    /// </summary>
    protected void SetTestUser(int userId, string? email = null)
    {
        Client.DefaultRequestHeaders.Remove("X-Test-UserId");
        Client.DefaultRequestHeaders.Remove("X-Test-Email");
        
        Client.DefaultRequestHeaders.Add("X-Test-UserId", userId.ToString());
        
        if (!string.IsNullOrEmpty(email))
        {
            Client.DefaultRequestHeaders.Add("X-Test-Email", email);
        }
    }

    /// <summary>
    /// Limpia los headers de autenticación de test.
    /// </summary>
    protected void ClearTestUser()
    {
        Client.DefaultRequestHeaders.Remove("X-Test-UserId");
        Client.DefaultRequestHeaders.Remove("X-Test-Email");
    }

    /// <summary>
    /// Registra un proveedor y devuelve su ID (sin necesidad de tokens JWT).
    /// Útil cuando necesitas un ID de proveedor en la BD para referencias.
    /// </summary>
    protected async Task<int> RegisterTestProvider(string email)
    {
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<QuickMeetDbContext>();
        
        var provider = new Provider
        {
            Email = email,
            Username = email.Split('@')[0] + Guid.NewGuid().ToString("N").Substring(0, 5),
            FullName = "Test Provider",
            PasswordHash = "hashedpassword", // Mock password hash
            Status = ProviderStatus.Active,
            EmailVerifiedAt = DateTimeOffset.UtcNow,
            CreatedAt = DateTimeOffset.UtcNow
        };
        
        db.Providers.Add(provider);
        await db.SaveChangesAsync();
        
        return provider.Id;
    }
}
