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

namespace SimpleAuth.WebSite.Authenticate
{
    using Actions;
    using Parameters;
    using Results;
    using System;
    using System.Security.Claims;
    using System.Threading.Tasks;

    public class AuthenticateActions : IAuthenticateActions
    {
        private readonly IAuthenticateResourceOwnerOpenIdAction _authenticateResourceOwnerOpenIdAction;
        private readonly IGenerateAndSendCodeAction _generateAndSendCodeAction;
        private readonly IValidateConfirmationCodeAction _validateConfirmationCodeAction;
        private readonly IRemoveConfirmationCodeAction _removeConfirmationCodeAction;
        //private readonly IEventPublisher _eventPublisher;

        public AuthenticateActions(
            IAuthenticateResourceOwnerOpenIdAction authenticateResourceOwnerOpenIdAction,
            IGenerateAndSendCodeAction generateAndSendCodeAction,
            IValidateConfirmationCodeAction validateConfirmationCodeAction,
            IRemoveConfirmationCodeAction removeConfirmationCodeAction)
        {
            _authenticateResourceOwnerOpenIdAction = authenticateResourceOwnerOpenIdAction;
            _generateAndSendCodeAction = generateAndSendCodeAction;
            _validateConfirmationCodeAction = validateConfirmationCodeAction;
            _removeConfirmationCodeAction = removeConfirmationCodeAction;
        }

        public async Task<EndpointResult> AuthenticateResourceOwnerOpenId(AuthorizationParameter parameter, ClaimsPrincipal claimsPrincipal, string code, string issuerName)
        {
            if (parameter == null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }

            if (claimsPrincipal == null)
            {
                throw new ArgumentNullException(nameof(claimsPrincipal));
            }

            return await _authenticateResourceOwnerOpenIdAction.Execute(parameter,
                claimsPrincipal,
                code, issuerName).ConfigureAwait(false);
        }

        public async Task<string> GenerateAndSendCode(string subject)
        {
            return await _generateAndSendCodeAction.ExecuteAsync(subject).ConfigureAwait(false);
        }

        public async Task<bool> ValidateCode(string code)
        {
            return await _validateConfirmationCodeAction.Execute(code).ConfigureAwait(false);
        }

        public async Task<bool> RemoveCode(string code)
        {
            return await _removeConfirmationCodeAction.Execute(code).ConfigureAwait(false);
        }
    }
}
