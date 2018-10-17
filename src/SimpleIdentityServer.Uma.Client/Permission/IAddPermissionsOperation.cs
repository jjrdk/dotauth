namespace SimpleIdentityServer.Uma.Client.Permission
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Common.DTOs;
    using Results;

    public interface IAddPermissionsOperation
    {
        Task<AddPermissionResult> ExecuteAsync(PostPermission request, string url, string token);
        Task<AddPermissionResult> ExecuteAsync(IEnumerable<PostPermission> request, string url, string token);
    }
}