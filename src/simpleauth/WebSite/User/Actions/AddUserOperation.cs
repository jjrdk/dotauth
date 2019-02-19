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

namespace SimpleAuth.WebSite.User.Actions
{
    using Extensions;
    using Shared;
    using Shared.Models;
    using Shared.Repositories;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Security.Claims;
    using System.Threading;
    using System.Threading.Tasks;
    using SimpleAuth.Shared.Events.Logging;

    internal class AddUserOperation
    {
        private readonly IResourceOwnerRepository _resourceOwnerRepository;
        private readonly IAccountFilter[] _accountFilters;
        private readonly IEventPublisher _eventPublisher;

        public AddUserOperation(
            IResourceOwnerRepository resourceOwnerRepository,
            IEnumerable<IAccountFilter> accountFilters,
            IEventPublisher eventPublisher)
        {
            _resourceOwnerRepository = resourceOwnerRepository;
            _accountFilters = accountFilters.ToArray();
            _eventPublisher = eventPublisher;
        }

        public async Task<bool> Execute(ResourceOwner resourceOwner, CancellationToken cancellationToken)
        {
            // 1. Check the resource owner already exists.
            if (await _resourceOwnerRepository.Get(resourceOwner.Subject, cancellationToken).ConfigureAwait(false) != null)
            {
                return false;
            }

            var newClaims = new List<Claim>
            {
                new Claim(OpenIdClaimTypes.UpdatedAt,
                    DateTime.UtcNow.ConvertToUnixTimestamp().ToString(CultureInfo.InvariantCulture)),
                new Claim(OpenIdClaimTypes.Subject, resourceOwner.Subject)
            };

            // 2. Populate the claims.
            //var existedClaims = await _claimRepository.GetAll().ConfigureAwait(false);
            if (resourceOwner.Claims != null)
            {
                foreach (var claim in resourceOwner.Claims)
                {
                    if (newClaims.All(nc => nc.Type != claim.Type))
                    {
                        newClaims.Add(claim);
                    }
                }
            }

            if (_accountFilters != null)
            {
                var isFilterValid = true;
                foreach (var resourceOwnerFilter in _accountFilters)
                {
                    var userFilterResult = await resourceOwnerFilter.Check(newClaims, cancellationToken).ConfigureAwait(false);
                    if (!userFilterResult.IsValid)
                    {
                        isFilterValid = false;
                        foreach (var ruleResult in userFilterResult.AccountFilterRules.Where(x => !x.IsValid))
                        {
                            await _eventPublisher.Publish(new FailureMessage(Id.Create(),
                                    $"the filter rule '{ruleResult.RuleName}' failed",
                                    DateTime.UtcNow))
                                .ConfigureAwait(false);
                            foreach (var errorMessage in ruleResult.ErrorMessages)
                            {
                                await _eventPublisher
                                    .Publish(new FailureMessage(
                                        Id.Create(),
                                        errorMessage,
                                        DateTime.UtcNow))
                                    .ConfigureAwait(false);
                            }
                        }
                    }
                }

                if (!isFilterValid)
                {
                    return false;
                }
            }

            // 4. Add the resource owner.
            var newResourceOwner = new ResourceOwner
            {
                Subject = resourceOwner.Subject,
                Claims = newClaims.ToArray(),
                ExternalLogins = resourceOwner.ExternalLogins,
                TwoFactorAuthentication = resourceOwner.TwoFactorAuthentication,
                IsLocalAccount = resourceOwner.IsLocalAccount,
                Password = resourceOwner.Password,
            };
            if (!await _resourceOwnerRepository.Insert(newResourceOwner, cancellationToken).ConfigureAwait(false))
            {
                return false;
            }

            await _eventPublisher.Publish(
                    new ResourceOwnerAdded(
                        Id.Create(),
                        newResourceOwner.Subject,
                        DateTime.UtcNow))
                .ConfigureAwait(false);
            return true;
        }
    }
}