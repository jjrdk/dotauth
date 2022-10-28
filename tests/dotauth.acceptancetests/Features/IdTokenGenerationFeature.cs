namespace DotAuth.AcceptanceTests.Features;

using System;
using System.IdentityModel.Tokens.Jwt;
using DotAuth.Client;
using DotAuth.Shared;
using DotAuth.Shared.Responses;
using Xbehave;
using Xunit;
using Xunit.Abstractions;

public sealed class IdTokenGenerationFeature : AuthFlowFeature
{
    /// <inheritdoc />
    public IdTokenGenerationFeature(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
    }

    [Scenario]
    public void WhenTokenRequestedMultipleTimesThenIdTokenHasSingleAudience()
    {
        TokenClient client = null!;
        GrantedTokenResponse token = null!;

        "Given a token client".x(
            () =>
            {
                client = new TokenClient(
                    TokenCredentials.FromClientCredentials("no_key", "no_key"),
                    _fixture.Client,
                    new Uri(WellKnownOpenidConfiguration));
            });

        "When getting token".x(
            async () =>
            {
                var response = await client
                    .GetToken(TokenRequest.FromPassword("administrator", "password", new[] { "api" }))
                    .ConfigureAwait(false) as Option<GrantedTokenResponse>.Result;
                token = response!.Item;

                Assert.NotNull(token);
            });

        "and getting token again".x(
            async () =>
            {
                var response = await client
                    .GetToken(TokenRequest.FromPassword("administrator", "password", new[] { "api" }))
                    .ConfigureAwait(false) as Option<GrantedTokenResponse>.Result;
                token = response!.Item;

                Assert.NotNull(token);
            });

        "and again".x(
            async () =>
            {
                var response = await client
                    .GetToken(TokenRequest.FromPassword("administrator", "password", new[] { "api" }))
                    .ConfigureAwait(false) as Option<GrantedTokenResponse>.Result;
                token = response!.Item;

                Assert.NotNull(token);
            });

        "Then token has single audience".x(
            () =>
            {
                var handler = new JwtSecurityTokenHandler();
                var jwt = handler.ReadJwtToken(token.IdToken);
                Assert.Equal("no_key", string.Join('$', jwt.Audiences));
            });
    }
}