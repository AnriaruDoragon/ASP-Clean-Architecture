using System.Text;
using Application.Common.Interfaces;
using Infrastructure.Auth;
using Infrastructure.BackgroundJobs;
using Infrastructure.Caching;
using Infrastructure.Data;
using Infrastructure.Data.Interceptors;
using Infrastructure.Events;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;

namespace Infrastructure;

public static class DependencyInjection
{
    /// <summary>
    /// Registers Infrastructure layer services with the DI container.
    /// </summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Register interceptors
        services.AddScoped<ISaveChangesInterceptor, AuditableEntityInterceptor>();

        // Register DbContext
        string? connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<ApplicationDbContext>(
            (sp, options) =>
            {
                options.AddInterceptors(sp.GetServices<ISaveChangesInterceptor>());

                if (!string.IsNullOrEmpty(connectionString))
                {
                    options.UseNpgsql(
                        connectionString,
                        npgsqlOptions =>
                        {
                            npgsqlOptions.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
                            npgsqlOptions.EnableRetryOnFailure(
                                maxRetryCount: 3,
                                maxRetryDelay: TimeSpan.FromSeconds(30),
                                errorCodesToAdd: null
                            );
                        }
                    );
                }
            }
        );

        // Register interfaces
        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<ApplicationDbContext>());

        // Register services
        services.AddScoped<IDateTimeProvider, DateTimeProvider>();
        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();

        // Register Auth services
        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));
        services.AddSingleton<IJwtService, JwtService>();
        services.AddSingleton<IPasswordHasher, PasswordHasher>();

        // Register Health Checks
        if (!string.IsNullOrEmpty(connectionString))
        {
            services.AddHealthChecks().AddNpgSql(connectionString, name: "database", tags: ["ready"]);
        }
        else
        {
            services.AddHealthChecks();
        }

        // Register Caching
        services.Configure<CacheSettings>(configuration.GetSection(CacheSettings.SectionName));
        CacheSettings? cacheSettings = configuration.GetSection(CacheSettings.SectionName).Get<CacheSettings>();

        if (cacheSettings?.Enabled == true)
        {
            switch (cacheSettings.Provider.ToLowerInvariant())
            {
                case "redis":
                    services.AddSingleton<IConnectionMultiplexer>(_ =>
                        ConnectionMultiplexer.Connect(cacheSettings.Redis.ConnectionString)
                    );
                    services.AddSingleton<ICacheService, RedisCacheService>();
                    break;

                case "memory":
                    services.AddMemoryCache();
                    services.AddSingleton<ICacheService, MemoryCacheService>();
                    break;

                default:
                    services.AddSingleton<ICacheService, NullCacheService>();
                    break;
            }
        }
        else
        {
            services.AddSingleton<ICacheService, NullCacheService>();
        }

        // Register Email service (placeholder - implement for production)
        services.AddSingleton<IEmailService, NullEmailService>();

        // Register Background Jobs
        services.Configure<BackgroundJobSettings>(configuration.GetSection(BackgroundJobSettings.SectionName));
        BackgroundJobSettings jobSettings =
            configuration.GetSection(BackgroundJobSettings.SectionName).Get<BackgroundJobSettings>()
            ?? new BackgroundJobSettings();

        switch (jobSettings.Provider)
        {
            case BackgroundJobProvider.Memory:
                var inMemoryService = new InMemoryJobService(
                    Microsoft.Extensions.Logging.Abstractions.NullLogger<InMemoryJobService>.Instance
                );
                services.AddSingleton(inMemoryService);
                services.AddSingleton<IBackgroundJobService>(inMemoryService);
                services.AddHostedService<BackgroundJobWorker>();
                break;

            case BackgroundJobProvider.None:
                services.AddSingleton<IBackgroundJobService, NullJobService>();
                break;

            case BackgroundJobProvider.Instant:
            default:
                services.AddSingleton<IBackgroundJobService, InstantJobService>();
                break;
        }

        // Configure JWT authentication
        JwtSettings? jwtSettings = configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>();
        if (jwtSettings is not null)
        {
            services
                .AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = jwtSettings.Issuer,
                        ValidAudience = jwtSettings.Audience,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
                        ClockSkew = TimeSpan.Zero, // No tolerance for token expiration
                    };
                });
        }

        return services;
    }
}
