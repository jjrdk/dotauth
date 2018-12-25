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

namespace SimpleAuth.Validators
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Errors;
    using Exceptions;
    using Helpers;
    using Parameters;
    using Shared.Models;
    using Shared.Repositories;

    public sealed class AuthorizationCodeGrantTypeParameterAuthEdpValidator : IAuthorizationCodeGrantTypeParameterAuthEdpValidator
    {
        private readonly IParameterParserHelper _parameterParserHelper;
        private readonly IClientStore _clientRepository;
        private readonly IClientValidator _clientValidator;

        public AuthorizationCodeGrantTypeParameterAuthEdpValidator(
            IParameterParserHelper parameterParserHelper,
            IClientStore clientRepository,
            IClientValidator clientValidator)
        {
            _parameterParserHelper = parameterParserHelper;
            _clientRepository = clientRepository;
            _clientValidator = clientValidator;
        }

        public async Task<Client> ValidateAsync(AuthorizationParameter parameter)
        {
            // Check the required parameters. Read this RFC : http://openid.net/specs/openid-connect-core-1_0.html#AuthRequest
            if (string.IsNullOrWhiteSpace(parameter.Scope))
            {
                throw new IdentityServerExceptionWithState(
                    ErrorCodes.InvalidRequestCode,
                    string.Format(ErrorDescriptions.MissingParameter, CoreConstants.StandardAuthorizationRequestParameterNames.ScopeName),
                    parameter.State);
            }

            if (string.IsNullOrWhiteSpace(parameter.ClientId))
            {
                throw new IdentityServerExceptionWithState(
                    ErrorCodes.InvalidRequestCode,
                    string.Format(ErrorDescriptions.MissingParameter, CoreConstants.StandardAuthorizationRequestParameterNames.ClientIdName),
                    parameter.State);
            }

            if (parameter.RedirectUrl == null)
            {
                throw new IdentityServerExceptionWithState(
                    ErrorCodes.InvalidRequestCode,
                    string.Format(ErrorDescriptions.MissingParameter, CoreConstants.StandardAuthorizationRequestParameterNames.RedirectUriName),
                    parameter.State);
            }

            if (string.IsNullOrWhiteSpace(parameter.ResponseType))
            {
                throw new IdentityServerExceptionWithState(
                    ErrorCodes.InvalidRequestCode,
                    string.Format(ErrorDescriptions.MissingParameter, CoreConstants.StandardAuthorizationRequestParameterNames.ResponseTypeName),
                    parameter.State);
            }

            ValidateResponseTypeParameter(parameter.ResponseType, parameter.State);
            ValidatePromptParameter(parameter.Prompt, parameter.State);

            // With this instruction
            // The redirect_uri is considered well-formed according to the RFC-3986
            var redirectUrlIsCorrect = parameter.RedirectUrl.IsAbsoluteUri;
            if (!redirectUrlIsCorrect)
            {
                throw new IdentityServerExceptionWithState(
                    ErrorCodes.InvalidRequestCode,
                    ErrorDescriptions.TheRedirectionUriIsNotWellFormed,
                    parameter.State);
            }

            var client = await _clientRepository.GetById(parameter.ClientId).ConfigureAwait(false);
            if (client == null)
            {
                throw new IdentityServerExceptionWithState(
                    ErrorCodes.InvalidRequestCode,
                    string.Format(ErrorDescriptions.ClientIsNotValid, parameter.ClientId),
                    parameter.State);
            }

            if (!_clientValidator.GetRedirectionUrls(client, parameter.RedirectUrl).Any())
            {
                throw new IdentityServerExceptionWithState(
                    ErrorCodes.InvalidRequestCode,
                    string.Format(ErrorDescriptions.RedirectUrlIsNotValid, parameter.RedirectUrl),
                    parameter.State);
            }

            return client;
        }

        /// <summary>
        /// Validate the response type parameter.
        /// Returns an exception if at least one response_type parameter is not supported
        /// </summary>
        /// <param name="responseType"></param>
        /// <param name="state"></param>
        private void ValidateResponseTypeParameter(
            string responseType,
            string state)
        {
            if (string.IsNullOrWhiteSpace(responseType))
            {
                return;
            }

            var responseTypeNames = Enum.GetNames(typeof(ResponseType));
            var atLeastOneResonseTypeIsNotSupported = responseType.Split(' ')
                .Any(r => !string.IsNullOrWhiteSpace(r) && !responseTypeNames.Contains(r));
            if (atLeastOneResonseTypeIsNotSupported)
            {
                throw new IdentityServerExceptionWithState(
                    ErrorCodes.InvalidRequestCode,
                    ErrorDescriptions.AtLeastOneResponseTypeIsNotSupported,
                    state);
            }
        }

        /// <summary>
        /// Validate the prompt parameter.
        /// </summary>
        /// <param name="prompt"></param>
        /// <param name="state"></param>
        private void ValidatePromptParameter(
            string prompt,
            string state)
        {
            if (string.IsNullOrWhiteSpace(prompt))
            {
                return;
            }

            var promptNames = Enum.GetNames(typeof(PromptParameter));
            var atLeastOnePromptIsNotSupported = prompt.Split(' ')
                .Any(r => !string.IsNullOrWhiteSpace(r) && !promptNames.Contains(r));
            if (atLeastOnePromptIsNotSupported)
            {
                throw new IdentityServerExceptionWithState(
                    ErrorCodes.InvalidRequestCode,
                    ErrorDescriptions.AtLeastOnePromptIsNotSupported,
                    state);
            }

            var prompts = _parameterParserHelper.ParsePrompts(prompt);
            if (prompts.Contains(PromptParameter.none) &&
                (prompts.Contains(PromptParameter.login) ||
                prompts.Contains(PromptParameter.consent) ||
                prompts.Contains(PromptParameter.select_account)))
            {
                throw new IdentityServerExceptionWithState(
                    ErrorCodes.InvalidRequestCode,
                    ErrorDescriptions.PromptParameterShouldHaveOnlyNoneValue,
                    state);
            }
        }
    }
}
