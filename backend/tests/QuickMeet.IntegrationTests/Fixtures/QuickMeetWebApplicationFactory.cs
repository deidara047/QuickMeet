using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Moq;
using QuickMeet.Core.Interfaces;
using QuickMeet.Infrastructure.Data;

namespace QuickMeet.IntegrationTests.Fixtures;

public class QuickMeetWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remover DbContext real (SQL Server)
            services.RemoveAll(typeof(DbContextOptions<QuickMeetDbContext>));

            // Agregar DbContext con InMemory Database
            services.AddDbContext<QuickMeetDbContext>(options =>
            {
                options.UseInMemoryDatabase("QuickMeetTestDb");
            });

            // Remover IEmailService real y reemplazar con Mock
            services.RemoveAll<IEmailService>();
            services.AddSingleton<IEmailService>(new Mock<IEmailService>().Object);

            // Crear el scope y asegurar que la BD se crea
            var sp = services.BuildServiceProvider();

            using (var scope = sp.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<QuickMeetDbContext>();
                db.Database.EnsureCreated();
            }
        });

        // Configurar logging: solo warnings y errores
        builder.ConfigureLogging(logging =>
        {
            logging.ClearProviders();
            logging.SetMinimumLevel(LogLevel.Warning);
        });
    }
}
