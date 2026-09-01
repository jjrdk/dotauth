namespace DotAuth.Uma.Web;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;

/// <summary>
/// A context for <see cref="UmaBearerEvents.OnMessageReceived"/>.
/// </summary>
public class MessageReceivedContext : ResultContext<UmaBearerOptions>
{
    /// <summary>
    /// Initializes a new instance of <see cref="MessageReceivedContext"/>.
    /// </summary>
    /// <inheritdoc />
    public MessageReceivedContext(
        HttpContext context,
        AuthenticationScheme scheme,
        UmaBearerOptions options)
        : base(context, scheme, options) { }

    /// <summary>
    /// Bearer Token. This will give the application an opportunity to retrieve a token from an alternative location.
    /// </summary>
    public string? Token { get; set; }
}
