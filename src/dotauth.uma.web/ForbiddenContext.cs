namespace DotAuth.Uma.Web;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;

/// <summary>
/// A <see cref="ResultContext{TOptions}"/> when access to a resource is forbidden.
/// </summary>
public class ForbiddenContext : ResultContext<UmaBearerOptions>
{
    /// <summary>
    /// Initializes a new instance of <see cref="ForbiddenContext"/>.
    /// </summary>
    /// <inheritdoc />
    public ForbiddenContext(
        HttpContext context,
        AuthenticationScheme scheme,
        UmaBearerOptions options)
        : base(context, scheme, options) { }
}
