namespace SimpleIdentityServer.Host.Tests.Introspection
{
    using Client;
    using Client.Operations;
    using Client.Results;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Microsoft.Extensions.WebEncoders.Testing;
    using Moq;
    using Newtonsoft.Json.Linq;
    using System.Threading.Tasks;
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
                    TokenRequest.FromPassword("superuser", "password", new[] { "role" }),
                    _server.Client,
                    new GetDiscoveryOperation(_server.Client))
                .ResolveAsync(baseUrl + "/.well-known/openid-configuration")
                .ConfigureAwait(false);
            var userInfoClient = new Mock<IUserInfoClient>();
            var userInfoResult = new GetUserInfoResult { Content = new JObject() };
            userInfoClient.Setup(x => x.Resolve(It.IsAny<string>(), It.IsAny<string>(), false))
                .ReturnsAsync(userInfoResult);
            var authResult = await new UserInfoIntrospectionHandler(
                    new Mock<IOptionsMonitor<UserInfoIntrospectionOptions>>().Object,
                    new Mock<ILoggerFactory>().Object,
                    new UrlTestEncoder(),
                    userInfoClient.Object,
                    new Mock<ISystemClock>().Object)
                .HandleAuthenticate(baseUrl + "/.well-known/openid-configuration", result.Content.AccessToken)
                .ConfigureAwait(false);

            // ASSERT
            Assert.True(authResult.Succeeded);
        }
    }
}
