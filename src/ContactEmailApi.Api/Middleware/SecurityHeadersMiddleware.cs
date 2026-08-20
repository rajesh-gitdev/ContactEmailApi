namespace ContactEmailApi.Api.Middleware;

/// <summary>Adds OWASP-recommended security response headers to every response.</summary>
public sealed class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;

    public SecurityHeadersMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        var headers = context.Response.Headers;
        var path = context.Request.Path.Value ?? string.Empty;

        // API documentation UIs (Scalar + Swagger) serve active content — inline
        // scripts, styles, and their own JS bundles — so they need a relaxed CSP.
        // Every other route is a pure JSON API and keeps the locked-down policy.
        var isDocsRoute = path.StartsWith("/scalar", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("/swagger", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("/openapi", StringComparison.OrdinalIgnoreCase);

        headers["Content-Security-Policy"] = isDocsRoute
            ? "default-src 'self'; script-src 'self' 'unsafe-inline' https://cdn.jsdelivr.net; " +
              "style-src 'self' 'unsafe-inline'; img-src 'self' data:; connect-src 'self'; " +
              "frame-ancestors 'none'; base-uri 'none'"
            : "default-src 'none'; frame-ancestors 'none'; base-uri 'none'; form-action 'none'";

        headers["X-Content-Type-Options"] = "nosniff";
        headers["X-Frame-Options"] = "DENY";
        headers["Referrer-Policy"] = "no-referrer";
        headers["Permissions-Policy"] = "geolocation=(), microphone=(), camera=(), interest-cohort=()";
        headers["Cross-Origin-Resource-Policy"] = "same-origin";

        // Remove headers that leak implementation details.
        headers.Remove("X-Powered-By");

        await _next(context);
    }
}
