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

namespace SimpleAuth.Api.Authorization.Actions
{
    using Common;
    using Errors;
    using Exceptions;
    using Parameters;
    using Results;
    using Shared.Models;
    using SimpleAuth.Common;
    using System;
    using System.Security.Claims;
    using System.Security.Principal;
    using System.Threading.Tasks;
    using Validators;

    internal sealed class GetTokenViaImplicitWorkflowOperation
    {
        private readonly ProcessAuthorizationRequest _processAuthorizationRequest;
        private readonly IGenerateAuthorizationResponse _generateAuthorizationResponse;
        private readonly ClientValidator _clientValidator;

        public GetTokenViaImplicitWorkflowOperation(
            ProcessAuthorizationRequest processAuthorizationRequest,
            IGenerateAuthorizationResponse generateAuthorizationResponse)
        {
            _processAuthorizationRequest = processAuthorizationRequest;
            _generateAuthorizationResponse = generateAuthorizationResponse;
            _clientValidator = new ClientValidator();
        }

        public async Task<EndpointResult> Execute(AuthorizationParameter authorizationParameter, IPrincipal principal, Client client, string issuerName)
        {
            if (authorizationParameter == null)
            {
                throw new ArgumentNullException(nameof(authorizationParameter));
            }

            if (client == null)
            {
                throw new ArgumentNullException(nameof(client));
            }

            if (string.IsNullOrWhiteSpace(authorizationParameter.Nonce))
            {
                throw new SimpleAuthExceptionWithState(
                    ErrorCodes.InvalidRequestCode,
                    string.Format(ErrorDescriptions.MissingParameter, CoreConstants.StandardAuthorizationRequestParameterNames.NonceName),
                    authorizationParameter.State);
            }

            if (!_clientValidator.CheckGrantTypes(client, GrantType.@implicit))
            {
                throw new SimpleAuthExceptionWithState(
                    ErrorCodes.InvalidRequestCode,
                    string.Format(ErrorDescriptions.TheClientDoesntSupportTheGrantType,
                        authorizationParameter.ClientId,
                        "implicit"),
                    authorizationParameter.State);
            }

            var result = await _processAuthorizationRequest.ProcessAsync(authorizationParameter, principal as ClaimsPrincipal, client, issuerName).ConfigureAwait(false);
            if (result.Type == TypeActionResult.RedirectToCallBackUrl)
            {
                var claimsPrincipal = principal as ClaimsPrincipal;
                await _generateAuthorizationResponse.ExecuteAsync(result, authorizationParameter, claimsPrincipal, client, issuerName).ConfigureAwait(false);
            }

            return result;
        }
    }
}
