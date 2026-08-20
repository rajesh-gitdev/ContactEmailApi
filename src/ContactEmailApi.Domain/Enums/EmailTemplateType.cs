namespace ContactEmailApi.Domain.Enums;

/// <summary>Identifies a built-in HTML email template.</summary>
public enum EmailTemplateType
{
    Contact = 0,
    BusinessInquiry = 1,
    Support = 2,
    Career = 3,
    Newsletter = 4,
    Feedback = 5,
    Callback = 6,
    Otp = 7,
    PasswordReset = 8,
    Welcome = 9,
    InternalNotification = 10
}
