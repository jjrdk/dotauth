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

namespace SimpleAuth.Controllers
{
    using Extensions;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Shared;
    using Shared.Models;
    using Shared.Repositories;
    using Shared.Requests;
    using SimpleAuth.Shared.Errors;
    using SimpleAuth.WebSite.User;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Security.Claims;
    using System.Threading;
    using System.Threading.Tasks;
    using SimpleAuth.Api.Token.Actions;
    using SimpleAuth.Parameters;
    using SimpleAuth.Shared.Events.OAuth;
    using SimpleAuth.Shared.Responses;

    /// <summary>
    /// Defines the resource owner controller.
    /// </summary>
    /// <seealso cref="Controller" />
    [Route(CoreConstants.EndPoints.ResourceOwners)]
    public class ResourceOwnersController : ControllerBase
    {
        private readonly IResourceOwnerRepository _resourceOwnerRepository;
        private readonly ITokenStore _tokenStore;
        private readonly IClientRepository _clientRepository;
        private readonly IEventPublisher _eventPublisher;
        private readonly AddUserOperation _addUserOperation;
        private readonly GetTokenByRefreshTokenGrantTypeAction _refreshOperation;

        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceOwnersController"/> class.
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="subjectBuilder"></param>
        /// <param name="resourceOwnerRepository">The resource owner repository.</param>
        /// <param name="tokenStore">The token cache</param>
        /// <param name="clientRepository"></param>
        /// <param name="accountFilters">The account filters.</param>
        /// <param name="eventPublisher">The event publisher.</param>
        /// <param name="scopeRepository"></param>
        /// <param name="jwksRepository"></param>
        public ResourceOwnersController(
            RuntimeSettings settings,
            ISubjectBuilder subjectBuilder,
            IResourceOwnerRepository resourceOwnerRepository,
            ITokenStore tokenStore,
            IScopeRepository scopeRepository,
            IJwksStore jwksRepository,
            IClientRepository clientRepository,
            IEnumerable<AccountFilter> accountFilters,
            IEventPublisher eventPublisher)
        {
            _refreshOperation = new GetTokenByRefreshTokenGrantTypeAction(
                settings,
                eventPublisher,
                tokenStore,
                scopeRepository,
                jwksRepository,
                resourceOwnerRepository,
                clientRepository);
            _resourceOwnerRepository = resourceOwnerRepository;
            _tokenStore = tokenStore;
            _clientRepository = clientRepository;
            _eventPublisher = eventPublisher;
            _addUserOperation = new AddUserOperation(settings, resourceOwnerRepository, accountFilters, subjectBuilder, eventPublisher);
        }

        /// <summary>
        /// Gets the specified cancellation token.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        [HttpGet]
        [Authorize(Policy = "manager")]
        public async Task<IActionResult> Get(CancellationToken cancellationToken)
        {
            var resourceOwners = (await _resourceOwnerRepository.GetAll(cancellationToken).ConfigureAwait(false));
            return new OkObjectResult(resourceOwners);
        }

        /// <summary>
        /// Gets the specified identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        /// <exception cref="SimpleAuthException"></exception>
        [HttpGet("{id}")]
        [Authorize(Policy = "manager")]
        public async Task<IActionResult> Get(string id, CancellationToken cancellationToken)
        {
            var resourceOwner = await _resourceOwnerRepository.Get(id, cancellationToken).ConfigureAwait(false);
            if (resourceOwner == null)
            {
                return BadRequest(
                    new ErrorDetails
                    {
                        Status = HttpStatusCode.BadRequest,
                        Detail = string.Format(ErrorDescriptions.TheResourceOwnerDoesntExist, id),
                        Title = ErrorCodes.InvalidRequestCode
                    });
            }

            return Ok(resourceOwner);
        }

        /// <summary>
        /// Deletes the specified identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        [Authorize(Policy = "manager")]
        public async Task<IActionResult> Delete(string id, CancellationToken cancellationToken)
        {
            if (!await _resourceOwnerRepository.Delete(id, cancellationToken).ConfigureAwait(false))
            {
                return BadRequest(
                    new ErrorDetails
                    {
                        Title = ErrorCodes.UnhandledExceptionCode,
                        Detail = ErrorDescriptions.TheResourceOwnerCannotBeRemoved,
                        Status = HttpStatusCode.BadRequest
                    });
            }

            return Ok();
        }

        /// <summary>
        /// Updates the claims.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        /// <exception cref="SimpleAuthException"></exception>
        [HttpPut("claims")]
        [Authorize(Policy = "manager")]
        public async Task<IActionResult> UpdateClaims(
            [FromBody] UpdateResourceOwnerClaimsRequest request,
            CancellationToken cancellationToken)
        {
            if (request == null)
            {
                return BadRequest(
                    new ErrorDetails
                    {
                        Title = ErrorCodes.InvalidRequestCode,
                        Detail = "Parameter in request body not valid",
                        Status = HttpStatusCode.BadRequest
                    });
            }

            var resourceOwner =
                await _resourceOwnerRepository.Get(request.Subject, cancellationToken).ConfigureAwait(false);
            if (resourceOwner == null)
            {
                throw new SimpleAuthException(
                    ErrorCodes.InvalidParameterCode,
                    string.Format(ErrorDescriptions.TheResourceOwnerDoesntExist, request.Subject));
            }

            //resourceOwner.UpdateDateTime = DateTime.UtcNow;
            var claims = request.Claims.Select(claim => new Claim(claim.Type, claim.Value)).ToList();
            var resourceOwnerClaims = resourceOwner.Claims
                .Where(c => !claims.Exists(x => x.Type == c.Type))
                .Concat(claims)
                .Where(c => c.Type != OpenIdClaimTypes.Subject)
                .Where(c => c.Type != OpenIdClaimTypes.UpdatedAt)
                .Concat(new[]
                {
                    new Claim(OpenIdClaimTypes.Subject, request.Subject),
                    new Claim(OpenIdClaimTypes.UpdatedAt, DateTime.UtcNow.ToString())
                });
            
            resourceOwner.Claims = resourceOwnerClaims.ToArray();

            var result = await _resourceOwnerRepository.Update(resourceOwner, cancellationToken).ConfigureAwait(false);
            if (!result)
            {
                return BadRequest(ErrorDescriptions.TheClaimsCannotBeUpdated);
            }

            return new OkResult();
        }

        /// <summary>
        /// Updates my claims.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        [HttpPost("claims")]
        [Authorize]
        public async Task<IActionResult> UpdateMyClaims(
            [FromBody] UpdateResourceOwnerClaimsRequest request,
            CancellationToken cancellationToken)
        {
            if (request == null)
            {
                return BadRequest("No parameter in body request");

            }

            var sub = User?.Claims?.GetSubject();

            if (sub == null || sub != request.Subject)
            {
                return BadRequest("Invalid user");
            }

            var resourceOwner = await _resourceOwnerRepository.Get(sub, cancellationToken).ConfigureAwait(false);

            var newTypes = request.Claims.Select(x => x.Type).ToArray();
            resourceOwner.Claims = resourceOwner.Claims.Where(x => newTypes.All(n => n != x.Type))
                .Concat(request.Claims.Select(x => new Claim(x.Type, x.Value)))
                .ToArray();

            var result = await _resourceOwnerRepository.Update(resourceOwner, cancellationToken).ConfigureAwait(false);
            var value = Request.Headers[HttpRequestHeader.Authorization.ToString()].FirstOrDefault();

            if (value == null)
            {
                return BadRequest();
            }

            var accessToken = value.Split(' ').Last();

            var existingToken = await _tokenStore.GetAccessToken(accessToken, cancellationToken).ConfigureAwait(false);

            var client = await _clientRepository.GetById(existingToken.ClientId, cancellationToken)
                .ConfigureAwait(false);
            var refreshedToken = await _refreshOperation.Execute(
                    new RefreshTokenGrantTypeParameter
                    {
                        ClientId = existingToken.ClientId,
                        ClientSecret = client.Secrets[0].Value,
                        RefreshToken = existingToken.RefreshToken
                    },
                    null,
                    Request.GetCertificate(),
                    Request.GetAbsoluteUriWithVirtualPath(),
                    cancellationToken)
                .ConfigureAwait(false);
            refreshedToken.ParentTokenId = existingToken.ParentTokenId;
            refreshedToken.RefreshToken = existingToken.RefreshToken;
            await _tokenStore.RemoveAccessToken(accessToken, cancellationToken).ConfigureAwait(false);
            await _tokenStore.RemoveAccessToken(refreshedToken.AccessToken, cancellationToken).ConfigureAwait(false);
            await _tokenStore.AddToken(refreshedToken, cancellationToken).ConfigureAwait(false);

            await _eventPublisher.Publish(new ClaimsUpdated(Id.Create(), DateTime.UtcNow)).ConfigureAwait(false);

            return result
                ? new JsonResult(new GrantedTokenResponse
                {
                    AccessToken = refreshedToken.AccessToken,
                    ExpiresIn = refreshedToken.ExpiresIn,
                    IdToken = refreshedToken.IdToken,
                    RefreshToken = refreshedToken.RefreshToken,
                    Scope = refreshedToken.Scope.Split(' '),
                    TokenType = refreshedToken.TokenType
                })
                : (IActionResult)BadRequest();
        }

        /// <summary>
        /// Updates the password.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        /// <exception cref="SimpleAuthException"></exception>
        [HttpPut("password")]
        [Authorize(Policy = "manager")]
        public async Task<IActionResult> UpdatePassword(
            [FromBody] UpdateResourceOwnerPasswordRequest request,
            CancellationToken cancellationToken)
        {
            if (request == null)
            {
                return BadRequest("Parameter in request body not valid");
            }

            var resourceOwner =
                await _resourceOwnerRepository.Get(request.Subject, cancellationToken).ConfigureAwait(false);
            if (resourceOwner == null)
            {
                throw new SimpleAuthException(
                    ErrorCodes.InvalidParameterCode,
                    string.Format(ErrorDescriptions.TheResourceOwnerDoesntExist, request.Subject));
            }

            var result = await _resourceOwnerRepository
                .SetPassword(request.Subject, request.Password, cancellationToken)
                .ConfigureAwait(false);
            if (!result)
            {
                return BadRequest(ErrorDescriptions.ThePasswordCannotBeUpdated);
            }

            return new OkResult();
        }

        /// <summary>
        /// Adds the specified add resource owner request.
        /// </summary>
        /// <param name="addResourceOwnerRequest">The add resource owner request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        [HttpPost]
        [Authorize(Policy = "manager")]
        public async Task<IActionResult> Add(
            [FromBody] AddResourceOwnerRequest addResourceOwnerRequest,
            CancellationToken cancellationToken)
        {
            if (addResourceOwnerRequest == null)
            {
                return BadRequest("Parameter in request body not valid");
            }

            var resourceOwner = new ResourceOwner
            {
                Subject = addResourceOwnerRequest.Subject,
                Password = addResourceOwnerRequest.Password,
                IsLocalAccount = true,
            };
            var (success, subject) =
                await _addUserOperation.Execute(resourceOwner, cancellationToken).ConfigureAwait(false);
            if (success)
            {
                return Ok(new { subject = subject });
            }

            return BadRequest(
                new ErrorDetails
                {
                    Title = ErrorCodes.UnhandledExceptionCode,
                    Detail = "a resource owner with same credentials already exists",
                    Status = HttpStatusCode.BadRequest
                });
        }

        /// <summary>
        /// Searches the specified search resource owners request.
        /// </summary>
        /// <param name="searchResourceOwnersRequest">The search resource owners request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        [HttpPost(".search")]
        [Authorize(Policy = "manager")]
        public async Task<IActionResult> Search(
            [FromBody] SearchResourceOwnersRequest searchResourceOwnersRequest,
            CancellationToken cancellationToken)
        {
            if (searchResourceOwnersRequest == null)
            {
                searchResourceOwnersRequest = new SearchResourceOwnersRequest { Descending = true, NbResults = 50, StartIndex = 0 };
                //return BadRequest(new ErrorDetails {Title = ErrorCodes.InvalidRequestCode, Detail = "Parameter in request body not valid", Status = HttpStatusCode.BadRequest});
            }

            var result = await _resourceOwnerRepository.Search(searchResourceOwnersRequest, cancellationToken)
                .ConfigureAwait(false);
            return Ok(result);
        }
    }
}
