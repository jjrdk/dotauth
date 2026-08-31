// Copyright © 2018 Jacob Reimers
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

namespace DotAuth.Mcp.Tests;

using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Xunit;

/// <summary>
/// Tests that the /mcp endpoint honours the security requirements:
/// - No token → 401 Unauthorized
/// - Valid token without "manager" scope → 403 Forbidden
/// - Valid token with "manager" scope → connection accepted (2xx or upgrade)
/// </summary>
public sealed class McpHostTests : IDisposable
{
    private readonly McpHostFixture _fixture;

    public McpHostTests()
    {
        _fixture = new McpHostFixture();
    }

    [Fact]
    public async Task When_no_token_then_401_is_returned()
    {
        using var client = _fixture.Server.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Post, "http://localhost/mcp");
        request.Content = new StringContent("{}", System.Text.Encoding.UTF8, "application/json");

        var response = await client.SendAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task When_token_lacks_manager_scope_then_403_is_returned()
    {
        using var client = _fixture.Server.CreateClient();
        var token = McpHostFixture.CreateBearerToken("openid profile");
        using var request = new HttpRequestMessage(HttpMethod.Post, "http://localhost/mcp");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Content = new StringContent("{}", System.Text.Encoding.UTF8, "application/json");

        var response = await client.SendAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task When_token_has_manager_scope_then_mcp_endpoint_is_reachable()
    {
        using var client = _fixture.Server.CreateClient();
        var token = McpHostFixture.CreateBearerToken("openid manager");
        using var request = new HttpRequestMessage(HttpMethod.Post, "http://localhost/mcp");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        // Send a minimal JSON-RPC initialize request so the MCP server can respond normally.
        request.Content = new StringContent(
            """{"jsonrpc":"2.0","id":1,"method":"initialize","params":{"protocolVersion":"2024-11-05","capabilities":{},"clientInfo":{"name":"test","version":"1.0"}}}""",
            System.Text.Encoding.UTF8,
            "application/json");

        var response = await client.SendAsync(request, TestContext.Current.CancellationToken);

        // The MCP framework may respond 200 (SSE) or 4xx for invalid negotiation;
        // crucially it must NOT respond 401 or 403.
        Assert.NotEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.NotEqual(HttpStatusCode.Forbidden, response.StatusCode);
    }

    public void Dispose()
    {
        _fixture.Dispose();
    }
}
