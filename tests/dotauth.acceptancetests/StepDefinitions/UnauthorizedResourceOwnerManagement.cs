namespace DotAuth.AcceptanceTests.StepDefinitions;

using System.Net;
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
    private Option<AddResourceOwnerResponse> _addResourceOwnerResponseOption = null!;
    private Option<ResourceOwner[]> _listResourceOwnerResponseOption = null!;
    private Option _option = null!;
    
    [Given(@"an admin token")]
    public async Task GivenAnAdminToken()
    {
        var option = await _tokenClient.GetToken(TokenRequest.FromScopes("admin")).ConfigureAwait(false);
        var result = Assert.IsType<Option<GrantedTokenResponse>.Result>(option);
        Assert.NotNull(result.Item);

        _token = result.Item;
    }

    [When(@"adding resource owner")]
    public async Task WhenAddingResourceOwner()
    {
        _addResourceOwnerResponseOption = await _managerClient.AddResourceOwner(
                new AddResourceOwnerRequest { Password = "test", Subject = "test" },
                _token.AccessToken)
            .ConfigureAwait(false);
    }

    [Then(@"add response has error")]
    public void ThenAddResponseHasError()
    {
        var response = Assert.IsType<Option<AddResourceOwnerResponse>.Error>(_addResourceOwnerResponseOption);
        Assert.Equal(HttpStatusCode.Forbidden, response.Details.Status);
    }

    [When(@"updating resource owner password")]
    public async Task WhenUpdatingResourceOwnerPassword()
    {
        _option = await _managerClient.UpdateResourceOwnerPassword(
                new UpdateResourceOwnerPasswordRequest { Password = "blah", Subject = "administrator" },
                _token.AccessToken)
            .ConfigureAwait(false);
    }

    [Then(@"update response has error")]
    public void ThenUpdateResponseHasError()
    {
        var response = Assert.IsType<Option.Error>(_option);
        Assert.Equal(HttpStatusCode.Forbidden, response.Details.Status);
    }

    [When(@"deleting resource owner")]
    public async Task WhenDeletingResourceOwner()
    {
        _option = await _managerClient.DeleteResourceOwner(
                "administrator",
                _token.AccessToken)
            .ConfigureAwait(false);
    }

    [Then(@"delete response has error")]
    public void ThenDeleteResponseHasError()
    {
        var response = Assert.IsType<Option.Error>(_option);
        Assert.Equal(HttpStatusCode.Forbidden, response.Details.Status);
    }

    [When(@"listing resource owners")]
    public async Task WhenListingResourceOwners()
    {
        _listResourceOwnerResponseOption = await _managerClient.GetAllResourceOwners(_token.AccessToken)
            .ConfigureAwait(false);
    }

    [Then(@"list response has error")]
    public void ThenListResponseHasError()
    {
        var response = Assert.IsType<Option<ResourceOwner[]>.Error>(_listResourceOwnerResponseOption);
        Assert.Equal(HttpStatusCode.Forbidden, response.Details.Status);
    }
}
