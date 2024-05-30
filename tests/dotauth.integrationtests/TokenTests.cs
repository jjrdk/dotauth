namespace DotAuth.IntegrationTests;

using System;
using System.Net.Http;
using System.Threading.Tasks;
using DotAuth.Client;
using DotAuth.Shared;
using DotAuth.Shared.Responses;
using Xunit;

public sealed class TokenTests : IClassFixture<DbFixture>
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
        await _fixture.GetUser();
        for (var i = 0; i < 100; i++)
        {
            var token = Assert.IsType<Option<GrantedTokenResponse>.Result>(await client
                .GetToken(TokenRequest.FromPassword("user", "password", ["read"])));

            Assert.NotNull(token.Item);
        }
    }
}
