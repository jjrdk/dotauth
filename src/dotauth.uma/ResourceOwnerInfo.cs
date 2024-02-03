namespace DotAuth.Uma;

using System;
using System.Collections.Generic;
using System.Linq;
using Shared.Responses;

public class ResourceOwnerInfo
{
    public required string Subject { get; init; }

    public string? Pat { get; set; }

    public DateTimeOffset PatExpires { get; set; }

    public string? IdToken { get; set; }

    public string? RefreshToken { get; set; }

    public List<ResourceRequest> ResourceRequests { get; } = new();

    public List<PermissionRegistration> PermissionRegistrations { get; } = new();

    public void Clean()
    {
        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        PermissionRegistrations.RemoveAll(x => x.Permissions.All(r => r.Expiry <= now));
    }

    public GrantedTokenResponse? GetExistingPermission(string resourceId, string[] scopes)
    {
        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        PermissionRegistrations.RemoveAll(x => x.Expires < now);
        var permission = PermissionRegistrations.FirstOrDefault(
            x =>
            {
                return x.ResourceId == resourceId
                       && x.Expires > now
                       && scopes.All(s => x.Permissions.Any(p => p.Scopes.Contains(s)))
                       && x.Permissions.Any(p => scopes.All(p.Scopes.Contains));
            });
        return permission?.UmaToken;
    }

    public bool GetExistingRequest(string subject, string resourceId, out ResourceRequest? existing, params string[] scopes)
    {
        existing = ResourceRequests.FirstOrDefault(
            x => x.Owner == subject && x.ResourceId == resourceId && scopes.All(x.Scope.Contains));
        return existing == null;
    }
}