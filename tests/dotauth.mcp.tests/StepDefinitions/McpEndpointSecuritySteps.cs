namespace DotAuth.Mcp.Tests.StepDefinitions;

using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using DotAuth.Mcp.Tests.Support;
using Reqnroll;
using Xunit;

public partial class FeatureTest
{
    private string? _authorizationToken;

    [Given(@"no authorization token")]
    public void GivenNoAuthorizationToken()
    {
        _authorizationToken = null;
    }

    [Given(@"a bearer token with scope ""(.+)""")]
    public void GivenABearerTokenWithScope(string scope)
    {
        _authorizationToken = McpServerFixture.CreateBearerToken(scope);
    }

    [When(@"a POST request is sent to the MCP endpoint")]
    public async Task WhenAPostRequestIsSentToTheMcpEndpoint()
    {
        _fixture ??= new McpServerFixture();
        using var client = _fixture.Server.CreateClient();

        using var request = new HttpRequestMessage(HttpMethod.Post, "http://localhost/mcp");
        if (_authorizationToken is not null)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _authorizationToken);
        }

        // Minimal JSON body so the transport can parse the request.
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        request.Content = new StringContent(
            """{"jsonrpc":"2.0","id":1,"method":"initialize","params":{"protocolVersion":"2024-11-05","capabilities":{},"clientInfo":{"name":"test","version":"1.0"}}}""",
            Encoding.UTF8,
            "application/json");

        _response = await client.SendAsync(request, TestContext.Current.CancellationToken);
    }

    [Then(@"the response status is (\d+)")]
    public void ThenTheResponseStatusIs(int statusCode)
    {
        Assert.NotNull(_response);
        Assert.Equal((HttpStatusCode)statusCode, _response.StatusCode);
    }

    [Then(@"the response status is not 401 or 403")]
    public void ThenTheResponseStatusIsNotUnauthorizedOrForbidden()
    {
        Assert.NotNull(_response);
        Assert.NotEqual(HttpStatusCode.Unauthorized, _response.StatusCode);
        Assert.NotEqual(HttpStatusCode.Forbidden, _response.StatusCode);
    }
}
