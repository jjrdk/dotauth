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
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.AspNetCore.Routing;
    using SimpleAuth.Results;

    internal static class RedirectInstructionParser
    {
        private static readonly Dictionary<SimpleAuthEndPoints, ActionInformation> MappingEnumToActionInformations = new Dictionary<SimpleAuthEndPoints, ActionInformation>
        {
            {
                SimpleAuthEndPoints.ConsentIndex,
                new ActionInformation("Consent", "Index", null)
            },
            {
                SimpleAuthEndPoints.AuthenticateIndex,
                new ActionInformation("Authenticate", "Index", null)
            },
            {
                SimpleAuthEndPoints.FormIndex,
                new ActionInformation("Form", "Index", null)
            }
        };

        public static ActionInformation GetActionInformation(this RedirectInstruction instruction)
        {
            if (!MappingEnumToActionInformations.ContainsKey(instruction.Action))
            {
                return null;
            }

            var actionInformation = MappingEnumToActionInformations[instruction.Action];
            var dic = GetRouteValueDictionary(instruction);
            actionInformation.RouteValueDictionary = dic;
            return actionInformation;
        }

        public static RouteValueDictionary GetRouteValueDictionary(this RedirectInstruction instruction)
        {
            var result = new RouteValueDictionary();
            if (instruction.Parameters != null)
            {
                foreach (var parameter in instruction.Parameters)
                {
                    result.Add(parameter.Name, parameter.Value);
                }
            }

            return result;
        }
    }
}