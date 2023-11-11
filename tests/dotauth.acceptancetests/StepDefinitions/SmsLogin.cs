namespace DotAuth.AcceptanceTests.StepDefinitions;

using System;
using System.Threading.Tasks;
using DotAuth.Client;
using DotAuth.Shared;
using DotAuth.Shared.Requests;
using DotAuth.Shared.Responses;
using TechTalk.SpecFlow;
using Xunit;

public partial class FeatureTest
{
    [Given(@"a basic authentication token client with (.+), (.+)")]
    public void GivenABasicAuthenticationTokenClientWith(string clientId, string clientSecret)
    {
        _tokenClient = new TokenClient(
            TokenCredentials.FromBasicAuthentication(clientId, clientSecret),
            _fixture.Client,
            new Uri(WellKnownOpenidConfiguration));
    }

    [When(@"requesting an sms")]
    public async Task WhenRequestingAnSms()
    {
        var response = await _tokenClient.RequestSms(new ConfirmationCodeRequest {PhoneNumber = "phone"})
            ;

        Assert.IsType<Option.Success>(response);
    }

    [When(@"then requesting token")]
    public async Task WhenThenRequestingToken()
    {
        var option = await _tokenClient
            .GetToken(TokenRequest.FromPassword("phone", "123", new[] {"openid"}, "sms"))
            ;
        var response = Assert.IsType<Option<GrantedTokenResponse>.Result>(option);
        _token = response.Item;
    }
}
