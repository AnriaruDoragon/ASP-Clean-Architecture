using System.Reflection;
using Application.Common.Behaviors;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Application;

public static class DependencyInjection
{
    /// <summary>
    /// Registers Application layer services with the DI container.
    /// </summary>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        // Register MediatR with pipeline behaviors
        services.AddMediatR(config =>
        {
            config.RegisterServicesFromAssembly(assembly);
            config.AddOpenBehavior(typeof(LoggingBehavior<,>));
            config.AddOpenBehavior(typeof(ValidationBehavior<,>));
            config.AddOpenBehavior(typeof(CachingBehavior<,>));
        });

        // Register FluentValidation validators
        services.AddValidatorsFromAssembly(assembly);

        return services;
    }
}
