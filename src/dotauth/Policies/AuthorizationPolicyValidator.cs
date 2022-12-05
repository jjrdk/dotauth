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

namespace DotAuth.Policies;

using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using DotAuth.Events;
using DotAuth.Extensions;
using DotAuth.Parameters;
using DotAuth.Shared.Events.Uma;
using DotAuth.Shared.Models;
using DotAuth.Shared.Repositories;
using DotAuth.Shared.Responses;

internal sealed class AuthorizationPolicyValidator : IAuthorizationPolicyValidator
{
    private readonly IAuthorizationPolicy _authorizationPolicy;
    private readonly IJwksStore _jwksStore;
    private readonly IResourceSetRepository _resourceSetRepository;
    private readonly IEventPublisher _eventPublisher;

    public AuthorizationPolicyValidator(
        IJwksStore jwksStore,
        IResourceSetRepository resourceSetRepository,
        IEventPublisher eventPublisher)
    {
        _authorizationPolicy = new DefaultAuthorizationPolicy();
        _jwksStore = jwksStore;
        _resourceSetRepository = resourceSetRepository;
        _eventPublisher = eventPublisher;
    }

    public async Task<AuthorizationPolicyResult> IsAuthorized(
        Ticket validTicket,
        Client client,
        ClaimTokenParameter claimTokenParameter,
        CancellationToken cancellationToken)
    {
        if (validTicket.Lines.Length == 0)
        {
            throw new ArgumentException(nameof(validTicket.Lines));
        }
        var handler = new JwtSecurityTokenHandler();
        var validationParameters = await client.CreateValidationParameters(_jwksStore, cancellationToken: cancellationToken).ConfigureAwait(false);
        var requester = handler.ValidateToken(claimTokenParameter.Token, validationParameters, out _);

        var resourceIds = validTicket.Lines.Select(l => l.ResourceSetId).ToArray();
        var resources = await _resourceSetRepository.Get(cancellationToken, resourceIds).ConfigureAwait(false);
        if (resources.Length == 0 || resources.Length != resourceIds.Length)
        {
            return new AuthorizationPolicyResult(AuthorizationPolicyResultKind.NotAuthorized, requester);
        }

        AuthorizationPolicyResult? validationResult = null;

        foreach (var ticketLine in validTicket.Lines)
        {
            var ticketLineParameter = new TicketLineParameter(
                client.ClientId,
                ticketLine.Scopes,
                validTicket.IsAuthorizedByRo);
            var resource = resources.First(r => r.Id == ticketLine.ResourceSetId);
            validationResult = await Validate(
                    ticketLineParameter,
                    resource,
                    claimTokenParameter.Format,
                    requester,
                    cancellationToken)
                .ConfigureAwait(false);

            switch (validationResult.Result)
            {
                case AuthorizationPolicyResultKind.RequestSubmitted:
                    await _eventPublisher.Publish(
                            new AuthorizationRequestSubmitted(
                                Id.Create(),
                                validTicket.Id,
                                client.ClientId,
                                requester.Claims.Select(claim => new ClaimData { Type = claim.Type, Value = claim.Value }),
                                DateTimeOffset.UtcNow))
                        .ConfigureAwait(false);

                    return validationResult;
                case AuthorizationPolicyResultKind.Authorized:
                    break;
                case AuthorizationPolicyResultKind.NotAuthorized:
                case AuthorizationPolicyResultKind.NeedInfo:
                default:
                {
                    await _eventPublisher.Publish(
                            new AuthorizationPolicyNotAuthorized(
                                Id.Create(),
                                validTicket.Id,
                                DateTimeOffset.UtcNow))
                        .ConfigureAwait(false);

                    return validationResult;
                }
            }
        }

        return validationResult!;
    }

    private async Task<AuthorizationPolicyResult> Validate(
        TicketLineParameter ticketLineParameter,
        ResourceSet resource,
        string? claimTokenFormat,
        ClaimsPrincipal requester,
        CancellationToken cancellationToken)
    {
        if (resource.AuthorizationPolicies.Length == 0)
        {
            return new AuthorizationPolicyResult(AuthorizationPolicyResultKind.RequestSubmitted, requester);
        }

        return await _authorizationPolicy.Execute(
                ticketLineParameter,
                claimTokenFormat,
                requester,
                cancellationToken, resource.AuthorizationPolicies)
            .ConfigureAwait(false);
    }
}