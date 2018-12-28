namespace SimpleAuth.Server.Tests.Introspection
{
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Microsoft.Extensions.WebEncoders.Testing;
    using Moq;
    using Newtonsoft.Json.Linq;
    using SimpleIdentityServer.Client;
    using SimpleIdentityServer.Client.Operations;
    using SimpleIdentityServer.Client.Results;
    using SimpleIdentityServer.UserInfoIntrospection;
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

                        Assert.True(authResult.Succeeded);
        }
    }
}
