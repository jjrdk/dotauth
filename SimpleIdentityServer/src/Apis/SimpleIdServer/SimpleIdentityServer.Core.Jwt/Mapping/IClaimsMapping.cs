namespace SimpleIdentityServer.Core.Jwt.Mapping
{
    using System.Collections.Generic;
    using System.Security.Claims;

    public interface IClaimsMapping
    {
        Dictionary<string, object> MapToOpenIdClaims(IEnumerable<Claim> claims);
    }
}