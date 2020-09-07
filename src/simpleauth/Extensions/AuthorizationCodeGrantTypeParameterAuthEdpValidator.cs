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

namespace SimpleAuth.Extensions
{
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using SimpleAuth.Exceptions;
    using SimpleAuth.Parameters;
    using SimpleAuth.Properties;
    using SimpleAuth.Shared;
    using SimpleAuth.Shared.Errors;
    using SimpleAuth.Shared.Models;
    using SimpleAuth.Shared.Repositories;

    internal sealed class AuthorizationCodeGrantTypeParameterAuthEdpValidator
    {
        private readonly IClientStore _clientRepository;

        public AuthorizationCodeGrantTypeParameterAuthEdpValidator(IClientStore clientRepository)
        {
            _clientRepository = clientRepository;
        }

        public async Task<Client> Validate(AuthorizationParameter parameter, CancellationToken cancellationToken)
        {
            // Check the required parameters. Read this RFC : http://openid.net/specs/openid-connect-core-1_0.html#AuthRequest
            if (string.IsNullOrWhiteSpace(parameter.Scope))
            {
                throw new SimpleAuthExceptionWithState(
                    ErrorCodes.InvalidRequest,
                    string.Format(
                        Strings.MissingParameter,
                        CoreConstants.StandardAuthorizationRequestParameterNames.ScopeName),
                    parameter.State);
            }

            if (string.IsNullOrWhiteSpace(parameter.ClientId))
            {
                throw new SimpleAuthExceptionWithState(
                    ErrorCodes.InvalidRequest,
                    string.Format(
                        Strings.MissingParameter,
                        CoreConstants.StandardAuthorizationRequestParameterNames.ClientIdName),
                    parameter.State);
            }

            if (parameter.RedirectUrl == null)
            {
                throw new SimpleAuthExceptionWithState(
                    ErrorCodes.InvalidRequest,
                    string.Format(
                        Strings.MissingParameter,
                        CoreConstants.StandardAuthorizationRequestParameterNames.RedirectUriName),
                    parameter.State);
            }

            if (string.IsNullOrWhiteSpace(parameter.ResponseType))
            {
                throw new SimpleAuthExceptionWithState(
                    ErrorCodes.InvalidRequest,
                    string.Format(
                        Strings.MissingParameter,
                        CoreConstants.StandardAuthorizationRequestParameterNames.ResponseTypeName),
                    parameter.State);
            }

            ValidateResponseTypeParameter(parameter.ResponseType, parameter.State);
            ValidatePromptParameter(parameter.Prompt, parameter.State);

            // With this instruction
            // The redirect_uri is considered well-formed according to the RFC-3986
            var redirectUrlIsCorrect = parameter.RedirectUrl.IsAbsoluteUri;
            if (!redirectUrlIsCorrect)
            {
                throw new SimpleAuthExceptionWithState(
                    ErrorCodes.InvalidRequest,
                    Strings.TheRedirectionUriIsNotWellFormed,
                    parameter.State);
            }

            var client = await _clientRepository.GetById(parameter.ClientId, cancellationToken).ConfigureAwait(false);
            if (client == null)
            {
                throw new SimpleAuthExceptionWithState(
                    ErrorCodes.InvalidRequest,
                    string.Format(Strings.ClientIsNotValid, parameter.ClientId),
                    parameter.State);
            }

            if (!client.GetRedirectionUrls(parameter.RedirectUrl).Any())
            {
                throw new SimpleAuthExceptionWithState(
                    ErrorCodes.InvalidRequest,
                    string.Format(Strings.RedirectUrlIsNotValid, parameter.RedirectUrl),
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
        private void ValidateResponseTypeParameter(string responseType, string? state)
        {
            if (string.IsNullOrWhiteSpace(responseType))
            {
                return;
            }

            //var responseTypeNames = Enum.GetNames(typeof(string));
            var atLeastOneResonseTypeIsNotSupported = responseType.Split(' ')
                .Any(r => !string.IsNullOrWhiteSpace(r) && !ResponseTypeNames.All.Contains(r));
            if (atLeastOneResonseTypeIsNotSupported)
            {
                throw new SimpleAuthExceptionWithState(
                    ErrorCodes.InvalidRequest,
                    Strings.AtLeastOneResponseTypeIsNotSupported,
                    state);
            }
        }

        /// <summary>
        /// Validate the prompt parameter.
        /// </summary>
        /// <param name="prompt"></param>
        /// <param name="state"></param>
        private void ValidatePromptParameter(string? prompt, string? state)
        {
            if (string.IsNullOrWhiteSpace(prompt))
            {
                return;
            }

            var promptNames = PromptParameters.All(); //Enum.GetNames(typeof(PromptParameter));
            var atLeastOnePromptIsNotSupported = prompt.Split(' ')
                .Any(r => !string.IsNullOrWhiteSpace(r) && !promptNames.Contains(r));
            if (atLeastOnePromptIsNotSupported)
            {
                throw new SimpleAuthExceptionWithState(
                    ErrorCodes.InvalidRequest,
                    Strings.AtLeastOnePromptIsNotSupported,
                    state);
            }

            var prompts = prompt.ParsePrompts();
            if (prompts.Contains(PromptParameters.None)
                && (prompts.Contains(PromptParameters.Login)
                    || prompts.Contains(PromptParameters.Consent)
                    || prompts.Contains(PromptParameters.SelectAccount)))
            {
                throw new SimpleAuthExceptionWithState(
                    ErrorCodes.InvalidRequest,
                    Strings.PromptParameterShouldHaveOnlyNoneValue,
                    state);
            }
        }
    }
}
