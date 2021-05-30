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
            Task<GenericResponse<GrantedTokenResponse>> pollingTask = null;

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
                            TokenRequest.FromPassword("user", "password", new[] {"openid"}))
                        .ConfigureAwait(false);

                    Assert.False(tokenResponse.HasError);

                    token = tokenResponse.Content;
                });

            "When a device requests authorization".x(
                async () =>
                {
                    var genericResponse = await tokenClient.GetAuthorization(new DeviceAuthorizationRequest(clientId))
                        .ConfigureAwait(false);

                    Assert.False(genericResponse.HasError);

                    response = genericResponse.Content;
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
                            new[] {new KeyValuePair<string, string>("code", response.UserCode)})
                    };
                    msg.Headers.Authorization = new AuthenticationHeaderValue(token.TokenType, token.AccessToken);

                    var approval = await client.SendAsync(msg).ConfigureAwait(false);

                    Assert.Equal(HttpStatusCode.OK, approval.StatusCode);
                });

            "then token is returned from polling".x(
                async () =>
                {
                    var tokenResponse = await pollingTask.ConfigureAwait(false);

                    Assert.False(tokenResponse.HasError);
                });
        }
    }
}
