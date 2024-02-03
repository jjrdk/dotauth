namespace DotAuth.Uma;

using System;
using System.IdentityModel.Tokens.Jwt;
using DotAuth.Shared.Models;
using DotAuth.Shared.Responses;
using Shared;

public record PermissionRegistration
{
    public PermissionRegistration(string resourceId, GrantedTokenResponse umaToken)
    {
        ResourceId = resourceId;
        UmaToken = umaToken;
        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(umaToken.AccessToken);
        var permissionsClaim = jwt.Claims.TryGetUmaTickets(out var permissions);
        Expires = permissionsClaim ? new DateTimeOffset(jwt.ValidTo).ToUnixTimeSeconds() : 0;
        Permissions = permissionsClaim ? permissions : Array.Empty<Permission>();
    }

    public Permission[] Permissions { get; }

    public long Expires { get; }

    public string ResourceId { get; }

    public GrantedTokenResponse UmaToken { get; }
}