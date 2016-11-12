﻿#region copyright
// Copyright 2015 Habart Thierry
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
#endregion

using System;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Claims;
using SimpleIdentityServer.Core.Authenticate;
using SimpleIdentityServer.Core.Errors;
using SimpleIdentityServer.Core.Exceptions;
using SimpleIdentityServer.Core.Extensions;
using SimpleIdentityServer.Core.Helpers;
using SimpleIdentityServer.Core.JwtToken;
using SimpleIdentityServer.Core.Models;
using SimpleIdentityServer.Core.Parameters;
using SimpleIdentityServer.Core.Repositories;
using SimpleIdentityServer.Core.Validators;
using SimpleIdentityServer.Logging;
using System.Collections.Generic;
using SimpleIdentityServer.Core.Services;

namespace SimpleIdentityServer.Core.Api.Token.Actions
{
    public interface IGetTokenByResourceOwnerCredentialsGrantTypeAction
    {
        GrantedToken Execute(
            ResourceOwnerGrantTypeParameter resourceOwnerGrantTypeParameter,
            AuthenticationHeaderValue authenticationHeaderValue);
    }

    public class GetTokenByResourceOwnerCredentialsGrantTypeAction : IGetTokenByResourceOwnerCredentialsGrantTypeAction
    {
        private readonly IGrantedTokenRepository _grantedTokenRepository;
        private readonly IGrantedTokenGeneratorHelper _grantedTokenGeneratorHelper;
        private readonly IScopeValidator _scopeValidator;
        private readonly IAuthenticateResourceOwnerService _authenticateResourceOwnerService;
        private readonly ISimpleIdentityServerEventSource _simpleIdentityServerEventSource;
        private readonly IAuthenticateClient _authenticateClient;
        private readonly IJwtGenerator _jwtGenerator;
        private readonly IAuthenticateInstructionGenerator _authenticateInstructionGenerator;
        private readonly IClientRepository _clientRepository;
        private readonly IClientHelper _clientHelper;
        private readonly IGrantedTokenHelper _grantedTokenHelper;

        public GetTokenByResourceOwnerCredentialsGrantTypeAction(
            IGrantedTokenRepository grantedTokenRepository,
            IGrantedTokenGeneratorHelper grantedTokenGeneratorHelper,
            IScopeValidator scopeValidator,
            IAuthenticateResourceOwnerService authenticateResourceOwnerService,
            ISimpleIdentityServerEventSource simpleIdentityServerEventSource,
            IAuthenticateClient authenticateClient,
            IJwtGenerator jwtGenerator,
            IAuthenticateInstructionGenerator authenticateInstructionGenerator,
            IClientRepository clientRepository,
            IClientHelper clientHelper,
            IGrantedTokenHelper grantedTokenHelper)
        {
            _grantedTokenRepository = grantedTokenRepository;
            _grantedTokenGeneratorHelper = grantedTokenGeneratorHelper;
            _scopeValidator = scopeValidator;
            _authenticateResourceOwnerService = authenticateResourceOwnerService;
            _simpleIdentityServerEventSource = simpleIdentityServerEventSource;
            _authenticateClient = authenticateClient;
            _jwtGenerator = jwtGenerator;
            _authenticateInstructionGenerator = authenticateInstructionGenerator;
            _clientRepository = clientRepository;
            _clientHelper = clientHelper;
            _grantedTokenHelper = grantedTokenHelper;
        }

        public GrantedToken Execute(
            ResourceOwnerGrantTypeParameter resourceOwnerGrantTypeParameter,
            AuthenticationHeaderValue authenticationHeaderValue)
        {
            if (resourceOwnerGrantTypeParameter == null)
            {
                throw new ArgumentNullException("the parameter cannot be null");
            }

            // Try to authenticate the client
            string errorMessage;
            var instruction = CreateAuthenticateInstruction(resourceOwnerGrantTypeParameter,
                authenticationHeaderValue);
            var client = _authenticateClient.Authenticate(instruction, out errorMessage);
            if (client == null)
            {
                _simpleIdentityServerEventSource.Info(ErrorDescriptions.TheClientCannotBeAuthenticated);
                client = _clientRepository.GetClientById(Constants.AnonymousClientId);
                if (client == null)
                {
                    throw new IdentityServerException(ErrorCodes.InternalError,
                        string.Format(ErrorDescriptions.ClientIsNotValid, Constants.AnonymousClientId));
                }
            }

            // Try to authenticate a resource owner
            var resourceOwner = _authenticateResourceOwnerService.AuthenticateResourceOwner(resourceOwnerGrantTypeParameter.UserName, resourceOwnerGrantTypeParameter.Password);
            if (resourceOwner == null)
            {
                throw new IdentityServerException(
                    ErrorCodes.InvalidGrant,
                    ErrorDescriptions.ResourceOwnerCredentialsAreNotValid);
            }

            // Check if the requested scopes are valid
            var allowedTokenScopes = string.Empty;
            if (!string.IsNullOrWhiteSpace(resourceOwnerGrantTypeParameter.Scope))
            {
                string messageErrorDescription;
                var scopes = _scopeValidator.IsScopesValid(resourceOwnerGrantTypeParameter.Scope, client, out messageErrorDescription);
                if (!scopes.Any())
                {
                    throw new IdentityServerException(
                        ErrorCodes.InvalidScope,
                        messageErrorDescription);
                }

                allowedTokenScopes = string.Join(" ", scopes);
            }

            // Generate the user information payload and store it.
            var claims = resourceOwner.Claims;
            var claimsIdentity = new ClaimsIdentity(claims, "simpleIdentityServer");
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
            var authorizationParameter = new AuthorizationParameter
            {
                Scope = resourceOwnerGrantTypeParameter.Scope
            };
            var payload = _jwtGenerator.GenerateUserInfoPayloadForScope(claimsPrincipal, authorizationParameter);

            var generatedToken = _grantedTokenHelper.GetValidGrantedToken(
                client.ClientId,
                allowedTokenScopes,
                payload,
                payload);
            if (generatedToken == null)
            {
                generatedToken = _grantedTokenGeneratorHelper.GenerateToken(
                    client.ClientId,
                    allowedTokenScopes,
                    payload,
                    payload);
                _grantedTokenRepository.Insert(generatedToken);
                // Fill-in the id-token
                if (generatedToken.IdTokenPayLoad != null)
                {
                    generatedToken.IdToken = _clientHelper.GenerateIdToken(
                        generatedToken.ClientId,
                        generatedToken.IdTokenPayLoad);
                }

                _simpleIdentityServerEventSource.GrantAccessToClient(client.ClientId,
                    generatedToken.AccessToken,
                    allowedTokenScopes);
            }

            return generatedToken;
        }
        
        #region Private methods

        private AuthenticateInstruction CreateAuthenticateInstruction(
            ResourceOwnerGrantTypeParameter resourceOwnerGrantTypeParameter,
            AuthenticationHeaderValue authenticationHeaderValue)
        {
            var result = _authenticateInstructionGenerator.GetAuthenticateInstruction(authenticationHeaderValue);
            result.ClientAssertion = resourceOwnerGrantTypeParameter.ClientAssertion;
            result.ClientAssertionType = resourceOwnerGrantTypeParameter.ClientAssertionType;
            result.ClientIdFromHttpRequestBody = resourceOwnerGrantTypeParameter.ClientId;
            result.ClientSecretFromHttpRequestBody = resourceOwnerGrantTypeParameter.ClientSecret;
            return result;
        }

        #endregion
    }
}