namespace DotAuth.Uma.Web;

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.BearerToken;

public class UmaBearerEvents
{
    public Func<MessageReceivedContext, Task> OnMessageReceived { get; set; } = _ => Task.CompletedTask;

    public Func<TokenValidatedContext, Task> OnTokenValidated { get; set; } = _ => Task.CompletedTask;

    public Func<AuthenticationFailedContext, Task> OnAuthenticationFailed { get; set; } = _ => Task.CompletedTask;

    public Func<UmaBearerChallengeContext, Task> OnChallenge { get; set; } = _ => Task.CompletedTask;

    public Func<ForbiddenContext, Task> OnForbidden { get; set; } = _ => Task.CompletedTask;
}
