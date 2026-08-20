using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace ContactEmailApi.Application;

/// <summary>Registers Application-layer services (validators, handlers).</summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Auto-registers every FluentValidation validator declared in this assembly.
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly(), includeInternalTypes: true);
        return services;
    }
}
