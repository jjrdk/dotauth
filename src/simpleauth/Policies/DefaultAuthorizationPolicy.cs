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

namespace SimpleAuth.Policies
{
    using Shared.Models;
    using Shared.Responses;
    using System.Collections.Generic;
    using System.IdentityModel.Tokens.Jwt;
    using System.Linq;
    using System.Security.Claims;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;
    using Newtonsoft.Json;

    internal class DefaultAuthorizationPolicy : IAuthorizationPolicy
    {
        public Task<AuthorizationPolicyResult> Execute(
            TicketLineParameter ticketLineParameter,
            string claimTokenFormat,
            Claim[] claims,
            CancellationToken cancellationToken,
            params PolicyRule[] authorizationPolicy)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (authorizationPolicy == null)
            {
                return Task.FromResult(new AuthorizationPolicyResult(AuthorizationPolicyResultKind.NotAuthorized));
            }

            AuthorizationPolicyResult result = null;
            foreach (var rule in authorizationPolicy)
            {
                cancellationToken.ThrowIfCancellationRequested();
                result = ExecuteAuthorizationPolicyRule(
                        ticketLineParameter,
                        rule,
                        claimTokenFormat,
                        claims,
                        cancellationToken);
                if (result.Result == AuthorizationPolicyResultKind.Authorized)
                {
                    //return Task.FromResult(result);
                    break;
                }
            }

            return Task.FromResult(result);
        }

        private AuthorizationPolicyResult ExecuteAuthorizationPolicyRule(
            TicketLineParameter ticketLineParameter,
            PolicyRule authorizationPolicy,
            string claimTokenFormat,
            Claim[] claims,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // 1. Check can access to the scope
            if (ticketLineParameter.Scopes.Any(s => authorizationPolicy.Scopes?.Contains(s) != true))
            {
                return new AuthorizationPolicyResult(AuthorizationPolicyResultKind.NotAuthorized);
            }

            // 2. Check clients are correct
            var clientAuthorizationResult = authorizationPolicy.ClientIdsAllowed == null
                || authorizationPolicy.ClientIdsAllowed.Length == 0
                || authorizationPolicy.ClientIdsAllowed?.Contains(ticketLineParameter.ClientId) == true;
            if (!clientAuthorizationResult)
            {
                return new AuthorizationPolicyResult(AuthorizationPolicyResultKind.NotAuthorized);
            }

            // 4. Check the resource owner consent is needed
            if (authorizationPolicy.IsResourceOwnerConsentNeeded && !ticketLineParameter.IsAuthorizedByRo)
            {
                return new AuthorizationPolicyResult(AuthorizationPolicyResultKind.RequestSubmitted);
            }

            // 3. Check claims are correct
            var claimAuthorizationResult = CheckClaims(
                    authorizationPolicy,
                    claimTokenFormat,
                    claims);
            if (claimAuthorizationResult != null
                && claimAuthorizationResult.Result != AuthorizationPolicyResultKind.Authorized)
            {
                return claimAuthorizationResult;
            }

            return new AuthorizationPolicyResult(AuthorizationPolicyResultKind.Authorized);
        }

        private static AuthorizationPolicyResult GetNeedInfoResult(ClaimData[] claims, string openidConfigurationUrl)
        {
            var requestingPartyClaims = new Dictionary<string, object>();
            var requiredClaims = claims.Select(
                    claim => new Dictionary<string, string>
                    {
                        {"name", claim.Type},
                        {"friendly_name", claim.Type},
                        {"issuer", openidConfigurationUrl}
                    })
                .ToList();

            requestingPartyClaims.Add("required_claims", requiredClaims);
            requestingPartyClaims.Add("redirect_user", false);
            return new AuthorizationPolicyResult(
                AuthorizationPolicyResultKind.NeedInfo,
                new Dictionary<string, object>
                {
                    {"requesting_party_claims", requestingPartyClaims}
                });
        }

        private AuthorizationPolicyResult CheckClaims(
            PolicyRule authorizationPolicy,
            string claimTokenFormat,
            Claim[] claims)
        {
            if (authorizationPolicy.Claims == null || !authorizationPolicy.Claims.Any())
            {
                return new AuthorizationPolicyResult(AuthorizationPolicyResultKind.Authorized);
            }

            if (claimTokenFormat != UmaConstants.IdTokenType)
            {
                return GetNeedInfoResult(authorizationPolicy.Claims, authorizationPolicy.OpenIdProvider);
            }

            if (claims == null)
            {
                return new AuthorizationPolicyResult(AuthorizationPolicyResultKind.NotAuthorized);
            }

            foreach (var policyClaim in authorizationPolicy.Claims)
            {
                var tokenClaim = claims.FirstOrDefault(j => j.Type == policyClaim.Type);
                if (tokenClaim == null)
                {
                    return new AuthorizationPolicyResult(AuthorizationPolicyResultKind.NotAuthorized);
                }

                if (tokenClaim.ValueType == JsonClaimValueTypes.JsonArray) // is IEnumerable<string> strings)
                {
                    var strings = JsonConvert.DeserializeObject<object[]>(tokenClaim.Value);
                    if (!strings.Any(s => Equals(s, policyClaim.Value)))
                    {
                        return new AuthorizationPolicyResult(AuthorizationPolicyResultKind.NotAuthorized);
                    }
                }

                var regex = new Regex(policyClaim.Value);
                if (!regex.IsMatch(tokenClaim.Value)) //tokenClaim.Value != policyClaim.Value)
                {
                    return new AuthorizationPolicyResult(AuthorizationPolicyResultKind.NotAuthorized);
                }
            }

            return new AuthorizationPolicyResult(AuthorizationPolicyResultKind.Authorized);
        }
    }
}
