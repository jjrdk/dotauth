namespace SimpleIdentityServer.Uma.Client.Permission
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Results;
    using SimpleAuth.Uma.Shared.DTOs;

    public interface IPermissionClient
    {
        Task<AddPermissionResult> Add(PostPermission request, string url, string token);
        Task<AddPermissionResult> AddByResolution(PostPermission request, string url, string token);
        Task<AddPermissionResult> Add(IEnumerable<PostPermission> request, string url, string token);
        Task<AddPermissionResult> AddByResolution(IEnumerable<PostPermission> request, string url, string token);
    }
}