namespace SimpleIdentityServer.Core.Common
{
    using System;
    using System.Threading.Tasks;
    using Models;
    using Newtonsoft.Json.Linq;

    public interface IUsersClient
    {
        Task<ScimResponse> AddUser(Uri baseUri,
            string subject = null,
            string accessToken = null,
            params JProperty[] properties);

        Task<ScimResponse> AddAuthenticatedUser(Uri baseUri, string accessToken);

        Task<ScimResponse> PartialUpdateUser(Uri baseUri, string id, string accessToken = null, params PatchOperation[] patchOperations);
        Task<ScimResponse> PartialUpdateAuthenticatedUser(
            Uri baseUri,
            string accessToken = null,
            params PatchOperation[] patchOperations);
        Task<ScimResponse> UpdateUser(Uri baseUri, string id, string accessToken = null, params JProperty[] properties);
        Task<ScimResponse> UpdateAuthenticatedUser(Uri baseUri, string accessToken = null, params JProperty[] properties);
        Task<ScimResponse> DeleteUser(Uri baseUri, string id, string accessToken = null);
        Task<ScimResponse> DeleteAuthenticatedUser(Uri baseUri, string accessToken);
        //Task<ScimResponse> GetUser(Uri baseUri, string id, string accessToken = null);
        Task<ScimResponse> GetAuthenticatedUser(Uri baseUri, string accessToken = null);
        Task<ScimResponse> SearchUsers(Uri baseUri, SearchParameter parameter, string accessToken = null);
    }
}