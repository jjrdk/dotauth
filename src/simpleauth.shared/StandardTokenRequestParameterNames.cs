namespace DotAuth.Shared;

/// <summary>
/// Parameter names of a token request
/// </summary>
internal static class StandardTokenRequestParameterNames
{
    public const string ClientIdName = "client_id";
    public const string UserName = "username";
    public const string PasswordName = "password";
    public const string AuthorizationCodeName = "code";
    public const string RefreshToken = "refresh_token";
    public const string ScopeName = "scope";
}