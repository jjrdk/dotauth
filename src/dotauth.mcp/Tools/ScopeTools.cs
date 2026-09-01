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
using DotAuth.Shared.Repositories;
using ModelContextProtocol.Server;

/// <summary>
/// MCP tools for managing OAuth 2.0 scopes registered in DotAuth.
/// Requires the calling user to have the <c>manager</c> scope.
/// </summary>
[McpServerToolType]
public sealed class ScopeTools(IScopeStore scopes)
{
    /// <summary>
    /// Returns all registered OAuth 2.0 scopes.
    /// </summary>
    /// <example>
    /// <code>list_scopes()</code>
    /// </example>
    [McpServerTool(Name = "list_scopes")]
    [Description("Lists all registered OAuth 2.0 scopes with their name, description, and type.")]
    public async Task<string> ListScopes(CancellationToken cancellationToken)
    {
        var all = await scopes.GetAll(cancellationToken);
        return JsonSerializer.Serialize(all);
    }

    /// <summary>
    /// Returns a single OAuth 2.0 scope by name.
    /// </summary>
    /// <param name="name">The exact scope name to retrieve.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <example>
    /// <code>get_scope(name: "openid")</code>
    /// </example>
    [McpServerTool(Name = "get_scope")]
    [Description("Returns a single OAuth 2.0 scope by its name.")]
    public async Task<string> GetScope(
        [Description("The exact scope name to retrieve.")] string name,
        CancellationToken cancellationToken)
    {
        var scope = await scopes.Get(name, cancellationToken);
        if (scope is null)
        {
            return JsonSerializer.Serialize(new { error = $"Scope '{name}' not found" });
        }

        return JsonSerializer.Serialize(scope);
    }
}
