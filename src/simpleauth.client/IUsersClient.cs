namespace SimpleIdentityServer.Client
{
    using System;
    using System.Threading.Tasks;
    using Newtonsoft.Json.Linq;
    using SimpleAuth.Shared;
    using SimpleAuth.Shared.DTOs;

    public interface IUsersClient
    {
        Task<ScimResponse<JObject>> AddUser(ScimUser scimUser, string accessToken = null);

        //Task<ScimResponse> AddAuthenticatedUser(Uri baseUri, string accessToken);

        //Task<ScimResponse> PartialUpdateUser(Uri baseUri, string id, string accessToken = null, params PatchOperation[] patchOperations);
        //Task<ScimResponse> PartialUpdateAuthenticatedUser(
        //    Uri baseUri,
        //    string accessToken = null,
        //    params PatchOperation[] patchOperations);
        Task<ScimResponse<JObject>> UpdateUser(Uri baseUri, ScimUser scimUser, string accessToken = null);
        Task<ScimResponse<JObject>> DeleteUser(Uri baseUri, string id, string accessToken = null);
        Task<ScimResponse<JObject>> DeleteAuthenticatedUser(Uri baseUri, string accessToken);
        Task<ScimResponse<JObject>> GetAuthenticatedUser(Uri baseUri, string accessToken = null);
        Task<ScimResponse<ScimUser[]>> SearchUsers(Uri baseUri, SearchParameter parameter, string accessToken = null);
    }
}