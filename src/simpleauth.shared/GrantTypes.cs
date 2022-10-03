namespace DotAuth.Shared;

/// <summary>
/// Defines the grant type names.
/// </summary>
public static class GrantTypes
{
    /// <summary>
    /// Client Credentials
    /// </summary>
    public const string ClientCredentials = "client_credentials";

    /// <summary>
    /// Password
    /// </summary>
    public const string Password = "password";

    /// <summary>
    /// Refresh Token
    /// </summary>
    public const string RefreshToken = "refresh_token";

    /// <summary>
    /// Authorization code
    /// </summary>
    public const string AuthorizationCode = "authorization_code";

    /// <summary>
    /// Validate bearer
    /// </summary>
    public const string ValidateBearer = "validate_bearer";

    /// <summary>
    /// UMA Ticket
    /// </summary>
    public const string UmaTicket = "urn:ietf:params:oauth:grant-type:uma-ticket"; //"uma_ticket";

    /// <summary>
    /// Device code
    /// </summary>
    public const string Device = "urn:ietf:params:oauth:grant-type:device_code";

    /// <summary>
    /// Implicit
    /// </summary>
    public const string Implicit = "implicit";

    /// <summary>
    /// All claims
    /// </summary>
    public static string[] All =>
        new[] {ClientCredentials, Password, AuthorizationCode, Implicit, RefreshToken, UmaTicket, ValidateBearer};
}