namespace SimpleAuth.Client
{
    using System;
    using System.Threading.Tasks;
    using Newtonsoft.Json.Linq;
    using Shared;
    using Shared.DTOs;

    public interface IUsersClient
    {
        Task<ScimResponse<JObject>> AddUser(ScimUser scimUser, string accessToken = null);

        Task<ScimResponse<JObject>> UpdateUser(Uri baseUri, ScimUser scimUser);
        Task<ScimResponse<JObject>> DeleteAuthenticatedUser(Uri baseUri, string accessToken);
        Task<ScimResponse<JObject>> GetAuthenticatedUser(Uri baseUri, string accessToken = null);
        Task<ScimResponse<ScimUser[]>> SearchUsers(Uri baseUri, SearchParameter parameter, string accessToken = null);
    }
}