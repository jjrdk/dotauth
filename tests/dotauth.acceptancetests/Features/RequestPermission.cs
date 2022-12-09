namespace DotAuth.AcceptanceTests.Features;

using System;
using System.IdentityModel.Tokens.Jwt;
using System.Threading;
using System.Threading.Tasks;
using DotAuth.Client;
using DotAuth.Shared;
using DotAuth.Shared.Models;
using DotAuth.Shared.Requests;
using DotAuth.Shared.Responses;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using TechTalk.SpecFlow;
using Xunit;
using Xunit.Abstractions;

[Binding]
[Scope(Feature = "Request permission to protected resource")]
public class RequestPermission : AuthFlowFeature, IDisposable
{
    private const string WellKnownUmaConfiguration = "https://localhost/.well-known/uma2-configuration";

    private GrantedTokenResponse _grantedToken = null!;
    private UmaClient _client = null!;
    private TokenClient _tokenClient = null!;
    private string _resourceId = null!;
    private string _ticketId = null!;

    public RequestPermission(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        IdentityModelEventSource.ShowPII = true;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Fixture?.Dispose();
        GC.SuppressFinalize(this);
    }

    [Given(@"a running auth server")]
    public void GivenARunningAuthServer()
    {
        Fixture = new TestServerFixture(_outputHelper, BaseUrl);
    }

    [Given(@"the server's signing key")]
    public async Task GivenTheServersSigningKey()
    {
        var json = await Fixture.Client().GetStringAsync(BaseUrl + "/jwks").ConfigureAwait(false);
        var jwks = new JsonWebKeySet(json);

        Assert.NotEmpty(jwks.Keys);
    }

    [Given(@"a properly configured token client")]
    public void GivenAProperlyConfiguredTokenClient()
    {
        _tokenClient = new TokenClient(
            TokenCredentials.FromClientCredentials("clientCredentials", "clientCredentials"),
            Fixture.Client,
            new Uri(WellKnownUmaConfiguration));
    }

    [Given(@"a valid UMA token")]
    public async Task GivenAValidUmaToken()
    {
        var token = (await _tokenClient.GetToken(TokenRequest.FromScopes("uma_protection"))
            .ConfigureAwait(false) as Option<GrantedTokenResponse>.Result)!;
        var handler = new JwtSecurityTokenHandler();
        var principal = handler.ReadJwtToken(token.Item.AccessToken);
        Assert.NotNull(principal.Issuer);
        _grantedToken = token.Item;
    }

    [Given(@"a properly configured uma client")]
    public void GivenAProperlyConfiguredUmaClient()
    {
        _client = new UmaClient(Fixture.Client, new Uri(WellKnownUmaConfiguration));
    }

    [When(@"registering resource")]
    public async Task WhenRegisteringResource()
    {
        var resource = (await _client.AddResource(
                new ResourceSet { Name = "picture", Scopes = new[] { "read", "write" } },
                _grantedToken.AccessToken)
            .ConfigureAwait(false) as Option<AddResourceSetResponse>.Result)!;
        _resourceId = resource.Item.Id;
    }

    [When(@"updating policy")]
    public async Task WhenSettingSettingPolicy()
    {
        var option = await _client.UpdateResource(
            new ResourceSet
            {
                Id = _resourceId,
                Name = "picture",
                Scopes = new[] { "read", "write" },
                AuthorizationPolicies = new[]
                {
                    new PolicyRule
                    {
                        ClientIdsAllowed = new[] { "clientCredentials" },
                        Scopes = new[] { "read" },
                        IsResourceOwnerConsentNeeded = false
                    }
                }
            },
            _grantedToken.AccessToken);

        Assert.IsType<Option<UpdateResourceSetResponse>.Result>(option);
    }

    [When(@"requesting permission")]
    public async Task WhenRequestingPermission()
    {
        var response = await _client.RequestPermission(
                _grantedToken.AccessToken,
                requests: new PermissionRequest { IdToken = _grantedToken.IdToken, ResourceSetId = _resourceId, Scopes = new[] { "read" } })
            .ConfigureAwait(false) as Option<TicketResponse>.Result;

        Assert.NotNull(response);

        _ticketId = response.Item.TicketId;
    }

    [When(@"requesting permissions")]
    public async Task WhenRequestingPermissions()
    {
        var response = await _client.RequestPermission(
                _grantedToken.AccessToken,
                CancellationToken.None,
                new PermissionRequest { ResourceSetId = _resourceId, Scopes = new[] { "write" } },
                new PermissionRequest { ResourceSetId = _resourceId, Scopes = new[] { "read" } })
            .ConfigureAwait(false);

        Assert.IsType<Option<TicketResponse>.Result>(response);

        _ticketId = ((Option<TicketResponse>.Result)response).Item.TicketId;
    }

    [Then(@"returns ticket id")]
    public void ThenReturnsTicketId()
    {
        Assert.NotNull(_ticketId);
    }

    [Then(@"can get access token for resource")]
    public async Task ThenCanGetAccessTokenForResource()
    {
        var option = await _tokenClient.GetToken(
            TokenRequest.FromPassword("administrator", "password", new[] { "uma_protection" }),
            CancellationToken.None);

        switch (option)
        {
            case Option<GrantedTokenResponse>.Result result:
                {
                    var rpt = await _tokenClient.GetToken(TokenRequest.FromTicketId(_ticketId, result.Item.IdToken));

                    Assert.IsType<Option<GrantedTokenResponse>.Result>(rpt);
                    break;
                }
            case Option<GrantedTokenResponse>.Error error:
                Assert.Fail(error.Details.Title);
                break;
        }
    }
}