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

namespace SimpleAuth.Api.Authorization
{
    using Actions;
    using Common;
    using Errors;
    using Exceptions;
    using Helpers;
    using JwtToken;
    using Logging;
    using Parameters;
    using Results;
    using Shared;
    using Shared.Events.OAuth;
    using Shared.Repositories;
    using SimpleAuth.Common;
    using System;
    using System.Security.Principal;
    using System.Threading.Tasks;
    using Validators;

    public class AuthorizationActions : IAuthorizationActions
    {
        private readonly GetAuthorizationCodeOperation _getAuthorizationCodeOperation;
        private readonly GetTokenViaImplicitWorkflowOperation _getTokenViaImplicitWorkflowOperation;
        private readonly GetAuthorizationCodeAndTokenViaHybridWorkflowOperation
            _getAuthorizationCodeAndTokenViaHybridWorkflowOperation;
        private readonly AuthorizationCodeGrantTypeParameterAuthEdpValidator _authorizationCodeGrantTypeParameterValidator;
        private readonly IParameterParserHelper _parameterParserHelper;
        private readonly IOAuthEventSource _oauthEventSource;
        private readonly IAuthorizationFlowHelper _authorizationFlowHelper;
        private readonly IEventPublisher _eventPublisher;
        private readonly IAmrHelper _amrHelper;
        private readonly IResourceOwnerAuthenticateHelper _resourceOwnerAuthenticateHelper;

        public AuthorizationActions(
            IConsentHelper consentHelper,
            IGenerateAuthorizationResponse generateAuthorizationResponse,
            IParameterParserHelper parameterParserHelper,
            IClientStore clientStore,
            IOAuthEventSource oauthEventSource,
            IAuthorizationFlowHelper authorizationFlowHelper,
            IEventPublisher eventPublisher,
            IAmrHelper amrHelper,
            IResourceOwnerAuthenticateHelper resourceOwnerAuthenticateHelper)
        {
            var processAuthorizationRequest = new ProcessAuthorizationRequest(
                clientStore,
                consentHelper,
                oauthEventSource);
            _getAuthorizationCodeOperation = new GetAuthorizationCodeOperation(
                processAuthorizationRequest,
                generateAuthorizationResponse,
                oauthEventSource);
            _getTokenViaImplicitWorkflowOperation = new GetTokenViaImplicitWorkflowOperation(
                processAuthorizationRequest,
                generateAuthorizationResponse,
                oauthEventSource);
            _getAuthorizationCodeAndTokenViaHybridWorkflowOperation =
                new GetAuthorizationCodeAndTokenViaHybridWorkflowOperation(
                    oauthEventSource,
                    processAuthorizationRequest,
                    generateAuthorizationResponse);
            _authorizationCodeGrantTypeParameterValidator = new AuthorizationCodeGrantTypeParameterAuthEdpValidator(
                parameterParserHelper,
                clientStore);
            _parameterParserHelper = parameterParserHelper;
            _oauthEventSource = oauthEventSource;
            _authorizationFlowHelper = authorizationFlowHelper;
            _eventPublisher = eventPublisher;
            _amrHelper = amrHelper;
            _resourceOwnerAuthenticateHelper = resourceOwnerAuthenticateHelper;
        }

        public async Task<EndpointResult> GetAuthorization(AuthorizationParameter parameter, IPrincipal claimsPrincipal, string issuerName)
        {
            var processId = Guid.NewGuid().ToString();
            _eventPublisher.Publish(new AuthorizationRequestReceived(Guid.NewGuid().ToString(), processId, parameter, 0));
            try
            {
                var client = await _authorizationCodeGrantTypeParameterValidator.ValidateAsync(parameter).ConfigureAwait(false);
                EndpointResult endpointResult = null;
                _oauthEventSource.StartAuthorization(parameter.ClientId,
                    parameter.ResponseType,
                    parameter.Scope,
                    parameter.Claims == null ? string.Empty : parameter.Claims.ToString());
                if (client.RequirePkce && (string.IsNullOrWhiteSpace(parameter.CodeChallenge) || parameter.CodeChallengeMethod == null))
                {
                    throw new SimpleAuthExceptionWithState(ErrorCodes.InvalidRequestCode, string.Format(ErrorDescriptions.TheClientRequiresPkce, parameter.ClientId), parameter.State);
                }

                var responseTypes = _parameterParserHelper.ParseResponseTypes(parameter.ResponseType);
                var authorizationFlow = _authorizationFlowHelper.GetAuthorizationFlow(responseTypes, parameter.State);
                switch (authorizationFlow)
                {
                    case AuthorizationFlow.AuthorizationCodeFlow:
                        endpointResult = await _getAuthorizationCodeOperation.Execute(parameter, claimsPrincipal, client, issuerName).ConfigureAwait(false);
                        break;
                    case AuthorizationFlow.ImplicitFlow:
                        endpointResult = await _getTokenViaImplicitWorkflowOperation.Execute(parameter, claimsPrincipal, client, issuerName).ConfigureAwait(false);
                        break;
                    case AuthorizationFlow.HybridFlow:
                        endpointResult = await _getAuthorizationCodeAndTokenViaHybridWorkflowOperation.Execute(parameter, claimsPrincipal, client, issuerName).ConfigureAwait(false);
                        break;
                }

                if (endpointResult != null)
                {
                    var actionTypeName = Enum.GetName(typeof(TypeActionResult), endpointResult.Type);
                    var actionName = string.Empty;
                    if (endpointResult.Type == TypeActionResult.RedirectToAction)
                    {
                        var actionEnum = endpointResult.RedirectInstruction.Action;
                        actionName = Enum.GetName(typeof(SimpleAuthEndPoints), actionEnum);
                    }

                    var serializedParameters = endpointResult.RedirectInstruction?.Parameters == null ? string.Empty :
                        endpointResult.RedirectInstruction.Parameters.SerializeWithJavascript();
                    _oauthEventSource.EndAuthorization(actionTypeName,
                        actionName,
                        serializedParameters);
                }

                _eventPublisher.Publish(new AuthorizationGranted(Guid.NewGuid().ToString(), processId, endpointResult, 1));
                endpointResult.ProcessId = processId;
                endpointResult.Amr = _amrHelper.GetAmr(_resourceOwnerAuthenticateHelper.GetAmrs(), parameter.AmrValues);
                return endpointResult;
            }
            catch (SimpleAuthException ex)
            {
                _eventPublisher.Publish(new OAuthErrorReceived(Guid.NewGuid().ToString(), processId, ex.Code, ex.Message, 1));
                throw;
            }
        }
    }
}
