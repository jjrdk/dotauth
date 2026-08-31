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

namespace DotAuth.Mcp.Tools;

using System.ComponentModel;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DotAuth.Shared.Models;
using DotAuth.Shared.Repositories;
using ModelContextProtocol.Server;

/// <summary>
/// MCP tools for managing OAuth 2.0 clients registered in DotAuth.
/// Requires the calling user to have the <c>manager</c> scope.
/// </summary>
[McpServerToolType]
public sealed class ClientTools(IClientStore clients)
{
    /// <summary>
    /// Returns all registered OAuth clients (client_id and client_name only; secrets are omitted).
    /// </summary>
    /// <example>
    /// <code>list_clients()</code>
    /// </example>
    [McpServerTool(Name = "list_clients")]
    [Description("Lists all registered OAuth 2.0 clients (client_id and name). Secrets are never returned.")]
    public async Task<string> ListClients(CancellationToken cancellationToken)
    {
        var all = await clients.GetAll(cancellationToken);
        var summary = all.Select(c => new { c.ClientId, c.ClientName });
        return JsonSerializer.Serialize(summary);
    }

    /// <summary>
    /// Returns the full configuration for a single OAuth client identified by <paramref name="clientId"/>.
    /// Secrets and cryptographic key material are omitted from the result.
    /// </summary>
    /// <param name="clientId">The unique client identifier to look up.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <example>
    /// <code>get_client(clientId: "my-client")</code>
    /// </example>
    [McpServerTool(Name = "get_client")]
    [Description("Returns the configuration of a single OAuth 2.0 client by client_id. Secrets are never returned.")]
    public async Task<string> GetClient(
        [Description("The client_id to retrieve.")] string clientId,
        CancellationToken cancellationToken)
    {
        var client = await clients.GetById(clientId, cancellationToken);
        if (client is null)
        {
            return $"{{\"error\":\"Client '{clientId}' not found\"}}";
        }

        // Return the client without secret material.
        var safe = RedactSecrets(client);
        return JsonSerializer.Serialize(safe);
    }

    // Returns a copy of the client with secrets cleared so they are never transmitted over MCP.
    private static object RedactSecrets(Client c) => new
    {
        c.ClientId,
        c.ClientName,
        c.AllowedScopes,
        c.GrantTypes,
        c.RedirectionUrls,
        c.ApplicationType,
        c.TokenEndPointAuthMethod,
        c.IdTokenSignedResponseAlg,
        c.RequirePkce
    };
}
