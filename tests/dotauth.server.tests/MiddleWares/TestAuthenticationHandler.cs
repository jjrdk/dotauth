﻿namespace DotAuth.Server.Tests.MiddleWares;

using System.Collections.Generic;
using System.Globalization;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using DotAuth.Shared;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

public sealed class TestAuthenticationHandler : AuthenticationHandler<TestAuthenticationOptions>
{
    public TestAuthenticationHandler(
        IOptionsMonitor<TestAuthenticationOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISystemClock clock) : base(options, logger, encoder, clock)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (UserStore.Instance().IsInactive)
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var claims = new List<Claim> { new("sub", UserStore.Instance().Subject) };
        if (UserStore.Instance().AuthenticationOffset != null)
        {
            claims.Add(new Claim(ClaimTypes.AuthenticationInstant,
                UserStore.Instance().AuthenticationOffset.Value.ConvertToUnixTimestamp()
                    .ToString(CultureInfo.InvariantCulture)));
        }

        var claimsIdentity = new ClaimsIdentity(claims, FakeStartup.DefaultSchema);
        var authenticationTicket = new AuthenticationTicket(
            new ClaimsPrincipal(claimsIdentity),
            new AuthenticationProperties(),
            FakeStartup.DefaultSchema);
        return Task.FromResult(AuthenticateResult.Success(authenticationTicket));
    }
}
