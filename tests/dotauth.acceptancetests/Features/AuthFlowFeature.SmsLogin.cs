namespace DotAuth.AcceptanceTests.Features;

using System;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;
using DotAuth.Client;
using DotAuth.Extensions;
using DotAuth.Shared;
using DotAuth.Shared.Requests;
using DotAuth.Shared.Responses;
using Microsoft.IdentityModel.Tokens;
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
            .ConfigureAwait(false);

        Assert.IsType<Option.Success>(response);
    }

    [When(@"then requesting token")]
    public async Task WhenThenRequestingToken()
    {
        var option = await _tokenClient
            .GetToken(TokenRequest.FromPassword("phone", "123", new[] {"openid"}, "sms"))
            .ConfigureAwait(false);
        var response = Assert.IsType<Option<GrantedTokenResponse>.Result>(option);
        _token = response.Item;
    }
}