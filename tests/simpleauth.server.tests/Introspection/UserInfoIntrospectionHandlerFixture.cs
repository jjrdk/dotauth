namespace SimpleAuth.Server.Tests.Introspection
{
    using Client;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Microsoft.Extensions.WebEncoders.Testing;
    using Moq;
    using System;
    using System.Threading.Tasks;
    using UserInfoIntrospection;
    using Xunit;

    public class UserInfoIntrospectionHandlerFixture
    {
        private const string BaseUrl = "http://localhost:5000";
        private const string WellKnownOpenidConfiguration = "/.well-known/openid-configuration";
        private readonly TestOauthServerFixture _server;

        public UserInfoIntrospectionHandlerFixture()
        {
            _server = new TestOauthServerFixture();
        }

        [Fact]
        public async Task When_Introspect_Identity_Token_Then_Claims_Are_Returned()
        {
            var client = await TokenClient.Create(
                    TokenCredentials.FromClientCredentials("client", "client"),
                    _server.Client,
                    new Uri(BaseUrl + WellKnownOpenidConfiguration))
                .ConfigureAwait(false);
            var result = await client.GetToken(TokenRequest.FromPassword("superuser", "password", new[] { "role" }))
                .ConfigureAwait(false);

            var authResult = await new UserInfoIntrospectionHandler(
                    null,
                    new Mock<IOptionsMonitor<AuthenticationSchemeOptions>>().Object,
                    new Mock<ILoggerFactory>().Object,
                    new UrlTestEncoder(),
                    new Mock<ISystemClock>().Object)
                .AuthenticateAsync()
                .ConfigureAwait(false);

            Assert.True(authResult.Succeeded);
        }
    }
}
