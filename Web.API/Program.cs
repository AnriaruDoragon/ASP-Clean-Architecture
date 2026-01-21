using System.Reflection;
using Application;
using Application.Common.Interfaces;
using Common.ApiVersioning.Extensions;
using Common.ApiVersioning.Middlewares;
using FluentValidation;
using Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Web.API.Authorization;
using Web.API.Authorization.Handlers;
using Web.API.Middlewares;
using Web.API.Services;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services from each layer
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// Add Web.API services
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

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

// Global exception handling (first in pipeline)
app.UseGlobalExceptionHandler();

if (app.Environment.IsDevelopment())
{
    // API Docs for development
    app.UseScalarApiReference();
}

app.UseHttpsRedirection();

// Handle API version lifecycle
app.UseMiddleware<ApiVersioningDeprecationMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
