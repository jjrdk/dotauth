﻿// Copyright © 2015 Habart Thierry, © 2018 Jacob Reimers
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
    using Parameters;
    using Shared;
    using System.Collections.Generic;
    using System.Linq;

    internal static class ClaimsParameterExtensions
    {
        /// <summary>
        /// Gets all the standard claim names from the ClaimsParameter
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public static List<string> GetClaimNames(this ClaimsParameter parameter)
        {
            var result = new List<string>();
            if (parameter.IdToken != null)
            {
                result.AddRange(parameter.IdToken.Where(cl => IsStandardClaim(cl.Name)).Select(s => s.Name));
            }
            if (parameter.UserInfo != null)
            {
                result.AddRange(parameter.UserInfo.Where(cl => IsStandardClaim(cl.Name)).Select(s => s.Name));
            }

            return result;
        }

        /// <summary>
        /// Return a boolean which indicates if the ClaimsParameter contains at least one user-info claim parameter
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public static bool IsAnyUserInfoClaimParameter(this ClaimsParameter parameter)
        {
            return parameter?.UserInfo != null && parameter.UserInfo.Any();
        }

        /// <summary>
        /// Returns a boolean which indicates if the ClaimsParameter contains at least one identity-token claim parameter
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public static bool IsAnyIdentityTokenClaimParameter(this ClaimsParameter parameter)
        {
            return parameter?.IdToken != null && parameter.IdToken.Any();
        }

        private static bool IsStandardClaim(string claimName)
        {
            return JwtConstants.AllStandardResourceOwnerClaimNames.Contains(claimName) ||
                JwtConstants.AllStandardClaimNames.Contains(claimName);
        }
    }
}
