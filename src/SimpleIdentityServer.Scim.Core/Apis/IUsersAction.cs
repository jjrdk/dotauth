namespace SimpleIdentityServer.Scim.Core.Apis
{
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Newtonsoft.Json.Linq;
    using Results;
    using SimpleIdentityServer.Core.Common.DTOs;

    //public interface IUsersAction
    //{
    //    Task<ApiActionResult> AddUser(ScimUser jObj, string locationPattern);
    //    Task<ApiActionResult> UpdateUser(string id, JObject jObj, string locationPattern);
    //    Task<ApiActionResult> PatchUser(string id, JObject jObj, string locationPattern);
    //    Task<ApiActionResult> RemoveUser(string id);
    //    Task<ApiActionResult> GetUser(string id, string locationPattern);
    //    Task<ApiActionResult> SearchUsers(JObject jObj, string locationPattern);
    //    Task<ApiActionResult> SearchUsers(IQueryCollection query, string locationPattern);
    //}
}