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

namespace SimpleAuth.Server.Parsers
{
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.AspNetCore.Routing;
    using Results;

    public class RedirectInstructionParser : IRedirectInstructionParser
    {
        private readonly Dictionary<SimpleAuthEndPoints, ActionInformation> _mappingEnumToActionInformations = new Dictionary<SimpleAuthEndPoints, ActionInformation>
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

        public ActionInformation GetActionInformation(RedirectInstruction instruction)
        {
            if (!_mappingEnumToActionInformations.ContainsKey(instruction.Action))
            {
                return null;
            }

            var actionInformation = _mappingEnumToActionInformations[instruction.Action];
            var dic = GetRouteValueDictionary(instruction);
            actionInformation.RouteValueDictionary = dic;
            return actionInformation;
        }

        public RouteValueDictionary GetRouteValueDictionary(RedirectInstruction instruction)
        {
            var result = new RouteValueDictionary();
            if (instruction.Parameters != null && instruction.Parameters.Any())
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