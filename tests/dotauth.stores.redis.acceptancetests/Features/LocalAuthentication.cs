namespace DotAuth.Stores.Redis.AcceptanceTests.Features;

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using TechTalk.SpecFlow;
using Xunit;

public partial class FeatureTest
{
    private HttpResponseMessage _responseMessage = null!;

    [When(@"logging out")]
    public async Task WhenLoggingOut()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, new Uri($"{BaseUrl}/authenticate/logout"));

        _responseMessage = await _fixture.Client().SendAsync(request).ConfigureAwait(false);
    }

    [Then(@"receives redirect to login page")]
    public void ThenReceivesRedirectToLoginPage()
    {
        Assert.Equal(HttpStatusCode.Redirect, _responseMessage.StatusCode);
    }

    [When(@"posting valid local authorization credentials")]
    public async Task WhenPostingValidLocalAuthorizationCredentials()
    {
        var request = new HttpRequestMessage(HttpMethod.Post, new Uri($"{BaseUrl}/authenticate/locallogin"))
        {
            Content = new FormUrlEncodedContent(
                new[]
                {
                    new KeyValuePair<string, string>("Login", "user"),
                    new KeyValuePair<string, string>("Password", "password"),
                })
        };

        _responseMessage = await _fixture.Client().SendAsync(request).ConfigureAwait(false);
    }

    [Then(@"receives auth cookie")]
    public void ThenReceivesAuthCookie()
    {
        Assert.Equal(HttpStatusCode.Redirect, _responseMessage.StatusCode);
    }

    [When(@"posting invalid local authorization credentials")]
    public async Task WhenPostingInvalidLocalAuthorizationCredentials()
    {
        var request = new HttpRequestMessage(HttpMethod.Post, new Uri($"{BaseUrl}/authenticate/locallogin"))
        {
            Content = new FormUrlEncodedContent(
                new[]
                {
                    new KeyValuePair<string, string>("Login", "blah"),
                    new KeyValuePair<string, string>("Password", "blah"),
                })
        };

        _responseMessage = await _fixture.Client().SendAsync(request).ConfigureAwait(false);

        Assert.NotNull(_responseMessage);
    }

    [Then(@"returns login page")]
    public void ThenReturnsLoginPage()
    {
        Assert.Equal(HttpStatusCode.OK, _responseMessage.StatusCode);
    }
}