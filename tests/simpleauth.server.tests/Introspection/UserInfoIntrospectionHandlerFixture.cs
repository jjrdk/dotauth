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
    using Microsoft.AspNetCore.Http;
    using Microsoft.Net.Http.Headers;
    using SimpleAuth.Server.Tests.MiddleWares;
    using UserInfoIntrospection;
    using Xunit;

    public class UserInfoIntrospectionHandlerFixture
    {
        private const string WellKnownOpenidConfiguration = "http://localhost:5000/.well-known/openid-configuration";
        private readonly TestOauthServerFixture _server;

        public UserInfoIntrospectionHandlerFixture()
        {
            _server = new TestOauthServerFixture();
        }

        [Fact]
        public async Task When_Introspect_Identity_Token_Then_Claims_Are_Returned()
        {
            var discoveryDocumentationUrl = new Uri(WellKnownOpenidConfiguration);
            var client = new TokenClient(
                   TokenCredentials.FromClientCredentials("client", "client"),
                   _server.Client,
                   discoveryDocumentationUrl);
            var result = await client.GetToken(TokenRequest.FromPassword("superuser", "password", new[] {"role"}))
                .ConfigureAwait(false);
            var mock = new Mock<IOptionsMonitor<AuthenticationSchemeOptions>>();
            mock.Setup(x => x.Get(It.IsAny<string>())).Returns(new FakeUserInfoIntrospectionOptions());
            var mockLoggerFactory = new Mock<ILoggerFactory>();
            mockLoggerFactory.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(new Mock<ILogger>().Object);
            var introspectionHandler = new UserInfoIntrospectionHandler(
                await UserInfoClient.Create(_server.Client, discoveryDocumentationUrl).ConfigureAwait(false),
                mock.Object,
                mockLoggerFactory.Object,
                new UrlTestEncoder(),
                new Mock<ISystemClock>().Object);
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Host = new HostString("somewhere");
            httpContext.Request.Scheme = Uri.UriSchemeHttps;
            httpContext.Request.Path = "/";
            httpContext.Request.Headers[HeaderNames.Authorization] = "Bearer " + result.Content.AccessToken;
            await introspectionHandler.InitializeAsync(
                    new AuthenticationScheme(
                        UserIntrospectionDefaults.AuthenticationScheme,
                        UserIntrospectionDefaults.AuthenticationScheme,
                        typeof(UserInfoIntrospectionHandler)),
                    httpContext)
                .ConfigureAwait(false);
            var authResult = await introspectionHandler.AuthenticateAsync().ConfigureAwait(false);

            Assert.True(authResult.Succeeded);
        }
    }
}
