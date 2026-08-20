using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ContactEmailApi.Application.Abstractions.Email;
using ContactEmailApi.Application.Models.Email;
using ContactEmailApi.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ContactEmailApi.IntegrationTests;

/// <summary>Known API key injected for tests. The Admin role satisfies every policy.</summary>
public static class TestAuth
{
    public const string ApiKey = "integration-test-admin-key";
}

/// <summary>
/// Boots the real API host but swaps SQL Server for an in-memory database and the SMTP
/// sender for a no-op fake, and injects a deterministic Admin API key.
/// </summary>
public sealed class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ApiKeys:Keys:0:Key"] = TestAuth.ApiKey,
                ["ApiKeys:Keys:0:Owner"] = "IntegrationTests",
                ["ApiKeys:Keys:0:Role"] = "Admin",
                ["ApiKeys:Keys:0:Enabled"] = "true",
                // Keep reCAPTCHA off and give forms room so timestamp checks don't trip.
                ["Recaptcha:Enabled"] = "false",
                ["SpamProtection:MinFormFillSeconds"] = "0"
            });
        });

        builder.ConfigureServices(services =>
        {
            // Remove every EF descriptor tied to the SQL Server context (options, the
            // context itself, and EF Core 10's IDbContextOptionsConfiguration<T>) so the
            // in-memory provider is the only one registered.
            var toRemove = services
                .Where(d =>
                    d.ServiceType == typeof(ApplicationDbContext) ||
                    (d.ServiceType.FullName?.Contains("DbContextOptions") ?? false))
                .ToList();

            foreach (var descriptor in toRemove)
            {
                services.Remove(descriptor);
            }

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseInMemoryDatabase("integration-tests"));

            // Replace the real SMTP sender so the background processor "sends" successfully.
            services.RemoveAll(typeof(IEmailService));
            services.AddScoped<IEmailService, NoOpEmailService>();
        });
    }

    private sealed class NoOpEmailService : IEmailService
    {
        public Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }
}
