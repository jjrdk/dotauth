namespace DotAuth.AcceptanceTests.Features;

using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using DotAuth.Client;
using DotAuth.Extensions;
using DotAuth.Shared;
using DotAuth.Shared.Responses;
using Microsoft.IdentityModel.Tokens;
using Xbehave;
using Xunit;
using Xunit.Abstractions;

public sealed class IdTokenSigningFeature : AuthFlowFeature
{
    /// <inheritdoc />
    public IdTokenSigningFeature(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
    }

    [Scenario]
    public void WhenClientHasNoSigningKeysThenUsesServerKey()
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
                    .GetToken(TokenRequest.FromPassword("administrator", "password", new[] {"api"}))
                    .ConfigureAwait(false) as Option<GrantedTokenResponse>.Result;
                token = response.Item;

                Assert.NotNull(token);
            });

        "Then token is signed with server key".x(
            () =>
            {
                var key = _jwks.GetSignKeys().First();
                var validationParameters = new TokenValidationParameters
                {
                    IssuerSigningKey = key,
                    ValidateAudience = false,
                    ValidateActor = false,
                    ValidateIssuer = false,
                    ValidateLifetime = false,
                    ValidateTokenReplay = false
                };
                var handler = new JwtSecurityTokenHandler();
                handler.ValidateToken(token.IdToken, validationParameters, out _);
            });
    }
}