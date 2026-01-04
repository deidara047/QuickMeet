using System.Linq;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
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
    private readonly string _dbName = $"QuickMeetTestDb_{Guid.NewGuid()}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Test");

        builder.ConfigureTestServices(services =>
        {
            // 1. Registrar DbContext con InMemory (SQL Server NO está registrado en ambiente Test)
            services.AddDbContext<QuickMeetDbContext>(options =>
            {
                options.UseInMemoryDatabase(_dbName);
            });
            
            services.AddScoped<IQuickMeetDbContext>(sp => 
                sp.GetRequiredService<QuickMeetDbContext>());

            // 2. Mock de Email Service
            var emailMock = new Mock<IEmailService>();
            emailMock
                .Setup(x => x.SendEmailVerificationAsync(
                    It.IsAny<string>(), 
                    It.IsAny<string>(), 
                    It.IsAny<string>()))
                .Returns(Task.CompletedTask);
            emailMock
                .Setup(x => x.SendWelcomeEmailAsync(
                    It.IsAny<string>(), 
                    It.IsAny<string>()))
                .Returns(Task.CompletedTask);
            
            services.RemoveAll<IEmailService>();
            services.AddSingleton(emailMock.Object);

            // 3. Reemplazar autenticación JWT por TestAuthHandler
            services.PostConfigure<AuthenticationOptions>(options =>
            {
                options.DefaultAuthenticateScheme = TestAuthHandler.SchemeName;
                options.DefaultChallengeScheme = TestAuthHandler.SchemeName;
            });

            services.AddAuthentication()
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                    TestAuthHandler.SchemeName, 
                    options => { });
        });

        builder.ConfigureLogging(logging =>
        {
            logging.ClearProviders();
            logging.SetMinimumLevel(LogLevel.Warning);
        });
    }
}

