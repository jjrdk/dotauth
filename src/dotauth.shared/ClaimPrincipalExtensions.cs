// Copyright © 2015 Habart Thierry, © 2018 Jacob Reimers
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

namespace DotAuth.Shared;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using DotAuth.Shared.Models;

/// <summary>
/// Defines the ClaimPrincipal extensions.
/// </summary>
public static class ClaimPrincipalExtensions
{
    /// <summary>
    /// Tries to get the ticket lines from the current user claims.
    /// </summary>
    /// <param name="identity">The user as a <see cref="ClaimsIdentity"/> instance.</param>
    /// <param name="tickets">The found array of <see cref="TicketLine"/>. If none are found, then returns an empty array.
    /// If no user is found then returns <c>null</c>.</param>
    /// <returns><c>true</c> if any tickets are found, otherwise <c>false</c>.</returns>
    public static bool TryGetUmaTickets(this ClaimsIdentity identity, out Permission[] tickets)
    {
        return identity.Claims.TryGetUmaTickets(out tickets);
    }

    /// <summary>
    /// Tries to get the ticket lines from the current user claims.
    /// </summary>
    /// <param name="claims">The user claims.</param>
    /// <param name="tickets">The found array of <see cref="TicketLine"/>. If none are found, then returns an empty array.
    /// If no user is found then returns <c>null</c>.</param>
    /// <returns><c>true</c> if any tickets are found, otherwise <c>false</c>.</returns>
    public static bool TryGetUmaTickets(this IEnumerable<Claim> claims, out Permission[] tickets)
    {
        tickets = Array.Empty<Permission>();

        try
        {
            tickets = claims.Where(c => c.Type == "permissions")
                .SelectMany(
                    c => c.Value.StartsWith("[")
                        ? JsonSerializer.Deserialize<Permission[]>(c.Value, DefaultJsonSerializerOptions.Instance)!
                        : new[]
                        {
                            JsonSerializer.Deserialize<Permission>(c.Value, DefaultJsonSerializerOptions.Instance)!
                        })
                .ToArray();
            return tickets.Length > 0;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Returns if the user is authenticated
    /// </summary>
    /// <param name="principal">The user principal</param>
    /// <returns>The user is authenticated</returns>
    public static bool IsAuthenticated(this ClaimsPrincipal? principal)
    {
        return principal?.Identity?.IsAuthenticated == true;
    }

    /// <summary>
    /// Returns the subject from an authenticated user
    /// Otherwise returns null.
    /// </summary>
    /// <param name="principal">The user principal</param>
    /// <returns>User's subject</returns>
    public static string? GetSubject(this ClaimsPrincipal? principal)
    {
        return principal?.Identities.SelectMany(x => x.Claims).GetSubject();
    }

    /// <summary>
    /// Returns the subject from a set of claims
    /// Otherwise returns null.
    /// </summary>
    /// <param name="claims">The claims to check</param>
    /// <returns>User's subject</returns>
    public static string? GetSubject(this IEnumerable<Claim> claims)
    {
        // ReSharper disable once PossibleMultipleEnumeration
        return (claims.FirstOrDefault(c => c.Type == OpenIdClaimTypes.Subject)
         ?? claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier))?.Value;
    }

    /// <summary>
    /// Gets the client application id claim value.
    /// </summary>
    /// <param name="principal">The user principal.</param>
    /// <returns>the user's client.</returns>
    public static string GetClientId(this ClaimsPrincipal? principal)
    {
        if (principal?.Identity == null || !principal.Identity.IsAuthenticated)
        {
            return string.Empty;
        }

        return principal.Claims.GetClientId();
    }

    /// <summary>
    /// Gets the client application id claim value.
    /// </summary>
    /// <param name="claims">The user claims.</param>
    /// <returns>the user's client.</returns>
    public static string GetClientId(this IEnumerable<Claim>? claims)
    {
        if (claims == null)
        {
            return string.Empty;
        }

        var claim = claims.FirstOrDefault(c => c.Type == StandardClaimNames.Azp);
        return claim == null ? string.Empty : claim.Value;
    }

    /// <summary>
    /// Gets the name of the authenticated user.
    /// </summary>
    /// <param name="principal">The user principal.</param>
    /// <returns>The user's name.</returns>
    public static string? GetName(this ClaimsPrincipal? principal)
    {
        return GetClaimValue(principal, OpenIdClaimTypes.Name)
         ?? GetClaimValue(principal, StandardClaimNames.Subject)
         ?? GetClaimValue(principal, ClaimTypes.Name)
         ?? GetClaimValue(principal, ClaimTypes.NameIdentifier);
    }

    /// <summary>
    /// Gets the name of the authenticated user.
    /// </summary>
    /// <param name="principal">The user principal.</param>
    /// <returns>The user's name.</returns>
    public static string? GetEmail(this ClaimsPrincipal? principal)
    {
        return GetClaimValue(principal, OpenIdClaimTypes.Email)
         ?? GetClaimValue(principal, ClaimTypes.Email);
    }

    /// <summary>
    /// Returns the email claim value from a set of claims.
    /// Otherwise returns null.
    /// </summary>
    /// <param name="claims">The claims to check</param>
    /// <returns>User's email</returns>
    public static string? GetEmail(this IEnumerable<Claim> claims)
    {
        // ReSharper disable once PossibleMultipleEnumeration
        return claims.FirstOrDefault(c => c.Type == OpenIdClaimTypes.Email)?.Value;
    }

    private static string? GetClaimValue(ClaimsPrincipal? principal, string claimName)
    {
        var claim = principal?.FindFirst(claimName);

        return claim?.Value;
    }
}
