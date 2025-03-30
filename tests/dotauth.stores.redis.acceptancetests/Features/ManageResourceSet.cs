namespace DotAuth.Stores.Redis.AcceptanceTests.Features;

using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using DotAuth.Client;
using DotAuth.Shared;
using DotAuth.Shared.Models;
using DotAuth.Shared.Responses;

using TechTalk.SpecFlow;
using Xunit;

public partial class FeatureTest
{
    AddResourceSetResponse _resourceSetResponse = null!;
    EditPolicyResponse _policyRules = null!;

    [Given(@"a UMA client")]
    public void GivenAUmaClient()
    {
        _umaClient = new UmaClient(_fixture.Client, new Uri(BaseUrl));
    }

    [When(@"getting a PAT token")]
    public async Task WhenGettingAPatToken()
    {
        var option = await _tokenClient.GetToken(
                TokenRequest.FromPassword("administrator", "password", ["uma_protection"]))
            .ConfigureAwait(false);

        var tokenResponse = Assert.IsType<Option<GrantedTokenResponse>.Result>(option);

        _token = tokenResponse.Item;

        Assert.NotNull(_token);
    }

    [Then(@"can register a resource")]
    public async Task ThenCanRegisterAResource()
    {
        var resource = new ResourceSet
        {
            AuthorizationPolicies =
            [
                new PolicyRule
                {
                    ClientIdsAllowed = ["clientCredentials"],
                    IsResourceOwnerConsentNeeded = true,
                    Scopes = ["read"]
                }
            ],
            Name = "test resource",
            Scopes = ["read"],
            Type = "test"
        };
        var option = await _umaClient.AddResourceSet(resource, _token.AccessToken).ConfigureAwait(false);

        var response = Assert.IsType<Option<AddResourceSetResponse>.Result>(option);

        Assert.NotNull(response);

        _resourceSetResponse = response.Item;
    }

    [Then(@"can view resource policies")]
    public async Task ThenCanViewResourcePolicies()
    {
        var msg = new HttpRequestMessage
        {
            Method = HttpMethod.Get, RequestUri = new Uri(_resourceSetResponse.UserAccessPolicyUri)
        };
        msg.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        msg.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token.AccessToken);

        var policyResponse = await _fixture.Client().SendAsync(msg).ConfigureAwait(false);

        Assert.True(policyResponse.IsSuccessStatusCode);

        var content = await policyResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
        _policyRules = JsonSerializer.Deserialize<EditPolicyResponse>(content, DefaultJsonSerializerOptions.Instance)!;

        Assert.Single(_policyRules.Rules);
    }

    [Then(@"can update resource policies")]
    public async Task ThenCanUpdateResourcePolicies()
    {
        _policyRules.Rules[0] = _policyRules.Rules[0] with { IsResourceOwnerConsentNeeded = false };

        var msg = new HttpRequestMessage
        {
            Method = HttpMethod.Put,
            RequestUri = new Uri(_resourceSetResponse.UserAccessPolicyUri),
            Content = new StringContent(JsonSerializer.Serialize(_policyRules, DefaultJsonSerializerOptions.Instance))
        };
        msg.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token.AccessToken);

        var policyResponse = await _fixture.Client().SendAsync(msg).ConfigureAwait(false);

        Assert.True(policyResponse.IsSuccessStatusCode);
    }
}
