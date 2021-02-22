namespace SimpleAuth.IntegrationTests
{
    using System;
    using System.Net.Http;
    using System.Threading.Tasks;
    using SimpleAuth.Client;
    using Xunit;

    public class TokenTests : IClassFixture<DbFixture>
    {
        private readonly DbFixture _fixture;

        public TokenTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task CanGetToken()
        {
            var client = new TokenClient(
                TokenCredentials.FromClientCredentials("client", "secret"),
                () => new HttpClient(),
                new Uri("http://localhost:8080/.well-known/openid-configuration"));
            await _fixture.GetUser().ConfigureAwait(false);
            for (int i = 0; i < 100; i++)
            {
                var token = await client.GetToken(TokenRequest.FromPassword("user", "password", new[] { "read" }))
                    .ConfigureAwait(false);

                Assert.NotNull(token.Content);
            }
        }
    }
}