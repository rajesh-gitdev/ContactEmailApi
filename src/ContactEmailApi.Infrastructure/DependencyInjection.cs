using System.Text;
using ContactEmailApi.Application.Abstractions.Common;
using ContactEmailApi.Application.Abstractions.Email;
using ContactEmailApi.Application.Abstractions.Security;
using ContactEmailApi.Application.Abstractions.Services;
using ContactEmailApi.Application.Abstractions.Spam;
using ContactEmailApi.Infrastructure.Configuration;
using ContactEmailApi.Infrastructure.Email;
using ContactEmailApi.Infrastructure.HealthChecks;
using ContactEmailApi.Infrastructure.RateLimiting;
using ContactEmailApi.Infrastructure.Security.ApiKey;
using ContactEmailApi.Infrastructure.Security.Common;
using ContactEmailApi.Infrastructure.Security.Jwt;
using ContactEmailApi.Infrastructure.Services;
using ContactEmailApi.Infrastructure.Services.Common;
using ContactEmailApi.Infrastructure.Spam;
using ContactEmailApi.Persistence;
using ContactEmailApi.Shared.Constants;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace ContactEmailApi.Infrastructure;

/// <summary>Composition root for the Infrastructure layer.</summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddInfrastructureOptions(configuration)
            .AddInfrastructureServices()
            .AddEmailServices()
            .AddSpamProtection()
            .AddBusinessServices()
            .AddApiAuthentication(configuration)
            .AddApiAuthorization()
            .AddApiRateLimiting()
            .AddInfrastructureHealthChecks();

        return services;
    }

    private static IServiceCollection AddInfrastructureOptions(this IServiceCollection services, IConfiguration configuration)
    {
        // Bind + validate on startup (fail fast on misconfiguration).
        services.AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<SmtpOptions>()
            .Bind(configuration.GetSection(SmtpOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.Configure<ApiKeyOptions>(configuration.GetSection(ApiKeyOptions.SectionName));
        services.Configure<CorsOptions>(configuration.GetSection(CorsOptions.SectionName));
        services.Configure<SpamProtectionOptions>(configuration.GetSection(SpamProtectionOptions.SectionName));
        services.Configure<RecaptchaOptions>(configuration.GetSection(RecaptchaOptions.SectionName));
        services.Configure<OtpOptions>(configuration.GetSection(OtpOptions.SectionName));

        return services;
    }

    private static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddMemoryCache();

        services.AddSingleton<IDateTimeProvider, SystemDateTimeProvider>();
        services.AddSingleton<IApiKeyValidator, ApiKeyValidator>();
        services.AddSingleton<IJwtTokenService, JwtTokenService>();
        services.AddScoped<ICurrentUser, CurrentUser>();

        return services;
    }

    private static IServiceCollection AddEmailServices(this IServiceCollection services)
    {
        // Renderer and queue are stateless/process-wide singletons.
        services.AddSingleton<IEmailTemplateRenderer, EmailTemplateRenderer>();
        services.AddSingleton<IEmailQueue>(_ => new ChannelEmailQueue(capacity: 1000));

        // The MailKit SMTP client is created per send, so a scoped service is appropriate.
        services.AddScoped<IEmailService, MailKitEmailService>();

        // Drains the queue and delivers messages off the request thread.
        services.AddHostedService<EmailQueueProcessor>();

        return services;
    }

    private static IServiceCollection AddSpamProtection(this IServiceCollection services)
    {
        services.AddSingleton<IReferenceCodeGenerator, ReferenceCodeGenerator>();
        services.AddScoped<ISpamGuard, SpamGuard>();

        // reCAPTCHA verification uses a typed HttpClient (pooled, resilient).
        services.AddHttpClient<IRecaptchaVerifier, GoogleRecaptchaVerifier>();

        return services;
    }

    private static IServiceCollection AddBusinessServices(this IServiceCollection services)
    {
        services.AddScoped<IContactService, ContactService>();
        services.AddScoped<ISupportService, SupportService>();
        services.AddScoped<ICareerService, CareerService>();
        services.AddScoped<INewsletterService, NewsletterService>();
        services.AddScoped<IFeedbackService, FeedbackService>();
        services.AddScoped<ICallbackService, CallbackService>();
        services.AddScoped<IEmailDispatchService, EmailDispatchService>();

        return services;
    }

    private static IServiceCollection AddApiAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var jwt = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();

        services
            .AddAuthentication(options =>
            {
                // Default to a policy scheme that dispatches to the right scheme per request.
                options.DefaultScheme = AuthSchemes.MultiAuth;
                options.DefaultChallengeScheme = AuthSchemes.MultiAuth;
            })
            .AddPolicyScheme(AuthSchemes.MultiAuth, "JWT or API Key", options =>
            {
                options.ForwardDefaultSelector = context =>
                    context.Request.Headers.ContainsKey(AuthSchemes.ApiKeyHeader)
                        ? AuthSchemes.ApiKey
                        : JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                options.MapInboundClaims = false;
                options.RequireHttpsMetadata = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwt.Issuer,
                    ValidAudience = jwt.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(string.IsNullOrEmpty(jwt.SigningKey)
                            ? new string('0', 32)
                            : jwt.SigningKey)),
                    ClockSkew = TimeSpan.FromSeconds(jwt.ClockSkewSeconds),
                    RoleClaimType = System.Security.Claims.ClaimTypes.Role,
                    NameClaimType = "sub"
                };
            })
            .AddScheme<ApiKeyAuthenticationOptions, ApiKeyAuthenticationHandler>(AuthSchemes.ApiKey, _ => { });

        return services;
    }

    private static IServiceCollection AddApiAuthorization(this IServiceCollection services)
    {
        services.AddAuthorizationBuilder()
            .AddPolicy(Policies.AdminOnly, p => p.RequireRole(Roles.Admin))
            .AddPolicy(Policies.WebsiteClients, p => p.RequireRole(Roles.Website, Roles.Admin))
            .AddPolicy(Policies.InternalClients, p => p.RequireRole(Roles.Internal, Roles.Admin))
            .AddPolicy(Policies.SystemClients, p => p.RequireRole(Roles.System, Roles.Admin));

        return services;
    }

    private static IServiceCollection AddInfrastructureHealthChecks(this IServiceCollection services)
    {
        services.AddHealthChecks()
            .AddCheck("self", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy("API is running."), tags: ["live"])
            .AddCheck<MemoryHealthCheck>("memory", tags: ["ready"])
            .AddCheck<SmtpHealthCheck>("smtp", tags: ["ready"])
            .AddDbContextCheck<ApplicationDbContext>("sqlserver", tags: ["ready"]);

        return services;
    }
}
