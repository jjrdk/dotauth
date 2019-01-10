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

namespace SimpleAuth.Shared
{
    using System.Collections.Generic;
    using System.Security.Claims;

    public class JwtConstants
    {
        public static class StandardResourceOwnerClaimNames
        {
            public const string Subject = "sub";
            public const string Name = "name";
            public const string GivenName = "given_name";
            public const string FamilyName = "family_name";
            public const string MiddleName = "middle_name";
            public const string NickName = "nickname";
            public const string PreferredUserName = "preferred_username";
            public const string Profile = "profile";
            public const string Picture = "picture";
            public const string WebSite = "website";
            public const string Email = "email";
            public const string EmailVerified = "email_verified";
            public const string Gender = "gender";
            public const string BirthDate = "birthdate";
            public const string ZoneInfo = "zoneinfo";
            public const string Locale = "locale";
            public const string PhoneNumber = "phone_number";
            public const string PhoneNumberVerified = "phone_number_verified";
            public const string Address = "address";
            public const string UpdatedAt = "updated_at";
            public const string Role = "role";
            public const string ScimId = "scim_id";
            public const string ScimLocation = "scim_location";
        }

        public static IEnumerable<string> NotEditableResourceOwnerClaimNames = new List<string>
        {
            StandardResourceOwnerClaimNames.Subject,
            StandardResourceOwnerClaimNames.EmailVerified,
            StandardResourceOwnerClaimNames.PhoneNumberVerified,
            StandardResourceOwnerClaimNames.UpdatedAt,
            StandardResourceOwnerClaimNames.ScimId,
            StandardResourceOwnerClaimNames.ScimLocation
        };

        public static class StandardAddressClaimNames
        {
            public const string StreetAddress = "street_address";
            public const string Locality = "locality";
            public const string Region = "region";
            public const string PostalCode = "postal_code";
            public const string Country = "country";
        }

        public static List<string> AllStandardResourceOwnerClaimNames = new List<string>
        {
            StandardResourceOwnerClaimNames.Subject,
            StandardResourceOwnerClaimNames.Address,
            StandardResourceOwnerClaimNames.BirthDate,
            StandardResourceOwnerClaimNames.Email,
            StandardResourceOwnerClaimNames.EmailVerified,
            StandardResourceOwnerClaimNames.FamilyName,
            StandardResourceOwnerClaimNames.Gender,
            StandardResourceOwnerClaimNames.GivenName,
            StandardResourceOwnerClaimNames.Locale,
            StandardResourceOwnerClaimNames.MiddleName,
            StandardResourceOwnerClaimNames.Name,
            StandardResourceOwnerClaimNames.NickName,
            StandardResourceOwnerClaimNames.PhoneNumber,
            StandardResourceOwnerClaimNames.PhoneNumberVerified,
            StandardResourceOwnerClaimNames.Picture,
            StandardResourceOwnerClaimNames.PreferredUserName,
            StandardResourceOwnerClaimNames.Profile,
            StandardResourceOwnerClaimNames.Role,
            StandardResourceOwnerClaimNames.UpdatedAt,
            StandardResourceOwnerClaimNames.WebSite,
            StandardResourceOwnerClaimNames.ZoneInfo
        };

        public static List<string> AllStandardClaimNames = new List<string>
        {
            StandardClaimNames.Acr,
            StandardClaimNames.Amr,
            StandardClaimNames.Audiences,
            StandardClaimNames.AuthenticationTime,
            StandardClaimNames.Azp,
            StandardClaimNames.ExpirationTime,
            StandardClaimNames.Iat,
            StandardClaimNames.Issuer,
            StandardClaimNames.Jti,
            StandardClaimNames.Nonce,
            StandardClaimNames.Subject
        };

        public static readonly Dictionary<string, string> MapWifClaimsToOpenIdClaims = new Dictionary<string, string>
        {
            {
                ClaimTypes.Name, StandardResourceOwnerClaimNames.Name
            },
            {
                ClaimTypes.GivenName, StandardResourceOwnerClaimNames.GivenName
            },
            {
                ClaimTypes.Webpage, StandardResourceOwnerClaimNames.WebSite
            },
            {
                ClaimTypes.Email, StandardResourceOwnerClaimNames.Email
            },
            {
                ClaimTypes.Gender, StandardResourceOwnerClaimNames.Gender
            },
            {
                ClaimTypes.DateOfBirth, StandardResourceOwnerClaimNames.BirthDate
            },
            {
                ClaimTypes.Locality, StandardResourceOwnerClaimNames.Locale
            },
            {
                ClaimTypes.HomePhone, StandardResourceOwnerClaimNames.PhoneNumber
            },
            {
                ClaimTypes.MobilePhone, StandardResourceOwnerClaimNames.PhoneNumberVerified
            },
            {
                ClaimTypes.StreetAddress, StandardResourceOwnerClaimNames.Address
            },
            {
                ClaimTypes.Role, StandardResourceOwnerClaimNames.Role
            }
        };

        public static class JsonWebKeyParameterNames
        {
            public static string KeyTypeName = "kty";
            public static string UseName = "use";
            public static string KeyOperationsName = "key_ops";
            public static string AlgorithmName = "alg";
            public static string KeyIdentifierName = "kid";
            public static string X5Url = "x5u";
            public static string X5CertificateChain = "x5c";
            public static string X5ThumbPrint = "x5t";
            public static string X5Sha256ThumbPrint = "x5t#S256";
            public static class RsaKey
            {
                public static string ModulusName = "n";
                public static string ExponentName = "e";
            }

            public static class EcKey
            {
                public static string XCoordinateName = "x";
                public static string YCoordinateName = "y";
            }
        }
    }
}
