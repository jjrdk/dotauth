namespace DotAuth.Uma.Web;

using System;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;

/// <summary>
/// A <see cref="PropertiesContext{TOptions}"/> when access to a resource authenticated using JWT bearer is challenged.
/// </summary>
public class UmaBearerChallengeContext : PropertiesContext<UmaBearerOptions>
{
    /// <summary>
    /// Initializes a new instance of <see cref="UmaBearerChallengeContext"/>.
    /// </summary>
    /// <inheritdoc />
    public UmaBearerChallengeContext(
        HttpContext context,
        AuthenticationScheme scheme,
        UmaBearerOptions options,
        AuthenticationProperties properties)
        : base(context, scheme, options, properties) { }

    /// <summary>
    /// Any failures encountered during the authentication process.
    /// </summary>
    public Exception? AuthenticateFailure { get; set; }

    /// <summary>
    /// Gets or sets the "error" value returned to the caller as part
    /// of the WWW-Authenticate header. This property may be null when
    /// <see cref="UmaBearerOptions.IncludeErrorDetails"/> is set to <c>false</c>.
    /// </summary>
    public string? Error { get; set; }

    /// <summary>
    /// Gets or sets the "error_description" value returned to the caller as part
    /// of the WWW-Authenticate header. This property may be null when
    /// <see cref="UmaBearerOptions.IncludeErrorDetails"/> is set to <c>false</c>.
    /// </summary>
    public string? ErrorDescription { get; set; }

    /// <summary>
    /// Gets or sets the "error_uri" value returned to the caller as part of the
    /// WWW-Authenticate header. This property is always null unless explicitly set.
    /// </summary>
    public string? ErrorUri { get; set; }

    /// <summary>
    /// Gets or sets the UMA permission ticket obtained from the Authorization Server's Permission Endpoint.
    /// This value is included in the <c>WWW-Authenticate: UMA … ticket="…"</c> response header.
    /// It may be set or overridden by the <see cref="UmaBearerEvents.OnChallenge"/> event handler.
    /// </summary>
    public string? TicketId { get; set; }

    /// <summary>
    /// Gets or sets the base URI of the UMA Authorization Server (the <c>as_uri</c> parameter).
    /// This value is included in the <c>WWW-Authenticate: UMA … as_uri="…"</c> response header.
    /// It may be set or overridden by the <see cref="UmaBearerEvents.OnChallenge"/> event handler.
    /// </summary>
    public string? AsUri { get; set; }

    /// <summary>
    /// If true, will skip any default logic for this challenge.
    /// </summary>
    public bool Handled { get; private set; }

    /// <summary>
    /// Skips any default logic for this challenge.
    /// </summary>
    public void HandleResponse() => Handled = true;
}
