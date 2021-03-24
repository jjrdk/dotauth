namespace SimpleAuth
{
    using System.IdentityModel.Tokens.Jwt;
    using System.Linq;

    internal static class JwtPayloadExtensions
    {
        public static string? GetClaimValue(this JwtPayload payload, string claimType)
        {
            return payload.Claims.FirstOrDefault(c => c.Type == claimType)?.Value;
        }

        public static string[] GetArrayValue(this JwtPayload payload, string claimType)
        {
            return payload.Claims
                .Where(c => c.Type == claimType && !string.IsNullOrWhiteSpace(c.Value))
                .Select(c => c.Value)
                .ToArray();
        }
    }
}