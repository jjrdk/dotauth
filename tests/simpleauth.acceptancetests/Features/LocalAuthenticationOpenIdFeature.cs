namespace SimpleAuth.AcceptanceTests.Features
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using Microsoft.AspNetCore.DataProtection;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.IdentityModel.Logging;
    using SimpleAuth.Shared.Requests;
    using Xbehave;
    using Xunit;

    public class LocalAuthenticationOpenIdFeature
    {
        private const string BaseUrl = "http://localhost:5000";

        public LocalAuthenticationOpenIdFeature()
        {
            IdentityModelEventSource.ShowPII = true;
        }

        [Scenario(DisplayName = "Invalid open id code")]
        public void InvalidOpenIdCode()
        {
            TestServerFixture fixture = null;
            HttpResponseMessage response = null;
            IDataProtector dataProtector = null;

            "Given a running auth server".x(() => fixture = new TestServerFixture(BaseUrl))
                .Teardown(() => fixture.Dispose());

            "and a data protector instance".x(
                () => dataProtector = fixture.Server.Host.Services.GetService<IDataProtectionProvider>().CreateProtector("Request"));

            "When posting code to openid authentication".x(
                async () =>
                {
                    var authorizationRequest = new AuthorizationRequest
                    {
                        client_id = "client"
                    };
                    var code = Uri.EscapeUriString(Protect(dataProtector, authorizationRequest));
                    var request = new HttpRequestMessage(HttpMethod.Get, BaseUrl + $"/authenticate/openid?code={code}");
                    response = await fixture.Client.SendAsync(request).ConfigureAwait(false);
                });

            "then response has status code OK".x(() => { Assert.Equal(HttpStatusCode.OK, response.StatusCode); });
        }

        private static string Protect<T>(IDataProtector dataProtector, T toEncode) where T : class
        {
            var serialized = Newtonsoft.Json.JsonConvert.SerializeObject(toEncode);

            var bytes = Encoding.ASCII.GetBytes(serialized);
            var protectedBytes = dataProtector.Protect(bytes);
            return Convert.ToBase64String(protectedBytes);
        }
    }
}