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

namespace DotAuth.Extensions;

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DotAuth.Parameters;
using DotAuth.Shared.Models;
using DotAuth.Shared.Repositories;

internal static class ConsentHelper
{
    public static async Task<Consent?> GetConfirmedConsents(
        this IConsentRepository consentRepository,
        string subject,
        AuthorizationParameter authorizationParameter,
        CancellationToken cancellationToken)
    {
        var consents =
            (await consentRepository.GetConsentsForGivenUser(subject, cancellationToken).ConfigureAwait(false))
            ?.ToArray()
            ?? Array.Empty<Consent>();
        Consent? confirmedConsent = null;
        if (consents.Length <= 0)
        {
            return confirmedConsent;
        }

        var claimsParameter = authorizationParameter.Claims;
        if (claimsParameter.IsAnyUserInfoClaimParameter() || claimsParameter.IsAnyIdentityTokenClaimParameter())
        {
            var expectedClaims = claimsParameter.GetClaimNames();
            confirmedConsent = consents.FirstOrDefault(
                c => c.ClientId == authorizationParameter.ClientId
                     && c.Claims.Length > 0
                     && expectedClaims.Length == c.Claims.Length
                     && expectedClaims.All(cl => c.Claims.Contains(cl)));
        }
        else
        {
            var scopeNames = authorizationParameter.Scope.ParseScopes();
            confirmedConsent = consents.FirstOrDefault(
                c => c.ClientId == authorizationParameter.ClientId
                     && c.GrantedScopes.Length > 0
                     && scopeNames.Length == c.GrantedScopes.Length
                     && c.GrantedScopes.All(g => scopeNames.Contains(g)));
        }

        return confirmedConsent;
    }
}