namespace DotAuth.AcceptanceTests.Features;

using System;
using System.IdentityModel.Tokens.Jwt;
using DotAuth.Client;
using DotAuth.Shared;
using DotAuth.Shared.Responses;
using Microsoft.IdentityModel.Tokens;
using Xbehave;
using Xunit;
using Xunit.Abstractions;

public sealed class JwksFeature : AuthFlowFeature
{
    /// <inheritdoc />
    public JwksFeature(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
    }

    [Scenario]
    public void SuccessfulPermissionCreation()
    {
        string jwksJson = null!;

        "then can download json web key set".x(
            async () =>
            {
                jwksJson = await _fixture.Client().GetStringAsync(BaseUrl + "/jwks").ConfigureAwait(false);

                Assert.NotNull(jwksJson);
            });

        "and can create JWKS from json".x(
            () =>
            {
                var jwks = new JsonWebKeySet(jwksJson);

                Assert.NotEmpty(jwks.Keys);
            });
    }

    [Scenario]
    public void SuccessfulTokenValidationFromMetadata()
    {
        GrantedTokenResponse tokenResponse = null!;
        JsonWebKeySet jwks = null!;

        "And a valid token".x(
            async () =>
            {
                var tokenClient = new TokenClient(
                    TokenCredentials.FromClientCredentials("clientCredentials", "clientCredentials"),
                    _fixture.Client,
                    new Uri(WellKnownOpenidConfiguration));
                var response =
                    await tokenClient.GetToken(TokenRequest.FromScopes("api1")).ConfigureAwait(false) as
                        Option<GrantedTokenResponse>.Result;

                Assert.NotNull(response);

                tokenResponse = response.Item;
            });

        "then can download json web key set".x(
            async () =>
            {
                var jwksJson = await _fixture.Client().GetStringAsync(BaseUrl + "/jwks").ConfigureAwait(false);

                Assert.NotNull(jwksJson);

                jwks = JsonWebKeySet.Create(jwksJson);
            });

        "Then can create token validation parameters from service metadata".x(
            () =>
            {
                var validationParameters = new TokenValidationParameters
                {
                    IssuerSigningKeys = jwks.Keys,
                    ValidIssuer = "https://localhost",
                    ValidAudience = "clientCredentials"
                };

                var handler = new JwtSecurityTokenHandler();

                handler.ValidateToken(tokenResponse.AccessToken, validationParameters, out var securityToken);

                Assert.NotNull(securityToken);
            });
    }
}