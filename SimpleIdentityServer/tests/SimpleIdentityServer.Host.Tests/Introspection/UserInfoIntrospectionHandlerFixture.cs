namespace SimpleIdentityServer.Host.Tests.Introspection
{
    using System.Threading.Tasks;
    using Client;
    using Client.Builders;
    using Client.Operations;
    using Core.Jwt;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Microsoft.Extensions.WebEncoders.Testing;
    using Moq;
    using UserInfoIntrospection;
    using Xunit;

    public class UserInfoIntrospectionHandlerFixture : IClassFixture<TestOauthServerFixture>
    {
        private const string baseUrl = "http://localhost:5000";
        private readonly TestOauthServerFixture _server;

        public UserInfoIntrospectionHandlerFixture(TestOauthServerFixture server)
        {
            _server = server;
        }

        [Fact]
        public async Task When_Introspect_Identity_Token_Then_Claims_Are_Returned()
        {
            // ACT
            var result = await new TokenClient(
                    TokenCredentials.FromClientCredentials("client", "client"),
                    TokenRequest.FromPassword("superuser", "password", new[] {"role"}),
                    _server.Client,
                    new GetDiscoveryOperation(_server.Client))
                .ResolveAsync(baseUrl + "/.well-known/openid-configuration")
                .ConfigureAwait(false);
            var authResult = await new UserInfoIntrospectionHandler(
                    new Mock<IOptionsMonitor<UserInfoIntrospectionOptions>>().Object,
                    new Mock<ILoggerFactory>().Object,
                    new UrlTestEncoder(),
                    new Mock<IUserInfoClient>().Object,
                    new Mock<ISystemClock>().Object)
                .HandleAuthenticate(baseUrl + "/.well-known/openid-configuration", result.Content.AccessToken)
                .ConfigureAwait(false);

            // ASSERT
            Assert.True(authResult.Succeeded);
        }
    }
}
