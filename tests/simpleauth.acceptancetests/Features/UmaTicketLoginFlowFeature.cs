namespace SimpleAuth.AcceptanceTests.Features
{
    using System;
    using System.IdentityModel.Tokens.Jwt;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text.RegularExpressions;
    using Microsoft.IdentityModel.Tokens;
    using SimpleAuth.Client;
    using SimpleAuth.Client.Results;
    using SimpleAuth.Shared.DTOs;
    using SimpleAuth.Shared.Responses;
    using Xbehave;
    using Xunit;

    public class UmaTicketLoginFlowFeature : AuthFlowFeature
    {
        [Scenario(DisplayName = "Successful ticket authentication")]
        public void SuccessfulTicketAuthentication()
        {
            GrantedTokenResponse umaToken = null;
            AddResourceSetResponse resourceSetResponse = null;
            UmaClient umaClient = null;
            TokenClient client = null;
            GrantedTokenResponse result = null;
            string policyId = null;
            string ticketId = null;

            "and a properly configured token client".x(
                () => client = new TokenClient(
                    TokenCredentials.FromClientCredentials("post_client", "post_client"),
                    _fixture.Client,
                    new Uri(WellKnownOpenidConfiguration)));

            "when requesting token".x(
                async () =>
                {
                    var response = await client
                        .GetToken(TokenRequest.FromPassword("user", "password", new[] { "uma_protection" }))
                        .ConfigureAwait(false);
                    result = response.Content;
                });

            "then has valid access token".x(
                () =>
                {
                    var tokenHandler = new JwtSecurityTokenHandler();
                    var validationParameters = new TokenValidationParameters
                    {
                        IssuerSigningKeys = _jwks.GetSigningKeys(),
                        ValidAudience = "post_client",
                        ValidIssuer = "https://localhost"
                    };
                    tokenHandler.ValidateToken(result.AccessToken, validationParameters, out var token);

                    Assert.NotEmpty(((JwtSecurityToken)token).Claims);
                });

            "given a uma client".x(
                () =>
                {
                    umaClient = new UmaClient(
                        _fixture.Client,
                        new Uri("https://localhost/.well-known/uma2-configuration"));
                });

            "and setting a policy".x(
                async () =>
                {
                    var policy = new PolicyData
                    {
                        Rules = new[]
                        {
                            new PolicyRuleData
                            {
                                Scopes = new[] {"api1"},
                                Claims = new[] {new ClaimData {Type = "sub", Value = "user"}},
                                ClientIdsAllowed = new[] {"post_client"},
                                IsResourceOwnerConsentNeeded = false
                            }
                        }
                    };
                    var policyResponse = await umaClient.AddPolicy(policy, result.AccessToken).ConfigureAwait(false);
                    Assert.False(policyResponse.ContainsError);

                    policyId = policyResponse.Content.PolicyId;
                });

            "when creating resource set".x(
                async () =>
                {
                    var resourceSet = new ResourceSet
                    {
                        Name = "Local",
                        Scopes = new[] { "api1" },
                        Type = "url",
                        AuthorizationPolicies = new[] { policyId }
                    };

                    var resourceResponse = await umaClient.AddResource(resourceSet, result.AccessToken).ConfigureAwait(false);
                    resourceSetResponse = resourceResponse.Content;

                    Assert.False(resourceResponse.ContainsError);
                });

            "then can get redirection".x(
                async () =>
                {
                    var request = new HttpRequestMessage
                    {
                        Method = HttpMethod.Get,
                        RequestUri = new Uri("http://localhost/data/" + resourceSetResponse.Id)
                    };

                    var response = await _fixture.Client.SendAsync(request).ConfigureAwait(false);

                    Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
                    var httpHeaderValueCollection = response.Headers.WwwAuthenticate;
                    Assert.True(httpHeaderValueCollection != null);

                    var match = Regex.Match(
                        httpHeaderValueCollection.First().Parameter,
                        ".+ticket=\"(.+)\".*",
                        RegexOptions.Compiled);
                    ticketId = match.Groups[1].Value;
                });

            "when requesting token".x(
                async () =>
                {
                    var response = await client.GetToken(TokenRequest.FromTicketId(ticketId, result.IdToken))
                        .ConfigureAwait(false);
                    umaToken = response.Content;

                    Assert.Null(response.Error);
                    Assert.NotNull(umaToken.AccessToken);
                });

            "then can get resource with token".x(
                async () =>
                {
                    var request = new HttpRequestMessage
                    {
                        Method = HttpMethod.Get,
                        RequestUri = new Uri("http://localhost/data/" + resourceSetResponse.Id)
                    };
                    request.Headers.Authorization = new AuthenticationHeaderValue(umaToken.TokenType, umaToken.AccessToken);
                    var response = await _fixture.Client.SendAsync(request).ConfigureAwait(false);
                    var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                    Assert.Equal("Hello", content);
                });
        }

        [Scenario(DisplayName = "Default policy ticket authentication")]
        public void DefaultPolicyTicketAuthentication()
        {
            GrantedTokenResponse umaToken = null;
            AddResourceSetResponse resourceSetResponse = null;
            UmaClient umaClient = null;
            TokenClient client = null;
            GrantedTokenResponse result = null;
            string ticketId = null;

            "and a properly configured token client".x(
                () => client = new TokenClient(
                    TokenCredentials.FromClientCredentials("post_client", "post_client"),
                    _fixture.Client,
                    new Uri(WellKnownOpenidConfiguration)));

            "when requesting token".x(
                async () =>
                {
                    var response = await client
                        .GetToken(TokenRequest.FromPassword("user", "password", new[] { "uma_protection" }))
                        .ConfigureAwait(false);
                    result = response.Content;
                });

            "then has valid access token".x(
                () =>
                {
                    var tokenHandler = new JwtSecurityTokenHandler();
                    var validationParameters = new TokenValidationParameters
                    {
                        IssuerSigningKeys = _jwks.GetSigningKeys(),
                        ValidAudience = "post_client",
                        ValidIssuer = "https://localhost"
                    };
                    tokenHandler.ValidateToken(result.AccessToken, validationParameters, out var token);

                    Assert.NotEmpty(((JwtSecurityToken)token).Claims);
                });

            "given a uma client".x(
                () =>
                {
                    umaClient = new UmaClient(
                        _fixture.Client,
                        new Uri("https://localhost/"));
                });

            "when creating resource set without a policy".x(
                async () =>
                {
                    var resourceSet = new ResourceSet
                    {
                        Name = "Local",
                        Scopes = new[] { "api1" },
                        Type = "url",
                        AuthorizationPolicies = null
                    };

                    var resourceResponse = await umaClient.AddResource(resourceSet, result.AccessToken).ConfigureAwait(false);
                    resourceSetResponse = resourceResponse.Content;

                    Assert.False(resourceResponse.ContainsError);
                });

            "then can get redirection".x(
                async () =>
                {
                    var request = new HttpRequestMessage
                    {
                        Method = HttpMethod.Get,
                        RequestUri = new Uri("http://localhost/data/" + resourceSetResponse.Id)
                    };

                    var response = await _fixture.Client.SendAsync(request).ConfigureAwait(false);

                    Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
                    var httpHeaderValueCollection = response.Headers.WwwAuthenticate;
                    Assert.True(httpHeaderValueCollection != null);

                    var match = Regex.Match(
                        httpHeaderValueCollection.First().Parameter,
                        ".+ticket=\"(.+)\".*",
                        RegexOptions.Compiled);
                    ticketId = match.Groups[1].Value;
                });

            "when requesting token".x(
                async () =>
                {
                    var response = await client.GetToken(TokenRequest.FromTicketId(ticketId, result.IdToken))
                        .ConfigureAwait(false);
                    umaToken = response.Content;

                    Assert.True(response.HasError);
                });

            "then has no token".x(
                () =>
                {
                    Assert.Null(umaToken);
                });
        }

        [Scenario(DisplayName = "Unsuccessful ticket authentication")]
        public void UnsuccessfulTicketAuthentication()
        {
            BaseSidContentResult<GrantedTokenResponse> ticketResponse = null;
            AddResourceSetResponse resourceSetResponse = null;
            UmaClient umaClient = null;
            TokenClient client = null;
            GrantedTokenResponse result = null;
            string policyId = null;
            string ticketId = null;

            "and a properly configured token client".x(
                () => client = new TokenClient(
                    TokenCredentials.FromClientCredentials("post_client", "post_client"),
                    _fixture.Client,
                    new Uri(WellKnownOpenidConfiguration)));

            "when requesting token".x(
                async () =>
                {
                    var response = await client
                        .GetToken(TokenRequest.FromPassword("user", "password", new[] { "uma_protection" }))
                        .ConfigureAwait(false);
                    result = response.Content;
                });

            "then has valid access token".x(
                () =>
                {
                    var tokenHandler = new JwtSecurityTokenHandler();
                    var validationParameters = new TokenValidationParameters
                    {
                        IssuerSigningKeys = _jwks.GetSigningKeys(),
                        ValidAudience = "post_client",
                        ValidIssuer = "https://localhost"
                    };
                    tokenHandler.ValidateToken(result.AccessToken, validationParameters, out var token);

                    Assert.NotEmpty(((JwtSecurityToken)token).Claims);
                });

            "given a uma client".x(
                () =>
                {
                    umaClient = new UmaClient(
                        _fixture.Client,
                        new Uri("https://localhost/.well-known/uma2-configuration"));
                });

            "and setting a policy".x(
                async () =>
                {
                    var policy = new PolicyData
                    {
                        Rules = new[]
                        {
                            new PolicyRuleData
                            {
                                Scopes = new[] {"anotherApi"},
                                Claims = new[] {new ClaimData {Type = "sub", Value = "user"}},
                                ClientIdsAllowed = new[] {"post_client"},
                                IsResourceOwnerConsentNeeded = false
                            }
                        }
                    };
                    var policyResponse = await umaClient.AddPolicy(policy, result.AccessToken).ConfigureAwait(false);
                    Assert.False(policyResponse.ContainsError);

                    policyId = policyResponse.Content.PolicyId;
                });

            "when creating resource set with deviating scopes".x(
                async () =>
                {
                    var resourceSet = new ResourceSet
                    {
                        Name = "Local",
                        Scopes = new[] { "api1" },
                        Type = "url",
                        AuthorizationPolicies = new[] { policyId }
                    };

                    var resourceResponse = await umaClient.AddResource(resourceSet, result.AccessToken).ConfigureAwait(false);
                    resourceSetResponse = resourceResponse.Content;

                    Assert.False(resourceResponse.ContainsError);
                });

            "and requesting permission ticket".x(
                async () =>
                {
                    var permission =
                        new PermissionRequest { ResourceSetId = resourceSetResponse.Id, Scopes = new[] { "api1" } };
                    var permissionResponse = await umaClient.RequestPermission(result.AccessToken, permission).ConfigureAwait(false);
                    ticketId = permissionResponse.Content.TicketId;

                    Assert.Null(permissionResponse.Error);
                });

            "and requesting token from ticket".x(
                async () =>
                {
                    ticketResponse = await client.GetToken(TokenRequest.FromTicketId(ticketId, result.IdToken))
                        .ConfigureAwait(false);
                });

            "then has error".x(
                () =>
                {
                    Assert.NotNull(ticketResponse.Error);
                    Assert.Null(ticketResponse.Content);
                });
        }
    }
}
