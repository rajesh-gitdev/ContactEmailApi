using ContactEmailApi.Application.Abstractions.Common;

namespace ContactEmailApi.Infrastructure.Services.Common;

/// <summary>Builds the token dictionary shared by every email template.</summary>
internal static class EmailTokenBuilder
{
    public static Dictionary<string, string> Create(string appName, IDateTimeProvider clock)
        => new(StringComparer.OrdinalIgnoreCase)
        {
            ["AppName"] = appName,
            ["TimestampUtc"] = clock.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")
        };
}
