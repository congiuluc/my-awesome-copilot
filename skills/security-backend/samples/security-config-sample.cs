// Sample: Security Configuration for .NET Minimal API
// Shows CORS, rate limiting, security headers, HTTPS, and secrets management.

using System.Threading.RateLimiting;
using Azure.Identity;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace MyApp.Api.Configuration;

/// <summary>
/// Security-related service and middleware configuration.
/// </summary>
public static class SecurityConfiguration
{
    /// <summary>
    /// Registers security-related services.
    /// </summary>
    public static IServiceCollection AddSecurityServices(
        this IServiceCollection services,
        IConfiguration config)
    {
        // CORS — allow only known origins
        services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                policy
                    .WithOrigins(config.GetSection("Cors:AllowedOrigins").Get<string[]>()!)
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials();
            });
        });

        // Rate limiting — 100 requests per minute per IP
        services.AddRateLimiter(options =>
        {
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 100,
                        Window = TimeSpan.FromMinutes(1),
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 10
                    }));

            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
        });

        return services;
    }

    /// <summary>
    /// Configures security middleware in the pipeline.
    /// </summary>
    public static WebApplication UseSecurityMiddleware(this WebApplication app)
    {
        // Security headers
        app.Use(async (context, next) =>
        {
            context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
            context.Response.Headers.Append("X-Frame-Options", "DENY");
            context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
            context.Response.Headers.Append("Permissions-Policy",
                "camera=(), microphone=(), geolocation=()");
            context.Response.Headers.Append("Content-Security-Policy",
                "default-src 'self'; img-src 'self' data: https:; style-src 'self' 'unsafe-inline'");
            await next();
        });

        // HTTPS redirect in non-development
        if (!app.Environment.IsDevelopment())
        {
            app.UseHsts();
        }
        app.UseHttpsRedirection();

        // Apply CORS and rate limiting
        app.UseCors();
        app.UseRateLimiter();

        return app;
    }

    /// <summary>
    /// Adds Azure Key Vault configuration for production secrets.
    /// </summary>
    public static WebApplicationBuilder AddKeyVaultConfiguration(
        this WebApplicationBuilder builder)
    {
        if (builder.Environment.IsProduction())
        {
            var keyVaultUri = builder.Configuration["KeyVault:Uri"];
            if (!string.IsNullOrEmpty(keyVaultUri))
            {
                builder.Configuration.AddAzureKeyVault(
                    new Uri(keyVaultUri),
                    new DefaultAzureCredential());
            }
        }

        return builder;
    }
}

// --- Usage in Program.cs ---
// var builder = WebApplication.CreateBuilder(args);
// builder.AddKeyVaultConfiguration();
// builder.Services.AddSecurityServices(builder.Configuration);
//
// var app = builder.Build();
// app.UseSecurityMiddleware();
