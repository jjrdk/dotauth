namespace DotAuth.AcceptanceTests.StepDefinitions;

using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using DotAuth.Shared.Requests;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;
using TechTalk.SpecFlow;
using Xunit;

public partial class FeatureTest
{
    private IDataProtector _dataProtector = null!;
    
    [Given(@"a data protector instance")]
    public void GivenADataProtectorInstance()
    {
        _dataProtector = _fixture.Server.Host.Services
            .GetRequiredService<IDataProtectionProvider>()
            .CreateProtector("Request");
    }

    [When(@"posting code to openid authentication")]
    public async Task WhenPostingCodeToOpenidAuthentication()
    {
        var authorizationRequest = new AuthorizationRequest {client_id = "client"};
        var code = Uri.EscapeDataString(Protect(_dataProtector, authorizationRequest));
        var request = new HttpRequestMessage(HttpMethod.Get, $"{BaseUrl}/authenticate/openid?code={code}");
        _responseMessage = await _fixture.Client().SendAsync(request).ConfigureAwait(false);
    }
    
    private static string Protect<T>(IDataProtector dataProtector, T toEncode)
        where T : class
    {
        var serialized = Newtonsoft.Json.JsonConvert.SerializeObject(toEncode);

        var bytes = Encoding.ASCII.GetBytes(serialized);
        var protectedBytes = dataProtector.Protect(bytes);
        return Convert.ToBase64String(protectedBytes);
    }

    [Then(@"response has status code OK")]
    public void ThenResponseHasStatusCodeOk()
    {
        Assert.Equal(HttpStatusCode.OK, _responseMessage.StatusCode);
    }
}