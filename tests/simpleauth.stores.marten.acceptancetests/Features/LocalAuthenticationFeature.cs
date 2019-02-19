namespace SimpleAuth.Stores.Marten.AcceptanceTests.Features
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using Microsoft.IdentityModel.Logging;
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
            string connectionString = null;
            TestServerFixture fixture = null;
            HttpResponseMessage result = null;

            "Given an initialized database".x(
                    async () =>
                    {
                        connectionString = await DbInitializer.Init(
                                TestData.ConnectionString,
                                DefaultStores.Consents(),
                                DefaultStores.Users(),
                                DefaultStores.Clients(new SharedContext()))
                            .ConfigureAwait(false);
                    })
                .Teardown(async () => await DbInitializer.Drop(connectionString).ConfigureAwait(false));

            "and a running auth server".x(() => { fixture = new TestServerFixture(connectionString, BaseUrl); })
                .Teardown(() => fixture.Dispose());

            "when logging out".x(
                async () =>
                {
                    var request = new HttpRequestMessage(HttpMethod.Get, new Uri(BaseUrl + "/authenticate/logout"));

                    result = await fixture.Client.SendAsync(request).ConfigureAwait(false);
                });

            "then receives redirect to login page".x(
                () => { Assert.Equal(HttpStatusCode.Redirect, result.StatusCode); });
        }

        [Scenario(DisplayName = "Valid local login")]
        public void SuccessfulLocalLogin()
        {
            string connectionString = null;
            TestServerFixture fixture = null;
            HttpResponseMessage result = null;

            "Given an initialized database".x(
                    async () =>
                    {
                        connectionString = await DbInitializer.Init(
                                TestData.ConnectionString,
                                DefaultStores.Consents(),
                                DefaultStores.Users(),
                                DefaultStores.Clients(new SharedContext()))
                            .ConfigureAwait(false);
                    })
                .Teardown(async () => await DbInitializer.Drop(connectionString).ConfigureAwait(false));

            "and a running auth server".x(() => { fixture = new TestServerFixture(connectionString, BaseUrl); })
                .Teardown(() => fixture.Dispose());

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

                    result = await fixture.Client.SendAsync(request).ConfigureAwait(false);
                });

            "then receives auth cookie".x(() => { Assert.Equal(HttpStatusCode.Redirect, result.StatusCode); });
        }

        [Scenario(DisplayName = "Invalid local login")]
        public void InvalidLocalLogin()
        {
            string connectionString = null;
            TestServerFixture fixture = null;
            HttpResponseMessage result = null;

            "Given an initialized database".x(
                    async () =>
                    {
                        connectionString = await DbInitializer.Init(
                                TestData.ConnectionString,
                                DefaultStores.Consents(),
                                DefaultStores.Users(),
                                DefaultStores.Clients(new SharedContext()))
                            .ConfigureAwait(false);
                    })
                .Teardown(async () => await DbInitializer.Drop(connectionString).ConfigureAwait(false));

            "and a running auth server".x(() => { fixture = new TestServerFixture(connectionString, BaseUrl); })
                .Teardown(() => fixture.Dispose());

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

                    result = await fixture.Client.SendAsync(request).ConfigureAwait(false);
                });

            "then receives auth cookie".x(() => { Assert.Equal(HttpStatusCode.OK, result.StatusCode); });
        }
    }
}
