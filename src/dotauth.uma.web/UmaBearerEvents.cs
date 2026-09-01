namespace DotAuth.Uma.Web;

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.BearerToken;

/// <summary>
/// Defines events which the <see cref="UmaBearerHandler"/> invokes to enable developer control over the authentication process.
/// </summary>
public class UmaBearerEvents
{
    /// <summary>
    /// Gets or sets the function that is invoked when a request has a bearer token. The default behavior is to do nothing.
    /// </summary>
    public Func<MessageReceivedContext, Task> OnMessageReceived { get; set; } = _ => Task.CompletedTask;

    /// <summary>
    /// Gets or sets the function that is invoked when a token has been validated. The default behavior is to do nothing.
    /// </summary>
    public Func<TokenValidatedContext, Task> OnTokenValidated { get; set; } = _ => Task.CompletedTask;

    /// <summary>
    /// Gets or sets the function that is invoked when authentication fails. The default behavior is to do nothing.
    /// </summary>
    public Func<AuthenticationFailedContext, Task> OnAuthenticationFailed { get; set; } = _ => Task.CompletedTask;

    /// <summary>
    /// Gets or sets the function that is invoked when a challenge is triggered. The default behavior is to do nothing.
    /// </summary>
    public Func<UmaBearerChallengeContext, Task> OnChallenge { get; set; } = _ => Task.CompletedTask;

    /// <summary>
    /// Gets or sets the function that is invoked when a forbidden response is returned. The default behavior is to do nothing.
    /// </summary>
    public Func<ForbiddenContext, Task> OnForbidden { get; set; } = _ => Task.CompletedTask;
}
