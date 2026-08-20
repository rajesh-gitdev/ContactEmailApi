using ContactEmailApi.Infrastructure.Configuration;

namespace ContactEmailApi.Api.Extensions;

/// <summary>Configures a restrictive, config-driven CORS policy (no wildcard origins).</summary>
public static class CorsExtensions
{
    public const string PolicyName = "ConfiguredOrigins";

    public static IServiceCollection AddApiCors(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
    {
        var corsOptions = configuration.GetSection(CorsOptions.SectionName).Get<CorsOptions>() ?? new CorsOptions();
        var origins = corsOptions.AllowedOrigins ?? [];

        if (environment.IsProduction() && (origins.Length == 0 || origins.Contains("*")))
        {
            throw new InvalidOperationException(
                "CORS misconfiguration: Production requires an explicit, non-wildcard 'Cors:AllowedOrigins' list.");
        }

        services.AddCors(options =>
        {
            options.AddPolicy(PolicyName, policy =>
            {
                if (origins.Length > 0)
                {
                    policy.WithOrigins(origins)
                          .AllowAnyHeader()
                          .WithMethods("GET", "POST", "OPTIONS")
                          .AllowCredentials();
                }
            });
        });

        return services;
    }
}
