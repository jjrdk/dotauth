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

using Newtonsoft.Json.Linq;
using SimpleIdentityServer.Scim.Client.Builders;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Scim.Client
{
    public interface IUsersClient
    {
        RequestBuilder AddUser(Uri baseUri, string accessToken = null);
        //Task<ScimResponse> AddAuthenticatedUser(Uri baseUri, string accessToken);
        //PatchRequestBuilder PartialUpdateUser(Uri baseUri, string id, string accessToken = null);
        //PatchRequestBuilder PartialUpdateAuthenticatedUser(Uri baseUri, string accessToken = null);
        RequestBuilder UpdateUser(Uri baseUri, string id, string accessToken = null);
        RequestBuilder UpdateAuthenticatedUser(Uri baseUri, string accessToken = null);
        Task<ScimResponse> DeleteUser(Uri baseUri, string id, string accessToken = null);
        Task<ScimResponse> DeleteAuthenticatedUser(Uri baseUri, string accessToken);
        Task<ScimResponse> GetUser(Uri baseUri, string id, string accessToken = null);
        Task<ScimResponse> GetAuthenticatedUser(Uri baseUri, string accessToken = null);
        Task<ScimResponse> SearchUsers(Uri baseUri, SearchParameter parameter, string accessToken = null);
    }

    internal class UsersClient : IUsersClient
    {
        private readonly string _schema = Common.ScimConstants.SchemaUrns.User;
        private readonly HttpClient _httpClientFactory;

        public UsersClient(HttpClient httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public RequestBuilder AddUser(Uri baseUri, string accessToken = null)
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

            return new RequestBuilder(_schema, (obj) => AddUser(obj, new Uri(url), accessToken));
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

        public RequestBuilder UpdateUser(Uri baseUri, string id, string accessToken = null)
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
            return new RequestBuilder(_schema, (obj) => UpdateUser(obj, new Uri(url), accessToken));
        }

        public RequestBuilder UpdateAuthenticatedUser(Uri baseUri, string accessToken = null)
        {
            if (baseUri == null)
            {
                throw new ArgumentNullException(nameof(baseUri));
            }

            var url = $"{FormatUrl(baseUri.AbsoluteUri)}/Me";
            return new RequestBuilder(_schema, (obj) => UpdateUser(obj, new Uri(url), accessToken));
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

        public async Task<ScimResponse> GetUser(Uri baseUri, string id, string accessToken = null)
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

        private Task<ScimResponse> AddUser(JObject jObj, Uri uri, string accessToken = null)
        {
            return ExecuteRequest(jObj, uri, HttpMethod.Post, accessToken);
        }

        private Task<ScimResponse> UpdateUser(JObject jObj, Uri uri, string accessToken = null)
        {
            return ExecuteRequest(jObj, uri, HttpMethod.Put, accessToken);
        }

        private async Task<ScimResponse> ExecuteRequest(JObject jObj, Uri uri, HttpMethod method, string accessToken = null)
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
