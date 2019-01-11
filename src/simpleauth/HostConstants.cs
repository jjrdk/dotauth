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
    using System.Collections.Generic;
    using Microsoft.AspNetCore.Authentication.Cookies;
    using Results;

    public static class HostConstants
    {
        public static class CookieNames
        {
            public const string CookieName = CookieAuthenticationDefaults.AuthenticationScheme;
            public const string ExternalCookieName = "saoie";
            public const string PasswordLessCookieName = "sapl";
            public const string TwoFactorCookieName = "sa2fa";
        }

        public static Dictionary<SimpleAuthEndPoints, string> MappingEndPointToPartialUrl = new Dictionary<SimpleAuthEndPoints, string>
        {
            {
                SimpleAuthEndPoints.AuthenticateIndex,
                "/Authenticate/OpenId"
            },
            {
                SimpleAuthEndPoints.ConsentIndex,
                "/Consent"
            },
            {
                SimpleAuthEndPoints.FormIndex,
                "/Form"
            }
        };
    }
}