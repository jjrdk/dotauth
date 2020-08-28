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
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using SimpleAuth.Parameters;
    using SimpleAuth.Shared;

    internal static class ParameterParserHelper
    {
        /// <summary>
        /// Parse the parameter and returns a list of prompt parameter.
        /// </summary>
        /// <param name="parameter">List of prompts separated by whitespace</param>
        /// <returns>List of prompts.</returns>
        public static ICollection<string> ParsePrompts(this string? parameter)
        {
            if (string.IsNullOrWhiteSpace(parameter))
            {
                return new List<string>();
            }

            var promptNames = PromptParameters.All(); //Enum.GetNames(typeof(PromptParameter));

            var prompts = parameter.Split(' ')
                .Where(c => !string.IsNullOrWhiteSpace(c) && promptNames.Contains(c))
                .ToList();
            return prompts;
        }

        /// <summary>
        /// Parse the parameter and returns a list of response types
        /// </summary>
        /// <param name="parameter">List of response types separated by whitespace</param>
        /// <returns>List of response types</returns>
        public static string[] ParseResponseTypes(this string? parameter)
        {
            //var responseTypeNames = Enum.GetNames(typeof (string));
            if (string.IsNullOrWhiteSpace(parameter))
            {
                return Array.Empty<string>();
            }

            var responses = parameter.Split(' ')
                .Where(r => !string.IsNullOrWhiteSpace(r) && ResponseTypeNames.All.Contains(r))
                .ToArray();
            return responses;
        }

        /// <summary>
        /// Parse the parameter and returns a list of scopes.
        /// </summary>
        /// <param name="parameter">Parameter to parse.</param>
        /// <returns>list of scopes or null</returns>
        public static string[] ParseScopes(this string? parameter)
        {
            return string.IsNullOrWhiteSpace(parameter)
                ? Array.Empty<string>()
                : parameter.Split(' ').Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();
        }
    }
}
