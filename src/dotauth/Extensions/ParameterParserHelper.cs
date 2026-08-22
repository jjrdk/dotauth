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

using System;
using System.Collections.Generic;
using System.Linq;
using DotAuth.Parameters;
using DotAuth.Shared;

internal static class ParameterParserHelper
{
    /// <param name="parameter">List of prompts separated by whitespace</param>
    extension(string? parameter)
    {
        /// <summary>
        /// Parse the parameter and returns a list of prompt parameter.
        /// </summary>
        /// <returns>List of prompts.</returns>
        public ICollection<string> ParsePrompts()
        {
            if (string.IsNullOrWhiteSpace(parameter))
            {
                return new List<string>();
            }

            var promptNames = PromptParameters.All(); //Enum.GetNames(typeof(PromptParameter));

            var prompts = parameter.Split(' ', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                .Where(c => promptNames.Contains(c))
                .ToList();
            return prompts;
        }

        /// <summary>
        /// Parse the parameter and returns a list of response types
        /// </summary>
        /// <returns>List of response types</returns>
        public string[] ParseResponseTypes()
        {
            //var responseTypeNames = Enum.GetNames(typeof (string));
            if (string.IsNullOrWhiteSpace(parameter))
            {
                return [];
            }

            var responses = parameter.Split(' ', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                .Where(r => !string.IsNullOrWhiteSpace(r) && ResponseTypeNames.All.Contains(r))
                .ToArray();
            return responses;
        }

        /// <summary>
        /// Parse the parameter and returns a list of scopes.
        /// </summary>
        /// <returns>list of scopes or null</returns>
        public string[] ParseScopes()
        {
            return string.IsNullOrWhiteSpace(parameter)
                ? []
                : parameter.Split(' ', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        }
    }
}
