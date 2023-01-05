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

namespace DotAuth.Policies;

using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using DotAuth.Shared;
using DotAuth.Shared.Models;
using DotAuth.Shared.Policies;
using DotAuth.Shared.Responses;
using Newtonsoft.Json;

internal sealed class DefaultAuthorizationPolicy : IAuthorizationPolicy
{
    public Task<AuthorizationPolicyResult> Execute(
        TicketLineParameter ticketLineParameter,
        string? claimTokenFormat,
        ClaimsPrincipal requester,
        CancellationToken cancellationToken,
        params PolicyRule[] authorizationPolicy)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (authorizationPolicy.Length == 0)
        {
            return Task.FromResult(
                new AuthorizationPolicyResult(AuthorizationPolicyResultKind.NotAuthorized, requester));
        }

        if (claimTokenFormat != UmaConstants.IdTokenType)
        {
            return GetNeedInfoResult(
                authorizationPolicy[0].Claims,
                requester,
                authorizationPolicy[0].OpenIdProvider ?? string.Empty);
        }

        AuthorizationPolicyResult? result = null;
        foreach (var rule in authorizationPolicy)
        {
            cancellationToken.ThrowIfCancellationRequested();
            result = ExecuteAuthorizationPolicyRule(ticketLineParameter, rule, requester, cancellationToken);
            if (result.Result == AuthorizationPolicyResultKind.Authorized)
            {
                break;
            }
        }

        return Task.FromResult(result!);
    }

    private static AuthorizationPolicyResult ExecuteAuthorizationPolicyRule(
        TicketLineParameter ticketLineParameter,
        PolicyRule authorizationPolicy,
        ClaimsPrincipal requester,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        // 1. Check can access to the scope
        if (ticketLineParameter.Scopes.Length == 0
            || ticketLineParameter.Scopes.Any(s => !authorizationPolicy.Scopes.Contains(s)))
        {
            return new AuthorizationPolicyResult(AuthorizationPolicyResultKind.NotAuthorized, requester);
        }

        // 2. Check clients are correct
        var clientAuthorizationResult = authorizationPolicy.ClientIdsAllowed.Length == 0
                                        || authorizationPolicy.ClientIdsAllowed.Contains(ticketLineParameter.ClientId);
        if (!clientAuthorizationResult)
        {
            return new AuthorizationPolicyResult(AuthorizationPolicyResultKind.NotAuthorized, requester);
        }

        // 4. Check the resource owner consent is needed
        if (authorizationPolicy.IsResourceOwnerConsentNeeded && !ticketLineParameter.IsAuthorizedByRo)
        {
            return new AuthorizationPolicyResult(AuthorizationPolicyResultKind.RequestSubmitted, requester);
        }

        // 3. Check claims are correct
        var claimAuthorizationResult = CheckClaims(authorizationPolicy, requester);
        return claimAuthorizationResult.Result != AuthorizationPolicyResultKind.Authorized
            ? claimAuthorizationResult
            : new AuthorizationPolicyResult(AuthorizationPolicyResultKind.Authorized, requester);
    }

    private static Task<AuthorizationPolicyResult> GetNeedInfoResult(
        ClaimData[] claims,
        ClaimsPrincipal requester,
        string openidConfigurationUrl)
    {
        var requestingPartyClaims = new Dictionary<string, object>();
        var requiredClaims = claims.Select(
                claim => new Dictionary<string, string>
                {
                    { "name", claim.Type }, { "friendly_name", claim.Type }, { "issuer", openidConfigurationUrl }
                })
            .ToList();

        requestingPartyClaims.Add("required_claims", requiredClaims);
        requestingPartyClaims.Add("redirect_user", false);
        return Task.FromResult(
            new AuthorizationPolicyResult(
                AuthorizationPolicyResultKind.NeedInfo,
                requester,
                new Dictionary<string, object> { { "requesting_party_claims", requestingPartyClaims } }));
    }

    private static AuthorizationPolicyResult CheckClaims(PolicyRule authorizationPolicy, ClaimsPrincipal requester)
    {
        if (!authorizationPolicy.Claims.Any())
        {
            return new AuthorizationPolicyResult(AuthorizationPolicyResultKind.Authorized, requester);
        }

        var unmatchedPolicyClaim = (from policyClaim in authorizationPolicy.Claims
            let tokenClaims =
                requester.Claims.Where(j => j.Type == policyClaim.Type && j.ValueType != JsonClaimValueTypes.JsonArray)
                    .ToArray()
            where tokenClaims.Length == 0 || tokenClaims.All(tc => tc.Value != policyClaim.Value)
            select policyClaim).Any();

        return unmatchedPolicyClaim
            ? new AuthorizationPolicyResult(AuthorizationPolicyResultKind.NotAuthorized, requester)
            : new AuthorizationPolicyResult(AuthorizationPolicyResultKind.Authorized, requester);
    }
}
