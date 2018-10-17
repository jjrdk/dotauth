namespace SimpleIdentityServer.Uma.Core.Api.PermissionController
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Parameters;

    public interface IPermissionControllerActions
    {
        Task<string> Add(AddPermissionParameter addPermissionParameter, string clientId);
        Task<string> Add(IEnumerable<AddPermissionParameter> addPermissionParameters, string clientId);
    }
}