namespace SimpleAuth.Stores.Redis.AcceptanceTests.Features
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using Xbehave;
    using Xunit;
    using Xunit.Abstractions;

    public class LocalAuthenticationFeature : AuthFlowFeature
    {
        /// <inheritdoc />
        public LocalAuthenticationFeature(ITestOutputHelper output)
            : base(output)
        {
        }

        [Scenario(DisplayName = "Successful logout")]
        public void SuccessfulLogout()
        {
            HttpResponseMessage result = null!;

            "when logging out".x(
                async () =>
                {
                    var request = new HttpRequestMessage(HttpMethod.Get, new Uri(BaseUrl + "/authenticate/logout"));

                    result = await _fixture.Client().SendAsync(request).ConfigureAwait(false);
                });

            "then receives redirect to login page".x(
                () => { Assert.Equal(HttpStatusCode.Redirect, result.StatusCode); });
        }

        [Scenario(DisplayName = "Valid local login")]
        public void SuccessfulLocalLogin()
        {
            HttpResponseMessage result = null!;

            "when posting valid local authorization credentials".x(
                async () =>
                {
                    var request = new HttpRequestMessage(HttpMethod.Post, new Uri(BaseUrl + "/authenticate/locallogin"))
                    {
                        Content = new FormUrlEncodedContent(
                            new[]
                            {
                                new KeyValuePair<string, string>("Login", "user"),
                                new KeyValuePair<string, string>("Password", "password"),
                            })
                    };

                    result = await _fixture.Client().SendAsync(request).ConfigureAwait(false);
                });

            "then receives auth cookie".x(() => { Assert.Equal(HttpStatusCode.Redirect, result.StatusCode); });
        }

        [Scenario(DisplayName = "Invalid local login")]
        public void InvalidLocalLogin()
        {
            HttpResponseMessage result = null!;

            "when posting invalid local authorization credentials".x(
                async () =>
                {
                    var request = new HttpRequestMessage(HttpMethod.Post, new Uri(BaseUrl + "/authenticate/locallogin"))
                    {
                        Content = new FormUrlEncodedContent(
                            new[]
                            {
                                new KeyValuePair<string, string>("Login", "blah"),
                                new KeyValuePair<string, string>("Password", "blah"),
                            })
                    };

                    result = await _fixture.Client().SendAsync(request).ConfigureAwait(false);

                    Assert.NotNull(result);
                });

            "then receives auth cookie".x(() => { Assert.Equal(HttpStatusCode.OK, result.StatusCode); });
        }
    }
}
