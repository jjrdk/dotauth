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

namespace DotAuth.WebSite.Consent.Actions;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using DotAuth.Api.Authorization;
using DotAuth.Common;
using DotAuth.Events;
using DotAuth.Extensions;
using DotAuth.Parameters;
using DotAuth.Properties;
using DotAuth.Results;
using DotAuth.Shared;
using DotAuth.Shared.Errors;
using DotAuth.Shared.Events.Openid;
using DotAuth.Shared.Models;
using DotAuth.Shared.Repositories;
using Microsoft.Extensions.Logging;

internal sealed class ConfirmConsentAction
{
    private readonly IConsentRepository _consentRepository;
    private readonly IClientStore _clientRepository;
    private readonly IScopeRepository _scopeRepository;
    private readonly GenerateAuthorizationResponse _generateAuthorizationResponse;
    private readonly IEventPublisher _eventPublisher;

    public ConfirmConsentAction(
        IAuthorizationCodeStore authorizationCodeStore,
        ITokenStore tokenStore,
        IConsentRepository consentRepository,
        IClientStore clientRepository,
        IScopeRepository scopeRepository,
        IJwksStore jwksStore,
        IEventPublisher eventPublisher,
        ILogger logger)
    {
        _consentRepository = consentRepository;
        _clientRepository = clientRepository;
        _scopeRepository = scopeRepository;
        _generateAuthorizationResponse = new GenerateAuthorizationResponse(
            authorizationCodeStore,
            tokenStore,
            scopeRepository,
            clientRepository,
            consentRepository,
            jwksStore,
            eventPublisher,
            logger);
        _eventPublisher = eventPublisher;
    }

    /// <summary>
    /// This method is executed when the user confirm the consent
    /// 1). If there's already consent confirmed in the past by the resource owner
    /// 1).* then we generate an authorization code and redirects to the callback.
    /// 2). If there's no consent then we insert it and the authorization code is returned
    ///  2°.* to the callback url.
    /// </summary>
    /// <param name="authorizationParameter">Authorization code grant-type</param>
    /// <param name="claimsPrincipal">Resource owner's claims</param>
    /// <param name="issuerName"></param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the async operation.</param>
    /// <returns>Redirects the authorization code to the callback.</returns>
    public async Task<EndpointResult> Execute(
        AuthorizationParameter authorizationParameter,
        ClaimsPrincipal claimsPrincipal,
        string issuerName,
        CancellationToken cancellationToken)
    {
        var client = authorizationParameter.ClientId == null
            ? null
            : await _clientRepository.GetById(authorizationParameter.ClientId, cancellationToken)
                .ConfigureAwait(false);
        if (client == null)
        {
            throw new InvalidOperationException(
                string.Format(
                    Strings.TheClientIdDoesntExist,
                    authorizationParameter.ClientId));
        }

        var subject = claimsPrincipal.GetSubject()!;
        var assignedConsent = await _consentRepository
            .GetConfirmedConsents(subject, authorizationParameter, cancellationToken)
            .ConfigureAwait(false);
        // Insert a new consent.
        if (assignedConsent == null)
        {
            var claimsParameter = authorizationParameter.Claims;
            if (claimsParameter.IsAnyIdentityTokenClaimParameter() || claimsParameter.IsAnyUserInfoClaimParameter())
            {
                // A consent can be given to a set of claims
                assignedConsent = new Consent
                {
                    Id = Id.Create(),
                    ClientId = client.ClientId,
                    ClientName = client.ClientName,
                    Subject = subject,
                    Claims = claimsParameter.GetClaimNames()
                };
            }
            else
            {
                // A consent can be given to a set of scopes
                assignedConsent = new Consent
                {
                    Id = Id.Create(),
                    ClientId = client.ClientId,
                    ClientName = client.ClientName,
                    GrantedScopes =
                        authorizationParameter.Scope == null
                            ? []
                            : (await GetScopes(authorizationParameter.Scope, cancellationToken)
                                .ConfigureAwait(false)).ToArray(),
                    Subject = subject,
                };
            }

            // A consent can be given to a set of claims
            await _consentRepository.Insert(assignedConsent, cancellationToken).ConfigureAwait(false);

            await _eventPublisher.Publish(
                    new ConsentAccepted(
                        Id.Create(),
                        subject,
                        client.ClientId,
                        assignedConsent.GrantedScopes,
                        DateTimeOffset.UtcNow))
                .ConfigureAwait(false);
        }

        var result = await _generateAuthorizationResponse.Generate(
                EndpointResult.CreateAnEmptyActionResultWithRedirectionToCallBackUrl(),
                authorizationParameter,
                claimsPrincipal,
                client,
                issuerName,
                cancellationToken)
            .ConfigureAwait(false);

        // If redirect to the callback and the response mode has not been set.
        if (result.Type != ActionResultType.RedirectToCallBackUrl)
        {
            return result;
        }

        var responseMode = authorizationParameter.ResponseMode;
        if (responseMode == ResponseModes.None)
        {
            var responseTypes = authorizationParameter.ResponseType.ParseResponseTypes();
            var authorizationFlow = GetAuthorizationFlow(responseTypes, authorizationParameter.State);
            switch (authorizationFlow)
            {
                case Option<AuthorizationFlow>.Error e:
                    return EndpointResult.CreateBadRequestResult(e.Details);
                case Option<AuthorizationFlow>.Result r:
                    responseMode = GetResponseMode(r.Item);
                    break;
            }
        }

        result = result with
        {
            RedirectInstruction = result.RedirectInstruction! with { ResponseMode = responseMode }
        };

        return result;
    }

    private async Task<string[]> GetScopes(string concatenateListOfScopes, CancellationToken cancellationToken)
    {
        var scopeNames = concatenateListOfScopes.ParseScopes();
        var scopes = await _scopeRepository.SearchByNames(cancellationToken, scopeNames).ConfigureAwait(false);
        return scopes.Select(x => x.Name).ToArray();
    }

    private static Option<AuthorizationFlow> GetAuthorizationFlow(ICollection<string> responseTypes, string? state)
    {
        var record = CoreConstants.MappingResponseTypesToAuthorizationFlows.Keys.SingleOrDefault(
            k => k.Length == responseTypes.Count && k.All(responseTypes.Contains));
        if (record == null)
        {
            return new Option<AuthorizationFlow>.Error(
                new ErrorDetails
                {
                    Title = ErrorCodes.InvalidRequest,
                    Detail = Strings.TheAuthorizationFlowIsNotSupported,
                    Status = HttpStatusCode.BadRequest
                },
                state ?? string.Empty);
        }

        return new Option<AuthorizationFlow>.Result(CoreConstants.MappingResponseTypesToAuthorizationFlows[record]);
    }

    private static string GetResponseMode(AuthorizationFlow authorizationFlow)
    {
        return CoreConstants.MappingAuthorizationFlowAndResponseModes[authorizationFlow];
    }
}