using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Infrastructure.Data;

/// <summary>
/// Design-time factory for EF Core migrations.
/// Used when running 'dotnet ef migrations add' or 'dotnet ef database update'.
/// </summary>
public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

        // Default connection string for local development
        // Can be overridden via environment variable: CONNECTIONSTRINGS__DEFAULTCONNECTION
        string connectionString = Environment.GetEnvironmentVariable("CONNECTIONSTRINGS__DEFAULTCONNECTION")
                                  ?? "Host=localhost;Database=aspcleanarchitecture;Username=postgres;Password=postgres";

        optionsBuilder.UseNpgsql(connectionString, npgsqlOptions =>
        {
            npgsqlOptions.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
        });

        return new ApplicationDbContext(optionsBuilder.Options);
    }
}
