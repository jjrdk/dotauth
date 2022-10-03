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

namespace DotAuth.Extensions;

using System.Linq;
using DotAuth.Properties;
using DotAuth.Results;
using DotAuth.Shared.Models;

internal static class ScopeValidator
{
    public static ScopeValidationResult Check(this string? scope, Client client)
    {
        var scopes = scope.ParseScopes();
        if (scopes.Length == 0)
        {
            return new ScopeValidationResult(Strings.TheScopesNeedToBeSpecified);
        }

        var duplicates = scopes.GroupBy(p => p)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();
        if (duplicates.Count > 1)
        {
            return new ScopeValidationResult(
                string.Format(Strings.DuplicateScopeValues,
                    string.Join(",", duplicates)));
        }

        var scopeAllowed = client.AllowedScopes;
        var scopesNotAllowedOrInvalid = scopes
            .Where(s => !scopeAllowed.Contains(s))
            .ToList();
        if (scopesNotAllowedOrInvalid.Any())
        {
            return new ScopeValidationResult(
                string.Format(Strings.ScopesAreNotAllowedOrInvalid,
                    string.Join(",", scopesNotAllowedOrInvalid)));
        }

        return new ScopeValidationResult(scopes);
    }
}