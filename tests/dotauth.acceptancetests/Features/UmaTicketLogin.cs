namespace DotAuth.AcceptanceTests.Features;

using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DotAuth.Client;
using DotAuth.Shared;
using DotAuth.Shared.Models;
using DotAuth.Shared.Requests;
using DotAuth.Shared.Responses;
using TechTalk.SpecFlow;
using Xunit;

public partial class FeatureTest
{
    private GrantedTokenResponse _umaToken;

    [When(@"creating resource set")]
    public async Task WhenCreatingResourceSet()
    {
        var resourceSet = new ResourceSet
        {
            Name = "Local",
            Scopes = new[] { "api1" },
            Type = "url",
            AuthorizationPolicies = new[]
            {
                new PolicyRule
                {
                    Scopes = new[] { "api1" },
                    Claims = new[] { new ClaimData { Type = ClaimTypes.NameIdentifier, Value = "user" } },
                    ClientIdsAllowed = new[] { "post_client" },
                    IsResourceOwnerConsentNeeded = false
                }
            }
        };

        var option = await _umaClient.AddResource(resourceSet, _token.AccessToken).ConfigureAwait(false);
        var resourceResponse = Assert.IsType<Option<AddResourceSetResponse>.Result>(option);

        _resourceSetResponse = resourceResponse.Item;

        Assert.NotNull(_resourceSetResponse);
    }

    [When(@"getting a redirection")]
    public async Task WhenGettingARedirection()
    {
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Get, RequestUri = new Uri($"http://localhost/data/{_resourceSetResponse.Id}")
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token.AccessToken);

        var response = await _fixture.Client().SendAsync(request).ConfigureAwait(false);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        var httpHeaderValueCollection = response.Headers.WwwAuthenticate;
        Assert.True(httpHeaderValueCollection != null);

        var match = Regex.Match(
            httpHeaderValueCollection.First().Parameter!,
            ".+ticket=\"(.+)\".*",
            RegexOptions.Compiled);
        _ticketId = match.Groups[1].Value;
    }

    [When(@"getting token from ticket")]
    public async Task WhenGettingTokenFromTicket()
    {
        var option = await _tokenClient.GetToken(TokenRequest.FromTicketId(_ticketId, _token.IdToken!))
            .ConfigureAwait(false);
        var response = Assert.IsType<Option<GrantedTokenResponse>.Result>(option);
        _umaToken = response.Item;

        Assert.NotNull(_umaToken.AccessToken);
    }

    [Then(@"can get resource with token")]
    public async Task ThenCanGetResourceWithToken()
    {
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Get, RequestUri = new Uri($"http://localhost/data/{_resourceSetResponse.Id}")
        };
        request.Headers.Authorization = new AuthenticationHeaderValue(_umaToken.TokenType, _umaToken.AccessToken);
        var response = await _fixture.Client().SendAsync(request).ConfigureAwait(false);
        var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("\"Hello\"", content);
    }

    [When(@"creating resource set without policy")]
    public async Task WhenCreatingResourceSetWithoutPolicy()
    {
        var resourceSet = new ResourceSet
        {
            Name = "Local",
            Scopes = new[] { "api1" },
            Type = "url",
            AuthorizationPolicies = Array.Empty<PolicyRule>()
        };

        var resourceResponse =
            await _umaClient.AddResource(resourceSet, _token.AccessToken).ConfigureAwait(false) as
                Option<AddResourceSetResponse>.Result;
        _resourceSetResponse = resourceResponse.Item;

        Assert.NotNull(resourceResponse);
    }

    [Then(@"cannot get token")]
    public async Task ThenCannotGetToken()
    {
        var response = await _tokenClient.GetToken(TokenRequest.FromTicketId(_ticketId, _token.IdToken!))
            .ConfigureAwait(false);

        Assert.IsType<Option<GrantedTokenResponse>.Error>(response);
    }

    [When(@"creating resource set with deviating scopes")]
    public async Task WhenCreatingResourceSetWithDeviatingScopes()
    {
        var resourceSet = new ResourceSet
        {
            Name = "Local",
            Scopes = new[] { "api1" },
            Type = "url",
            AuthorizationPolicies = new[]
            {
                new PolicyRule
                {
                    Scopes = new[] { "anotherApi" },
                    Claims = new[] { new ClaimData { Type = "sub", Value = "user" } },
                    ClientIdsAllowed = new[] { "post_client" },
                    IsResourceOwnerConsentNeeded = false
                }
            }
        };

        var resourceResponse =
            await _umaClient.AddResource(resourceSet, _token.AccessToken).ConfigureAwait(false) as
                Option<AddResourceSetResponse>.Result;

        Assert.NotNull(resourceResponse);

        _resourceSetResponse = resourceResponse.Item;
    }

    [When(@"requesting permission ticket")]
    public async Task WhenRequestingPermissionTicket()
    {
        var permission = new PermissionRequest { ResourceSetId = _resourceSetResponse.Id, Scopes = new[] { "api1" } };
        var option = await _umaClient.RequestPermission(_token.AccessToken, requests: permission).ConfigureAwait(false);

        var permissionResponse = Assert.IsType<Option<TicketResponse>.Result>(option);

        _ticketId = permissionResponse.Item.TicketId;
    }

    [Then(@"cannot get token from ticket")]
    public async Task ThenCannotGetTokenFromTicket()
    {
        var option = await _tokenClient.GetToken(TokenRequest.FromTicketId(_ticketId, _token.IdToken))
            .ConfigureAwait(false);
        Assert.IsType<Option<GrantedTokenResponse>.Error>(option);
    }
}
