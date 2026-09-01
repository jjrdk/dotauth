namespace DotAuth.Uma.Web;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;

/// <summary>
/// A context for <see cref="UmaBearerEvents.OnTokenValidated"/>.
/// </summary>
public class TokenValidatedContext : ResultContext<UmaBearerOptions>
{
    /// <summary>
    /// Initializes a new instance of <see cref="TokenValidatedContext"/>.
    /// </summary>
    /// <inheritdoc />
    public TokenValidatedContext(
        HttpContext context,
        AuthenticationScheme scheme,
        UmaBearerOptions options)
        : base(context, scheme, options) { }

    /// <summary>
    /// Gets or sets the validated security token.
    /// </summary>
    public SecurityToken SecurityToken { get; set; } = default!;
}
