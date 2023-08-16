namespace DotAuth.Stores.Redis.AcceptanceTests.Features;

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using DotAuth.Client;
using DotAuth.Shared;
using DotAuth.Shared.Errors;
using DotAuth.Shared.Requests;
using DotAuth.Shared.Responses;
using Newtonsoft.Json;
using TechTalk.SpecFlow;
using Xunit;

public partial class FeatureTest
{
    private const string ClientId = "device";
    private DiscoveryInformation _doc = null!;
    private ITokenClient _tokenClient = null!;
    private Task<Option<GrantedTokenResponse>> _pollingTask = null!;
    private Option<GrantedTokenResponse> _expiredPoll = null!;
    private DeviceAuthorizationResponse _deviceResponse = null!;

    [Given(@"a device token client")]
    public void GivenADeviceTokenClient()
    {
        _tokenClient = new TokenClient(
            TokenCredentials.AsDevice(),
            _fixture.Client,
            new Uri(FeatureTest.WellKnownOpenidConfiguration));

        Assert.NotNull(_tokenClient);
    }

    [When(@"requesting discovery document")]
    public async Task WhenRequestingDiscoveryDocument()
    {
        var request =
            new HttpRequestMessage
                { Method = HttpMethod.Get, RequestUri = new Uri(FeatureTest.WellKnownOpenidConfiguration) };
        request.Headers.Accept.Clear();
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        var response = await _fixture.Client().SendAsync(request).ConfigureAwait(false);

        var serializedContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

        _doc = JsonConvert.DeserializeObject<DiscoveryInformation>(serializedContent)!;
    }

    [Then(@"discovery document has uri for device authorization")]
    public void ThenDiscoveryDocumentHasUriForDeviceAuthorization()
    {
        Assert.Equal("https://localhost/device_authorization", _doc.DeviceAuthorizationEndPoint.AbsoluteUri);
    }

    [Given(@"an access token")]
    public async Task GivenAnAccessToken()
    {
        var authClient = new TokenClient(
            TokenCredentials.FromClientCredentials(ClientId, "client"),
            _fixture.Client,
            new Uri(FeatureTest.WellKnownOpenidConfiguration));
        var option = await authClient.GetToken(TokenRequest.FromPassword("user", "password", new[] { "openid" }))
            .ConfigureAwait(false);

        var tokenResponse = Assert.IsType<Option<GrantedTokenResponse>.Result>(option);
        _token = tokenResponse.Item;
    }

    [When(@"a device requests authorization")]
    public async Task WhenADeviceRequestsAuthorization()
    {
        var option = await _tokenClient.GetAuthorization(new DeviceAuthorizationRequest(ClientId))
            .ConfigureAwait(false);

        var genericResponse = Assert.IsType<Option<DeviceAuthorizationResponse>.Result>(option);

        _deviceResponse = genericResponse.Item;
    }

    [When(@"the device polls the token server")]
    public void WhenTheDevicePollsTheTokenServer()
    {
        _pollingTask = _tokenClient.GetToken(
            TokenRequest.FromDeviceCode(ClientId, _deviceResponse.DeviceCode, _deviceResponse.Interval));

        Assert.False(_pollingTask.IsCompleted);
    }

    [When(@"user successfully posts user code")]
    public async Task WhenUserSuccessfullyPostsUserCode()
    {
        var client = _fixture.Client();
        var msg = new HttpRequestMessage
        {
            Method = HttpMethod.Post,
            RequestUri = new Uri(_deviceResponse.VerificationUri),
            Content = new FormUrlEncodedContent(
                new[] { new KeyValuePair<string, string>("code", _deviceResponse.UserCode) })
        };
        msg.Headers.Authorization = new AuthenticationHeaderValue(_token.TokenType, _token.AccessToken);

        var approval = await client.SendAsync(msg).ConfigureAwait(false);

        Assert.Equal(HttpStatusCode.OK, approval.StatusCode);
    }

    [Then(@"token is returned from polling")]
    public async Task ThenTokenIsReturnedFromPolling()
    {
        var tokenResponse = await _pollingTask.ConfigureAwait(false);

        Assert.IsType<Option<GrantedTokenResponse>.Result>(tokenResponse);
    }

    [When(@"the device polls the token server too fast")]
    public async Task WhenTheDevicePollsTheTokenServerTooFast()
    {
        var fastPoll = Assert.IsType<Option<GrantedTokenResponse>.Error>(
            await _tokenClient.GetToken(TokenRequest.FromDeviceCode(ClientId, _deviceResponse.DeviceCode, 1))
                .ConfigureAwait(false));

        Assert.NotNull(fastPoll);
        Assert.Equal(ErrorCodes.SlowDown, fastPoll.Details.Title);
    }

    [When(@"the device polls the token server polls properly")]
    public void WhenTheDevicePollsTheTokenServerPollsProperly()
    {
        _pollingTask = _tokenClient.GetToken(
            TokenRequest.FromDeviceCode(ClientId, _deviceResponse.DeviceCode, _deviceResponse.Interval));

        Assert.False(_pollingTask.IsCompleted);
    }

    [When(@"the device polls the token server after expiry")]
    public async Task WhenTheDevicePollsTheTokenServerAfterExpiry()
    {
        _expiredPoll = await _tokenClient.GetToken(TokenRequest.FromDeviceCode(ClientId, _deviceResponse.DeviceCode, 7))
            .ConfigureAwait(false);
    }

    [Then(@"error shows request expiry")]
    public void ThenErrorShowsRequestExpiry()
    {
        var error = Assert.IsType<Option<GrantedTokenResponse>.Error>(_expiredPoll);
        Assert.Equal(ErrorCodes.ExpiredToken, error.Details.Title);
    }
}
