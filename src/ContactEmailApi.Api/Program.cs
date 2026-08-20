using ContactEmailApi.Api.Extensions;
using ContactEmailApi.Api.Middleware;
using ContactEmailApi.Application;
using ContactEmailApi.Infrastructure;
using ContactEmailApi.Persistence;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.ResponseCompression;
using Serilog;

// Bootstrap logger: captures failures that occur before the host is fully built.
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting Enterprise Contact & Email API host.");

    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseApiSerilog();

    // Kestrel hardening: do not advertise the server implementation.
    builder.WebHost.ConfigureKestrel(options => options.AddServerHeader = false);

    // --- Service registration (composition root) ---------------------------------
    builder.Services.AddApplication();
    builder.Services.AddPersistence(builder.Configuration);
    builder.Services.AddInfrastructure(builder.Configuration);

    builder.Services.AddApiControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddApiDocumentation();
    builder.Services.AddApiCors(builder.Configuration, builder.Environment);
    builder.Services.AddProblemDetails();

    // Trust forwarded headers so client IP (used for rate limiting) is accurate behind proxies.
    builder.Services.Configure<ForwardedHeadersOptions>(options =>
    {
        options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
        options.KnownNetworks.Clear();
        options.KnownProxies.Clear();
    });

    builder.Services.AddResponseCompression(options =>
    {
        options.EnableForHttps = true; // See README note on BREACH considerations.
        options.Providers.Add<BrotliCompressionProvider>();
        options.Providers.Add<GzipCompressionProvider>();
    });

    var app = builder.Build();

    // --- HTTP request pipeline (order matters) -----------------------------------
    app.UseForwardedHeaders();

    // Correlation id first so every later log line and error carries it.
    app.UseMiddleware<CorrelationIdMiddleware>();
    app.UseMiddleware<GlobalExceptionHandlingMiddleware>();
    app.UseMiddleware<SecurityHeadersMiddleware>();

    app.UseSerilogRequestLogging();

    if (!app.Environment.IsDevelopment())
    {
        app.UseHsts();
    }

    app.UseHttpsRedirection();
    app.UseResponseCompression();

    // API documentation UIs (Scalar + Swagger) over the native OpenAPI document.
    app.MapApiDocumentation();

    app.UseCors(CorsExtensions.PolicyName);

    //app.UseRateLimiter();
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();
    app.MapApiHealthChecks();

    app.Run();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "Enterprise Contact & Email API host terminated unexpectedly.");
}
finally
{
    Log.CloseAndFlush();
}

// Exposes the implicit Program class to the integration-test project (WebApplicationFactory).
public partial class Program;
