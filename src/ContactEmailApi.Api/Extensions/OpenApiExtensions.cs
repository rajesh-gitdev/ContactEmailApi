using ContactEmailApi.Infrastructure.OpenApi;
using Scalar.AspNetCore;

namespace ContactEmailApi.Api.Extensions;

/// <summary>
/// Wires the first-party .NET 10 OpenAPI generator (Microsoft.AspNetCore.OpenApi,
/// OpenAPI 3.1) plus two UIs over the same document: Scalar (modern) and the classic
/// Swagger UI, both reading /openapi/v1.json.
/// </summary>
public static class OpenApiExtensions
{
    private const string DocumentName = "v1";

    public static IServiceCollection AddApiDocumentation(this IServiceCollection services)
    {
        services.AddOpenApi(DocumentName, options =>
        {
            options.AddDocumentTransformer<SecuritySchemeDocumentTransformer>();
            options.AddDocumentTransformer((document, _, _) =>
            {
                document.Info.Title = "Enterprise Contact & Email API";
                document.Info.Version = "v1";
                document.Info.Description = "Production-ready contact and email delivery API (.NET 10, Clean Architecture).";
                return Task.CompletedTask;
            });
        });

        return services;
    }

    public static WebApplication MapApiDocumentation(this WebApplication app)
    {
        // Serves the OpenAPI 3.1 JSON at /openapi/v1.json
        app.MapOpenApi();

        // Modern UI at /scalar/v1. The parameterless overload defaults to reading
        // /openapi/{documentName}.json, which matches the document mapped above.
        // (Kept minimal to avoid Scalar options-API differences across package versions.)
        app.MapScalarApiReference();

        // Classic Swagger UI at /swagger, pointed at the native OpenAPI document.
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/openapi/v1.json", "Enterprise Contact & Email API v1");
            options.DocumentTitle = "Enterprise Contact & Email API";
            options.RoutePrefix = "swagger";
        });

        return app;
    }
}
