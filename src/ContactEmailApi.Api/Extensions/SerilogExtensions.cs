using Serilog;

namespace ContactEmailApi.Api.Extensions;

/// <summary>Configures Serilog as the host logger, reading sinks/enrichers from configuration.</summary>
public static class SerilogExtensions
{
    public static IHostBuilder UseApiSerilog(this IHostBuilder host)
    {
        return host.UseSerilog((context, services, configuration) => configuration
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext()
            .Enrich.WithMachineName()
            .Enrich.WithThreadId());
    }
}
