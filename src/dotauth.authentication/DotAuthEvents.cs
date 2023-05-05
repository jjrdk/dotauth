namespace dotauth.authentication;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;

/// <summary>
/// Default implementation.
/// </summary>
public class DotAuthEvents : RemoteAuthenticationEvents
{
    /// <summary>
    /// Gets or sets the function that is invoked when the CreatingTicket method is invoked.
    /// </summary>
    public Func<DotAuthCreatingTicketContext, Task> OnCreatingTicket { get; set; } = context => Task.CompletedTask;

    /// <summary>
    /// Gets or sets the delegate that is invoked when the RedirectToAuthorizationEndpoint method is invoked.
    /// </summary>
    public Func<RedirectContext<DotAuthOptions>, Task> OnRedirectToAuthorizationEndpoint { get; set; } = context =>
    {
        context.Response.Redirect(context.RedirectUri);
        return Task.CompletedTask;
    };

    /// <summary>
    /// Invoked after the provider successfully authenticates a user.
    /// </summary>
    /// <param name="context">Contains information about the login session as well as the user <see cref="System.Security.Claims.ClaimsIdentity"/>.</param>
    /// <returns>A <see cref="Task"/> representing the completed operation.</returns>
    public virtual Task CreatingTicket(DotAuthCreatingTicketContext context) => OnCreatingTicket(context);

    /// <summary>
    /// Called when a Challenge causes a redirect to authorize endpoint in the OAuth handler.
    /// </summary>
    /// <param name="context">Contains redirect URI and <see cref="AuthenticationProperties"/> of the challenge.</param>
    public virtual Task RedirectToAuthorizationEndpoint(RedirectContext<DotAuthOptions> context) =>
        OnRedirectToAuthorizationEndpoint(context);
}

/// <summary>
/// Defines the creating ticket context.
/// </summary>
public class DotAuthCreatingTicketContext : ResultContext<DotAuthOptions>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DotAuthCreatingTicketContext"/> class.
    /// </summary>
    /// <param name="context">The HTTP environment.</param>
    /// <param name="scheme">The authentication scheme.</param>
    /// <param name="options">The options used by the authentication middleware.</param>
    public DotAuthCreatingTicketContext(HttpContext context, AuthenticationScheme scheme, DotAuthOptions options) :
        base(context, scheme, options)
    {
    }
}
