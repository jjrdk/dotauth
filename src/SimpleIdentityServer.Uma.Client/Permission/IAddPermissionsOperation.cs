namespace SimpleAuth.Uma.Client.Permission
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Results;
    using Shared.DTOs;

    public interface IAddPermissionsOperation
    {
        Task<AddPermissionResult> ExecuteAsync(PostPermission request, string url, string token);
        Task<AddPermissionResult> ExecuteAsync(IEnumerable<PostPermission> request, string url, string token);
    }
}