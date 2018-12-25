namespace SimpleIdentityServer.Core.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using SimpleAuth.Jwt;

    public static class ClaimsExtensions
    {
        private static readonly Dictionary<string, string> _mappingToOpenidClaims = new Dictionary<string, string>
        {
            { ClaimTypes.NameIdentifier, JwtConstants.StandardResourceOwnerClaimNames.Subject },
            { ClaimTypes.DateOfBirth, JwtConstants.StandardResourceOwnerClaimNames.BirthDate },
            { ClaimTypes.Email, JwtConstants.StandardResourceOwnerClaimNames.Email },
            { ClaimTypes.Name, JwtConstants.StandardResourceOwnerClaimNames.Name },
            { ClaimTypes.GivenName, JwtConstants.StandardResourceOwnerClaimNames.GivenName },
            { ClaimTypes.Surname, JwtConstants.StandardResourceOwnerClaimNames.FamilyName },
            { ClaimTypes.Gender, JwtConstants.StandardResourceOwnerClaimNames.Gender },
            { ClaimTypes.Locality, JwtConstants.StandardResourceOwnerClaimNames.Locale }
        };

        public static IEnumerable<Claim> ToOpenidClaims(this IEnumerable<Claim> claims)
        {
            if (claims == null)
            {
                throw new ArgumentNullException(nameof(claims));
            }

            return claims.Select(claim => _mappingToOpenidClaims.ContainsKey(claim.Type)
                    ? new Claim(_mappingToOpenidClaims[claim.Type], claim.Value)
                    : claim)
                .ToList();
        }
    }
}