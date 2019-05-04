namespace SimpleAuth.Extensions
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using Shared;

    internal static class ClaimsExtensions
    {
        private static readonly Dictionary<string, string> MappingToOpenidClaims = new Dictionary<string, string>
        {
            { ClaimTypes.NameIdentifier, OpenIdClaimTypes.Subject },
            { ClaimTypes.DateOfBirth, OpenIdClaimTypes.BirthDate },
            { ClaimTypes.Email, OpenIdClaimTypes.Email },
            { ClaimTypes.Name, OpenIdClaimTypes.Name },
            { ClaimTypes.GivenName, OpenIdClaimTypes.GivenName },
            { ClaimTypes.Surname, OpenIdClaimTypes.FamilyName },
            { ClaimTypes.Gender, OpenIdClaimTypes.Gender },
            { ClaimTypes.Locality, OpenIdClaimTypes.Locale }
        };

        public static Claim[] ToOpenidClaims(this IEnumerable<Claim> claims)
        {
            return claims?.Select(claim => MappingToOpenidClaims.ContainsKey(claim.Type)
                    ? new Claim(MappingToOpenidClaims[claim.Type], claim.Value)
                    : claim)
                .ToArray();
        }
    }
}