namespace DotAuth.Sms;

/// <summary>
/// Defines the Twilio SMS credential content.
/// </summary>
public sealed class TwilioSmsCredentials
{
    /// <summary>
    /// Gets or sets the account sid.
    /// </summary>
    /// <value>
    /// The account sid.
    /// </value>
    public string AccountSid { get; set; } = null!;

    /// <summary>
    /// Gets or sets the authentication token.
    /// </summary>
    /// <value>
    /// The authentication token.
    /// </value>
    public string AuthToken { get; set; } = null!;

    /// <summary>
    /// Gets or sets from number.
    /// </summary>
    /// <value>
    /// From number.
    /// </value>
    public string FromNumber { get; set; } = null!;
}