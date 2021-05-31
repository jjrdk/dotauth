namespace SimpleAuth.AcceptanceTests.Features
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;
    using Newtonsoft.Json;
    using SimpleAuth.Client;
    using SimpleAuth.Shared;
    using SimpleAuth.Shared.Errors;
    using SimpleAuth.Shared.Requests;
    using SimpleAuth.Shared.Responses;
    using Xbehave;
    using Xunit;
    using Xunit.Abstractions;

    public class DeviceAuthorizationFeature : AuthFlowFeature
    {
        /// <inheritdoc />
        public DeviceAuthorizationFeature(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
        }

        [Scenario(DisplayName = "Can get device authorization endpoint from discovery document")]
        public void CanGetDeviceAuthorizationEndpointFromDiscoveryDocument()
        {
            DiscoveryInformation doc = null;

            "When requesting discovery document".x(
                async () =>
                {
                    var request =
                        new HttpRequestMessage { Method = HttpMethod.Get, RequestUri = new Uri(WellKnownOpenidConfiguration) };
                    request.Headers.Accept.Clear();
                    request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    var response = await _fixture.Client().SendAsync(request).ConfigureAwait(false);

                    var serializedContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                    doc = JsonConvert.DeserializeObject<DiscoveryInformation>(serializedContent)!;
                });

            "Then discovery document has uri for device authorization".x(
                () =>
                {
                    Assert.Equal("https://localhost/device_authorization", doc.DeviceAuthorizationEndPoint.AbsoluteUri);
                });
        }

        [Scenario(DisplayName = "Can authorize device with user approval")]
        public void ExecuteDeviceAuthorizationFlowWithUserApproval()
        {
            const string clientId = "device";
            ITokenClient tokenClient = null;
            DeviceAuthorizationResponse response = null;
            GrantedTokenResponse token = null;
            Task<Option<GrantedTokenResponse>> pollingTask = null;

            "Given a token client".x(
                () =>
                {
                    tokenClient = new TokenClient(
                        TokenCredentials.AsDevice(),
                        _fixture.Client,
                        new Uri(WellKnownOpenidConfiguration));

                    Assert.NotNull(tokenClient);
                });

            "and an access token".x(
                async () =>
                {
                    var authClient = new TokenClient(
                        TokenCredentials.FromClientCredentials(clientId, "client"),
                        _fixture.Client,
                        new Uri(WellKnownOpenidConfiguration));
                    var tokenResponse = await authClient.GetToken(
                            TokenRequest.FromPassword("user", "password", new[] { "openid" }))
                        .ConfigureAwait(false);

                    Assert.IsType<Option<GrantedTokenResponse>.Result>(tokenResponse);

                    token = (tokenResponse as Option<GrantedTokenResponse>.Result).Item;
                });

            "When a device requests authorization".x(
                async () =>
                {
                    var genericResponse = await tokenClient.GetAuthorization(new DeviceAuthorizationRequest(clientId))
                        .ConfigureAwait(false);

                    Assert.IsType<Option<DeviceAuthorizationResponse>.Result>(genericResponse);

                    response = (genericResponse as Option<DeviceAuthorizationResponse>.Result).Item;
                });

            "and the device polls the token server".x(
                async () =>
                {
                    pollingTask = tokenClient.GetToken(
                        TokenRequest.FromDeviceCode(clientId, response.DeviceCode, response.Interval));

                    Assert.False(pollingTask.IsCompleted);
                });

            "and user successfully posts user code".x(
                async () =>
                {
                    var client = _fixture.Client();
                    var msg = new HttpRequestMessage
                    {
                        Method = HttpMethod.Post,
                        RequestUri = new Uri(response.VerificationUri),
                        Content = new FormUrlEncodedContent(
                            new[] { new KeyValuePair<string, string>("code", response.UserCode) })
                    };
                    msg.Headers.Authorization = new AuthenticationHeaderValue(token.TokenType, token.AccessToken);

                    var approval = await client.SendAsync(msg).ConfigureAwait(false);

                    Assert.Equal(HttpStatusCode.OK, approval.StatusCode);
                });

            "then token is returned from polling".x(
                async () =>
                {
                    var tokenResponse = await pollingTask.ConfigureAwait(false);

                    Assert.IsType<Option<GrantedTokenResponse>.Result>(tokenResponse);
                });
        }

        [Scenario(DisplayName = "Can authorize device with user approval when polled too fast")]
        public void ExecuteDeviceAuthorizationFlowWithUserApprovalWhenPolledTooFast()
        {
            const string clientId = "device";
            ITokenClient tokenClient = null;
            DeviceAuthorizationResponse response = null;
            GrantedTokenResponse token = null;
            Task<Option<GrantedTokenResponse>> pollingTask = null;

            "Given a token client".x(
                () =>
                {
                    tokenClient = new TokenClient(
                        TokenCredentials.AsDevice(),
                        _fixture.Client,
                        new Uri(WellKnownOpenidConfiguration));

                    Assert.NotNull(tokenClient);
                });

            "and an access token".x(
                async () =>
                {
                    var authClient = new TokenClient(
                        TokenCredentials.FromClientCredentials(clientId, "client"),
                        _fixture.Client,
                        new Uri(WellKnownOpenidConfiguration));
                    var tokenResponse = await authClient.GetToken(
                            TokenRequest.FromPassword("user", "password", new[] { "openid" }))
                        .ConfigureAwait(false) as Option<GrantedTokenResponse>.Result;

                    Assert.NotNull(tokenResponse);

                    token = tokenResponse.Item;
                });

            "When a device requests authorization".x(
                async () =>
                {
                    var genericResponse = await tokenClient.GetAuthorization(new DeviceAuthorizationRequest(clientId))
                        .ConfigureAwait(false) as Option<DeviceAuthorizationResponse>.Result;

                    Assert.NotNull(genericResponse);

                    response = genericResponse.Item;
                });

            "and the device polls the token server too fast".x(
                async () =>
                {
                    var fastPoll = await tokenClient.GetToken(
                            TokenRequest.FromDeviceCode(clientId, response.DeviceCode, 1))
                        .ConfigureAwait(false) as Option<GrantedTokenResponse>.Error;

                    Assert.NotNull(fastPoll);
                    Assert.Equal(ErrorCodes.SlowDown, fastPoll.Details.Title);
                });

            "and the device polls the token server polls properly".x(
                async () =>
                {
                    pollingTask = tokenClient.GetToken(
                        TokenRequest.FromDeviceCode(clientId, response.DeviceCode, response.Interval));

                    Assert.False(pollingTask.IsCompleted);
                });

            "and user successfully posts user code".x(
                async () =>
                {
                    var client = _fixture.Client();
                    var msg = new HttpRequestMessage
                    {
                        Method = HttpMethod.Post,
                        RequestUri = new Uri(response.VerificationUri),
                        Content = new FormUrlEncodedContent(
                            new[] { new KeyValuePair<string, string>("code", response.UserCode) })
                    };
                    msg.Headers.Authorization = new AuthenticationHeaderValue(token.TokenType, token.AccessToken);

                    var approval = await client.SendAsync(msg).ConfigureAwait(false);

                    Assert.Equal(HttpStatusCode.OK, approval.StatusCode);
                });

            "then token is returned from polling".x(
                async () =>
                {
                    var tokenResponse = await pollingTask.ConfigureAwait(false);

                    Assert.IsType<Option<GrantedTokenResponse>.Result>(tokenResponse);
                });
        }

        [Scenario(DisplayName = "Polling after expiry gets error")]
        public void ExecuteDeviceAuthorizationAfterExpiry()
        {
            const string clientId = "device";
            ITokenClient tokenClient = null;
            DeviceAuthorizationResponse response = null;

            "Given a token client".x(
                () =>
                {
                    tokenClient = new TokenClient(
                        TokenCredentials.AsDevice(),
                        _fixture.Client,
                        new Uri(WellKnownOpenidConfiguration));

                    Assert.NotNull(tokenClient);
                });

            "When a device requests authorization".x(
                async () =>
                {
                    var genericResponse = await tokenClient.GetAuthorization(new DeviceAuthorizationRequest(clientId))
                        .ConfigureAwait(false) as Option<DeviceAuthorizationResponse>.Result;

                    Assert.NotNull(genericResponse);

                    response = genericResponse.Item;
                });

            Option<GrantedTokenResponse> expiredPoll = null;

            "and the device polls the token server after expiry".x(
                async () =>
                {
                    expiredPoll = await tokenClient.GetToken(
                            TokenRequest.FromDeviceCode(clientId, response.DeviceCode, 7))
                        .ConfigureAwait(false);
                });

            "then error shows request expiry".x(
                async () =>
                {
                    Assert.IsType<Option<GrantedTokenResponse>.Error>(expiredPoll);
                    Assert.Equal(
                        ErrorCodes.ExpiredToken,
                        (expiredPoll as Option<GrantedTokenResponse>.Error).Details.Title);
                });
        }
    }
}
