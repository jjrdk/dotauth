namespace SimpleAuth.Shared.Events.Openid;

using System;

/// <summary>
/// Defines the two factor authentication failed event.
/// </summary>
public sealed record TwoFactorAuthenticationFailed : Event
{
    /// <inheritdoc />
    public TwoFactorAuthenticationFailed(
        string id,
        string subject,
        string? authRequestCode,
        string? code,
        DateTimeOffset timestamp)
        : base(id, timestamp)
    {
        Subject = subject;
        AuthRequestCode = authRequestCode;
        Code = code;
    }

    /// <summary>
    /// Gets the subject id.
    /// </summary>
    public string Subject { get; }

    /// <summary>
    /// Gets the auth request code.
    /// </summary>
    public string? AuthRequestCode { get; }

    /// <summary>
    /// Gets the two factor code.
    /// </summary>
    public string? Code { get; }
}