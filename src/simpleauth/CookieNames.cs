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

namespace SimpleAuth
{
    using Microsoft.AspNetCore.Authentication.Cookies;

    /// <summary>
    /// Defines the cookie names.
    /// </summary>
    public static class CookieNames
    {
        /// <summary>
        /// Default cookie name
        /// </summary>
        public const string CookieName = CookieAuthenticationDefaults.AuthenticationScheme;

        /// <summary>
        /// External cookie name
        /// </summary>
        public const string ExternalCookieName = "saoie";

        /// <summary>
        /// Password less cookie name
        /// </summary>
        public const string PasswordLessCookieName = "sapl";

        /// <summary>
        /// Two-factor cookie name
        /// </summary>
        public const string TwoFactorCookieName = "sa2fa";
    }
}