using ContactEmailApi.Api.Filters;
using ContactEmailApi.Shared.Constants;
using ContactEmailApi.Shared.Models;
using Microsoft.AspNetCore.Mvc;

namespace ContactEmailApi.Api.Extensions;

/// <summary>Registers controllers with the FluentValidation filter and a consistent
/// envelope for automatic model-binding (400) errors.</summary>
public static class MvcExtensions
{
    public static IServiceCollection AddApiControllers(this IServiceCollection services)
    {
        services.AddControllers(options => options.Filters.Add<ValidationFilter>());

        services.Configure<ApiBehaviorOptions>(options =>
        {
            options.InvalidModelStateResponseFactory = context =>
            {
                var errors = context.ModelState
                    .Where(kvp => kvp.Value?.Errors.Count > 0)
                    .SelectMany(kvp => kvp.Value!.Errors.Select(e =>
                        string.IsNullOrWhiteSpace(e.ErrorMessage) ? "Invalid request." : e.ErrorMessage))
                    .ToArray();

                var traceId = context.HttpContext.Items[CustomHeaderNames.CorrelationId]?.ToString();
                var payload = ApiResponse.Fail("The request could not be processed.",
                    StatusCodes.Status400BadRequest, errors, traceId: traceId);

                return new BadRequestObjectResult(payload);
            };
        });

        return services;
    }
}
