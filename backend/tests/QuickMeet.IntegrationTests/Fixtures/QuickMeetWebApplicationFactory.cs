using System.Linq;
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
        // Configurar ambiente como Test para evitar Migrate() en Program.cs
        builder.UseEnvironment("Test");

        builder.ConfigureServices(services =>
        {
            // PRIMERO: Obtener el proveedor de servicios actual para hacer debug
            var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(DbContextOptions<QuickMeetDbContext>));
            
            // Remover TODOS los descriptores de DbContextOptions previos
            var dbContextOptionsDescriptors = services
                .Where(d => d.ServiceType.Name.Contains("DbContextOptions"))
                .ToList();
            
            foreach (var d in dbContextOptionsDescriptors)
            {
                services.Remove(d);
            }

            // Remover DbContext y sus interfaces
            services.RemoveAll<QuickMeetDbContext>();
            services.RemoveAll<DbContextOptions<QuickMeetDbContext>>();
            services.RemoveAll(typeof(IQuickMeetDbContext));

            // Agregar DbContext NUEVO con InMemory Database
            services.AddDbContext<QuickMeetDbContext>(options =>
            {
                options.UseInMemoryDatabase("QuickMeetTestDb");
            }, ServiceLifetime.Scoped);

            // Re-registrar la interfaz con el nuevo DbContext
            services.AddScoped<IQuickMeetDbContext>(sp => sp.GetRequiredService<QuickMeetDbContext>());

            // Remover IEmailService real y reemplazar con Mock
            services.RemoveAll<IEmailService>();
            services.AddSingleton<IEmailService>(new Mock<IEmailService>().Object);
        });

        // Configurar logging: solo warnings y errores
        builder.ConfigureLogging(logging =>
        {
            logging.ClearProviders();
            logging.SetMinimumLevel(LogLevel.Warning);
        });
    }
}
