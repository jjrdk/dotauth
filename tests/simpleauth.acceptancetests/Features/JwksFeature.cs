namespace SimpleAuth.AcceptanceTests.Features
{
    using Microsoft.IdentityModel.Tokens;
    using SimpleAuth.Client;
    using SimpleAuth.Shared.Responses;
    using System;
    using System.IdentityModel.Tokens.Jwt;
    using Xbehave;
    using Xunit;

    public class JwksFeature : AuthFlowFeature
    {
        [Scenario]
        public void SuccessfulPermissionCreation()
        {
            string jwksJson = null;

            "then can download json web key set".x(
                async () =>
                {
                    jwksJson = await fixture.Client.GetStringAsync(BaseUrl + "/jwks").ConfigureAwait(false);

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
            GrantedTokenResponse tokenResponse = null;
            JsonWebKeySet jwks = null;

            "And a valid token".x(
                async () =>
                {
                    var tokenClient = await TokenClient.Create(
                            TokenCredentials.FromClientCredentials("clientCredentials", "clientCredentials"),
                            fixture.Client,
                            new Uri(WellKnownOpenidConfiguration))
                        .ConfigureAwait(false);
                    var response = await tokenClient.GetToken(TokenRequest.FromScopes("api1")).ConfigureAwait(false);

                    Assert.False(response.ContainsError);

                    tokenResponse = response.Content;
                });

            "then can download json web key set".x(
                async () =>
                {
                    var jwksJson = await fixture.Client.GetStringAsync(BaseUrl + "/jwks").ConfigureAwait(false);

                    Assert.NotNull(jwksJson);

                    jwks = JsonWebKeySet.Create(jwksJson);
                });

            "Then can create token validation parameters from service metadata".x(
                () =>
                {
                    var validationParameters = new TokenValidationParameters
                    {
                        IssuerSigningKeys = jwks.Keys, ValidIssuer = "https://localhost", ValidAudience = "clientCredentials"
                    };

                    var handler = new JwtSecurityTokenHandler();

                    handler.ValidateToken(tokenResponse.AccessToken, validationParameters, out var securityToken);

                    Assert.NotNull(securityToken);
                });
        }
    }
}
