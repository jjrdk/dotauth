// Copyright © 2015 Habart Thierry, © 2018 Jacob Reimers
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

namespace SimpleAuth.Uma.Client.Policy
{
    using Configuration;
    using Helpers;
    using Newtonsoft.Json;
    using Results;
    using Shared.DTOs;
    using SimpleAuth.Shared;
    using SimpleAuth.Shared.Responses;
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;

    internal class PolicyClient
    {
        private const string JsonMimeType = "application/json";
        private const string AuthorizationHeader = "Authorization";
        private const string Bearer = "Bearer ";
        private readonly HttpClient _client;
        private readonly IGetConfigurationOperation _getConfigurationOperation;

        public PolicyClient(
            HttpClient client,
            IGetConfigurationOperation getConfigurationOperation)
        {
            _client = client;
            _getConfigurationOperation = getConfigurationOperation;
        }

        public async Task<AddPolicyResult> Add(PostPolicy request, string url, string authorizationHeaderValue)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (string.IsNullOrWhiteSpace(url))
            {
                throw new ArgumentNullException(nameof(url));
            }

            if (string.IsNullOrWhiteSpace(authorizationHeaderValue))
            {
                throw new ArgumentNullException(nameof(authorizationHeaderValue));
            }

            var serializedPostResourceSet = JsonConvert.SerializeObject(request);
            var body = new StringContent(serializedPostResourceSet, Encoding.UTF8, JsonMimeType);
            var httpRequest = new HttpRequestMessage
            {
                Content = body,
                Method = HttpMethod.Post,
                RequestUri = new Uri(url)
            };
            httpRequest.Headers.Add(AuthorizationHeader, Bearer + authorizationHeaderValue);
            var httpResult = await _client.SendAsync(httpRequest).ConfigureAwait(false);
            var content = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);
            try
            {
                httpResult.EnsureSuccessStatusCode();
            }
            catch (Exception)
            {
                return new AddPolicyResult
                {
                    ContainsError = true,
                    Error = JsonConvert.DeserializeObject<ErrorResponse>(content),
                    HttpStatus = httpResult.StatusCode
                };
            }

            return new AddPolicyResult
            {
                Content = JsonConvert.DeserializeObject<AddPolicyResponse>(content)
            };
        }

        public async Task<AddPolicyResult> AddByResolution(PostPolicy request, string url, string token)
        {
            var policyEndpoint = await GetPolicyEndPoint(UriHelpers.GetUri(url)).ConfigureAwait(false);
            return await Add(request, policyEndpoint, token).ConfigureAwait(false);
        }

        public async Task<GetPolicyResult> Get(string id, string url, string token)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            if (string.IsNullOrWhiteSpace(url))
            {
                throw new ArgumentNullException(nameof(url));
            }

            if (string.IsNullOrWhiteSpace(token))
            {
                throw new ArgumentNullException(nameof(token));
            }

            if (url.EndsWith("/"))
            {
                url = url.Remove(0, url.Length - 1);
            }

            url = url + "/" + id;
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(url)
            };
            request.Headers.Add(AuthorizationHeader, Bearer + token);
            var httpResult = await _client.SendAsync(request).ConfigureAwait(false);
            var content = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);
            try
            {
                httpResult.EnsureSuccessStatusCode();
            }
            catch (Exception)
            {
                return new GetPolicyResult
                {
                    ContainsError = true,
                    Error = JsonConvert.DeserializeObject<ErrorResponse>(content),
                    HttpStatus = httpResult.StatusCode
                };
            }

            return new GetPolicyResult
            {
                Content = JsonConvert.DeserializeObject<PolicyResponse>(content)
            };
        }

        public async Task<GetPolicyResult> GetByResolution(string id, string url, string token)
        {
            var policyEndpoint = await GetPolicyEndPoint(UriHelpers.GetUri(url)).ConfigureAwait(false);
            return await Get(id, policyEndpoint, token).ConfigureAwait(false);
        }

        public async Task<GetPoliciesResult> GetAll(string url, string token)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                throw new ArgumentNullException(nameof(url));
            }

            if (string.IsNullOrWhiteSpace(token))
            {
                throw new ArgumentNullException(nameof(token));
            }

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(url)
            };
            request.Headers.Add(AuthorizationHeader, Bearer + token);
            var httpResult = await _client.SendAsync(request).ConfigureAwait(false);
            var content = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);
            try
            {
                httpResult.EnsureSuccessStatusCode();
            }
            catch (Exception)
            {
                return new GetPoliciesResult
                {
                    ContainsError = true,
                    Error = JsonConvert.DeserializeObject<ErrorResponse>(content),
                    HttpStatus = httpResult.StatusCode
                };
            }

            return new GetPoliciesResult
            {
                Content = JsonConvert.DeserializeObject<IEnumerable<string>>(content)
            };
        }

        public async Task<GetPoliciesResult> GetAllByResolution(string url, string token)
        {
            var policyEndpoint = await GetPolicyEndPoint(UriHelpers.GetUri(url)).ConfigureAwait(false);
            return await GetAll(policyEndpoint, token).ConfigureAwait(false);
        }

        public async Task<BaseResponse> Delete(string id, string url, string token)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            if (string.IsNullOrWhiteSpace(url))
            {
                throw new ArgumentNullException(nameof(url));
            }

            if (string.IsNullOrWhiteSpace(token))
            {
                throw new ArgumentNullException(nameof(token));
            }

            if (url.EndsWith("/"))
            {
                url = url.Remove(0, url.Length - 1);
            }

            url = url + "/" + id;
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Delete,
                RequestUri = new Uri(url)
            };
            request.Headers.Add(AuthorizationHeader, Bearer + token);
            var httpResult = await _client.SendAsync(request).ConfigureAwait(false);
            var content = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);
            try
            {
                httpResult.EnsureSuccessStatusCode();
            }
            catch (Exception)
            {
                return new BaseResponse
                {
                    ContainsError = true,
                    Error = JsonConvert.DeserializeObject<ErrorResponse>(content),
                    HttpStatus = httpResult.StatusCode
                };
            }

            return new BaseResponse();
        }

        public async Task<BaseResponse> DeleteByResolution(string id, string url, string token)
        {
            var policyEndpoint = await GetPolicyEndPoint(UriHelpers.GetUri(url)).ConfigureAwait(false);
            return await Delete(id, policyEndpoint, token).ConfigureAwait(false);
        }

        public async Task<BaseResponse> Update(PutPolicy request, string url, string token)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (string.IsNullOrWhiteSpace(url))
            {
                throw new ArgumentNullException(nameof(url));
            }

            if (string.IsNullOrWhiteSpace(token))
            {
                throw new ArgumentNullException(nameof(token));
            }

            var serializedPostResourceSet = JsonConvert.SerializeObject(request);
            var body = new StringContent(serializedPostResourceSet, Encoding.UTF8, JsonMimeType);
            var httpRequest = new HttpRequestMessage
            {
                Content = body,
                Method = HttpMethod.Put,
                RequestUri = new Uri(url)
            };
            httpRequest.Headers.Add(AuthorizationHeader, Bearer + token);
            var httpResult = await _client.SendAsync(httpRequest).ConfigureAwait(false);
            var content = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);
            try
            {
                httpResult.EnsureSuccessStatusCode();
            }
            catch (Exception)
            {
                return new BaseResponse
                {
                    ContainsError = true,
                    Error = JsonConvert.DeserializeObject<ErrorResponse>(content),
                    HttpStatus = httpResult.StatusCode
                };
            }

            return new BaseResponse();
        }

        public async Task<BaseResponse> UpdateByResolution(PutPolicy request, string url, string token)
        {
            var policyEndpoint = await GetPolicyEndPoint(UriHelpers.GetUri(url)).ConfigureAwait(false);
            return await Update(request, policyEndpoint, token).ConfigureAwait(false);
        }

        public async Task<BaseResponse> AddResource(string id, PostAddResourceSet request, string url, string token)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (string.IsNullOrWhiteSpace(url))
            {
                throw new ArgumentNullException(nameof(url));
            }

            if (string.IsNullOrWhiteSpace(token))
            {
                throw new ArgumentNullException(nameof(token));
            }

            if (url.EndsWith("/"))
            {
                url = url.Remove(0, url.Length - 1);
            }

            url = url + "/" + id + "/resources";
            var serializedPostResourceSet = JsonConvert.SerializeObject(request);
            var body = new StringContent(serializedPostResourceSet, Encoding.UTF8, JsonMimeType);
            var httpRequest = new HttpRequestMessage
            {
                Content = body,
                Method = HttpMethod.Post,
                RequestUri = new Uri(url)
            };
            httpRequest.Headers.Add(AuthorizationHeader, Bearer + token);
            var httpResult = await _client.SendAsync(httpRequest).ConfigureAwait(false);
            var content = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);
            try
            {
                httpResult.EnsureSuccessStatusCode();
            }
            catch (Exception)
            {
                return new BaseResponse
                {
                    ContainsError = true,
                    Error = JsonConvert.DeserializeObject<ErrorResponse>(content),
                    HttpStatus = httpResult.StatusCode
                };
            }

            return new BaseResponse();
        }

        public async Task<BaseResponse> AddResourceByResolution(string id, PostAddResourceSet request, string url, string token)
        {
            var policyEndpoint = await GetPolicyEndPoint(UriHelpers.GetUri(url)).ConfigureAwait(false);
            return await AddResource(id, request, policyEndpoint, token).ConfigureAwait(false);
        }

        public async Task<BaseResponse> DeleteResource(string id, string resourceId, string url, string token)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            if (string.IsNullOrWhiteSpace(resourceId))
            {
                throw new ArgumentNullException(nameof(resourceId));
            }

            if (string.IsNullOrWhiteSpace(url))
            {
                throw new ArgumentNullException(nameof(url));
            }

            if (string.IsNullOrWhiteSpace(token))
            {
                throw new ArgumentNullException(nameof(token));
            }

            if (url.EndsWith("/"))
            {
                url = url.Remove(0, url.Length - 1);
            }

            url = url + "/" + id + "/resources/" + resourceId;
            var httpRequest = new HttpRequestMessage
            {
                Method = HttpMethod.Delete,
                RequestUri = new Uri(url)
            };
            httpRequest.Headers.Add(AuthorizationHeader, Bearer + token);
            var httpResult = await _client.SendAsync(httpRequest).ConfigureAwait(false);
            var content = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);
            try
            {
                httpResult.EnsureSuccessStatusCode();
            }
            catch (Exception)
            {
                return new BaseResponse
                {
                    ContainsError = true,
                    Error = JsonConvert.DeserializeObject<ErrorResponse>(content),
                    HttpStatus = httpResult.StatusCode
                };
            }

            return new BaseResponse();
        }

        public async Task<BaseResponse> DeleteResourceByResolution(string id, string resourceId, string url, string token)
        {
            var policyEndpoint = await GetPolicyEndPoint(UriHelpers.GetUri(url)).ConfigureAwait(false);
            return await DeleteResource(id, resourceId, policyEndpoint, token).ConfigureAwait(false);
        }

        public async Task<SearchAuthPoliciesResult> ResolveSearch(string url, SearchAuthPolicies parameter, string authorizationHeaderValue = null)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                throw new ArgumentNullException(nameof(url));
            }
            var policyEndpoint = await GetPolicyEndPoint(UriHelpers.GetUri(url)).ConfigureAwait(false);
            url = policyEndpoint + "/.search";
            var serializedPostPermission = JsonConvert.SerializeObject(parameter);
            var body = new StringContent(serializedPostPermission, Encoding.UTF8, JsonMimeType);
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(url),
                Content = body
            };
            if (!string.IsNullOrWhiteSpace(authorizationHeaderValue))
            {
                request.Headers.Add(AuthorizationHeader, Bearer + authorizationHeaderValue);
            }

            var httpResult = await _client.SendAsync(request).ConfigureAwait(false);
            var content = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);
            try
            {
                httpResult.EnsureSuccessStatusCode();
            }
            catch (Exception)
            {
                return new SearchAuthPoliciesResult
                {
                    ContainsError = true,
                    Error = JsonConvert.DeserializeObject<ErrorResponse>(content),
                    HttpStatus = httpResult.StatusCode
                };
            }

            return new SearchAuthPoliciesResult
            {
                Content = JsonConvert.DeserializeObject<SearchAuthPoliciesResponse>(content)
            };
        }

        private async Task<string> GetPolicyEndPoint(Uri configurationUri)
        {
            if (configurationUri == null)
            {
                throw new ArgumentNullException(nameof(configurationUri));
            }

            var configuration = await _getConfigurationOperation.ExecuteAsync(configurationUri).ConfigureAwait(false);
            return configuration.PoliciesEndpoint;
        }
    }
}
