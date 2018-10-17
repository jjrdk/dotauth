// Copyright 2016 Habart Thierry
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

namespace SimpleIdentityServer.Core.Common
{
    using Models;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;

    internal class UsersClient : IUsersClient
    {
        private readonly string _schema = ScimConstants.SchemaUrns.User;
        private readonly HttpClient _httpClientFactory;

        public UsersClient(HttpClient httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public Task<ScimResponse> AddUser(Uri baseUri,
            string subject = null,
            string accessToken = null,
            params JProperty[] properties)
        {
            if (baseUri == null)
            {
                throw new ArgumentNullException(nameof(baseUri));
            }

            var url = FormatUrl(baseUri.AbsoluteUri);
            if (url == null)
            {
                throw new ArgumentException($"{baseUri} is not a valid uri");
            }

            var arr = new JArray(new[] { _schema });
            var obj = new JObject
            {
                [ScimConstants.ScimResourceNames.Schemas] = arr
            };
            if (!string.IsNullOrWhiteSpace(subject))
            {
                obj[ScimConstants.IdentifiedScimResourceNames.ExternalId] = subject;
            }

            foreach (var property in properties)
            {
                obj.Add(property);
            }

            return ExecuteRequest(obj, new Uri(url), HttpMethod.Post, accessToken);
        }

        public Task<ScimResponse> AddAuthenticatedUser(Uri baseUri, string accessToken)
        {
            return AddUser(baseUri, "Me", accessToken);
        }

        public async Task<ScimResponse> DeleteAuthenticatedUser(Uri baseUri, string accessToken)
        {
            if (baseUri == null)
            {
                throw new ArgumentNullException(nameof(baseUri));
            }

            var url = $"{FormatUrl(baseUri.AbsoluteUri)}/Me";
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Delete,
                RequestUri = new Uri(url)
            };

            if (!string.IsNullOrWhiteSpace(accessToken))
            {
                request.Headers.Add("Authorization", "Bearer " + accessToken);
            }

            var response = await _httpClientFactory.SendAsync(request).ConfigureAwait(false);
            return await ParseHttpResponse(response).ConfigureAwait(false);
        }

        public Task<ScimResponse> UpdateUser(Uri baseUri, string id, string accessToken = null, params JProperty[] properties)
        {
            if (baseUri == null)
            {
                throw new ArgumentNullException(nameof(baseUri));
            }

            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            var url = $"{FormatUrl(baseUri.AbsoluteUri)}/{id}";
            var obj = new JObject { [ScimConstants.ScimResourceNames.Schemas] = new JArray(new object[] { _schema }) };

            foreach (var property in properties)
            {
                obj.Add(property);
            }

            return ExecuteRequest(obj, new Uri(url), HttpMethod.Put, accessToken);
        }

        public async Task<ScimResponse> DeleteUser(Uri baseUri, string id, string accessToken = null)
        {
            if (baseUri == null)
            {
                throw new ArgumentNullException(nameof(baseUri));
            }

            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            var url = $"{FormatUrl(baseUri.AbsoluteUri)}/{id}";
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Delete,
                RequestUri = new Uri(url)
            };

            if (!string.IsNullOrWhiteSpace(accessToken))
            {
                request.Headers.Add("Authorization", "Bearer " + accessToken);
            }

            var response = await _httpClientFactory.SendAsync(request).ConfigureAwait(false);
            return await ParseHttpResponse(response).ConfigureAwait(false);
        }

        //public async Task<ScimResponse> GetUser(Uri baseUri, string id, string accessToken = null)
        //{
        //    if (baseUri == null)
        //    {
        //        throw new ArgumentNullException(nameof(baseUri));
        //    }

        //    if (string.IsNullOrWhiteSpace(id))
        //    {
        //        throw new ArgumentNullException(nameof(id));
        //    }

        //    var url = $"{FormatUrl(baseUri.AbsoluteUri)}/{id}";
        //    var request = new HttpRequestMessage
        //    {
        //        Method = HttpMethod.Get,
        //        RequestUri = new Uri(url)
        //    };

        //    if (!string.IsNullOrWhiteSpace(accessToken))
        //    {
        //        request.Headers.Add("Authorization", "Bearer " + accessToken);
        //    }

        //    var response = await _httpClientFactory.SendAsync(request).ConfigureAwait(false);
        //    return await ParseHttpResponse(response).ConfigureAwait(false);
        //}

        public Task<ScimResponse> UpdateAuthenticatedUser(Uri baseUri, string accessToken = null, params JProperty[] properties)
        {
            if (baseUri == null)
            {
                throw new ArgumentNullException(nameof(baseUri));
            }

            //var url = $"{FormatUrl(baseUri.AbsoluteUri)}/Me";
            //return new RequestBuilder(_schema, (obj) => ExecuteRequest(obj, new Uri(url), HttpMethod.Put, accessToken));

            return UpdateUser(baseUri, "Me", accessToken, properties);
        }

        public async Task<ScimResponse> GetAuthenticatedUser(Uri baseUri, string accessToken = null)
        {
            if (baseUri == null)
            {
                throw new ArgumentNullException(nameof(baseUri));
            }

            var url = $"{FormatUrl(baseUri.AbsoluteUri)}/Me";
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(url)
            };

            if (!string.IsNullOrWhiteSpace(accessToken))
            {
                request.Headers.Add("Authorization", "Bearer " + accessToken);
            }

            var response = await _httpClientFactory.SendAsync(request).ConfigureAwait(false);
            return await ParseHttpResponse(response).ConfigureAwait(false);
        }

        public Task<ScimResponse> PartialUpdateUser(
            Uri baseUri,
            string id,
            string accessToken = null,
            params PatchOperation[] patchOperations)
        {
            if (baseUri == null)
            {
                throw new ArgumentNullException(nameof(baseUri));
            }

            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            var url = $"{FormatUrl(baseUri.AbsoluteUri)}/{id}";
            return ExecuteRequest(BuildPatchRequest(patchOperations), new Uri(url), new HttpMethod("PATCH"), accessToken);
        }

        public Task<ScimResponse> PartialUpdateAuthenticatedUser(
            Uri baseUri,
            string accessToken = null,
            params PatchOperation[] patchOperations)
        {
            return PartialUpdateUser(baseUri, "Me", accessToken, patchOperations);
        }

        public async Task<ScimResponse> SearchUsers(Uri baseUri, SearchParameter parameter, string accessToken = null)
        {
            if (baseUri == null)
            {
                throw new ArgumentNullException(nameof(baseUri));
            }

            if (parameter == null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }

            var url = $"{FormatUrl(baseUri.AbsoluteUri)}/.search";
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(url),
                Content = new StringContent(parameter.ToJson())
            };
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            if (!string.IsNullOrWhiteSpace(accessToken))
            {
                request.Headers.Add("Authorization", "Bearer " + accessToken);
            }

            var response = await _httpClientFactory.SendAsync(request).ConfigureAwait(false);
            return await ParseHttpResponse(response).ConfigureAwait(false);
        }

        private static string FormatUrl(string baseUrl)
        {
            Uri.TryCreate(baseUrl, UriKind.Absolute, out var result);
            if (result == null)
            {
                return null;
            }

            return baseUrl.TrimEnd('/', '\\') + "/Users";
        }

        private JObject BuildPatchRequest(IEnumerable<PatchOperation> operations)
        {
            var patch = new JObject { [ScimConstants.ScimResourceNames.Schemas] = new JArray(new object[] { ScimConstants.Messages.PatchOp }) };
            var arr = new JArray();
            foreach (var operation in operations)
            {
                var obj = new JObject
                {
                    new JProperty(ScimConstants.PatchOperationRequestNames.Operation,
                        Enum.GetName(typeof(PatchOperations), operation.Type))
                };
                if (!string.IsNullOrWhiteSpace(operation.Path))
                {
                    obj.Add(new JProperty(ScimConstants.PatchOperationRequestNames.Path, operation.Path));
                }

                if (operation.Value != null)
                {
                    obj.Add(new JProperty(ScimConstants.PatchOperationRequestNames.Value, operation.Value));
                }

                arr.Add(obj);
            }

            patch.Add(new JProperty(ScimConstants.PatchOperationsRequestNames.Operations, arr));
            return patch;
        }

        private async Task<ScimResponse> ExecuteRequest(
            JObject jObj,
            Uri uri,
            HttpMethod method,
            string accessToken = null)
        {
            var request = new HttpRequestMessage
            {
                Method = method,
                RequestUri = uri,
                Content = new StringContent(jObj.ToString())
            };
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            if (!string.IsNullOrWhiteSpace(accessToken))
            {
                request.Headers.Add("Authorization", "Bearer " + accessToken);
            }

            var response = await _httpClientFactory.SendAsync(request).ConfigureAwait(false);
            return await ParseHttpResponse(response).ConfigureAwait(false);
        }

        private static async Task<ScimResponse> ParseHttpResponse(HttpResponseMessage response)
        {
            var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            var result = new ScimResponse
            {
                StatusCode = response.StatusCode
            };
            if (!string.IsNullOrWhiteSpace(json))
            {
                result.Content = JObject.Parse(json);
            }

            return result;
        }
    }
}
