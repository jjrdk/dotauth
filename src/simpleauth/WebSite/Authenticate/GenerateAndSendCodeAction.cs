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
    using System;
    using System.Linq;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using SimpleAuth.Exceptions;
    using SimpleAuth.Properties;
    using SimpleAuth.Services;
    using SimpleAuth.Shared;
    using SimpleAuth.Shared.Errors;
    using SimpleAuth.Shared.Models;
    using SimpleAuth.Shared.Repositories;

    internal class GenerateAndSendCodeAction
    {
        private readonly IResourceOwnerRepository _resourceOwnerRepository;
        private readonly IConfirmationCodeStore _confirmationCodeStore;
        private readonly ITwoFactorAuthenticationHandler _twoFactorAuthenticationHandler;
        private readonly ILogger _logger;

        public GenerateAndSendCodeAction(
            IResourceOwnerRepository resourceOwnerRepository,
            IConfirmationCodeStore confirmationCodeStore,
            ITwoFactorAuthenticationHandler twoFactorAuthenticationHandler,
            ILogger logger)
        {
            _resourceOwnerRepository = resourceOwnerRepository;
            _confirmationCodeStore = confirmationCodeStore;
            _twoFactorAuthenticationHandler = twoFactorAuthenticationHandler;
            _logger = logger;
        }


        public async Task<Option<string>> Send(string subject, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(subject))
            {
                _logger.LogError(Strings.TheSubjectCannotBeRetrieved);
                return new Option<string>.Error(
                    new ErrorDetails
                    {
                        Title = ErrorCodes.UnhandledExceptionCode,
                        Detail = Strings.TheSubjectCannotBeRetrieved,
                        Status = HttpStatusCode.NotFound
                    });
            }

            var resourceOwner = await _resourceOwnerRepository.Get(subject, cancellationToken).ConfigureAwait(false);
            if (resourceOwner == null)
            {
                _logger.LogError(Strings.TheRoDoesntExist);
                return new Option<string>.Error(
                    new ErrorDetails
                    {
                        Title = ErrorCodes.UnhandledExceptionCode,
                        Detail = Strings.TheRoDoesntExist,
                        Status = HttpStatusCode.NotFound
                    });
            }

            if (string.IsNullOrWhiteSpace(resourceOwner.TwoFactorAuthentication))
            {
                _logger.LogError(Strings.TwoFactorAuthenticationIsNotEnabled);
                return new Option<string>.Error(
                    new ErrorDetails
                    {
                        Title = ErrorCodes.UnhandledExceptionCode,
                        Detail = Strings.TwoFactorAuthenticationIsNotEnabled,
                        Status = HttpStatusCode.NotFound
                    });
            }

            var confirmationCode = new ConfirmationCode
            {
                Value = await GetCode(subject, cancellationToken).ConfigureAwait(false),
                IssueAt = DateTimeOffset.UtcNow,
                ExpiresIn = 300
            };

            var service = _twoFactorAuthenticationHandler.Get(resourceOwner.TwoFactorAuthentication)!;
            if (resourceOwner.Claims.All(c => c.Type != service.RequiredClaim))
            {
                throw new ClaimRequiredException(service.RequiredClaim);
            }

            if (!await _confirmationCodeStore.Add(confirmationCode, cancellationToken).ConfigureAwait(false))
            {
                _logger.LogError(Strings.TheConfirmationCodeCannotBeSaved);
                return new Option<string>.Error(
                    new ErrorDetails
                    {
                        Title = ErrorCodes.UnhandledExceptionCode,
                        Detail = Strings.TheConfirmationCodeCannotBeSaved,
                        Status = HttpStatusCode.InternalServerError
                    });
            }

            await _twoFactorAuthenticationHandler.SendCode(confirmationCode.Value, resourceOwner.TwoFactorAuthentication, resourceOwner).ConfigureAwait(false);
            return new Option<string>.Result(confirmationCode.Value);
        }

        private async Task<string> GetCode(string subject, CancellationToken cancellationToken)
        {
            var random = new Random();
            var number = random.Next(100000, 999999);
            if (await _confirmationCodeStore.Get(number.ToString(), subject, cancellationToken).ConfigureAwait(false) != null)
            {
                return await GetCode(subject, cancellationToken).ConfigureAwait(false);
            }

            return number.ToString();
        }
    }
}
