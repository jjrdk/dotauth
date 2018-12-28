namespace SimpleAuth.Uma.Api.PermissionController.Actions
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Parameters;

    internal interface IAddPermissionAction
    {
        Task<string> Execute(string clientId, AddPermissionParameter addPermissionParameters);
        Task<string> Execute(string clientId, IEnumerable<AddPermissionParameter> addPermissionParameters);
    }
}