namespace SimpleAuth.AcceptanceTests.Features
{
    using System;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using Newtonsoft.Json;
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
    }
}
