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
/// MCP tools for managing resource owners (users) registered in DotAuth.
/// Passwords are never returned by any tool in this class.
/// Requires the calling user to have the <c>manager</c> scope.
/// </summary>
[McpServerToolType]
public sealed class UserTools(IResourceOwnerStore users)
{
    /// <summary>
    /// Returns all resource owners (users). Password hashes are never included in the response.
    /// </summary>
    /// <example>
    /// <code>list_users()</code>
    /// </example>
    [McpServerTool(Name = "list_users")]
    [Description("Lists all resource owners (users). Password hashes are never returned.")]
    public async Task<string> ListUsers(CancellationToken cancellationToken)
    {
        // IResourceOwnerStore only exposes Get-by-id and Get-by-claim;
        // full listing requires the repository interface — return an informative message when unavailable.
        if (users is IResourceOwnerRepository repo)
        {
            var all = await repo.GetAll(cancellationToken);
            var safe = all.Select(Redact);
            return JsonSerializer.Serialize(safe);
        }

        return "{\"error\":\"Full listing not supported by the current user store.\"}";
    }

    /// <summary>
    /// Returns a single resource owner by subject identifier. Passwords are never returned.
    /// </summary>
    /// <param name="subject">The subject identifier (sub claim) of the user.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <example>
    /// <code>get_user(subject: "alice")</code>
    /// </example>
    [McpServerTool(Name = "get_user")]
    [Description("Returns a single resource owner by subject identifier. Password is never returned.")]
    public async Task<string> GetUser(
        [Description("The subject identifier (sub) of the user to retrieve.")] string subject,
        CancellationToken cancellationToken)
    {
        var user = await users.Get(subject, cancellationToken);
        if (user is null)
        {
            return JsonSerializer.Serialize(new { error = $"User '{subject}' not found" });
        }

        return JsonSerializer.Serialize(Redact(user));
    }

    // Returns a safe view of the resource owner — password is always stripped.
    private static object Redact(ResourceOwner u) => new
    {
        u.Subject,
        u.TwoFactorAuthentication,
        Claims = u.Claims.Select(c => new { c.Type, c.Value })
    };
}
