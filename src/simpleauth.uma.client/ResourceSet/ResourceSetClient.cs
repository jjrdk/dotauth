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

namespace SimpleAuth.Uma.Client.ResourceSet
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

    internal class ResourceSetClient
    {
        private const string JsonMimeType = "application/json";
        private const string AuthorizationHeader = "Authorization";
        private const string Bearer = "Bearer ";
        private readonly HttpClient _client;
        private readonly IGetConfigurationOperation _getConfigurationOperation;

        public ResourceSetClient(
            HttpClient client,
            IGetConfigurationOperation getConfigurationOperation)
        {
            _client = client;
            _getConfigurationOperation = getConfigurationOperation;
        }

        public async Task<UpdateResourceSetResult> Update(PutResourceSet request, string url, string token)
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
                return new UpdateResourceSetResult
                {
                    ContainsError = true,
                    Error = JsonConvert.DeserializeObject<ErrorResponse>(content),
                    HttpStatus = httpResult.StatusCode
                };
            }

            return new UpdateResourceSetResult
            {
                Content = JsonConvert.DeserializeObject<UpdateResourceSetResponse>(content)
            };
        }

        public async Task<UpdateResourceSetResult> UpdateByResolution(PutResourceSet request, string url, string token)
        {
            var configuration = await _getConfigurationOperation.ExecuteAsync(UriHelpers.GetUri(url)).ConfigureAwait(false);
            return await Update(request, configuration.ResourceRegistrationEndpoint, token).ConfigureAwait(false);
        }

        public async Task<AddResourceSetResult> Add(PostResourceSet request, string url, string token)
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
                return new AddResourceSetResult
                {
                    ContainsError = true,
                    Error = JsonConvert.DeserializeObject<ErrorResponse>(content),
                    HttpStatus = httpResult.StatusCode
                };
            }

            return new AddResourceSetResult
            {
                Content = JsonConvert.DeserializeObject<AddResourceSetResponse>(content)
            };
        }

        public async Task<AddResourceSetResult> AddByResolution(PostResourceSet request, string url, string token)
        {
            var configuration = await _getConfigurationOperation.ExecuteAsync(UriHelpers.GetUri(url)).ConfigureAwait(false);
            return await Add(request, configuration.ResourceRegistrationEndpoint, token).ConfigureAwait(false);
        }

        public async Task<BaseResponse> Delete(string resourceSetId, string resourceSetUrl, string authorizationHeaderValue)
        {
            if (string.IsNullOrWhiteSpace(resourceSetId))
            {
                throw new ArgumentNullException(nameof(resourceSetId));
            }

            if (string.IsNullOrWhiteSpace(resourceSetUrl))
            {
                throw new ArgumentNullException(nameof(resourceSetUrl));
            }

            if (string.IsNullOrWhiteSpace(authorizationHeaderValue))
            {
                throw new ArgumentNullException(nameof(authorizationHeaderValue));
            }

            if (resourceSetUrl.EndsWith("/"))
            {
                resourceSetUrl = resourceSetUrl.Remove(0, resourceSetUrl.Length - 1);
            }

            resourceSetUrl = resourceSetUrl + "/" + resourceSetId;
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Delete,
                RequestUri = new Uri(resourceSetUrl)
            };
            request.Headers.Add(AuthorizationHeader, Bearer + authorizationHeaderValue);
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
            var configuration = await _getConfigurationOperation.ExecuteAsync(UriHelpers.GetUri(url)).ConfigureAwait(false);
            return await Delete(id, configuration.ResourceRegistrationEndpoint, token).ConfigureAwait(false);
        }

        public async Task<GetResourcesResult> GetAll(string resourceSetUrl, string authorizationHeaderValue)
        {
            if (string.IsNullOrWhiteSpace(resourceSetUrl))
            {
                throw new ArgumentNullException(nameof(resourceSetUrl));
            }

            if (string.IsNullOrWhiteSpace(authorizationHeaderValue))
            {
                throw new ArgumentNullException(nameof(authorizationHeaderValue));
            }

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(resourceSetUrl)
            };
            request.Headers.Add(AuthorizationHeader, Bearer + authorizationHeaderValue);
            var httpResult = await _client.SendAsync(request).ConfigureAwait(false);
            var json = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);
            try
            {
                httpResult.EnsureSuccessStatusCode();
            }
            catch (Exception)
            {
                return new GetResourcesResult
                {
                    ContainsError = true,
                    Error = JsonConvert.DeserializeObject<ErrorResponse>(json),
                    HttpStatus = httpResult.StatusCode
                };
            }

            return new GetResourcesResult
            {
                Content = JsonConvert.DeserializeObject<IEnumerable<string>>(json)
            };
        }

        public async Task<GetResourcesResult> GetAllByResolution(string url, string token)
        {
            var configuration = await _getConfigurationOperation.ExecuteAsync(UriHelpers.GetUri(url)).ConfigureAwait(false);
            return await GetAll(configuration.ResourceRegistrationEndpoint, token).ConfigureAwait(false);
        }

        public async Task<GetResourceSetResult> Get(string resourceSetId, string resourceSetUrl, string authorizationHeaderValue)
        {
            if (string.IsNullOrWhiteSpace(resourceSetId))
            {
                throw new ArgumentNullException(nameof(resourceSetId));
            }

            if (string.IsNullOrWhiteSpace(resourceSetUrl))
            {
                throw new ArgumentNullException(nameof(resourceSetUrl));
            }

            if (string.IsNullOrWhiteSpace(authorizationHeaderValue))
            {
                throw new ArgumentNullException(nameof(authorizationHeaderValue));
            }

            if (resourceSetUrl.EndsWith("/"))
            {
                resourceSetUrl = resourceSetUrl.Remove(0, resourceSetUrl.Length - 1);
            }

            resourceSetUrl = resourceSetUrl + "/" + resourceSetId;
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(resourceSetUrl)
            };
            request.Headers.Add(AuthorizationHeader, Bearer + authorizationHeaderValue);
            var httpResult = await _client.SendAsync(request).ConfigureAwait(false);
            var json = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);
            try
            {
                httpResult.EnsureSuccessStatusCode();
            }
            catch (Exception)
            {
                return new GetResourceSetResult
                {
                    ContainsError = true,
                    Error = JsonConvert.DeserializeObject<ErrorResponse>(json),
                    HttpStatus = httpResult.StatusCode
                };
            }

            return new GetResourceSetResult
            {
                Content = JsonConvert.DeserializeObject<ResourceSetResponse>(json)
            };
        }

        public async Task<GetResourceSetResult> GetByResolution(string id, string url, string token)
        {
            var configuration = await _getConfigurationOperation.ExecuteAsync(UriHelpers.GetUri(url)).ConfigureAwait(false);
            return await Get(id, configuration.ResourceRegistrationEndpoint, token).ConfigureAwait(false);
        }

        public async Task<SearchResourceSetResult> ResolveSearch(string url, SearchResourceSet parameter, string authorizationHeaderValue = null)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                throw new ArgumentNullException(nameof(url));
            }

            var configuration = await _getConfigurationOperation.ExecuteAsync(UriHelpers.GetUri(url)).ConfigureAwait(false);
            url = configuration.ResourceRegistrationEndpoint + "/.search";

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
                return new SearchResourceSetResult
                {
                    ContainsError = true,
                    Error = JsonConvert.DeserializeObject<ErrorResponse>(content),
                    HttpStatus = httpResult.StatusCode
                };
            }

            return new SearchResourceSetResult
            {
                Content = JsonConvert.DeserializeObject<SearchResourceSetResponse>(content)
            };
        }
    }
}
