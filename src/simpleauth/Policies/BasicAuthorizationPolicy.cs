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
    using System.Security.Claims;
    using System.Threading;
    using System.Threading.Tasks;

    internal class BasicAuthorizationPolicy : IAuthorizationPolicy
    {
        private readonly IClientStore _clientStore;
        private readonly IJwksStore _jwksStore;

        public BasicAuthorizationPolicy(IClientStore clientStore, IJwksStore jwksStore)
        {
            _clientStore = clientStore;
            _jwksStore = jwksStore;
        }

        public async Task<AuthorizationPolicyResult> Execute(
            TicketLineParameter ticketLineParameter,
            Policy authorizationPolicy,
            ClaimTokenParameter claimTokenParameter,
            CancellationToken cancellationToken)
        {
            if (authorizationPolicy.Rules == null)
            {
                return new AuthorizationPolicyResult { Type = AuthorizationPolicyResultEnum.NotAuthorized };
            }

            AuthorizationPolicyResult result = null;
            foreach (var rule in authorizationPolicy.Rules)
            {
                result = await ExecuteAuthorizationPolicyRule(
                        ticketLineParameter,
                        rule,
                        claimTokenParameter,
                        cancellationToken)
                    .ConfigureAwait(false);
                if (result.Type == AuthorizationPolicyResultEnum.Authorized)
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
                return new AuthorizationPolicyResult { Type = AuthorizationPolicyResultEnum.NotAuthorized };
            }

            // 2. Check clients are correct
            var clientAuthorizationResult = CheckClients(authorizationPolicy, ticketLineParameter);
            if (clientAuthorizationResult != null
                && clientAuthorizationResult.Type != AuthorizationPolicyResultEnum.Authorized)
            {
                return clientAuthorizationResult;
            }

            // 3. Check claims are correct
            var claimAuthorizationResult = await CheckClaims(
                    ticketLineParameter.ClientId,
                    authorizationPolicy,
                    claimTokenParameter,
                    cancellationToken)
                .ConfigureAwait(false);
            if (claimAuthorizationResult != null
                && claimAuthorizationResult.Type != AuthorizationPolicyResultEnum.Authorized)
            {
                return claimAuthorizationResult;
            }

            // 4. Check the resource owner consent is needed
            if (authorizationPolicy.IsResourceOwnerConsentNeeded && !ticketLineParameter.IsAuthorizedByRo)
            {
                return new AuthorizationPolicyResult { Type = AuthorizationPolicyResultEnum.RequestSubmitted };
            }

            return new AuthorizationPolicyResult { Type = AuthorizationPolicyResultEnum.Authorized };
        }

        private AuthorizationPolicyResult GetNeedInfoResult(Claim[] claims, string openidConfigurationUrl)
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
            return new AuthorizationPolicyResult
            {
                Type = AuthorizationPolicyResultEnum.NeedInfo,
                ErrorDetails = new Dictionary<string, object>
                {
                    {UmaConstants.ErrorDetailNames.RequestingPartyClaims, requestingPartyClaims}
                }
            };
        }

        private async Task<AuthorizationPolicyResult> CheckClaims(
            string clientId,
            PolicyRule authorizationPolicy,
            ClaimTokenParameter claimTokenParameter,
            CancellationToken cancellationToken)
        {
            if (authorizationPolicy.Claims == null || !authorizationPolicy.Claims.Any())
            {
                return null;
            }

            if (claimTokenParameter == null || claimTokenParameter.Format != UmaConstants._idTokenType)
            {
                return GetNeedInfoResult(authorizationPolicy.Claims, authorizationPolicy.OpenIdProvider);
            }

            var client = await _clientStore.GetById(clientId, cancellationToken).ConfigureAwait(false);

            var handler = new JwtSecurityTokenHandler();
            var validationParameters = await client.CreateValidationParameters(_jwksStore);
            handler.ValidateToken(
                claimTokenParameter.Token,
                validationParameters,
                out var securityToken);
            var jwsPayload = (securityToken as JwtSecurityToken)?.Payload;

            if (jwsPayload == null)
            {
                return new AuthorizationPolicyResult { Type = AuthorizationPolicyResultEnum.NotAuthorized };
            }

            foreach (var claim in authorizationPolicy.Claims)
            {
                var payload = jwsPayload.FirstOrDefault(j => j.Key == claim.Type);
                if (payload.Equals(default(KeyValuePair<string, object>)))
                {
                    return new AuthorizationPolicyResult { Type = AuthorizationPolicyResultEnum.NotAuthorized };
                }

                if (payload.Value is IEnumerable<string> strings)
                {
                    if (!strings.Any(s => string.Equals(s, claim.Value)))
                    {
                        return new AuthorizationPolicyResult { Type = AuthorizationPolicyResultEnum.NotAuthorized };
                    }
                }

                //if (claim.Type == OpenIdClaimTypes.Role)
                //{
                //    IEnumerable<string> roles = null;
                //    if (payload.Value is string)
                //    {
                //        roles = payload.Value.ToString().Split(',');
                //    }
                //    else
                //    {
                //        if (payload.Value is object[] arr)
                //        {
                //            roles = arr.Select(c => c.ToString());
                //        }
                //        else if (payload.Value is JArray jArr)
                //        {
                //            roles = jArr.Select(c => c.ToString());
                //        }
                //    }

                //    if (roles == null || roles.All(v => claim.Value != v))
                //    {
                //        return new AuthorizationPolicyResult
                //        {
                //            Type = AuthorizationPolicyResultEnum.NotAuthorized
                //        };
                //    }
                //}
                else
                {
                    if (payload.Value.ToString() != claim.Value)
                    {
                        return new AuthorizationPolicyResult { Type = AuthorizationPolicyResultEnum.NotAuthorized };
                    }
                }
            }

            return new AuthorizationPolicyResult { Type = AuthorizationPolicyResultEnum.Authorized };
        }

        private AuthorizationPolicyResult CheckClients(
            PolicyRule authorizationPolicy,
            TicketLineParameter ticketLineParameter)
        {
            if (authorizationPolicy.ClientIdsAllowed == null || !authorizationPolicy.ClientIdsAllowed.Any())
            {
                return null;
            }

            if (!authorizationPolicy.ClientIdsAllowed.Contains(ticketLineParameter.ClientId))
            {
                return new AuthorizationPolicyResult { Type = AuthorizationPolicyResultEnum.NotAuthorized };
            }

            return new AuthorizationPolicyResult { Type = AuthorizationPolicyResultEnum.Authorized };
        }
    }
}
