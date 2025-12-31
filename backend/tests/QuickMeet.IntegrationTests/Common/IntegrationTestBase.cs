using Microsoft.Extensions.DependencyInjection;
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

    protected Task<HttpResponseMessage> AuthenticatedRequest(
        HttpMethod method,
        string url,
        object? body = null,
        string? token = null)
    {
        // Phase 2 - Implementar cuando necesites tests con autenticación
        throw new NotImplementedException("Fase 2 - Por implementar");
    }
}
