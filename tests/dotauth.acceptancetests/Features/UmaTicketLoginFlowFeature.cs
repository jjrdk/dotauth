namespace DotAuth.AcceptanceTests.Features;

using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.RegularExpressions;
using DotAuth.Client;
using DotAuth.Shared;
using DotAuth.Shared.Models;
using DotAuth.Shared.Requests;
using DotAuth.Shared.Responses;
using Microsoft.IdentityModel.Tokens;
using Xbehave;
using Xunit;
using Xunit.Abstractions;

public sealed class UmaTicketLoginFlowFeature : AuthFlowFeature
{
    /// <inheritdoc />
    public UmaTicketLoginFlowFeature(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
    }

    [Scenario(DisplayName = "Successful ticket authentication")]
    public void SuccessfulTicketAuthentication()
    {
        GrantedTokenResponse umaToken = null!;
        AddResourceSetResponse resourceSetResponse = null!;
        UmaClient umaClient = null!;
        TokenClient client = null!;
        GrantedTokenResponse result = null!;
        string ticketId = null!;

        "and a properly configured token client".x(
            () => client = new TokenClient(
                TokenCredentials.FromClientCredentials("post_client", "post_client"),
                _fixture.Client,
                new Uri(WellKnownOpenidConfiguration)));

        "when requesting token".x(
            async () =>
            {
                var response = await client
                    .GetToken(TokenRequest.FromPassword("user", "password", new[] {"uma_protection", "offline"}))
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
                    ValidAudience = "post_client",
                    ValidIssuer = "https://localhost"
                };
                tokenHandler.ValidateToken(result.AccessToken, validationParameters, out var token);

                Assert.NotEmpty(((JwtSecurityToken) token).Claims);
            });

        "given a uma client".x(
            () =>
            {
                umaClient = new UmaClient(
                    _fixture.Client,
                    new Uri("https://localhost/.well-known/uma2-configuration"));
            });

        "when creating resource set".x(
            async () =>
            {
                var resourceSet = new ResourceSet {Name = "Local", Scopes = new[] {"api1"}, Type = "url",};

                var resourceResponse =
                    await umaClient.AddResource(resourceSet, result.AccessToken).ConfigureAwait(false) as
                        Option<AddResourceSetResponse>.Result;
                resourceSetResponse = resourceResponse.Item;

                Assert.NotNull(resourceResponse);
            });

        "and setting access policy".x(
            async () =>
            {
                var resourceSet = new ResourceSet
                {
                    Id = resourceSetResponse.Id,
                    Name = "Local",
                    Scopes = new[] {"api1"},
                    Type = "url",
                    AuthorizationPolicies = new[]
                    {
                        new PolicyRule
                        {
                            Scopes = new[] {"api1"},
                            Claims = new[]
                            {
                                new ClaimData {Type = ClaimTypes.NameIdentifier, Value = "user"}
                            },
                            ClientIdsAllowed = new[] {"post_client"},
                            IsResourceOwnerConsentNeeded = false
                        }
                    }
                };
                var resourceResponse =
                    await umaClient.UpdateResource(resourceSet, result.AccessToken).ConfigureAwait(false) as
                        Option<UpdateResourceSetResponse>.Result;

                Assert.NotNull(resourceResponse);
            });

        "then can get redirection".x(
            async () =>
            {
                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri("http://localhost/data/" + resourceSetResponse.Id)
                };
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", result.AccessToken);

                var response = await _fixture.Client().SendAsync(request).ConfigureAwait(false);

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
                    .ConfigureAwait(false) as Option<GrantedTokenResponse>.Result;
                umaToken = response.Item;

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
                request.Headers.Authorization = new AuthenticationHeaderValue(
                    umaToken.TokenType,
                    umaToken.AccessToken);
                var response = await _fixture.Client().SendAsync(request).ConfigureAwait(false);
                var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.Equal("\"Hello\"", content);
            });
    }

    [Scenario(DisplayName = "Instance policy ticket authentication")]
    public void DefaultPolicyTicketAuthentication()
    {
        AddResourceSetResponse resourceSetResponse = null!;
        UmaClient umaClient = null!;
        TokenClient client = null!;
        GrantedTokenResponse result = null!;
        string ticketId = null!;

        "and a properly configured token client".x(
            () => client = new TokenClient(
                TokenCredentials.FromClientCredentials("post_client", "post_client"),
                _fixture.Client,
                new Uri(WellKnownOpenidConfiguration)));

        "when requesting token".x(
            async () =>
            {
                var response = await client
                    .GetToken(TokenRequest.FromPassword("user", "password", new[] {"uma_protection"}))
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
                    ValidAudience = "post_client",
                    ValidIssuer = "https://localhost"
                };
                tokenHandler.ValidateToken(result.AccessToken, validationParameters, out var token);

                Assert.NotEmpty(((JwtSecurityToken) token).Claims);
            });

        "given a uma client".x(
            () => { umaClient = new UmaClient(_fixture.Client, new Uri("https://localhost/")); });

        "when creating resource set without a policy".x(
            async () =>
            {
                var resourceSet = new ResourceSet
                {
                    Name = "Local", Scopes = new[] {"api1"}, Type = "url", AuthorizationPolicies = null
                };

                var resourceResponse =
                    await umaClient.AddResource(resourceSet, result.AccessToken).ConfigureAwait(false) as
                        Option<AddResourceSetResponse>.Result;
                resourceSetResponse = resourceResponse.Item;

                Assert.NotNull(resourceResponse);
            });

        "then can get redirection".x(
            async () =>
            {
                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri("http://localhost/data/" + resourceSetResponse.Id)
                };
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", result.AccessToken);

                var response = await _fixture.Client().SendAsync(request).ConfigureAwait(false);

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

                Assert.IsType<Option<GrantedTokenResponse>.Error>(response);
            });
    }

    [Scenario(DisplayName = "Unsuccessful ticket authentication")]
    public void UnsuccessfulTicketAuthentication()
    {
        Option<GrantedTokenResponse> ticketResponse = null!;
        AddResourceSetResponse resourceSetResponse = null!;
        UmaClient umaClient = null!;
        TokenClient client = null!;
        GrantedTokenResponse result = null!;
        string ticketId = null!;

        "and a properly configured token client".x(
            () => client = new TokenClient(
                TokenCredentials.FromClientCredentials("post_client", "post_client"),
                _fixture.Client,
                new Uri(WellKnownOpenidConfiguration)));

        "when requesting token".x(
            async () =>
            {
                var response = await client
                    .GetToken(TokenRequest.FromPassword("user", "password", new[] {"uma_protection"}))
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
                    ValidAudience = "post_client",
                    ValidIssuer = "https://localhost"
                };
                tokenHandler.ValidateToken(result.AccessToken, validationParameters, out var token);

                Assert.NotEmpty(((JwtSecurityToken) token).Claims);
            });

        "given a uma client".x(
            () =>
            {
                umaClient = new UmaClient(
                    _fixture.Client,
                    new Uri("https://localhost/.well-known/uma2-configuration"));
            });

        "when creating resource set with deviating scopes".x(
            async () =>
            {
                var resourceSet = new ResourceSet
                {
                    Name = "Local",
                    Scopes = new[] {"api1"},
                    Type = "url",
                    AuthorizationPolicies = new[]
                    {
                        new PolicyRule
                        {
                            Scopes = new[] {"anotherApi"},
                            Claims = new[] {new ClaimData {Type = "sub", Value = "user"}},
                            ClientIdsAllowed = new[] {"post_client"},
                            IsResourceOwnerConsentNeeded = false
                        }
                    }
                };

                var resourceResponse =
                    await umaClient.AddResource(resourceSet, result.AccessToken).ConfigureAwait(false) as
                        Option<AddResourceSetResponse>.Result;

                Assert.NotNull(resourceResponse);

                resourceSetResponse = resourceResponse.Item;
            });

        "and requesting permission ticket".x(
            async () =>
            {
                var permission =
                    new PermissionRequest {ResourceSetId = resourceSetResponse.Id, Scopes = new[] {"api1"}};
                var permissionResponse = await umaClient.RequestPermission(result.AccessToken, requests: permission)
                    .ConfigureAwait(false) as Option<TicketResponse>.Result;

                Assert.NotNull(permissionResponse);

                ticketId = permissionResponse.Item.TicketId;
            });

        "and requesting token from ticket".x(
            async () =>
            {
                ticketResponse = await client.GetToken(TokenRequest.FromTicketId(ticketId, result.IdToken))
                    .ConfigureAwait(false);
            });

        "then has error".x(() => { Assert.IsType<Option<GrantedTokenResponse>.Error>(ticketResponse); });
    }
}