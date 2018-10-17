namespace SimpleIdentityServer.Scim.Client
{
    using System;
    using System.Threading.Tasks;
    using Core.Common;
    using Core.Common.Models;
    using Newtonsoft.Json.Linq;

    public interface IGroupsClient
    {
        Task<ScimResponse> AddGroup(Uri baseUri, string id, string accessToken = null);
        Task<ScimResponse> GetGroup(Uri baseUri, string id, string accessToken = null);
        Task<ScimResponse> DeleteGroup(Uri baseUri, string id, string accessToken = null);
        Task<ScimResponse> UpdateGroup(Uri baseUri, string id, string accessToken = null, string newId = null, params JProperty[] properties);
        Task<ScimResponse> PartialUpdateGroup(Uri baseUri, string id, string accessToken = null, params PatchOperation[] operations);
        Task<ScimResponse> SearchGroups(Uri baseUri, SearchParameter parameter, string accessToken = null);
    }
}