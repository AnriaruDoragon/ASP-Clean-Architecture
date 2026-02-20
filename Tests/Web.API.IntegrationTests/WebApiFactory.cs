using Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Testcontainers.PostgreSql;
using Xunit;

namespace Web.API.IntegrationTests;

/// <summary>
/// Custom WebApplicationFactory for integration testing with Testcontainers PostgreSQL.
/// </summary>
public class WebApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder("postgres:18-alpine")
        .WithDatabase("testdb")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration(
            (_, config) =>
            {
                // Add test-specific configuration
                config.AddInMemoryCollection(
                    new Dictionary<string, string?>
                    {
                        ["Jwt:SecretKey"] = "test-secret-key-that-is-at-least-32-characters-long",
                        ["Jwt:Issuer"] = "TestIssuer",
                        ["Jwt:Audience"] = "TestAudience",
                        ["Security:EnforceHttps"] = "false",
                    }
                );
            }
        );

        builder.ConfigureTestServices(services =>
        {
            // Remove existing DbContext registration
            ServiceDescriptor? descriptor = services.SingleOrDefault(d =>
                d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>)
            );

            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            // Add DbContext with Testcontainers connection string
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseNpgsql(_dbContainer.GetConnectionString());
            });

            // Replace the health check registered by AddInfrastructure (which captured the
            // .env connection string) with one pointing at the Testcontainers database.
            services.Configure<HealthCheckServiceOptions>(options =>
            {
                HealthCheckRegistration? existing = options.Registrations.FirstOrDefault(r => r.Name == "database");
                if (existing != null)
                    options.Registrations.Remove(existing);
            });

            services.AddHealthChecks().AddNpgSql(_dbContainer.GetConnectionString(), name: "database", tags: ["ready"]);
        });

        builder.UseEnvironment("Testing");
    }

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();

        // Create the database schema
        using IServiceScope scope = Services.CreateScope();
        ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await dbContext.Database.EnsureCreatedAsync();
    }

    public new async Task DisposeAsync() => await _dbContainer.DisposeAsync();
}
