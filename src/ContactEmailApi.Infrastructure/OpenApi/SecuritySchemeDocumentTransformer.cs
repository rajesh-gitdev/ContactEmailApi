using ContactEmailApi.Shared.Constants;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace ContactEmailApi.Infrastructure.OpenApi;

/// <summary>
/// Registers the JWT Bearer and API-key security schemes on the generated OpenAPI 3.1
/// document and applies them as a document-wide requirement, so Scalar / Swagger UI
/// render working "Authorize" controls.
/// </summary>
/// <remarks>
/// Targets the Microsoft.OpenApi v2 object model shipped with .NET 10, where security
/// schemes are referenced via <see cref="OpenApiSecuritySchemeReference"/> rather than the
/// legacy <c>OpenApiReference</c> property.
/// </remarks>
public sealed class SecuritySchemeDocumentTransformer : IOpenApiDocumentTransformer
{
    public Task TransformAsync(
        OpenApiDocument document,
        OpenApiDocumentTransformerContext context,
        CancellationToken cancellationToken)
    {
        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>(StringComparer.Ordinal);

        document.Components.SecuritySchemes["Bearer"] = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "JWT access token. Enter the raw token; the 'Bearer' prefix is added automatically."
        };

        document.Components.SecuritySchemes["ApiKey"] = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.ApiKey,
            Name = AuthSchemes.ApiKeyHeader,
            In = ParameterLocation.Header,
            Description = "API key issued to trusted clients, sent in the X-Api-Key header."
        };

        // Advertise both schemes as acceptable at the document level.
        document.Security ??= [];
        document.Security.Add(new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference("Bearer", document)] = new List<string>(),
            [new OpenApiSecuritySchemeReference("ApiKey", document)] = new List<string>()
        });

        return Task.CompletedTask;
    }
}
