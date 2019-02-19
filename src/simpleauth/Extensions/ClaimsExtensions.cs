namespace SimpleAuth.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using Shared;

    internal static class ClaimsExtensions
    {
        private static readonly Dictionary<string, string> MappingToOpenidClaims = new Dictionary<string, string>
        {
            { ClaimTypes.NameIdentifier, JwtConstants.OpenIdClaimTypes.Subject },
            { ClaimTypes.DateOfBirth, JwtConstants.OpenIdClaimTypes.BirthDate },
            { ClaimTypes.Email, JwtConstants.OpenIdClaimTypes.Email },
            { ClaimTypes.Name, JwtConstants.OpenIdClaimTypes.Name },
            { ClaimTypes.GivenName, JwtConstants.OpenIdClaimTypes.GivenName },
            { ClaimTypes.Surname, JwtConstants.OpenIdClaimTypes.FamilyName },
            { ClaimTypes.Gender, JwtConstants.OpenIdClaimTypes.Gender },
            { ClaimTypes.Locality, JwtConstants.OpenIdClaimTypes.Locale }
        };

        public static Claim[] ToOpenidClaims(this IEnumerable<Claim> claims)
        {
            if (claims == null)
            {
                throw new ArgumentNullException(nameof(claims));
            }

            return claims.Select(claim => MappingToOpenidClaims.ContainsKey(claim.Type)
                    ? new Claim(MappingToOpenidClaims[claim.Type], claim.Value)
                    : claim)
                .ToArray();
        }
    }
}