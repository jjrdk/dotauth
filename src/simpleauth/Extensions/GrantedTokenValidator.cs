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
    using SimpleAuth.Shared.Errors;
    using SimpleAuth.Shared.Models;

    internal static class GrantedTokenValidator
    {
        public static GrantedTokenValidationResult CheckGrantedToken(this GrantedToken grantedToken)
        {
            if (grantedToken == null)
            {
                return new GrantedTokenValidationResult
                {
                    MessageErrorCode = ErrorCodes.InvalidToken,
                    MessageErrorDescription = ErrorDescriptions.TheTokenIsNotValid,
                    IsValid = false
                };
            }

            var expirationDateTime = grantedToken.CreateDateTime.AddSeconds(grantedToken.ExpiresIn);
            var tokenIsExpired = DateTime.UtcNow > expirationDateTime;
            if (tokenIsExpired)
            {
                return new GrantedTokenValidationResult
                {
                    MessageErrorCode = ErrorCodes.InvalidToken,
                    MessageErrorDescription = ErrorDescriptions.TheTokenIsExpired,
                    IsValid = false
                };
            }

            return new GrantedTokenValidationResult
            {
                IsValid = true
            };
        }
    }
}
