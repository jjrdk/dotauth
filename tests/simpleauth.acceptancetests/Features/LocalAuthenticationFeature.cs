namespace SimpleAuth.AcceptanceTests.Features
{
    using Microsoft.IdentityModel.Logging;
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using Xbehave;
    using Xunit;

    public class LocalAuthenticationFeature
    {
        private const string BaseUrl = "http://localhost:5000";

        public LocalAuthenticationFeature()
        {
            IdentityModelEventSource.ShowPII = true;
        }

        [Scenario]
        public void SuccessfulLogout()
        {
            TestServerFixture fixture = null;
            HttpResponseMessage result = null;

            "Given a running auth server".x(() => fixture = new TestServerFixture(BaseUrl))
                .Teardown(() => fixture.Dispose());

            "when logging out".x(
                async () =>
                {
                    var request = new HttpRequestMessage(HttpMethod.Get, new Uri(BaseUrl + "/authenticate/logout"));

                    result = await fixture.Client.SendAsync(request).ConfigureAwait(false);
                });

            "then receives redirect to login page".x(() => { Assert.Equal(HttpStatusCode.Redirect, result.StatusCode); });
        }

        [Scenario(DisplayName = "Valid local login")]
        public void SuccessfulLocalLogin()
        {
            TestServerFixture fixture = null;
            HttpResponseMessage result = null;

            "Given a running auth server".x(() => fixture = new TestServerFixture(BaseUrl))
                .Teardown(() => fixture.Dispose());

            "when posting valid local authorization credentials".x(
                async () =>
                {
                    var request = new HttpRequestMessage(HttpMethod.Post, new Uri(BaseUrl + "/authenticate/locallogin"))
                    {
                        Content = new FormUrlEncodedContent(new[]
                        {
                            new KeyValuePair<string, string>("Login", "user"),
                            new KeyValuePair<string, string>("Password", "password"),
                        })
                    };

                    result = await fixture.Client.SendAsync(request).ConfigureAwait(false);
                });

            "then receives auth cookie".x(() => { Assert.Equal(HttpStatusCode.Redirect, result.StatusCode); });
        }

        [Scenario(DisplayName = "Invalid local login")]
        public void InvalidLocalLogin()
        {
            TestServerFixture fixture = null;
            HttpResponseMessage result = null;

            "Given a running auth server".x(() => fixture = new TestServerFixture(BaseUrl))
                .Teardown(() => fixture.Dispose());

            "when posting invalid local authorization credentials".x(
                async () =>
                {
                    var request = new HttpRequestMessage(HttpMethod.Post, new Uri(BaseUrl + "/authenticate/locallogin"))
                    {
                        Content = new FormUrlEncodedContent(new[]
                        {
                            new KeyValuePair<string, string>("Login", "blah"),
                            new KeyValuePair<string, string>("Password", "blah"),
                        })
                    };

                    result = await fixture.Client.SendAsync(request).ConfigureAwait(false);
                });

            "then receives auth cookie".x(() => { Assert.Equal(HttpStatusCode.OK, result.StatusCode); });
        }
    }
}