//// Copyright 2015 Habart Thierry
//// 
//// Licensed under the Apache License, Version 2.0 (the "License");
//// you may not use this file except in compliance with the License.
//// You may obtain a copy of the License at
//// 
////     http://www.apache.org/licenses/LICENSE-2.0
//// 
//// Unless required by applicable law or agreed to in writing, software
//// distributed under the License is distributed on an "AS IS" BASIS,
//// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//// See the License for the specific language governing permissions and
//// limitations under the License.

//using Newtonsoft.Json.Linq;
//using System;
//using System.Net.Http;
//using System.Net.Http.Headers;
//using System.Threading.Tasks;

//namespace SimpleIdentityServer.Scim.Client
//{
//    using Core.Common;
//    using Core.Common.DTOs;
//    using Core.Common.Models;
//    using Newtonsoft.Json;

//    internal class GroupsClient : IGroupsClient
//    {
//        private readonly string _schema = ScimConstants.SchemaUrns.Group;
//        private readonly HttpClient _client;

//        public GroupsClient(HttpClient client)
//        {
//            _client = client;
//        }

//        public Task<ScimResponse<JObject>> AddGroup(Uri baseUri, string id, string accessToken = null)
//        {
//            if (baseUri == null)
//            {
//                throw new ArgumentNullException(nameof(baseUri));
//            }

//            var url = FormatUrl(baseUri.AbsoluteUri, null);

//            //var arr = new JArray(_schema);
//            //var request = new JObject
//            //{
//            //    [ScimConstants.ScimResourceNames.Schemas] = arr,
//            //    [ScimConstants.IdentifiedScimResourceNames.ExternalId] = id
//            //};
//            var request = new GroupResource { Id = id };

//            return ExecuteRequest(request, url, HttpMethod.Post, accessToken);
//            //return new RequestBuilder(_schema, (obj) => AddGroup(obj, new Uri(url), accessToken));
//        }

//        public async Task<ScimResponse<JObject>> GetGroup(Uri baseUri, string id, string accessToken = null)
//        {
//            if (baseUri == null)
//            {
//                throw new ArgumentNullException(nameof(baseUri));
//            }

//            if (string.IsNullOrWhiteSpace(id))
//            {
//                throw new ArgumentNullException(nameof(id));
//            }

//            var url = FormatUrl(baseUri.AbsoluteUri, id);

//            var request = new HttpRequestMessage
//            {
//                Method = HttpMethod.Get,
//                RequestUri = url
//            };
//            if (!string.IsNullOrWhiteSpace(accessToken))
//            {
//                request.Headers.Add("Authorization", "Bearer " + accessToken);
//            }

//            var response = await _client.SendAsync(request).ConfigureAwait(false);
//            return await ParseHttpResponse(response).ConfigureAwait(false);
//        }

//        public async Task<ScimResponse<JObject>> DeleteGroup(Uri baseUri, string id, string accessToken = null)
//        {
//            if (baseUri == null)
//            {
//                throw new ArgumentNullException(nameof(baseUri));
//            }

//            if (string.IsNullOrWhiteSpace(id))
//            {
//                throw new ArgumentNullException(nameof(id));
//            }

//            var url = FormatUrl(baseUri.AbsoluteUri, id); //$"{}/{id}";
//            var request = new HttpRequestMessage
//            {
//                Method = HttpMethod.Delete,
//                RequestUri = url
//            };
//            if (!string.IsNullOrWhiteSpace(accessToken))
//            {
//                request.Headers.Add("Authorization", "Bearer " + accessToken);
//            }

//            var response = await _client.SendAsync(request).ConfigureAwait(false);
//            return await ParseHttpResponse(response).ConfigureAwait(false);
//        }

//        public Task<ScimResponse<JObject>> UpdateGroup(Uri baseUri, string id, GroupResource group, string accessToken = null)
//        {
//            if (baseUri == null)
//            {
//                throw new ArgumentNullException(nameof(baseUri));
//            }

//            if (string.IsNullOrWhiteSpace(id))
//            {
//                throw new ArgumentNullException(nameof(id));
//            }

//            var uri = FormatUrl(baseUri.AbsoluteUri, id);

//            return ExecuteRequest(group, uri, HttpMethod.Put, accessToken);
//        }

//        public Task<ScimResponse<JObject>> PartialUpdateGroup(Uri baseUri, string id, string accessToken = null, params PatchOperation[] operations)
//        {
//            if (baseUri == null)
//            {
//                throw new ArgumentNullException(nameof(baseUri));
//            }

//            if (string.IsNullOrWhiteSpace(id))
//            {
//                throw new ArgumentNullException(nameof(id));
//            }

//            var url = FormatUrl(baseUri.AbsoluteUri, id); //$"{}/{id}";
//            var request = new PatchRequest { Operations = operations };

//            return ExecuteRequest(request, url, new HttpMethod("PATCH"), accessToken);
//        }

//        public async Task<ScimResponse<JObject>> SearchGroups(Uri baseUri, SearchParameter parameter, string accessToken = null)
//        {
//            if (baseUri == null)
//            {
//                throw new ArgumentNullException(nameof(baseUri));
//            }

//            if (parameter == null)
//            {
//                throw new ArgumentNullException(nameof(parameter));
//            }

//            var url = FormatUrl(baseUri.AbsoluteUri, ".search"); //$"{}/.search";
//            var request = new HttpRequestMessage
//            {
//                Method = HttpMethod.Post,
//                RequestUri = url,
//                Content = new StringContent(JsonConvert.SerializeObject(parameter))
//            };
//            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
//            if (!string.IsNullOrWhiteSpace(accessToken))
//            {
//                request.Headers.Add("Authorization", "Bearer " + accessToken);
//            }

//            var response = await _client.SendAsync(request).ConfigureAwait(false);
//            return await ParseHttpResponse(response).ConfigureAwait(false);
//        }

//        private async Task<ScimResponse<JObject>> ExecuteRequest<T>(T item, Uri uri, HttpMethod method, string accessToken = null)
//        {
//            var request = new HttpRequestMessage
//            {
//                Method = method,
//                RequestUri = uri,
//                Content = new StringContent(JsonConvert.SerializeObject(item))
//            };
//            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
//            if (!string.IsNullOrWhiteSpace(accessToken))
//            {
//                request.Headers.Add("Authorization", "Bearer " + accessToken);
//            }

//            var response = await _client.SendAsync(request).ConfigureAwait(false);
//            return await ParseHttpResponse(response).ConfigureAwait(false);
//        }

//        private static Uri FormatUrl(string baseUrl, string additionalPath = null)
//        {
//            var uriString = baseUrl.TrimEnd('/', '\\') + "/Groups";
//            if (!string.IsNullOrWhiteSpace(additionalPath))
//            {
//                uriString = $"{uriString}/{additionalPath}";
//            }
//            return new Uri(uriString);
//        }

//        //private JObject BuildPatchRequest(IEnumerable<PatchOperation> operations)
//        //{
//        //    var patchRequest = new JObject
//        //    { [ScimConstants.ScimResourceNames.Schemas] = new JArray(new[] { ScimConstants.Messages.PatchOp }) };
//        //    var arr = new JArray();
//        //    foreach (var operation in operations)
//        //    {
//        //        var obj = new JObject
//        //        {
//        //            new JProperty(ScimConstants.PatchOperationRequestNames.Operation,
//        //                Enum.GetName(typeof(PatchOperations), operation.Type))
//        //        };
//        //        if (!string.IsNullOrWhiteSpace(operation.Path))
//        //        {
//        //            obj.Add(new JProperty(ScimConstants.PatchOperationRequestNames.Path, operation.Path));
//        //        }

//        //        if (operation.Value != null)
//        //        {
//        //            obj.Add(new JProperty(ScimConstants.PatchOperationRequestNames.Value, operation.Value));
//        //        }

//        //        arr.Add(obj);
//        //    }

//        //    patchRequest.Add(new JProperty(ScimConstants.PatchOperationsRequestNames.Operations, arr));
//        //    return patchRequest;
//        //}

//        private static async Task<ScimResponse<JObject>> ParseHttpResponse(HttpResponseMessage response)
//        {
//            var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
//            var result = new ScimResponse<JObject>
//            {
//                StatusCode = response.StatusCode
//            };
//            if (!string.IsNullOrWhiteSpace(json))
//            {
//                result.Content = JObject.Parse(json);
//            }

//            return result;
//        }
//    }
//}
