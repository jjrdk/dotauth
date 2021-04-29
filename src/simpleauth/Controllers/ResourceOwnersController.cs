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
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Shared;
    using Shared.Models;
    using Shared.Repositories;
    using Shared.Requests;
    using SimpleAuth.Shared.Errors;
    using SimpleAuth.WebSite.User;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http.Headers;
    using System.Security.Claims;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Net.Http.Headers;
    using Shared.Events.Logging;
    using SimpleAuth.Api.Token.Actions;
    using SimpleAuth.Events;
    using SimpleAuth.Filters;
    using SimpleAuth.Parameters;
    using SimpleAuth.Properties;
    using SimpleAuth.Shared.Events.OAuth;
    using SimpleAuth.Shared.Responses;

    /// <summary>
    /// Defines the resource owner controller.
    /// </summary>
    /// <seealso cref="ControllerBase" />
    [Route(CoreConstants.EndPoints.ResourceOwners)]
    [ThrottleFilter]
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
        /// <param name="jwksRepository"></param>
        /// <param name="clientRepository"></param>
        /// <param name="accountFilters">The account filters.</param>
        /// <param name="eventPublisher">The event publisher.</param>
        public ResourceOwnersController(
            RuntimeSettings settings,
            ISubjectBuilder subjectBuilder,
            IResourceOwnerRepository resourceOwnerRepository,
            ITokenStore tokenStore,
            IJwksStore jwksRepository,
            IClientRepository clientRepository,
            IEnumerable<AccountFilter> accountFilters,
            IEventPublisher eventPublisher)
        {
            _refreshOperation = new GetTokenByRefreshTokenGrantTypeAction(
                eventPublisher,
                tokenStore,
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
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
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
                        Detail = Strings.TheRoDoesntExist,
                        Title = ErrorCodes.InvalidRequest
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
        [HttpPost("{0}/delete")]
        [Authorize(Policy = "manager")]
        public async Task<IActionResult> Delete(string id, CancellationToken cancellationToken)
        {
            if (!await _resourceOwnerRepository.Delete(id, cancellationToken).ConfigureAwait(false))
            {
                return BadRequest(
                    new ErrorDetails
                    {
                        Title = ErrorCodes.UnhandledExceptionCode,
                        Detail = Strings.TheResourceOwnerCannotBeRemoved,
                        Status = HttpStatusCode.BadRequest
                    });
            }

            return Ok();
        }

        /// <summary>
        /// Deletes the specified identifier.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        [HttpDelete]
        [Authorize]
        public async Task<IActionResult> DeleteMe(CancellationToken cancellationToken)
        {
            var sub = User.GetSubject();

            if (sub == null)
            {
                return BadRequest("Invalid user");
            }

            var value = Request.Headers[HttpRequestHeader.Authorization.ToString()].FirstOrDefault();

            if (value == null)
            {
                return BadRequest();
            }

            var accessToken = value.Split(' ').Last();

            var resourceOwner = await _resourceOwnerRepository.Get(sub, cancellationToken).ConfigureAwait(false);

            if (await _resourceOwnerRepository.Delete(sub, cancellationToken).ConfigureAwait(false)
                && await _tokenStore.RemoveAccessToken(accessToken, cancellationToken).ConfigureAwait(false))
            {
                await _eventPublisher.Publish(
                        new ResourceOwnerDeleted(
                            Id.Create(),
                            sub,
                            resourceOwner?.Claims == null
                                ? Array.Empty<ClaimData>()
                                : resourceOwner.Claims.Select(x => new ClaimData { Type = x.Type, Value = x.Value })
                                    .ToArray(),
                            DateTimeOffset.UtcNow))
                    .ConfigureAwait(false);
                return Ok();
            }

            return BadRequest(
                new ErrorDetails
                {
                    Title = ErrorCodes.UnhandledExceptionCode,
                    Detail = Strings.TheResourceOwnerCannotBeRemoved,
                    Status = HttpStatusCode.BadRequest
                });
        }

        /// <summary>
        /// Updates the <see cref="ResourceOwner"/> with the passed claims.
        /// </summary>
        /// <param name="id">The id of the resource owner.</param>
        /// <param name="request">The update request.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the async request.</param>
        /// <returns>An <see cref="IActionResult"/> instance.</returns>
        [HttpPost("{id}/update")]
        [Authorize(Policy = "manager")]
        public async Task<IActionResult> Update(string id, UpdateResourceOwnerClaimsRequest? request, CancellationToken cancellationToken)
        {
            if (request == null)
            {
                return BadRequest(
                    new ErrorDetails
                    {
                        Title = ErrorCodes.InvalidRequest,
                        Detail = Strings.ParameterInRequestBodyIsNotValid,
                        Status = HttpStatusCode.BadRequest
                    });
            }

            request = request with { Subject = id };

            var resourceOwner =
                await _resourceOwnerRepository.Get(request.Subject, cancellationToken).ConfigureAwait(false);
            if (resourceOwner == null)
            {
                return BadRequest(
                    new ErrorDetails
                    {
                        Status = HttpStatusCode.BadRequest,
                        Title = ErrorCodes.InvalidParameterCode,
                        Detail = Strings.TheRoDoesntExist
                    });
            }

            var claims = request.Claims
                .Where(c => c.Type != OpenIdClaimTypes.Subject)
                .Where(c => c.Type != OpenIdClaimTypes.UpdatedAt)
                .Select(claim => new Claim(claim.Type, claim.Value))
                .Concat(
                    new[]
                    {
                        new Claim(OpenIdClaimTypes.Subject, request.Subject),
                        new Claim(
                            OpenIdClaimTypes.UpdatedAt,
                            DateTimeOffset.UtcNow.ConvertToUnixTimestamp().ToString())
                    });

            resourceOwner.Claims = claims.ToArray();

            var result = await _resourceOwnerRepository.Update(resourceOwner, cancellationToken).ConfigureAwait(false);
            return result switch
            {
                Option.Error => BadRequest(Strings.TheClaimsCannotBeUpdated),
                _ => RedirectToAction("Get", "ResourceOwners", new { id })
            };
        }

        /// <summary>
        /// Updates the claims.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        [HttpPut("claims")]
        [Authorize(Policy = "manager")]
        public async Task<IActionResult> UpdateClaims(
            [FromBody] UpdateResourceOwnerClaimsRequest? request,
            CancellationToken cancellationToken)
        {
            if (request?.Subject == null)
            {
                return BadRequest(
                    new ErrorDetails
                    {
                        Title = ErrorCodes.InvalidParameterCode,
                        Detail = string.Format(Strings.MissingParameter, "login"),
                        Status = HttpStatusCode.BadRequest
                    });
            }

            var resourceOwner =
                await _resourceOwnerRepository.Get(request.Subject, cancellationToken).ConfigureAwait(false);
            if (resourceOwner == null)
            {
                return BadRequest(
                       new ErrorDetails
                       {
                           Status = HttpStatusCode.BadRequest,
                           Title = ErrorCodes.InvalidParameterCode,
                           Detail = Strings.TheRoDoesntExist
                       });
            }

            var claims = request.Claims.Select(claim => new Claim(claim.Type, claim.Value)).ToList();
            var resourceOwnerClaims = resourceOwner.Claims
                .Where(c => !claims.Exists(x => x.Type == c.Type))
                .Concat(claims)
                .Where(c => c.Type != OpenIdClaimTypes.Subject)
                .Where(c => c.Type != OpenIdClaimTypes.UpdatedAt)
                .Concat(new[]
                {
                    new Claim(OpenIdClaimTypes.Subject, request.Subject),
                    new Claim(OpenIdClaimTypes.UpdatedAt, DateTimeOffset.UtcNow.ConvertToUnixTimestamp().ToString())
                });

            resourceOwner.Claims = resourceOwnerClaims.ToArray();

            var result = await _resourceOwnerRepository.Update(resourceOwner, cancellationToken).ConfigureAwait(false);

            return result is Option.Success ? new OkResult() : BadRequest(Strings.TheClaimsCannotBeUpdated);
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
            [FromBody] UpdateResourceOwnerClaimsRequest? request,
            CancellationToken cancellationToken)
        {
            if (request == null)
            {
                return BadRequest(Strings.NoParameterInBodyRequest);
            }

            var sub = User.GetSubject();

            if (sub == null || sub != request.Subject)
            {
                return BadRequest(Strings.InvalidUser);
            }

            var resourceOwner = await _resourceOwnerRepository.Get(sub, cancellationToken).ConfigureAwait(false);
            var previousClaims = resourceOwner!.Claims
                .Select(claim => new ClaimData { Type = claim.Type, Value = claim.Value }).ToArray();
            var newTypes = request.Claims.Select(x => x.Type).ToArray();
            resourceOwner.Claims = resourceOwner.Claims.Where(x => newTypes.All(n => n != x.Type))
                .Concat(request.Claims.Select(x => new Claim(x.Type, x.Value)))
                .ToArray();
            return await UpdateMyResourceOwner(resourceOwner, previousClaims, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Updates my claims.
        /// </summary>
        /// <param name="type">The claims types to delete.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        [HttpDelete("claims")]
        [Authorize]
        public async Task<IActionResult> DeleteMyClaims(
            [FromQuery] string[] type,
            CancellationToken cancellationToken)
        {
            var sub = User.GetSubject();

            if (sub == null)
            {
                return BadRequest(Strings.InvalidUser);
            }

            var resourceOwner = await _resourceOwnerRepository.Get(sub, cancellationToken).ConfigureAwait(false);
            var previousClaims = resourceOwner!.Claims
                .Select(claim => new ClaimData { Type = claim.Type, Value = claim.Value }).ToArray();
            var toDelete = type.Where(t => User.HasClaim(x => x.Type == t)).ToArray();
            resourceOwner.Claims = resourceOwner.Claims
                .Where(x => !toDelete.Contains(x.Type))
                .ToArray();

            return await UpdateMyResourceOwner(resourceOwner, previousClaims, cancellationToken).ConfigureAwait(false);
        }

        private async Task<IActionResult> UpdateMyResourceOwner(
            ResourceOwner resourceOwner, ClaimData[] previousClaims, CancellationToken cancellationToken)
        {
            var result = await _resourceOwnerRepository.Update(resourceOwner, cancellationToken).ConfigureAwait(false);
            if (!AuthenticationHeaderValue.TryParse(Request.Headers[HeaderNames.Authorization], out var value))
            {
                return BadRequest();
            }

            var accessToken = value.Parameter!;

            var existingToken = await _tokenStore.GetAccessToken(accessToken, cancellationToken).ConfigureAwait(false);
            if (existingToken == null)
            {
                return BadRequest();
            }
            var client = await _clientRepository.GetById(existingToken.ClientId, cancellationToken)
                .ConfigureAwait(false);
            if (client == null)
            {
                return BadRequest();
            }
            var refreshedResponse = await _refreshOperation.Execute(
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
            if (refreshedResponse.HasError)
            {
                return new BadRequestObjectResult(refreshedResponse.Error);
            }

            var refreshedToken = refreshedResponse.Content;
            refreshedToken = refreshedToken with
            {
                ParentTokenId = existingToken.ParentTokenId,
                RefreshToken = existingToken.RefreshToken
            };
            await _tokenStore.RemoveAccessToken(accessToken, cancellationToken).ConfigureAwait(false);
            await _tokenStore.RemoveAccessToken(refreshedToken.AccessToken, cancellationToken).ConfigureAwait(false);
            await _tokenStore.AddToken(refreshedToken, cancellationToken).ConfigureAwait(false);

            await _eventPublisher.Publish(
                    new ClaimsUpdated(
                        Id.Create(),
                        resourceOwner.Subject!,
                        previousClaims,
                        resourceOwner.Claims.Select(claim => new ClaimData { Type = claim.Type, Value = claim.Value })
                            .ToArray(),
                        DateTimeOffset.UtcNow))
                .ConfigureAwait(false);

            return result switch
            {
                Option.Error e => BadRequest(e),
                _ => new JsonResult(
                    new GrantedTokenResponse
                    {
                        AccessToken = refreshedToken.AccessToken,
                        ExpiresIn = refreshedToken.ExpiresIn,
                        IdToken = refreshedToken.IdToken,
                        RefreshToken = refreshedToken.RefreshToken,
                        Scope = refreshedToken.Scope,
                        TokenType = refreshedToken.TokenType
                    })
            };
        }

        /// <summary>
        /// Updates the password.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        [HttpPut("password")]
        [Authorize(Policy = "manager")]
        public async Task<IActionResult> UpdatePassword(
            [FromBody] UpdateResourceOwnerPasswordRequest? request,
            CancellationToken cancellationToken)
        {
            if (request?.Subject == null)
            {
                return BadRequest(new ErrorDetails
                {
                    Status = HttpStatusCode.BadRequest,
                    Title = ErrorCodes.InvalidParameterCode,
                    Detail = string.Format(Strings.MissingParameter, "login")
                });
            }

            if (request.Password == null)
            {
                return BadRequest(new ErrorDetails
                {
                    Status = HttpStatusCode.BadRequest,
                    Title = ErrorCodes.InvalidParameterCode,
                    Detail = string.Format(Strings.MissingParameter, "password")
                });
            }

            var resourceOwner =
                await _resourceOwnerRepository.Get(request.Subject, cancellationToken).ConfigureAwait(false);
            if (resourceOwner == null)
            {
                return BadRequest(new ErrorDetails
                {
                    Status = HttpStatusCode.BadRequest,
                    Title = ErrorCodes.InvalidParameterCode,
                    Detail = Strings.TheRoDoesntExist
                });
            }

            var result = await _resourceOwnerRepository
                .SetPassword(request.Subject, request.Password, cancellationToken)
                .ConfigureAwait(false);
            if (!result)
            {
                return BadRequest(Strings.ThePasswordCannotBeUpdated);
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
            var resourceOwner = new ResourceOwner
            {
                Subject = addResourceOwnerRequest.Subject ?? string.Empty,
                Password = addResourceOwnerRequest.Password,
                IsLocalAccount = true,
            };
            var (success, subject) =
                await _addUserOperation.Execute(resourceOwner, cancellationToken).ConfigureAwait(false);
            if (success)
            {
                return Ok(new { subject });
            }

            return BadRequest(
                new ErrorDetails
                {
                    Title = ErrorCodes.UnhandledExceptionCode,
                    Detail = Strings.DuplicateResourceOwner,
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
            [FromBody] SearchResourceOwnersRequest? searchResourceOwnersRequest,
            CancellationToken cancellationToken)
        {
            searchResourceOwnersRequest ??= new SearchResourceOwnersRequest { Descending = true, NbResults = 50, StartIndex = 0 };

            var result = await _resourceOwnerRepository.Search(searchResourceOwnersRequest, cancellationToken)
                .ConfigureAwait(false);
            return Ok(result);
        }
    }
}
