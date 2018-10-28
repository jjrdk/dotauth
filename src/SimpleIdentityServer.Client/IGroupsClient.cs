//namespace SimpleIdentityServer.Scim.Client
//{
//    using System;
//    using System.Threading.Tasks;
//    using Core.Common;
//    using Core.Common.DTOs;
//    using Core.Common.Models;
//    using Newtonsoft.Json.Linq;

//    public interface IGroupsClient
//    {
//        Task<ScimResponse<JObject>> AddGroup(Uri baseUri, string id, string accessToken = null);
//        Task<ScimResponse<JObject>> GetGroup(Uri baseUri, string id, string accessToken = null);
//        Task<ScimResponse<JObject>> DeleteGroup(Uri baseUri, string id, string accessToken = null);
//        Task<ScimResponse<JObject>> UpdateGroup(Uri baseUri,
//            string id,
//            GroupResource @group,
//            string accessToken = null);
//        Task<ScimResponse<JObject>> PartialUpdateGroup(Uri baseUri, string id, string accessToken = null, params PatchOperation[] operations);
//        Task<ScimResponse<JObject>> SearchGroups(Uri baseUri, SearchParameter parameter, string accessToken = null);
//    }
//}