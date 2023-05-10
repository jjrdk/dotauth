namespace dotauth.authentication;

using DotAuth.Shared;
using DotAuth.Shared.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.IdentityModel.Tokens;

/// <summary>
/// Defines the options for <see cref="DotAuthHandler{TOptions}"/>.
/// </summary>
public class DotAuthOptions : RemoteAuthenticationOptions
{
    /// <summary>
    /// Gets or sets the provider-assigned client id.
    /// </summary>
    public string ClientId { get; set; } = default!;

    /// <summary>
    /// Gets or sets the provider-assigned client secret.
    /// </summary>
    public string ClientSecret { get; set; } = default!;

    /// <summary>
    /// Gets or sets the base URI of the authentication authority.
    /// </summary>
    public Uri Authority { get; set; } = default!;

    /// <summary>
    /// Gets or sets the token validation parameters.
    /// </summary>
    public TokenValidationParameters TokenValidationParameters { get; set; } = new();

    /// <summary>
    /// Gets or sets the authentication scopes requested.
    /// </summary>
    public List<string> Scopes { get; } = new();

    /// <summary>
    /// Gets or sets the response types requested.
    /// </summary>
    public string[] ResponseTypes { get; set; } = ResponseTypeNames.All;

    /// <summary>
    /// Gets or sets the code challenge method. If <see cref="UsePkce"/> is <c>false</c> then this property is ignored.
    /// </summary>
    public string CodeChallengeMethod { get; set; } = CodeChallengeMethods.S256;

    /// <summary>
    /// Gets or sets whether to use PKCE. Defaults to <c>true</c>.
    /// </summary>
    public bool UsePkce { get; set; } = true;

    /// <summary>
    /// Gets or sets the <see cref="DotAuthEvents"/> used to handle authentication events.
    /// </summary>
    public new DotAuthEvents Events { get; set; } = new DotAuthEvents();
}
