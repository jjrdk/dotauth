namespace SimpleAuth.Server.Tests.Apis;

using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

public sealed class SessionApiFixture
{
    private const string BaseUrl = "http://localhost:5000";
    private readonly TestOauthServerFixture _server;

    public SessionApiFixture(ITestOutputHelper outputHelper)
    {
        _server = new TestOauthServerFixture(outputHelper);
    }

    [Fact]
    public async Task When_Check_Session_Then_Ok_Is_Returned()
    {
        var httpRequest = new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = new System.Uri($"{BaseUrl}/check_session")
        };

        var httpResult = await _server.Client().SendAsync(httpRequest).ConfigureAwait(false);
        _ = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);

        Assert.Equal(HttpStatusCode.OK, httpResult.StatusCode);
    }
}