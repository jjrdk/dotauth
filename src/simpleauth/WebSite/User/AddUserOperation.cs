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

namespace SimpleAuth.WebSite.User
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using System.Threading;
    using System.Threading.Tasks;
    using Shared.DTOs;
    using SimpleAuth.Shared;
    using SimpleAuth.Shared.Events.Logging;
    using SimpleAuth.Shared.Models;
    using SimpleAuth.Shared.Repositories;

    internal class AddUserOperation
    {
        private readonly RuntimeSettings _settings;
        private readonly IResourceOwnerRepository _resourceOwnerRepository;
        private readonly ISubjectBuilder _subjectBuilder;
        private readonly IAccountFilter[] _accountFilters;
        private readonly IEventPublisher _eventPublisher;

        public AddUserOperation(
            RuntimeSettings settings,
            IResourceOwnerRepository resourceOwnerRepository,
            IEnumerable<IAccountFilter> accountFilters,
            ISubjectBuilder subjectBuilder,
            IEventPublisher eventPublisher)
        {
            _settings = settings;
            _resourceOwnerRepository = resourceOwnerRepository;
            _subjectBuilder = subjectBuilder;
            _accountFilters = accountFilters.ToArray();
            _eventPublisher = eventPublisher;
        }

        public async Task<(bool Success, string Subject)> Execute(ResourceOwner resourceOwner, CancellationToken cancellationToken)
        {
            if (!resourceOwner.IsLocalAccount || string.IsNullOrWhiteSpace(resourceOwner.Subject))
            {
                var subject = await _subjectBuilder.BuildSubject(resourceOwner.Claims, cancellationToken)
                    .ConfigureAwait(false);
                resourceOwner.Subject = subject;
            }

            // 1. Check the resource owner already exists.
            if (await _resourceOwnerRepository.Get(resourceOwner.Subject, cancellationToken).ConfigureAwait(false) != null)
            {
                return (false, null);
            }

            resourceOwner.UpdateDateTime = DateTimeOffset.UtcNow;
            var additionalClaims = _settings.ClaimsIncludedInUserCreation
                .Except(resourceOwner.Claims.Select(x => x.Type))
                .Select(x => new Claim(x, string.Empty));
            resourceOwner.Claims = resourceOwner.Claims.Add(additionalClaims);

            // Customize new resource owner.
            _settings.OnResourceOwnerCreated(resourceOwner);

            if (_accountFilters != null)
            {
                var isFilterValid = true;
                foreach (var resourceOwnerFilter in _accountFilters)
                {
                    var userFilterResult = await resourceOwnerFilter.Check(resourceOwner.Claims, cancellationToken).ConfigureAwait(false);
                    if (userFilterResult.IsValid)
                    {
                        continue;
                    }

                    isFilterValid = false;
                    foreach (var ruleResult in userFilterResult.AccountFilterRules.Where(x => !x.IsValid))
                    {
                        await _eventPublisher.Publish(
                                new FilterValidationFailure(
                                    Id.Create(),
                                    $"The filter rule '{ruleResult.RuleName}' failed",
                                    DateTimeOffset.UtcNow))
                            .ConfigureAwait(false);
                        foreach (var errorMessage in ruleResult.ErrorMessages)
                        {
                            await _eventPublisher
                                .Publish(new FilterValidationFailure(Id.Create(), errorMessage, DateTimeOffset.UtcNow))
                                .ConfigureAwait(false);
                        }
                    }
                }

                if (!isFilterValid)
                {
                    return (false, null);
                }
            }

            resourceOwner.CreateDateTime = DateTimeOffset.UtcNow;
            if (!await _resourceOwnerRepository.Insert(resourceOwner, cancellationToken).ConfigureAwait(false))
            {
                return (false, null);
            }

            await _eventPublisher.Publish(
                    new ResourceOwnerAdded(
                        Id.Create(),
                        resourceOwner.Subject,
                        resourceOwner.Claims.Select(claim => new ClaimData
                            {
                                Type = claim.Type,
                                Value = claim.Value
                            })
                            .ToArray(),
                        DateTimeOffset.UtcNow))
                .ConfigureAwait(false);
            return (true, resourceOwner.Subject);
        }
    }
}