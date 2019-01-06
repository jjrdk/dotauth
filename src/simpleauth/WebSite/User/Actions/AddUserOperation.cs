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
    using Helpers;
    using Logging;
    using Shared;
    using Shared.Models;
    using Shared.Repositories;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using System.Threading.Tasks;

    public class AddUserOperation : IAddUserOperation
    {
        private readonly IResourceOwnerRepository _resourceOwnerRepository;
        private readonly IEnumerable<IAccountFilter> _accountFilters;
        private readonly IEventPublisher _eventPublisher;

        public AddUserOperation(
            IResourceOwnerRepository resourceOwnerRepository,
            IEnumerable<IAccountFilter> accountFilters,
            IEventPublisher eventPublisher)
        {
            _resourceOwnerRepository = resourceOwnerRepository;
            _accountFilters = accountFilters;
            _eventPublisher = eventPublisher;
        }

        public async Task<bool> Execute(ResourceOwner resourceOwner, Uri scimBaseUrl = null)
        {
            if (resourceOwner == null)
            {
                throw new ArgumentNullException(nameof(resourceOwner));
            }

            if (string.IsNullOrEmpty(resourceOwner.Id))
            {
                throw new ArgumentNullException(nameof(resourceOwner.Id), "The parameter login is missing");
            }

            if (string.IsNullOrWhiteSpace(resourceOwner.Password))
            {
                throw new ArgumentNullException(nameof(resourceOwner.Password), "The parameter password is missing");
            }

            // 1. Check the resource owner already exists.
            if (await _resourceOwnerRepository.Get(resourceOwner.Id).ConfigureAwait(false) != null)
            {
                return false;
                //throw new SimpleAuthException(
                //    Errors.ErrorCodes.UnhandledExceptionCode,
                //    Errors.ErrorDescriptions.TheRoWithCredentialsAlreadyExists);
            }

            var newClaims = new List<Claim>
            {
                new Claim(JwtConstants.StandardResourceOwnerClaimNames.UpdatedAt, DateTime.UtcNow.ToString()),
                new Claim(JwtConstants.StandardResourceOwnerClaimNames.Subject, resourceOwner.Id)
            };

            // 2. Populate the claims.
            //var existedClaims = await _claimRepository.GetAllAsync().ConfigureAwait(false);
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
                    var userFilterResult = await resourceOwnerFilter.Check(newClaims).ConfigureAwait(false);
                    if (!userFilterResult.IsValid)
                    {
                        isFilterValid = false;
                        foreach (var ruleResult in userFilterResult.AccountFilterRules.Where(x => !x.IsValid))
                        {
                            await _eventPublisher.Publish(new FailureMessage(Guid.NewGuid().ToString("N"),
                                    $"the filter rule '{ruleResult.RuleName}' failed",
                                    DateTime.UtcNow))
                                .ConfigureAwait(false);
                            foreach (var errorMessage in ruleResult.ErrorMessages)
                            {
                                await _eventPublisher
                                    .Publish(new FailureMessage(Guid.NewGuid().ToString("N"),
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
                    //throw new SimpleAuthException(Errors.ErrorCodes.InternalError,
                    //    Errors.ErrorDescriptions.TheUserIsNotAuthorized);
                }
            }

            // 3. Add the scim resource.
            if (scimBaseUrl != null)
            {
                //var scimResource = await AddScimResource(authenticationParameter, scimBaseUrl, resourceOwner.Login).ConfigureAwait(false);
                //var scimUrl = newClaims.FirstOrDefault(c => c.Type == Jwt.JwtConstants.StandardResourceOwnerClaimNames.ScimId);
                //var scimLocation = newClaims.FirstOrDefault(c => c.Type == Jwt.JwtConstants.StandardResourceOwnerClaimNames.ScimLocation);
                //if (scimUrl != null)
                //{
                //    newClaims.Remove(scimUrl);
                //}

                //if (scimLocation != null)
                //{
                //    newClaims.Remove(scimLocation);
                //}

                newClaims.Add(new Claim(JwtConstants.StandardResourceOwnerClaimNames.ScimId, resourceOwner.Id));
                newClaims.Add(new Claim(JwtConstants.StandardResourceOwnerClaimNames.ScimLocation,
                    $"{scimBaseUrl}/Users/{resourceOwner.Id}"));
            }

            // 4. Add the resource owner.
            var newResourceOwner = new ResourceOwner
            {
                Id = resourceOwner.Id,
                Claims = newClaims,
                TwoFactorAuthentication = string.Empty,
                IsLocalAccount = true,
                Password = resourceOwner.Password.ToSha256Hash(),
                UserProfile = resourceOwner.UserProfile
            };
            if (!await _resourceOwnerRepository.InsertAsync(newResourceOwner).ConfigureAwait(false))
            {
                return false;
                //throw new SimpleAuthException(Errors.ErrorCodes.UnhandledExceptionCode,
                //    Errors.ErrorDescriptions.TheResourceOwnerCannotBeAdded);
            }

            //// 5. Link to a profile.
            //if (!string.IsNullOrWhiteSpace(issuer))
            //{
            //    await _linkProfileAction.Execute(resourceOwner.Login, resourceOwner.ExternalLogin, issuer)
            //        .ConfigureAwait(false);
            //}

            await _eventPublisher.Publish(new ResourceOwnerAdded(Guid.NewGuid().ToString("N"),newResourceOwner.Id, DateTime.UtcNow)).ConfigureAwait(false);
            return true;
        }

        ///// <summary>
        ///// CreateJwk the scim resource and the scim identifier.
        ///// </summary>
        ///// <param name="subject"></param>
        ///// <returns></returns>
        //private async Task<ScimUser> AddScimResource(AuthenticationParameter scimOpts, string scimBaseUrl, string subject)
        //{
        //    //var grantedToken = await _tokenStore.GetToken(scimOpts.WellKnownAuthorizationUrl, scimOpts.ClientId, scimOpts.ClientSecret, new[]
        //    //{
        //    //    ScimConstants.ScimPolicies.ScimManage
        //    //}).ConfigureAwait(false);

        //    var scimResponse = await _usersClient.AddUser(new AddUserParameter(), 
        //            new Uri(scimBaseUrl), new ScimUser { ExternalId = subject })
        //        //.SetCommonAttributes(subject)
        //        //.Execute()
        //        .ConfigureAwait(false);
        //    var scimId = scimResponse.Content["id"].ToString();
        //    return new ScimUser(scimId, $"{scimBaseUrl}/Users/{scimId}");
        //}
    }
}