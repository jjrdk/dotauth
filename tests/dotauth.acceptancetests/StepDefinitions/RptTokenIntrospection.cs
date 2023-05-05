namespace DotAuth.AcceptanceTests.StepDefinitions;

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
    private string? _rptToken;

    [Given(@"a PAT token")]
    public async Task GivenAPatToken()
    {
        var option = await _tokenClient.GetToken(
                TokenRequest.FromPassword("administrator", "password", new[] { "uma_protection" }))
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
            Description = "Test resource", Name = "Test resource", Scopes = new[] { "read" }, Type = "Test resource"
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
                new[]
                {
                    new PolicyRule
                    {
                        ClientIdsAllowed = new[] { "clientCredentials" }, Scopes = new[] { "read" }
                    }
                },
            Description = "Test resource",
            Name = "Test resource",
            Scopes = new[] { "read" },
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
        var ticketResponse = await _umaClient.RequestPermission(
                _token.AccessToken,
                requests: new PermissionRequest { ResourceSetId = _resourceId, Scopes = new[] { "read" } })
            .ConfigureAwait(false) as Option<TicketResponse>.Result;

        Assert.NotNull(ticketResponse);

        _ticketId = ticketResponse.Item.TicketId;
    }

    [When(@"getting an RPT token")]
    public async Task WhenGettingAnRptToken()
    {
        var rptResponse = await _tokenClient.GetToken(TokenRequest.FromTicketId(_ticketId, _token.IdToken!))
            .ConfigureAwait(false) as Option<GrantedTokenResponse>.Result;

        Assert.NotNull(rptResponse);

        _rptToken = rptResponse.Item.AccessToken;
    }

    [Then(@"can introspect RPT token using PAT token as authentication")]
    public async Task ThenCanIntrospectRptTokenUsingPatTokenAsAuthentication()
    {
        var introspectResult = await _umaClient
            .Introspect(DotAuth.Client.IntrospectionRequest.Create(_rptToken!, "access_token", _token.AccessToken))
            .ConfigureAwait(false);

        Assert.IsType<Option<UmaIntrospectionResponse>.Result>(introspectResult);
    }
}
