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

using System.Collections.Generic;
using System.Linq;
using System.Net;
using DotAuth.Api.Authorization;
using DotAuth.Properties;
using DotAuth.Shared;
using DotAuth.Shared.Errors;
using DotAuth.Shared.Models;

internal static class AuthorizationFlowHelper
{
    public static Option<AuthorizationFlow> GetAuthorizationFlow(this ICollection<string> responseTypes, string? state)
    {
        var record = CoreConstants.MappingResponseTypesToAuthorizationFlows.Keys
            .SingleOrDefault(k => k.Length == responseTypes.Count && k.All(responseTypes.Contains));
        if (record == null)
        {
            return new Option<AuthorizationFlow>.Error(
                new ErrorDetails
                {
                    Title = ErrorCodes.InvalidRequest,
                    Detail = Strings.TheAuthorizationFlowIsNotSupported,
                    Status = HttpStatusCode.BadRequest
                },
                state);
        }

        return new Option<AuthorizationFlow>.Result(CoreConstants.MappingResponseTypesToAuthorizationFlows[record]);
    }
}