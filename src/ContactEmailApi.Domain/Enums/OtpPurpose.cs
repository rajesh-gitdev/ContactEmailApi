namespace ContactEmailApi.Domain.Enums;

/// <summary>Why a one-time password was issued.</summary>
public enum OtpPurpose
{
    Login = 0,
    PasswordReset = 1,
    EmailVerification = 2,
    Transaction = 3
}
