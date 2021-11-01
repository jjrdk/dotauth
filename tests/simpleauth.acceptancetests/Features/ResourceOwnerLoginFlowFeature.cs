namespace SimpleAuth.AcceptanceTests.Features
{
    using Microsoft.IdentityModel.Tokens;
    using SimpleAuth.Client;
    using SimpleAuth.Shared;
    using SimpleAuth.Shared.Responses;
    using System;
    using System.IdentityModel.Tokens.Jwt;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Newtonsoft.Json;
    using SimpleAuth.Extensions;
    using SimpleAuth.Shared.Models;
    using SimpleAuth.Shared.Requests;
    using Xbehave;
    using Xunit;
    using Xunit.Abstractions;

    // In order to secure access to a resource
    // As a resource owner
    // I want to log in using resource owner flow

    // In order to secure access to a resource
    // As a resource owner
    // I want to log in using resource owner flow
    public class ResourceOwnerLoginFlowFeature : AuthFlowFeature
    {
        /// <inheritdoc />
        public ResourceOwnerLoginFlowFeature(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
        }

        [Scenario(DisplayName = "Successful authorization")]
        public void SuccessfulResourceOwnerAuthentication()
        {
            TokenClient client = null;
            GrantedTokenResponse result = null;

            "and a properly configured token client".x(
                () => client = new TokenClient(
                    TokenCredentials.FromBasicAuthentication("client", "client"),
                    _fixture.Client,
                    new Uri(WellKnownOpenidConfiguration)));

            "when requesting token".x(
                async () =>
                {
                    var response = await client
                        .GetToken(TokenRequest.FromPassword("user", "password", new[] {"openid"}))
                        .ConfigureAwait(false) as Option<GrantedTokenResponse>.Result;

                    Assert.NotNull(response);

                    result = response.Item;
                });

            "then has valid access token".x(
                () =>
                {
                    var tokenHandler = new JwtSecurityTokenHandler();
                    var validationParameters = new TokenValidationParameters
                    {
                        IssuerSigningKeys = _jwks.GetSigningKeys(),
                        ValidAudience = "client",
                        ValidIssuer = "https://localhost"
                    };
                    tokenHandler.ValidateToken(result.AccessToken, validationParameters, out var token);

                    Assert.NotEmpty(((JwtSecurityToken) token).Claims);
                });

            "and has valid id token".x(
                () =>
                {
                    var tokenHandler = new JwtSecurityTokenHandler();
                    var validationParameters = new TokenValidationParameters
                    {
                        IssuerSigningKey = TestKeys.SecretKey.CreateJwk(
                            JsonWebKeyUseNames.Sig,
                            KeyOperations.Sign,
                            KeyOperations.Verify),
                        ValidAudience = "client",
                        ValidIssuer = "https://localhost"
                    };
                    tokenHandler.ValidateToken(result.IdToken, validationParameters, out _);
                });
        }

        [Scenario(DisplayName = "UserInfo after successful authorization")]
        public void UserinfoAfterSuccessfulResourceOwnerAuthentication()
        {
            TokenClient client = null;
            GrantedTokenResponse result = null;

            "and a properly configured token client".x(
                () => client = new TokenClient(
                    TokenCredentials.FromBasicAuthentication("client", "client"),
                    _fixture.Client,
                    new Uri(WellKnownOpenidConfiguration)));

            "when requesting token".x(
                async () =>
                {
                    var response = await client
                        .GetToken(TokenRequest.FromPassword("user", "password", new[] {"openid"}))
                        .ConfigureAwait(false) as Option<GrantedTokenResponse>.Result;
                    result = response.Item;
                });

            "then has valid access token".x(
                () =>
                {
                    var tokenHandler = new JwtSecurityTokenHandler();
                    var validationParameters = new TokenValidationParameters
                    {
                        IssuerSigningKeys = _jwks.GetSigningKeys(),
                        ValidAudience = "client",
                        ValidIssuer = "https://localhost"
                    };
                    tokenHandler.ValidateToken(result.AccessToken, validationParameters, out var token);

                    Assert.NotEmpty(((JwtSecurityToken) token).Claims);
                });

            "and can get user info".x(
                async () =>
                {
                    var userinfoRequest = new HttpRequestMessage
                    {
                        Method = HttpMethod.Get, RequestUri = new Uri(BaseUrl + "/userinfo")
                    };
                    userinfoRequest.Headers.Authorization =
                        new AuthenticationHeaderValue(result.TokenType, result.AccessToken);
                    var userinfo = await _fixture.Client().SendAsync(userinfoRequest).ConfigureAwait(false);

                    Assert.True(userinfo.IsSuccessStatusCode);
                });
        }

        [Scenario(DisplayName = "Successful claims update")]
        public void SuccessfulResourceOwnerClaimsUpdate()
        {
            TokenClient client = null;
            GrantedTokenResponse tokenResponse = null;
            HttpResponseMessage updateResponse = null;

            "and a properly configured token client".x(
                () => client = new TokenClient(
                    TokenCredentials.FromBasicAuthentication("client", "client"),
                    _fixture.Client,
                    new Uri(WellKnownOpenidConfiguration)));

            "when requesting token".x(
                async () =>
                {
                    var response = await client
                        .GetToken(TokenRequest.FromPassword("user", "password", new[] {"openid", "offline"}))
                        .ConfigureAwait(false) as Option<GrantedTokenResponse>.Result;
                    tokenResponse = response.Item;
                });

            "and valid access token is received".x(
                () =>
                {
                    var tokenHandler = new JwtSecurityTokenHandler();
                    var validationParameters = new TokenValidationParameters
                    {
                        IssuerSigningKeys = _jwks.GetSigningKeys(),
                        ValidAudience = "client",
                        ValidIssuer = "https://localhost"
                    };
                    tokenHandler.ValidateToken(tokenResponse.AccessToken, validationParameters, out var token);

                    Assert.NotEmpty(((JwtSecurityToken) token).Claims);
                });

            "and updating own claims".x(
                async () =>
                {
                    var updateRequest = new UpdateResourceOwnerClaimsRequest
                    {
                        Subject = "user", Claims = new[] {new ClaimData {Type = "test", Value = "something"}}
                    };

                    var json = JsonConvert.SerializeObject(updateRequest);

                    var request = new HttpRequestMessage
                    {
                        Content = new StringContent(json, Encoding.UTF8, "application/json"),
                        Method = HttpMethod.Post,
                        RequestUri = new Uri(_fixture.Server.BaseAddress + "resource_owners/claims")
                    };
                    request.Headers.Authorization = new AuthenticationHeaderValue(
                        JwtBearerDefaults.AuthenticationScheme,
                        tokenResponse.AccessToken);
                    updateResponse = await _fixture.Client().SendAsync(request).ConfigureAwait(false);
                });

            "then update is successful".x(() => { Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode); });
        }

        [Scenario(DisplayName = "Successful token refresh")]
        public void SuccessfulResourceOwnerRefresh()
        {
            TokenClient client = null;
            GrantedTokenResponse result = null;
            GrantedTokenResponse refreshed = null;

            "and a properly token client".x(
                () => client = new TokenClient(
                    TokenCredentials.FromBasicAuthentication("client", "client"),
                    _fixture.Client,
                    new Uri(WellKnownOpenidConfiguration)));

            "when requesting auth token".x(
                async () =>
                {
                    var response = await client
                        .GetToken(TokenRequest.FromPassword("user", "password", new[] {"openid", "offline"}))
                        .ConfigureAwait(false) as Option<GrantedTokenResponse>.Result;
                    result = response.Item;
                });

            "then can get new token from refresh token".x(
                async () =>
                {
                    var response = await client.GetToken(TokenRequest.FromRefreshToken(result.RefreshToken))
                        .ConfigureAwait(false) as Option<GrantedTokenResponse>.Result;
                    Assert.NotNull(response);

                    refreshed = response.Item;
                });

            "and token has custom custom claims".x(
                () =>
                {
                    var handler = new JwtSecurityTokenHandler();
                    var refreshedClaims = handler.ReadJwtToken(refreshed.AccessToken).Claims;

                    Assert.Contains(refreshedClaims, c => c.Type == "acceptance_test");
                });
        }

        [Scenario(DisplayName = "Successful token revocation")]
        public void SuccessfulResourceOwnerRevocation()
        {
            TokenClient client = null;
            GrantedTokenResponse result = null;

            "and a properly token client".x(
                () => client = new TokenClient(
                    TokenCredentials.FromBasicAuthentication("client", "client"),
                    _fixture.Client,
                    new Uri(WellKnownOpenidConfiguration)));

            "when requesting auth token".x(
                async () =>
                {
                    var response = await client
                        .GetToken(TokenRequest.FromPassword("user", "password", new[] {"openid"}))
                        .ConfigureAwait(false) as Option<GrantedTokenResponse>.Result;
                    result = response.Item;
                });

            "then can revoke token".x(
                async () =>
                {
                    var response = await client.RevokeToken(RevokeTokenRequest.Create(result)).ConfigureAwait(false);
                    Assert.IsType<Option.Success>(response);
                });
        }

        [Scenario(DisplayName = "Invalid client")]
        public void InvalidClientCredentials()
        {
            TokenClient client = null;
            Option<GrantedTokenResponse> result = null;

            "and a token client with invalid client credentials".x(
                () => client = new TokenClient(
                    TokenCredentials.FromBasicAuthentication("xxx", "xxx"),
                    _fixture.Client,
                    new Uri(WellKnownOpenidConfiguration)));

            "when requesting auth token".x(
                async () =>
                {
                    result = await client.GetToken(TokenRequest.FromPassword("user", "password", new[] {"openid"}))
                        .ConfigureAwait(false);
                });

            "then does not have token".x(() => { Assert.IsType<Option<GrantedTokenResponse>.Error>(result); });
        }

        [Scenario(DisplayName = "Invalid user credentials")]
        public void InvalidUserCredentials()
        {
            TokenClient client = null;
            Option<GrantedTokenResponse> result = null;

            "and a token client with invalid client credentials".x(
                () => client = new TokenClient(
                    TokenCredentials.FromBasicAuthentication("client", "client"),
                    _fixture.Client,
                    new Uri(WellKnownOpenidConfiguration)));

            "when requesting auth token".x(
                async () =>
                {
                    result = await client.GetToken(TokenRequest.FromPassword("someone", "xxx", new[] {"openid"}))
                        .ConfigureAwait(false);
                });

            "then does not have token".x(() => { Assert.IsType<Option<GrantedTokenResponse>.Error>(result); });
        }
    }
}
