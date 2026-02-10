using System.Reflection;
using Application;
using Application.Common.Interfaces;
using Common.ApiVersioning.Extensions;
using Common.ApiVersioning.Middlewares;
using DotNetEnv;
using FluentValidation;
using Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Serilog;
using Web.API.Authorization;
using Web.API.Authorization.Handlers;
using Web.API.Extensions;
using Web.API.Middlewares;
using Web.API.Services;

Env.TraversePath().Load();

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Configure Serilog
builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));

// Add services from each layer
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// Add Web.API services
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

// Add security services
builder.Services.AddCorsPolicy(builder.Configuration, builder.Environment);
builder.Services.AddRateLimitingPolicies(builder.Configuration);
builder.Services.AddGracefulShutdown(builder.Configuration);

// Add authorization handlers
builder.Services.AddSingleton<IAuthorizationPolicyProvider, AuthorizationPolicyProvider>();
builder.Services.AddScoped<IAuthorizationHandler, RoleAuthorizationHandler>();
builder.Services.AddScoped<IAuthorizationHandler, EmailVerifiedAuthorizationHandler>();

// Configure authorization - require authentication by default
builder.Services.AddAuthorizationBuilder()
    .SetFallbackPolicy(new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build());

builder.Services.AddControllers();
builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
builder.Services.AddApiVersioningServices(builder.Configuration);

WebApplication app = builder.Build();

// Validate configuration on startup
app.ValidateConfiguration();

// Configure graceful shutdown
app.UseGracefulShutdown();

// Middleware pipeline
app.UseGlobalExceptionHandler();
app.UseSecurityHeaders();
app.UseCorrelationId();
app.UseRequestLogging();

app.UseCors();

if (app.Environment.IsDevelopment())
{
    // Enable Scalar UI for OpenApi
    app.UseScalarApiReference();
}

// Conditional HTTPS based on configuration
app.UseConditionalHttpsRedirection(app.Configuration);
app.UseRateLimiter();

// Handle API versions lifecycle
app.UseMiddleware<ApiVersioningDeprecationMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
