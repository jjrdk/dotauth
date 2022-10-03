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

namespace DotAuth.Api.ResourceSetController;

using System.Net;
using System.Threading;
using System.Threading.Tasks;
using DotAuth.Properties;
using DotAuth.Shared;
using DotAuth.Shared.Errors;
using DotAuth.Shared.Models;
using DotAuth.Shared.Repositories;
using Microsoft.Extensions.Logging;
using ResourceSet = Shared.Models.ResourceSet;

internal sealed class UpdateResourceSetAction
{
    private readonly IResourceSetRepository _resourceSetRepository;
    private readonly ILogger _logger;

    public UpdateResourceSetAction(IResourceSetRepository resourceSetRepository, ILogger logger)
    {
        _resourceSetRepository = resourceSetRepository;
        _logger = logger;
    }

    public async Task<Option> Execute(ResourceSet resourceSet, CancellationToken cancellationToken)
    {
        var checkResult = CheckResourceSetParameter(resourceSet);
        return checkResult switch
        {
            Option.Error => checkResult,
            _ => await _resourceSetRepository.Update(resourceSet, cancellationToken).ConfigureAwait(false)
        };
    }

    private Option CheckResourceSetParameter(ResourceSet resourceSet)
    {
        if (string.IsNullOrWhiteSpace(resourceSet.Id))
        {
            var message = string.Format(Strings.MissingParameter, "id");
            _logger.LogError(message);
            return new Option.Error(
                new ErrorDetails
                {
                    Title = ErrorCodes.InvalidRequest,
                    Detail = message,
                    Status = HttpStatusCode.NotFound
                });
        }

        if (string.IsNullOrWhiteSpace(resourceSet.Name))
        {
            var message = string.Format(Strings.MissingParameter, "name");
            _logger.LogError(
                message);
            return new Option.Error(
                new ErrorDetails
                {
                    Title = ErrorCodes.InvalidRequest,
                    Detail = message,
                    Status = HttpStatusCode.BadRequest
                });
        }

        switch (resourceSet.Scopes.Length)
        {
            case 0:
                var message = string.Format(Strings.MissingParameter, "scopes");
                _logger.LogError(message);
                return new Option.Error(
                    new ErrorDetails
                    {
                        Title = ErrorCodes.InvalidRequest,
                        Detail = message,
                        Status = HttpStatusCode.BadRequest
                    });
            default:
                return new Option.Success();
        }
    }
}