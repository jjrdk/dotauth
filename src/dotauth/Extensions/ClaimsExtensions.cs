namespace DotAuth.Extensions;

using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using DotAuth.Shared;

internal static class ClaimsExtensions
{
    private static readonly Dictionary<string, string> MappingToOpenidClaims = new()
    {
        { ClaimTypes.NameIdentifier, OpenIdClaimTypes.Subject },
        { ClaimTypes.DateOfBirth, OpenIdClaimTypes.BirthDate },
        { ClaimTypes.Email, OpenIdClaimTypes.Email },
        { ClaimTypes.Name, OpenIdClaimTypes.Name },
        { ClaimTypes.GivenName, OpenIdClaimTypes.GivenName },
        { ClaimTypes.Surname, OpenIdClaimTypes.FamilyName },
        { ClaimTypes.Gender, OpenIdClaimTypes.Gender },
        { ClaimTypes.Locality, OpenIdClaimTypes.Locale },
        { ClaimTypes.Role, OpenIdClaimTypes.Role },
        { ClaimTypes.HomePhone, OpenIdClaimTypes.PhoneNumber },
        { ClaimTypes.Webpage, OpenIdClaimTypes.WebSite },
    };

    extension(IEnumerable<string> claimTypes)
    {
        public IEnumerable<string> ToOpenIdClaimType()
        {
            return claimTypes.SelectMany(claim =>
                MappingToOpenidClaims.TryGetValue(claim, out var openidClaim)
                    ? [openidClaim, claim]
                    : new[] {claim});
        }
    }

    extension(IEnumerable<Claim> claims)
    {
        public Claim[] ToOpenidClaims()
        {
            return claims.Select(claim =>
                    MappingToOpenidClaims.TryGetValue(claim.Type, out var openidClaim)
                        ? new Claim(openidClaim, claim.Value)
                        : claim)
                .ToArray();
        }
    }

    extension(Claim claim)
    {
        public bool HasClaimValue(string value, char separator = ' ')
        {
            return HasClaimValue(claim, value, [separator]);
        }

        public bool HasClaimValue(string value, params char[] separators)
        {
            return claim.Value.Split(separators).Any(v => v == value);
        }
    }
}
