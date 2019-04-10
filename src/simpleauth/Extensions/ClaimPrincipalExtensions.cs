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
    using Shared;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;

    internal static class ClaimPrincipalExtensions
    {
        /// <summary>
        /// Returns if the user is authenticated
        /// </summary>
        /// <param name="principal">The user principal</param>
        /// <returns>The user is authenticated</returns>
        public static bool IsAuthenticated(this ClaimsPrincipal principal)
        {
            if (principal?.Identity == null)
            {
                return false;
            }

            return principal.Identity.IsAuthenticated;
        }

        /// <summary>
        /// Returns the subject from an authenticated user
        /// Otherwise returns null.
        /// </summary>
        /// <param name="principal">The user principal</param>
        /// <returns>User's subject</returns>
        public static string GetSubject(this ClaimsPrincipal principal)
        {
            return principal?.Identity == null ? null : principal.Claims.GetSubject();
        }

        public static string GetSubject(this IEnumerable<Claim> claims)
        {
            if (claims == null)
            {
                return null;
            }

            var claim = GetSubjectClaim(claims.ToArray());
            return claim?.Value;
        }

        private static Claim GetSubjectClaim(this Claim[] claims)
        {
            var claim = claims.FirstOrDefault(c => c.Type == OpenIdClaimTypes.Subject)
                        ?? claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);

            return claim;
        }

        public static string GetName(this ClaimsPrincipal principal)
        {
            return GetClaimValue(principal, OpenIdClaimTypes.Name)
                   ?? GetClaimValue(principal, ClaimTypes.Name)
                ?? GetClaimValue(principal, ClaimTypes.NameIdentifier);
        }

        public static string GetEmail(this ClaimsPrincipal principal)
        {
            return GetClaimValue(principal, OpenIdClaimTypes.Email);
        }

        public static bool GetEmailVerified(this ClaimsPrincipal principal)
        {
            return GetBooleanClaimValue(principal, OpenIdClaimTypes.EmailVerified);
        }

        public static string GetFamilyName(this ClaimsPrincipal principal)
        {
            return GetClaimValue(principal, OpenIdClaimTypes.FamilyName);
        }

        public static string GetGender(this ClaimsPrincipal principal)
        {
            return GetClaimValue(principal, OpenIdClaimTypes.Gender);
        }

        public static string GetGivenName(this ClaimsPrincipal principal)
        {
            return GetClaimValue(principal, OpenIdClaimTypes.GivenName);
        }

        public static string GetLocale(this ClaimsPrincipal principal)
        {
            return GetClaimValue(principal, OpenIdClaimTypes.Locale);
        }

        public static string GetMiddleName(this ClaimsPrincipal principal)
        {
            return GetClaimValue(principal, OpenIdClaimTypes.MiddleName);
        }

        public static string GetNickName(this ClaimsPrincipal principal)
        {
            return GetClaimValue(principal, OpenIdClaimTypes.NickName);
        }

        public static string GetPhoneNumber(this ClaimsPrincipal principal)
        {
            return GetClaimValue(principal, OpenIdClaimTypes.PhoneNumber);
        }

        public static bool GetPhoneNumberVerified(this ClaimsPrincipal principal)
        {
            return GetBooleanClaimValue(principal, OpenIdClaimTypes.PhoneNumberVerified);
        }

        public static string GetPicture(this ClaimsPrincipal principal)
        {
            return GetClaimValue(principal, OpenIdClaimTypes.Picture);
        }

        public static string GetPreferredUserName(this ClaimsPrincipal principal)
        {
            return GetClaimValue(principal, OpenIdClaimTypes.PreferredUserName);
        }

        public static string GetProfile(this ClaimsPrincipal principal)
        {
            return GetClaimValue(principal, OpenIdClaimTypes.Profile);
        }

        public static string GetRole(this ClaimsPrincipal principal)
        {
            return GetClaimValue(principal, OpenIdClaimTypes.Role);
        }

        public static string GetWebSite(this ClaimsPrincipal principal)
        {
            return GetClaimValue(principal, OpenIdClaimTypes.WebSite);
        }

        public static string GetZoneInfo(this ClaimsPrincipal principal)
        {
            return GetClaimValue(principal, OpenIdClaimTypes.ZoneInfo);
        }

        public static string GetBirthDate(this ClaimsPrincipal principal)
        {
            return GetClaimValue(principal, OpenIdClaimTypes.BirthDate);
        }

        private static string GetClaimValue(ClaimsPrincipal principal, string claimName)
        {
            if (principal?.Identity == null)
            {
                return null;
            }

            var claim = principal.FindFirst(claimName);

            return claim?.Value;
        }

        private static bool GetBooleanClaimValue(ClaimsPrincipal principal, string claimName)
        {
            var result = GetClaimValue(principal, claimName);
            if (string.IsNullOrWhiteSpace(result))
            {
                return false;
            }

            if (!bool.TryParse(result, out var res))
            {
                return false;
            }

            return true;
        }
    }
}
