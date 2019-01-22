﻿// Copyright © 2015 Habart Thierry, © 2018 Jacob Reimers
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

namespace SimpleAuth.Helpers
{
    using Extensions;
    using Parameters;
    using Shared.Models;
    using Shared.Repositories;
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    internal static class ConsentHelper
    {
        public static async Task<Consent> GetConfirmedConsents(
            this IConsentRepository consentRepository,
            string subject,
            AuthorizationParameter authorizationParameter)
        {
            if (string.IsNullOrWhiteSpace(subject))
            {
                throw new ArgumentNullException(nameof(subject));
            }

            if (authorizationParameter == null)
            {
                throw new ArgumentNullException(nameof(authorizationParameter));
            }

            var consents = (await consentRepository.GetConsentsForGivenUser(subject).ConfigureAwait(false))?.ToArray()??Array.Empty<Consent>();
            Consent confirmedConsent = null;
            if (consents.Length > 0)
            {
                var claimsParameter = authorizationParameter.Claims;
                if (claimsParameter.IsAnyUserInfoClaimParameter() || claimsParameter.IsAnyIdentityTokenClaimParameter())
                {
                    var expectedClaims = claimsParameter.GetClaimNames();
                    confirmedConsent = consents.FirstOrDefault(
                        c => c.Client.ClientId == authorizationParameter.ClientId
                             && c.Claims != null
                             && c.Claims.Count > 0
                             && expectedClaims.Count == c.Claims.Count
                             && expectedClaims.All(cl => c.Claims.Contains(cl)));
                }
                else
                {
                    var scopeNames = authorizationParameter.Scope.ParseScopes();
                    confirmedConsent = consents.FirstOrDefault(
                        c => c.Client.ClientId == authorizationParameter.ClientId
                             && c.GrantedScopes != null
                             && c.GrantedScopes.Count > 0
                             && scopeNames.Count == c.GrantedScopes.Count
                             && c.GrantedScopes.All(g => scopeNames.Contains(g.Name)));
                }
            }

            return confirmedConsent;
        }
    }
}
