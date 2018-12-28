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

namespace SimpleAuth.Client
{
    using System;
    using System.Globalization;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Shared;
    using Shared.DTOs;

    public class UsersClient : IUsersClient
    {
        private static readonly JsonSerializerSettings JsonSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto,
            DateFormatHandling = DateFormatHandling.IsoDateFormat,
            Culture = CultureInfo.GetCultureInfo("en-US"),
            DefaultValueHandling = DefaultValueHandling.Ignore,
            NullValueHandling = NullValueHandling.Ignore
        };
        //private readonly string _schema = ScimConstants.SchemaUrns.User;
        private readonly Uri _baseUri;
        private readonly HttpClient _httpClientFactory;

        public UsersClient(Uri baseUri, HttpClient httpClientFactory)
        {
            _baseUri = baseUri ?? throw new ArgumentNullException(nameof(baseUri));
            _httpClientFactory = httpClientFactory;
        }

        public Task<ScimResponse<JObject>> AddUser(
            ScimUser scimUser,
            string accessToken = null)
        {
            var url = string.IsNullOrWhiteSpace(accessToken)
                ? FormatUrl(_baseUri.AbsoluteUri)
                : $"{FormatUrl(_baseUri.AbsoluteUri)}/Me";
            if (url == null)
            {
                throw new ArgumentException("null is not a valid uri");
            }

            return ExecuteRequest<JObject>(scimUser, new Uri(url), HttpMethod.Post, accessToken);
        }

        public async Task<ScimResponse<JObject>> DeleteAuthenticatedUser(Uri baseUri, string accessToken)
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
            return await ParseHttpResponse<JObject>(response).ConfigureAwait(false);
        }

        public Task<ScimResponse<JObject>> UpdateUser(Uri baseUri, ScimUser scimUser, string accessToken = null)
        {
            if (baseUri == null)
            {
                throw new ArgumentNullException(nameof(baseUri));
            }

            if (string.IsNullOrWhiteSpace(scimUser.Id))
            {
                throw new ArgumentNullException(nameof(scimUser.Id));
            }

            var url = $"{FormatUrl(baseUri.AbsoluteUri)}/{scimUser.Id}";
            //var obj = new JObject { [ScimConstants.ScimResourceNames.Schemas] = new JArray(new object[] { _schema }) };
            scimUser.Schemas = new[] { ScimConstants.SchemaUrns.User };

            return ExecuteRequest<JObject>(scimUser, new Uri(url), HttpMethod.Put);
        }

        public async Task<ScimResponse<JObject>> DeleteUser(Uri baseUri, string id, string accessToken = null)
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
            return await ParseHttpResponse<JObject>(response).ConfigureAwait(false);
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

        //public Task<ScimResponse> UpdateAuthenticatedUser(Uri baseUri, ScimUser scimUser, params JProperty[] properties)
        //{
        //    if (baseUri == null)
        //    {
        //        throw new ArgumentNullException(nameof(baseUri));
        //    }

        //    var url = $"{FormatUrl(baseUri.AbsoluteUri)}/Me";
        //    //return new RequestBuilder(_schema, (obj) => ExecuteRequest(obj, new Uri(url), HttpMethod.Put, accessToken));

        //    //return UpdateUser(baseUri, "Me", TODO, properties);

        //}

        public async Task<ScimResponse<JObject>> GetAuthenticatedUser(Uri baseUri, string accessToken = null)
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
            return await ParseHttpResponse<JObject>(response).ConfigureAwait(false);
        }

        //public Task<ScimResponse> PartialUpdateUser(
        //    Uri baseUri,
        //    string id,
        //    string accessToken = null,
        //    params PatchOperation[] patchOperations)
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
        //    return ExecuteRequest(BuildPatchRequest(patchOperations), new Uri(url), new HttpMethod("PATCH"), accessToken);
        //}

        //public Task<ScimResponse> PartialUpdateAuthenticatedUser(
        //    Uri baseUri,
        //    string accessToken = null,
        //    params PatchOperation[] patchOperations)
        //{
        //    return PartialUpdateUser(baseUri, "Me", accessToken, patchOperations);
        //}

        public async Task<ScimResponse<ScimUser[]>> SearchUsers(Uri baseUri, SearchParameter parameter, string accessToken = null)
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
                Content = new StringContent(JsonConvert.SerializeObject(parameter))
            };
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            if (!string.IsNullOrWhiteSpace(accessToken))
            {
                request.Headers.Add("Authorization", "Bearer " + accessToken);
            }

            var response = await _httpClientFactory.SendAsync(request).ConfigureAwait(false);
            return await ParseHttpResponse<ScimUser[]>(response).ConfigureAwait(false);
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

        //private JObject BuildPatchRequest(IEnumerable<PatchOperation> operations)
        //{
        //    var patch = new JObject { [ScimConstants.ScimResourceNames.Schemas] = new JArray(new object[] { ScimConstants.Messages.PatchOp }) };
        //    var arr = new JArray();
        //    foreach (var operation in operations)
        //    {
        //        var obj = new JObject
        //        {
        //            new JProperty(ScimConstants.PatchOperationRequestNames.Operation,
        //                Enum.GetName(typeof(PatchOperations), operation.Type))
        //        };
        //        if (!string.IsNullOrWhiteSpace(operation.Path))
        //        {
        //            obj.Add(new JProperty(ScimConstants.PatchOperationRequestNames.Path, operation.Path));
        //        }

        //        if (operation.Value != null)
        //        {
        //            obj.Add(new JProperty(ScimConstants.PatchOperationRequestNames.Value, operation.Value));
        //        }

        //        arr.Add(obj);
        //    }

        //    patch.Add(new JProperty(ScimConstants.PatchOperationsRequestNames.Operations, arr));
        //    return patch;
        //}

        private async Task<ScimResponse<T>> ExecuteRequest<T>(
            ScimUser jObj,
            Uri uri,
            HttpMethod method,
            string accessToken = null)
        {
            var request = new HttpRequestMessage
            {
                Method = method,
                RequestUri = uri,
                Content = new StringContent(JsonConvert.SerializeObject(jObj, JsonSettings))
            };
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            if (!string.IsNullOrWhiteSpace(accessToken))
            {
                request.Headers.Add("Authorization", "Bearer " + accessToken);
            }

            var response = await _httpClientFactory.SendAsync(request).ConfigureAwait(false);
            return await ParseHttpResponse<T>(response).ConfigureAwait(false);
        }

        private static async Task<ScimResponse<T>> ParseHttpResponse<T>(HttpResponseMessage response)
        {
            var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            var result = new ScimResponse<T>
            {
                StatusCode = response.StatusCode
            };
            if (!string.IsNullOrWhiteSpace(json))
            {
                result.Content = JsonConvert.DeserializeObject<T>(json);
            }

            return result;
        }
    }
}
