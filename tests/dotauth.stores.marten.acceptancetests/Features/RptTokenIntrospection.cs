namespace DotAuth.Stores.Marten.AcceptanceTests.Features;

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
    private string _rptToken = null!;

    [Given(@"a PAT token")]
    public async Task GivenAPatToken()
    {
        var option = await _tokenClient.GetToken(
                TokenRequest.FromPassword("administrator", "password", ["uma_protection"]))
            .ConfigureAwait(false);

        var response = Assert.IsType<Option<GrantedTokenResponse>.Result>(option);
        Assert.NotNull(response.Item.AccessToken);
        Assert.NotNull(response.Item.IdToken);

        _token = response.Item;
    }

    [Given(@"a registered resource")]
    public async Task GivenARegisteredResource()
    {
        var resourceSet = new ResourceSet
        {
            Description = "Test resource", Name = "Test resource", Scopes = ["read"], Type = "Test resource"
        };
        var response =
            await _umaClient.AddResourceSet(resourceSet, _token.AccessToken).ConfigureAwait(false) as
                Option<AddResourceSetResponse>.Result;

        Assert.NotNull(response);

        _resourceId = response.Item.Id;
    }

    [Given(@"an updated authorization policy")]
    public async Task GivenAnUpdatedAuthorizationPolicy()
    {
        var resourceSet = new ResourceSet
        {
            Id = _resourceId,
            AuthorizationPolicies =
            [
                new PolicyRule
                    {
                        ClientIdsAllowed = ["clientCredentials"], Scopes = ["read"]
                    }
            ],
            Description = "Test resource",
            Name = "Test resource",
            Scopes = ["read"],
            Type = "Test resource"
        };
        var response =
            await _umaClient.UpdateResourceSet(resourceSet, _token.AccessToken).ConfigureAwait(false) as
                Option<UpdateResourceSetResponse>.Result;

        Assert.NotNull(response);

        _resourceId = response.Item.Id;
    }

    [When(@"getting a ticket")]
    public async Task WhenGettingATicket()
    {
        var ticketResponse = Assert.IsType<Option<TicketResponse>.Result>( await _umaClient.RequestPermission(
                _token.AccessToken,
                requests: new PermissionRequest { ResourceSetId = _resourceId, Scopes = ["read"] })
            .ConfigureAwait(false) );

        Assert.NotNull(ticketResponse);

        _ticketId = ticketResponse.Item.TicketId;
    }

    [When(@"getting an RPT token")]
    public async Task WhenGettingAnRptToken()
    {
        var rptResponse = Assert.IsType<Option<GrantedTokenResponse>.Result>(
            await _tokenClient.GetToken(TokenRequest.FromTicketId(_ticketId, _token.IdToken!))
            .ConfigureAwait(false) );

        Assert.NotNull(rptResponse);

        _rptToken = rptResponse.Item.AccessToken;
    }

    [Then(@"can introspect RPT token using PAT token as authentication")]
    public async Task ThenCanIntrospectRptTokenUsingPatTokenAsAuthentication()
    {
        var introspectResult = await _umaClient
            .Introspect(DotAuth.Client.IntrospectionRequest.Create(_rptToken, "access_token", _token.AccessToken))
            .ConfigureAwait(false);

        Assert.IsType<Option<UmaIntrospectionResponse>.Result>(introspectResult);
    }
}
