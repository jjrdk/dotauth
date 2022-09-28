namespace SimpleAuth.Server.Tests.MiddleWares;

using System.Collections.Generic;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SimpleAuth.Shared;

internal sealed class UmaAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public UmaAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISystemClock clock)
        : base(options, logger, encoder, clock)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (UmaUserStore.Instance().IsInactive)
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var claims = new List<Claim>();
        if (!string.IsNullOrWhiteSpace(UmaUserStore.Instance().ClientId))
        {
            claims.Add(new Claim(StandardClaimNames.Azp, UmaUserStore.Instance().ClientId));
        }

        claims.Add(new Claim("scope", "uma_protection"));
        claims.Add(new Claim("sub", "tester"));
        var claimsIdentity = new ClaimsIdentity(claims, FakeUmaStartup.DefaultSchema);
        var authenticationTicket = new AuthenticationTicket(
            new ClaimsPrincipal(claimsIdentity),
            new AuthenticationProperties(),
            FakeUmaStartup.DefaultSchema);
        return Task.FromResult(AuthenticateResult.Success(authenticationTicket));
    }
}