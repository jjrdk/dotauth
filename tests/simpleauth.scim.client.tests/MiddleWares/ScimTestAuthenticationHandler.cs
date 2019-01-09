namespace SimpleAuth.Scim.Client.Tests.MiddleWares
{
    using System.Collections.Generic;
    using System.Security.Claims;
    using System.Text.Encodings.Web;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    public class ScimTestAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public ScimTestAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock) : base(options, logger, encoder, clock)
        {
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (ScimUserStore.Instance().IsInactive)
            {
                return Task.FromResult(AuthenticateResult.NoResult());
            }

            var claims = new List<Claim>();
            claims.Add(new Claim("sub", ScimUserStore.Instance().Subject));
            var scimId = ScimUserStore.Instance().ScimId;
            if (!string.IsNullOrWhiteSpace(scimId))
            {
                claims.Add(new Claim("scim_id", scimId));
            }

            var claimsIdentity = new ClaimsIdentity(claims, FakeScimStartup.DefaultSchema);
            var authenticationTicket = new AuthenticationTicket(
                                             new ClaimsPrincipal(claimsIdentity),
                                             new AuthenticationProperties(),
                                             FakeScimStartup.DefaultSchema);
            return Task.FromResult(AuthenticateResult.Success(authenticationTicket));
        }
    }
}
