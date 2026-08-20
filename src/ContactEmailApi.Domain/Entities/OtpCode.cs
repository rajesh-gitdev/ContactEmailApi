using ContactEmailApi.Domain.Common;
using ContactEmailApi.Domain.Enums;

namespace ContactEmailApi.Domain.Entities;

/// <summary>
/// A one-time password issued via email/send-otp. Only a hash of the code is stored;
/// the plaintext is emailed to the recipient and never persisted.
/// </summary>
public sealed class OtpCode : BaseEntity
{
    public string Email { get; set; } = string.Empty;

    /// <summary>Salted hash of the OTP value (never the plaintext).</summary>
    public string CodeHash { get; set; } = string.Empty;

    public OtpPurpose Purpose { get; set; } = OtpPurpose.Login;

    public DateTimeOffset ExpiresAtUtc { get; set; }
    public DateTimeOffset? ConsumedAtUtc { get; set; }

    public int Attempts { get; set; }

    public bool IsConsumed => ConsumedAtUtc is not null;
    public bool IsExpired(DateTimeOffset now) => now >= ExpiresAtUtc;
}
