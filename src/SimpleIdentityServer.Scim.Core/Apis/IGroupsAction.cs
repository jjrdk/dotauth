namespace SimpleIdentityServer.Scim.Core.Apis
{
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Newtonsoft.Json.Linq;
    using Results;

    public interface IGroupsAction
    {
        Task<ApiActionResult> AddGroup(JObject jObj, string locationPattern);
        Task<ApiActionResult> GetGroup(string id, string locationPattern, IQueryCollection query);
        Task<ApiActionResult> RemoveGroup(string id);
        Task<ApiActionResult> UpdateGroup(string id, JObject jObj, string locationPattern);
        Task<ApiActionResult> PatchGroup(string id, JObject jObj, string locationPattern);
        Task<ApiActionResult> SearchGroups(JObject jObj, string locationPattern);
        Task<ApiActionResult> SearchGroups(IQueryCollection query, string locationPattern);
    }
}