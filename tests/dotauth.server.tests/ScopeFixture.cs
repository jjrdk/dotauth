namespace DotAuth.Server.Tests;

using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using DotAuth.Client;
using DotAuth.Properties;
using DotAuth.Shared;
using Xunit;

public sealed class ScopeFixture : IDisposable
{
    private readonly TestManagerServerFixture _server;

    public ScopeFixture()
    {
        _server = new TestManagerServerFixture();
    }

    [Fact]
    public async Task When_Getting_Scopes_As_Html_Then_Razor_View_Is_Returned()
    {
        using var client = _server.Server.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Get, $"http://localhost:5000/{CoreConstants.EndPoints.Scopes}");
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/html"));
        request.Headers.Authorization = new AuthenticationHeaderValue(JwtBearerConstants.BearerScheme, "token");

        var response = await client.SendAsync(request, cancellationToken: TestContext.Current.CancellationToken);
        var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("text/html", response.Content.Headers.ContentType?.MediaType);
        Assert.Contains("Manage Scopes", content);
    }

    [Fact]
    public async Task When_Getting_Scope_As_Html_Then_Razor_View_Is_Returned()
    {
        using var client = _server.Server.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Get, $"http://localhost:5000/{CoreConstants.EndPoints.Scopes}/openid");
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/html"));
        request.Headers.Authorization = new AuthenticationHeaderValue(JwtBearerConstants.BearerScheme, "token");

        var response = await client.SendAsync(request, cancellationToken: TestContext.Current.CancellationToken);
        var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("text/html", response.Content.Headers.ContentType?.MediaType);
        Assert.Contains("openid", content);
    }

    [Fact]
    public async Task When_Getting_Scopes_As_Json_Then_Json_Is_Returned()
    {
        using var client = _server.Server.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Get, $"http://localhost:5000/{CoreConstants.EndPoints.Scopes}");
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        request.Headers.Authorization = new AuthenticationHeaderValue(JwtBearerConstants.BearerScheme, "token");

        var response = await client.SendAsync(request, cancellationToken: TestContext.Current.CancellationToken);
        var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
        Assert.StartsWith("[", content);
    }

    public void Dispose()
    {
        _server.Dispose();
        GC.SuppressFinalize(this);
    }
}



