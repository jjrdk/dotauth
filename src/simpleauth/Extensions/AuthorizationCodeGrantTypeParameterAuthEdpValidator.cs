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

namespace DotAuth.Extensions;

using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using DotAuth.Parameters;
using DotAuth.Properties;
using DotAuth.Shared;
using DotAuth.Shared.Errors;
using DotAuth.Shared.Models;
using DotAuth.Shared.Repositories;
using Microsoft.Extensions.Logging;

internal sealed class AuthorizationCodeGrantTypeParameterAuthEdpValidator
{
    private readonly IClientStore _clientRepository;
    private readonly ILogger _logger;

    public AuthorizationCodeGrantTypeParameterAuthEdpValidator(IClientStore clientRepository, ILogger logger)
    {
        _clientRepository = clientRepository;
        _logger = logger;
    }

    public async Task<Option<Client>> Validate(
        AuthorizationParameter parameter,
        CancellationToken cancellationToken)
    {
        // Check the required parameters. Read this RFC : http://openid.net/specs/openid-connect-core-1_0.html#AuthRequest
        if (string.IsNullOrWhiteSpace(parameter.Scope))
        {
            var message = string.Format(
                Strings.MissingParameter,
                CoreConstants.StandardAuthorizationRequestParameterNames.ScopeName);
            _logger.LogError(message);
            return new Option<Client>.Error(
                new ErrorDetails
                {
                    Title = ErrorCodes.InvalidRequest, Detail = message, Status = HttpStatusCode.BadRequest
                },
                parameter.State);
        }

        if (string.IsNullOrWhiteSpace(parameter.ClientId))
        {
            var message = string.Format(
                Strings.MissingParameter,
                CoreConstants.StandardAuthorizationRequestParameterNames.ClientIdName);
            _logger.LogError(message);
            return new Option<Client>.Error(
                new ErrorDetails
                {
                    Title = ErrorCodes.InvalidRequest, Detail = message, Status = HttpStatusCode.BadRequest
                },
                parameter.State);
        }

        if (parameter.RedirectUrl == null)
        {
            var message = string.Format(
                Strings.MissingParameter,
                CoreConstants.StandardAuthorizationRequestParameterNames.RedirectUriName);
            _logger.LogError(message);
            return new Option<Client>.Error(
                new ErrorDetails
                {
                    Title = ErrorCodes.InvalidRequest, Detail = message, Status = HttpStatusCode.BadRequest
                },
                parameter.State);
        }

        if (string.IsNullOrWhiteSpace(parameter.ResponseType))
        {
            var message = string.Format(
                Strings.MissingParameter,
                CoreConstants.StandardAuthorizationRequestParameterNames.ResponseTypeName);
            _logger.LogError(message);
            return new Option<Client>.Error(
                new ErrorDetails
                {
                    Title = ErrorCodes.InvalidRequest, Detail = message, Status = HttpStatusCode.BadRequest
                },
                parameter.State);
        }

        var validationResult = ValidateResponseTypeParameter(parameter.ResponseType, parameter.State);
        if (validationResult is Option.Error e)
        {
            return new Option<Client>.Error(e.Details, e.State);
        }

        validationResult = ValidatePromptParameter(parameter.Prompt, parameter.State);
        if (validationResult is Option.Error e2)
        {
            return new Option<Client>.Error(e2.Details, e2.State);
        }

        // With this instruction
        // The redirect_uri is considered well-formed according to the RFC-3986
        var redirectUrlIsCorrect = parameter.RedirectUrl.IsAbsoluteUri;
        if (!redirectUrlIsCorrect)
        {
            var message = Strings.TheRedirectionUriIsNotWellFormed;
            _logger.LogError(message);
            return new Option<Client>.Error(
                new ErrorDetails
                {
                    Title = ErrorCodes.InvalidRequest, Detail = message, Status = HttpStatusCode.BadRequest
                },
                parameter.State);
        }

        var client = await _clientRepository.GetById(parameter.ClientId, cancellationToken).ConfigureAwait(false);
        if (client == null)
        {
            var message = string.Format(Strings.ClientIsNotValid, parameter.ClientId);
            _logger.LogError(message);
            return new Option<Client>.Error(
                new ErrorDetails
                {
                    Title = ErrorCodes.InvalidRequest, Detail = message, Status = HttpStatusCode.BadRequest
                },
                parameter.State);
        }

        if (!client.GetRedirectionUrls(parameter.RedirectUrl).Any())
        {
            var message = string.Format(Strings.RedirectUrlIsNotValid, parameter.RedirectUrl);
            _logger.LogError(message);
            return new Option<Client>.Error(
                new ErrorDetails
                {
                    Title = ErrorCodes.InvalidRequest, Detail = message, Status = HttpStatusCode.BadRequest
                },
                parameter.State);
        }

        return new Option<Client>.Result(client);
    }

    /// <summary>
    /// Validate the response type parameter.
    /// Returns an exception if at least one response_type parameter is not supported
    /// </summary>
    /// <param name="responseType"></param>
    /// <param name="state"></param>
    private Option ValidateResponseTypeParameter(string responseType, string? state)
    {
        if (string.IsNullOrWhiteSpace(responseType))
        {
            return new Option.Success();
        }

        //var responseTypeNames = Enum.GetNames(typeof(string));
        var atLeastOneResonseTypeIsNotSupported = responseType.Split(' ')
            .Any(r => !string.IsNullOrWhiteSpace(r) && !ResponseTypeNames.All.Contains(r));
        if (atLeastOneResonseTypeIsNotSupported)
        {
            var message = Strings.AtLeastOneResponseTypeIsNotSupported;
            _logger.LogError(message);
            return new Option.Error(
                new ErrorDetails
                {
                    Title = ErrorCodes.InvalidRequest, Detail = message, Status = HttpStatusCode.BadRequest
                },
                state);
        }

        return new Option.Success();
    }

    /// <summary>
    /// Validate the prompt parameter.
    /// </summary>
    /// <param name="prompt"></param>
    /// <param name="state"></param>
    private Option ValidatePromptParameter(string? prompt, string? state)
    {
        if (string.IsNullOrWhiteSpace(prompt))
        {
            return new Option.Success();
        }

        var promptNames = PromptParameters.All(); //Enum.GetNames(typeof(PromptParameter));
        var atLeastOnePromptIsNotSupported = prompt.Split(' ')
            .Any(r => !string.IsNullOrWhiteSpace(r) && !promptNames.Contains(r));
        if (atLeastOnePromptIsNotSupported)
        {
            var message = Strings.AtLeastOnePromptIsNotSupported;
            _logger.LogError(message);
            return new Option.Error(
                new ErrorDetails
                {
                    Title = ErrorCodes.InvalidRequest, Detail = message, Status = HttpStatusCode.BadRequest
                },
                state);
        }

        var prompts = prompt.ParsePrompts();
        if (prompts.Contains(PromptParameters.None)
            && (prompts.Contains(PromptParameters.Login)
                || prompts.Contains(PromptParameters.Consent)
                || prompts.Contains(PromptParameters.SelectAccount)))
        {
            var message = Strings.PromptParameterShouldHaveOnlyNoneValue;
            _logger.LogError(message);
            return new Option.Error(
                new ErrorDetails
                {
                    Title = ErrorCodes.InvalidRequest, Detail = message, Status = HttpStatusCode.BadRequest
                },
                state);
        }

        return new Option.Success();
    }
}