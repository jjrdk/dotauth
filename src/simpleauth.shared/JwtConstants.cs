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

namespace SimpleAuth.Shared;

using System.Collections.Generic;
using System.Security.Claims;

internal sealed class JwtConstants
{
    public static readonly string[] NotEditableResourceOwnerClaimNames =
    {
        OpenIdClaimTypes.Subject,
        OpenIdClaimTypes.EmailVerified,
        OpenIdClaimTypes.PhoneNumberVerified,
        OpenIdClaimTypes.UpdatedAt,
    };

    public static readonly string[] AllStandardResourceOwnerClaimNames =
    {
        OpenIdClaimTypes.Subject,
        OpenIdClaimTypes.Address,
        OpenIdClaimTypes.BirthDate,
        OpenIdClaimTypes.Email,
        OpenIdClaimTypes.EmailVerified,
        OpenIdClaimTypes.FamilyName,
        OpenIdClaimTypes.Gender,
        OpenIdClaimTypes.GivenName,
        OpenIdClaimTypes.Locale,
        OpenIdClaimTypes.MiddleName,
        OpenIdClaimTypes.Name,
        OpenIdClaimTypes.NickName,
        OpenIdClaimTypes.PhoneNumber,
        OpenIdClaimTypes.PhoneNumberVerified,
        OpenIdClaimTypes.Picture,
        OpenIdClaimTypes.PreferredUserName,
        OpenIdClaimTypes.Profile,
        OpenIdClaimTypes.Role,
        OpenIdClaimTypes.UpdatedAt,
        OpenIdClaimTypes.WebSite,
        OpenIdClaimTypes.ZoneInfo
    };

    public static readonly string[] AllStandardClaimNames =
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

    public static readonly Dictionary<string, string> MapWifClaimsToOpenIdClaims = new()
    {
        {ClaimTypes.Name, OpenIdClaimTypes.Name},
        {ClaimTypes.GivenName, OpenIdClaimTypes.GivenName},
        {ClaimTypes.Webpage, OpenIdClaimTypes.WebSite},
        {ClaimTypes.Email, OpenIdClaimTypes.Email},
        {ClaimTypes.Gender, OpenIdClaimTypes.Gender},
        {ClaimTypes.DateOfBirth, OpenIdClaimTypes.BirthDate},
        {ClaimTypes.Locality, OpenIdClaimTypes.Locale},
        {ClaimTypes.HomePhone, OpenIdClaimTypes.PhoneNumber},
        {ClaimTypes.MobilePhone, OpenIdClaimTypes.PhoneNumberVerified},
        {ClaimTypes.StreetAddress, OpenIdClaimTypes.Address},
        {ClaimTypes.Role, OpenIdClaimTypes.Role}
    };
}