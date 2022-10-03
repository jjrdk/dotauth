namespace DotAuth.AcceptanceTests.Features;

using System;
using System.Net;
using System.Net.Http;
using System.Text;
using DotAuth.Shared.Requests;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;
using Xbehave;
using Xunit;
using Xunit.Abstractions;

public sealed class LocalAuthenticationOpenIdFeature : AuthFlowFeature
{
    /// <inheritdoc />
    public LocalAuthenticationOpenIdFeature(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
    }

    [Scenario(DisplayName = "Invalid open id code")]
    public void InvalidOpenIdCode()
    {
        HttpResponseMessage response = null!;
        IDataProtector dataProtector = null!;

        "and a data protector instance".x(
            () => dataProtector = _fixture.Server.Host.Services.GetRequiredService<IDataProtectionProvider>()
                .CreateProtector("Request"));

        "When posting code to openid authentication".x(
            async () =>
            {
                var authorizationRequest = new AuthorizationRequest {client_id = "client"};
                var code = Uri.EscapeDataString(Protect(dataProtector, authorizationRequest));
                var request = new HttpRequestMessage(HttpMethod.Get, BaseUrl + $"/authenticate/openid?code={code}");
                response = await _fixture.Client().SendAsync(request).ConfigureAwait(false);
            });

        "then response has status code OK".x(() => { Assert.Equal(HttpStatusCode.OK, response.StatusCode); });
    }

    private static string Protect<T>(IDataProtector dataProtector, T toEncode)
        where T : class
    {
        var serialized = Newtonsoft.Json.JsonConvert.SerializeObject(toEncode);

        var bytes = Encoding.ASCII.GetBytes(serialized);
        var protectedBytes = dataProtector.Protect(bytes);
        return Convert.ToBase64String(protectedBytes);
    }
}