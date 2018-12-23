using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SimpleIdentityServer.Uma.Host.Tests.Fakes;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Uma.Host.Tests.MiddleWares
{
    public class TestAuthenticationHandler : AuthenticationHandler<TestAuthenticationOptions>
    {
        public TestAuthenticationHandler(IOptionsMonitor<TestAuthenticationOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock) : base(options, logger, encoder, clock)
        {
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (UserStore.Instance().IsInactive)
            {
                return Task.FromResult(AuthenticateResult.NoResult());
            }

            var claims = new List<Claim>();
            if (!string.IsNullOrWhiteSpace(UserStore.Instance().ClientId))
            {
                claims.Add(new Claim("client_id", UserStore.Instance().ClientId));
            }

            claims.Add(new Claim("scope", "uma_protection"));
            var claimsIdentity = new ClaimsIdentity(claims, FakeUmaStartup.DefaultSchema);
            var authenticationTicket = new AuthenticationTicket(
                                             new ClaimsPrincipal(claimsIdentity),
                                             new AuthenticationProperties(),
                                             FakeUmaStartup.DefaultSchema);
            return Task.FromResult(AuthenticateResult.Success(authenticationTicket));
        }
    }
}
