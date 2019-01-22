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

namespace SimpleAuth.Parsers
{
    using Microsoft.AspNetCore.Routing;
    using Results;

    internal static class ActionResultParser
    {
        public static ActionInformation GetControllerAndActionFromRedirectionActionResult(
            this EndpointResult endpointResult)
        {
            if (endpointResult.Type != TypeActionResult.RedirectToAction || endpointResult.RedirectInstruction == null)
            {
                return null;
            }

            return endpointResult.RedirectInstruction.GetActionInformation();
        }

        public static RouteValueDictionary GetRedirectionParameters(this EndpointResult endpointResult)
        {
            if (endpointResult.Type != TypeActionResult.RedirectToAction
                && endpointResult.Type != TypeActionResult.RedirectToCallBackUrl
                || endpointResult.RedirectInstruction == null)
            {
                return null;
            }

            return endpointResult.RedirectInstruction.GetRouteValueDictionary();
        }
    }
}
