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
    using Parameters;
    using Shared.Models;
    using Shared.Repositories;
    using Shared.Responses;
    using System.Collections.Generic;
    using System.IdentityModel.Tokens.Jwt;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Newtonsoft.Json;

    internal class DefaultAuthorizationPolicy : IAuthorizationPolicy
    {
        private readonly IClientStore _clientStore;
        private readonly IJwksStore _jwksStore;

        public DefaultAuthorizationPolicy(IClientStore clientStore, IJwksStore jwksStore)
        {
            _clientStore = clientStore;
            _jwksStore = jwksStore;
        }

        public async Task<AuthorizationPolicyResult> Execute(
            TicketLineParameter ticketLineParameter,
            ClaimTokenParameter claimTokenParameter,
            CancellationToken cancellationToken,
            params PolicyRule[] authorizationPolicy)
        {
            if (authorizationPolicy == null)
            {
                return new AuthorizationPolicyResult(AuthorizationPolicyResultKind.NotAuthorized);
            }

            AuthorizationPolicyResult result = null;
            foreach (var rule in authorizationPolicy)
            {
                result = await ExecuteAuthorizationPolicyRule(
                        ticketLineParameter,
                        rule,
                        claimTokenParameter,
                        cancellationToken)
                    .ConfigureAwait(false);
                if (result.Result == AuthorizationPolicyResultKind.Authorized)
                {
                    return result;
                }
            }

            return result;
        }

        private async Task<AuthorizationPolicyResult> ExecuteAuthorizationPolicyRule(
            TicketLineParameter ticketLineParameter,
            PolicyRule authorizationPolicy,
            ClaimTokenParameter claimTokenParameter,
            CancellationToken cancellationToken)
        {
            // 1. Check can access to the scope
            if (ticketLineParameter.Scopes.Any(s => !authorizationPolicy.Scopes.Contains(s)))
            {
                return new AuthorizationPolicyResult(AuthorizationPolicyResultKind.NotAuthorized);
            }

            // 2. Check clients are correct
            var clientAuthorizationResult =
                authorizationPolicy.ClientIdsAllowed?.Contains(ticketLineParameter.ClientId);
            if (clientAuthorizationResult != true)
            {
                return new AuthorizationPolicyResult(AuthorizationPolicyResultKind.NotAuthorized);
            }

            // 4. Check the resource owner consent is needed
            if (authorizationPolicy.IsResourceOwnerConsentNeeded && !ticketLineParameter.IsAuthorizedByRo)
            {
                return new AuthorizationPolicyResult(AuthorizationPolicyResultKind.RequestSubmitted);
            }

            // 3. Check claims are correct
            var claimAuthorizationResult = await CheckClaims(
                    ticketLineParameter.ClientId,
                    authorizationPolicy,
                    claimTokenParameter,
                    cancellationToken)
                .ConfigureAwait(false);
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
                        {UmaConstants.ErrorDetailNames.ClaimName, claim.Type},
                        {UmaConstants.ErrorDetailNames.ClaimFriendlyName, claim.Type},
                        {UmaConstants.ErrorDetailNames.ClaimIssuer, openidConfigurationUrl}
                    })
                .ToList();

            requestingPartyClaims.Add(UmaConstants.ErrorDetailNames.RequiredClaims, requiredClaims);
            requestingPartyClaims.Add(UmaConstants.ErrorDetailNames.RedirectUser, false);
            return new AuthorizationPolicyResult(
                AuthorizationPolicyResultKind.NeedInfo,
                new Dictionary<string, object>
                {
                    {UmaConstants.ErrorDetailNames.RequestingPartyClaims, requestingPartyClaims}
                });
        }

        private async Task<AuthorizationPolicyResult> CheckClaims(
            string clientId,
            PolicyRule authorizationPolicy,
            ClaimTokenParameter claimTokenParameter,
            CancellationToken cancellationToken)
        {
            if (authorizationPolicy.Claims == null || !authorizationPolicy.Claims.Any())
            {
                return new AuthorizationPolicyResult(AuthorizationPolicyResultKind.Authorized);
            }

            if (claimTokenParameter == null || claimTokenParameter.Format != UmaConstants.IdTokenType)
            {
                return GetNeedInfoResult(authorizationPolicy.Claims, authorizationPolicy.OpenIdProvider);
            }

            var client = await _clientStore.GetById(clientId, cancellationToken).ConfigureAwait(false);

            var handler = new JwtSecurityTokenHandler();
            var validationParameters = await client.CreateValidationParameters(_jwksStore).ConfigureAwait(false);
            handler.ValidateToken(claimTokenParameter.Token, validationParameters, out var securityToken);
            var tokenClaims = (securityToken as JwtSecurityToken)?.Claims.ToArray();

            if (tokenClaims == null)
            {
                return new AuthorizationPolicyResult(AuthorizationPolicyResultKind.NotAuthorized);
            }

            foreach (var claim in authorizationPolicy.Claims)
            {
                var payload = tokenClaims.FirstOrDefault(j => j.Type == claim.Type);
                if (payload.Equals(default(KeyValuePair<string, object>)))
                {
                    return new AuthorizationPolicyResult(AuthorizationPolicyResultKind.NotAuthorized);
                }

                if (payload.ValueType == JsonClaimValueTypes.JsonArray) // is IEnumerable<string> strings)
                {
                    var strings = JsonConvert.DeserializeObject<object[]>(payload.Value);
                    if (!strings.Any(s => string.Equals(s, claim.Value)))
                    {
                        return new AuthorizationPolicyResult(AuthorizationPolicyResultKind.NotAuthorized);
                    }
                }

                if (payload.Value != claim.Value)
                {
                    return new AuthorizationPolicyResult(AuthorizationPolicyResultKind.NotAuthorized);
                }
            }

            return new AuthorizationPolicyResult(AuthorizationPolicyResultKind.Authorized);
        }
    }
}
