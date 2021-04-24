namespace SimpleAuth.AcceptanceTests.Features
{
    using System.Net.Http;
    using System.Net.Http.Headers;
    using Xbehave;
    using Xunit;
    using Xunit.Abstractions;

    public class EndpointRequestFeature : AuthorizedManagementFeatureBase
    {
        /// <inheritdoc />
        public EndpointRequestFeature(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
        }

        [Scenario]
        public void CanCallControllerEndpoint()
        {
            string response = null;

            "When calling controller endpoint".x(async () =>
            {
                var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/test");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _administratorToken.AccessToken);
                var responseMessage = await _fixture.Client().SendAsync(request).ConfigureAwait(false);
                response = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
            });

            "Then response is hello".x(() => { Assert.Equal("\"Hello administrator\"", response); });
        }
    }
}