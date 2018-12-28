namespace SimpleAuth.Server.Tests.MiddleWares
{
    using System.Text.Encodings.Web;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    public class FakeUserInfoIntrospectionHandler : AuthenticationHandler<FakeUserInfoIntrospectionOptions>
    {
        public FakeUserInfoIntrospectionHandler(IOptionsMonitor<FakeUserInfoIntrospectionOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock) : base(options, logger, encoder, clock)
        {
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }
    }
}
