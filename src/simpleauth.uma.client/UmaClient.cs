// Copyright © 2018 Habart Thierry, © 2018 Jacob Reimers
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

namespace SimpleAuth.Uma.Client
{
    using Newtonsoft.Json;
    using SimpleAuth.Shared;
    using SimpleAuth.Shared.DTOs;
    using SimpleAuth.Shared.Models;
    using SimpleAuth.Shared.Responses;
    using System;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// Defines the UMA client.
    /// </summary>
    public class UmaClient
    {
        private const string JsonMimeType = "application/json";
        private const string AuthorizationHeader = "Authorization";
        private const string Bearer = "Bearer ";
        private readonly HttpClient _client;
        private readonly UmaConfigurationResponse _configurationResponse;

        private UmaClient(HttpClient client, UmaConfigurationResponse configurationResponse)
        {
            _client = client;
            _configurationResponse = configurationResponse;
        }

        /// <summary>
        /// Creates the specified client.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="configurationUri">The configuration URI.</param>
        /// <returns></returns>
        public static async Task<UmaClient> Create(HttpClient client, Uri configurationUri)
        {
            var discoveryOperation = new GetConfigurationOperation(client);
            var configurationResponse = await discoveryOperation.Execute(configurationUri).ConfigureAwait(false);

            return new UmaClient(client, configurationResponse);
        }

        /// <summary>
        /// Adds the permission.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="token">The token.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">
        /// request
        /// or
        /// token
        /// </exception>
        public async Task<GenericResponse<AddPermissionResponse>> AddPermission(PostPermission request, string token)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (string.IsNullOrWhiteSpace(token))
            {
                throw new ArgumentNullException(nameof(token));
            }

            var serializedPostPermission = JsonConvert.SerializeObject(request);
            var body = new StringContent(serializedPostPermission, Encoding.UTF8, JsonMimeType);
            var httpRequest = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                Content = body,
                RequestUri = new Uri(_configurationResponse.PermissionEndpoint)
            };
            httpRequest.Headers.Add(AuthorizationHeader, Bearer + token);
            var result = await _client.SendAsync(httpRequest).ConfigureAwait(false);
            var content = await result.Content.ReadAsStringAsync().ConfigureAwait(false);
            if (!result.IsSuccessStatusCode)
            {
                return new GenericResponse<AddPermissionResponse>
                {
                    ContainsError = true,
                    HttpStatus = result.StatusCode,
                    Error = JsonConvert.DeserializeObject<ErrorDetails>(content)
                };
            }

            return new GenericResponse<AddPermissionResponse>
            {
                Content = JsonConvert.DeserializeObject<AddPermissionResponse>(content)
            };
        }

        /// <summary>
        /// Adds the permissions.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <param name="requests">The requests.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">
        /// requests
        /// or
        /// token
        /// </exception>
        public async Task<GenericResponse<AddPermissionResponse>> AddPermissions(
            string token,
            params PostPermission[] requests)
        {
            if (requests == null)
            {
                throw new ArgumentNullException(nameof(requests));
            }

            if (string.IsNullOrWhiteSpace(token))
            {
                throw new ArgumentNullException(nameof(token));
            }

            var url = _configurationResponse.PermissionEndpoint;

            url += url.EndsWith("/") ? "bulk" : "/bulk";

            var serializedPostPermission = JsonConvert.SerializeObject(requests);
            var body = new StringContent(serializedPostPermission, Encoding.UTF8, JsonMimeType);
            var httpRequest = new HttpRequestMessage
            {
                Method = HttpMethod.Post, Content = body, RequestUri = new Uri(url)
            };
            httpRequest.Headers.Add(AuthorizationHeader, Bearer + token);
            var result = await _client.SendAsync(httpRequest).ConfigureAwait(false);
            var content = await result.Content.ReadAsStringAsync().ConfigureAwait(false);
            if (!result.IsSuccessStatusCode)
            {
                return new GenericResponse<AddPermissionResponse>
                {
                    ContainsError = true,
                    HttpStatus = result.StatusCode,
                    Error = JsonConvert.DeserializeObject<ErrorDetails>(content)
                };
            }

            return new GenericResponse<AddPermissionResponse>
            {
                Content = JsonConvert.DeserializeObject<AddPermissionResponse>(content)
            };
        }

        /// <summary>
        /// Adds the policy.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="authorizationHeaderValue">The authorization header value.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">
        /// request
        /// or
        /// authorizationHeaderValue
        /// </exception>
        public async Task<GenericResponse<AddPolicyResponse>> AddPolicy(
            PostPolicy request,
            string authorizationHeaderValue)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (string.IsNullOrWhiteSpace(authorizationHeaderValue))
            {
                throw new ArgumentNullException(nameof(authorizationHeaderValue));
            }

            var serializedPostResourceSet = Serializer.Default.Serialize(request);
            var body = new StringContent(serializedPostResourceSet, Encoding.UTF8, JsonMimeType);
            var httpRequest = new HttpRequestMessage
            {
                Content = body,
                Method = HttpMethod.Post,
                RequestUri = new Uri(_configurationResponse.PoliciesEndpoint)
            };
            httpRequest.Headers.Add(AuthorizationHeader, Bearer + authorizationHeaderValue);
            var httpResult = await _client.SendAsync(httpRequest).ConfigureAwait(false);
            var content = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);
            if (!httpResult.IsSuccessStatusCode)
            {
                return new GenericResponse<AddPolicyResponse>
                {
                    ContainsError = true,
                    Error = Serializer.Default.Deserialize<ErrorDetails>(content),
                    HttpStatus = httpResult.StatusCode
                };
            }

            return new GenericResponse<AddPolicyResponse>
            {
                Content = Serializer.Default.Deserialize<AddPolicyResponse>(content)
            };
        }

        /// <summary>
        /// Gets the policy.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="token">The token.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">
        /// id
        /// or
        /// token
        /// </exception>
        public async Task<GenericResponse<PolicyResponse>> GetPolicy(string id, string token)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            if (string.IsNullOrWhiteSpace(token))
            {
                throw new ArgumentNullException(nameof(token));
            }

            var url = _configurationResponse.PoliciesEndpoint;

            url += url.EndsWith("/") ? id : "/" + id;

            var request = new HttpRequestMessage {Method = HttpMethod.Get, RequestUri = new Uri(url)};
            request.Headers.Add(AuthorizationHeader, Bearer + token);
            var httpResult = await _client.SendAsync(request).ConfigureAwait(false);
            var content = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);
            if (!httpResult.IsSuccessStatusCode)
            {
                return new GenericResponse<PolicyResponse>
                {
                    ContainsError = true,
                    Error = Serializer.Default.Deserialize<ErrorDetails>(content),
                    HttpStatus = httpResult.StatusCode
                };
            }

            return new GenericResponse<PolicyResponse>
            {
                Content = Serializer.Default.Deserialize<PolicyResponse>(content)
            };
        }

        /// <summary>
        /// Gets all policies.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">token</exception>
        public async Task<GenericResponse<string[]>> GetAllPolicies(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                throw new ArgumentNullException(nameof(token));
            }

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get, RequestUri = new Uri(_configurationResponse.PoliciesEndpoint)
            };
            request.Headers.Add(AuthorizationHeader, Bearer + token);
            var httpResult = await _client.SendAsync(request).ConfigureAwait(false);
            var content = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);
            if (!httpResult.IsSuccessStatusCode)
            {
                return new GenericResponse<string[]>
                {
                    ContainsError = true,
                    Error = JsonConvert.DeserializeObject<ErrorDetails>(content),
                    HttpStatus = httpResult.StatusCode
                };
            }

            return new GenericResponse<string[]> {Content = JsonConvert.DeserializeObject<string[]>(content)};
        }

        /// <summary>
        /// Deletes the policy.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="token">The token.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">
        /// id
        /// or
        /// token
        /// </exception>
        public async Task<GenericResponse<object>> DeletePolicy(string id, string token)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            if (string.IsNullOrWhiteSpace(token))
            {
                throw new ArgumentNullException(nameof(token));
            }

            var url = _configurationResponse.PoliciesEndpoint;

            url += url.EndsWith("/") ? id : "/" + id;

            var request = new HttpRequestMessage {Method = HttpMethod.Delete, RequestUri = new Uri(url)};
            request.Headers.Add(AuthorizationHeader, Bearer + token);
            var httpResult = await _client.SendAsync(request).ConfigureAwait(false);
            var content = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);
            if (!httpResult.IsSuccessStatusCode)
            {
                return new GenericResponse<object>
                {
                    ContainsError = true,
                    Error = JsonConvert.DeserializeObject<ErrorDetails>(content),
                    HttpStatus = httpResult.StatusCode
                };
            }

            return new GenericResponse<object>();
        }

        /// <summary>
        /// Updates the policy.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="token">The token.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">
        /// request
        /// or
        /// token
        /// </exception>
        public async Task<GenericResponse<object>> UpdatePolicy(PutPolicy request, string token)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
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
                RequestUri = new Uri(_configurationResponse.PoliciesEndpoint)
            };
            httpRequest.Headers.Add(AuthorizationHeader, Bearer + token);
            var httpResult = await _client.SendAsync(httpRequest).ConfigureAwait(false);
            var content = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);
            if (!httpResult.IsSuccessStatusCode)
            {
                return new GenericResponse<object>
                {
                    ContainsError = true,
                    Error = JsonConvert.DeserializeObject<ErrorDetails>(content),
                    HttpStatus = httpResult.StatusCode
                };
            }

            return new GenericResponse<object>();
        }

        /// <summary>
        /// Adds the resource.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="request">The request.</param>
        /// <param name="token">The token.</param>
        /// <returns></returns>
        public async Task<GenericResponse<object>> AddResource(string id, PostAddResourceSet request, string token)
        {
            var url = _configurationResponse.PoliciesEndpoint;
            url += url.EndsWith("/") ? id + "/resources" : "/" + id + "/resources";

            var serializedPostResourceSet = JsonConvert.SerializeObject(request);
            var body = new StringContent(serializedPostResourceSet, Encoding.UTF8, JsonMimeType);
            var httpRequest = new HttpRequestMessage
            {
                Content = body, Method = HttpMethod.Post, RequestUri = new Uri(url)
            };
            httpRequest.Headers.Add(AuthorizationHeader, Bearer + token);
            var httpResult = await _client.SendAsync(httpRequest).ConfigureAwait(false);
            var content = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);
            if (!httpResult.IsSuccessStatusCode)
            {
                return new GenericResponse<object>
                {
                    ContainsError = true,
                    Error = JsonConvert.DeserializeObject<ErrorDetails>(content),
                    HttpStatus = httpResult.StatusCode
                };
            }

            return new GenericResponse<object>();
        }

        /// <summary>
        /// Deletes the policy resource.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="resourceId">The resource identifier.</param>
        /// <param name="token">The token.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">
        /// id
        /// or
        /// resourceId
        /// or
        /// token
        /// </exception>
        public async Task<GenericResponse<object>> DeletePolicyResource(string id, string resourceId, string token)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            if (string.IsNullOrWhiteSpace(resourceId))
            {
                throw new ArgumentNullException(nameof(resourceId));
            }

            if (string.IsNullOrWhiteSpace(token))
            {
                throw new ArgumentNullException(nameof(token));
            }

            var url = _configurationResponse.PoliciesEndpoint;
            url += url.EndsWith("/") ? id + "/resources/" + resourceId : "/" + id + "/resources/" + resourceId;

            var httpRequest = new HttpRequestMessage {Method = HttpMethod.Delete, RequestUri = new Uri(url)};
            httpRequest.Headers.Add(AuthorizationHeader, Bearer + token);
            var httpResult = await _client.SendAsync(httpRequest).ConfigureAwait(false);
            var content = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);
            if (!httpResult.IsSuccessStatusCode)
            {
                return new GenericResponse<object>
                {
                    ContainsError = true,
                    Error = JsonConvert.DeserializeObject<ErrorDetails>(content),
                    HttpStatus = httpResult.StatusCode
                };
            }

            return new GenericResponse<object>();
        }

        /// <summary>
        /// Deletes the resource.
        /// </summary>
        /// <param name="resourceSetId">The resource set identifier.</param>
        /// <param name="authorizationHeaderValue">The authorization header value.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">
        /// resourceSetId
        /// or
        /// authorizationHeaderValue
        /// </exception>
        public async Task<GenericResponse<object>> DeleteResource(string resourceSetId, string authorizationHeaderValue)
        {
            if (string.IsNullOrWhiteSpace(resourceSetId))
            {
                throw new ArgumentNullException(nameof(resourceSetId));
            }

            if (string.IsNullOrWhiteSpace(authorizationHeaderValue))
            {
                throw new ArgumentNullException(nameof(authorizationHeaderValue));
            }

            var resourceSetUrl = _configurationResponse.ResourceRegistrationEndpoint;
            resourceSetUrl += resourceSetUrl.EndsWith("/") ? resourceSetId : "/" + resourceSetId;

            var request = new HttpRequestMessage {Method = HttpMethod.Delete, RequestUri = new Uri(resourceSetUrl)};
            request.Headers.Add(AuthorizationHeader, Bearer + authorizationHeaderValue);
            var httpResult = await _client.SendAsync(request).ConfigureAwait(false);
            var content = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);

            if (!httpResult.IsSuccessStatusCode)
            {
                return new GenericResponse<object>
                {
                    ContainsError = true,
                    Error = JsonConvert.DeserializeObject<ErrorDetails>(content),
                    HttpStatus = httpResult.StatusCode
                };
            }

            return new GenericResponse<object>();
        }

        /// <summary>
        /// Searches the policies.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        /// <param name="authorizationHeaderValue">The authorization header value.</param>
        /// <returns></returns>
        public async Task<GenericResponse<SearchAuthPoliciesResponse>> SearchPolicies(
            SearchAuthPolicies parameter,
            string authorizationHeaderValue = null)
        {
            var url = _configurationResponse.PoliciesEndpoint + "/.search";
            var serializedPostPermission = JsonConvert.SerializeObject(parameter);
            var body = new StringContent(serializedPostPermission, Encoding.UTF8, JsonMimeType);
            var request = new HttpRequestMessage {Method = HttpMethod.Post, RequestUri = new Uri(url), Content = body};
            if (!string.IsNullOrWhiteSpace(authorizationHeaderValue))
            {
                request.Headers.Add(AuthorizationHeader, Bearer + authorizationHeaderValue);
            }

            var httpResult = await _client.SendAsync(request).ConfigureAwait(false);
            var content = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);
            if (!httpResult.IsSuccessStatusCode)
            {
                return new GenericResponse<SearchAuthPoliciesResponse>
                {
                    ContainsError = true,
                    Error = JsonConvert.DeserializeObject<ErrorDetails>(content),
                    HttpStatus = httpResult.StatusCode
                };
            }

            return new GenericResponse<SearchAuthPoliciesResponse>
            {
                Content = JsonConvert.DeserializeObject<SearchAuthPoliciesResponse>(content)
            };
        }

        /// <summary>
        /// Updates the resource.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="token">The token.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">
        /// request
        /// or
        /// token
        /// </exception>
        public async Task<GenericResponse<UpdateResourceSetResponse>> UpdateResource(
            PutResourceSet request,
            string token)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
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
                RequestUri = new Uri(_configurationResponse.ResourceRegistrationEndpoint)
            };
            httpRequest.Headers.Add(AuthorizationHeader, Bearer + token);
            var httpResult = await _client.SendAsync(httpRequest).ConfigureAwait(false);
            var content = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);
            if (!httpResult.IsSuccessStatusCode)
            {
                return new GenericResponse<UpdateResourceSetResponse>
                {
                    ContainsError = true,
                    Error = JsonConvert.DeserializeObject<ErrorDetails>(content),
                    HttpStatus = httpResult.StatusCode
                };
            }

            return new GenericResponse<UpdateResourceSetResponse>
            {
                Content = JsonConvert.DeserializeObject<UpdateResourceSetResponse>(content)
            };
        }

        /// <summary>
        /// Adds the resource.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="token">The token.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">
        /// request
        /// or
        /// token
        /// </exception>
        public async Task<GenericResponse<AddResourceSetResponse>> AddResource(PostResourceSet request, string token)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
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
                RequestUri = new Uri(_configurationResponse.ResourceRegistrationEndpoint)
            };
            httpRequest.Headers.Add(AuthorizationHeader, Bearer + token);
            var httpResult = await _client.SendAsync(httpRequest).ConfigureAwait(false);
            var content = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);
            if (!httpResult.IsSuccessStatusCode)
            {
                return new GenericResponse<AddResourceSetResponse>
                {
                    ContainsError = true,
                    Error = JsonConvert.DeserializeObject<ErrorDetails>(content),
                    HttpStatus = httpResult.StatusCode
                };
            }

            return new GenericResponse<AddResourceSetResponse>
            {
                Content = JsonConvert.DeserializeObject<AddResourceSetResponse>(content)
            };
        }

        /// <summary>
        /// Gets all resources.
        /// </summary>
        /// <param name="authorizationHeaderValue">The authorization header value.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">authorizationHeaderValue</exception>
        public async Task<GenericResponse<string[]>> GetAllResources(string authorizationHeaderValue)
        {
            if (string.IsNullOrWhiteSpace(authorizationHeaderValue))
            {
                throw new ArgumentNullException(nameof(authorizationHeaderValue));
            }

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get, RequestUri = new Uri(_configurationResponse.ResourceRegistrationEndpoint)
            };
            request.Headers.Add(AuthorizationHeader, Bearer + authorizationHeaderValue);
            var httpResult = await _client.SendAsync(request).ConfigureAwait(false);
            var json = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);
            if (!httpResult.IsSuccessStatusCode)
            {
                return new GenericResponse<string[]>
                {
                    ContainsError = true,
                    Error = JsonConvert.DeserializeObject<ErrorDetails>(json),
                    HttpStatus = httpResult.StatusCode
                };
            }

            return new GenericResponse<string[]> {Content = JsonConvert.DeserializeObject<string[]>(json)};
        }

        /// <summary>
        /// Gets the resource.
        /// </summary>
        /// <param name="resourceSetId">The resource set identifier.</param>
        /// <param name="authorizationHeaderValue">The authorization header value.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">
        /// resourceSetId
        /// or
        /// authorizationHeaderValue
        /// </exception>
        public async Task<GenericResponse<ResourceSetResponse>> GetResource(
            string resourceSetId,
            string authorizationHeaderValue)
        {
            if (string.IsNullOrWhiteSpace(resourceSetId))
            {
                throw new ArgumentNullException(nameof(resourceSetId));
            }

            if (string.IsNullOrWhiteSpace(authorizationHeaderValue))
            {
                throw new ArgumentNullException(nameof(authorizationHeaderValue));
            }

            var resourceSetUrl = _configurationResponse.ResourceRegistrationEndpoint;

            resourceSetUrl += resourceSetUrl.EndsWith("/") ? resourceSetId : "/" + resourceSetId;

            var request = new HttpRequestMessage {Method = HttpMethod.Get, RequestUri = new Uri(resourceSetUrl)};
            request.Headers.Add(AuthorizationHeader, Bearer + authorizationHeaderValue);
            var httpResult = await _client.SendAsync(request).ConfigureAwait(false);
            var json = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);
            if (!httpResult.IsSuccessStatusCode)
            {
                return new GenericResponse<ResourceSetResponse>()
                {
                    ContainsError = true,
                    Error = JsonConvert.DeserializeObject<ErrorDetails>(json),
                    HttpStatus = httpResult.StatusCode
                };
            }

            return new GenericResponse<ResourceSetResponse>()
            {
                Content = JsonConvert.DeserializeObject<ResourceSetResponse>(json)
            };
        }

        /// <summary>
        /// Searches the resources.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        /// <param name="authorizationHeaderValue">The authorization header value.</param>
        /// <returns></returns>
        public async Task<GenericResponse<GenericResult<ResourceSet>>> SearchResources(
            SearchResourceSet parameter,
            string authorizationHeaderValue = null)
        {
            var url = _configurationResponse.ResourceRegistrationEndpoint + "/.search";

            var serializedPostPermission = JsonConvert.SerializeObject(parameter);
            var body = new StringContent(serializedPostPermission, Encoding.UTF8, JsonMimeType);
            var request = new HttpRequestMessage {Method = HttpMethod.Post, RequestUri = new Uri(url), Content = body};
            if (!string.IsNullOrWhiteSpace(authorizationHeaderValue))
            {
                request.Headers.Add(AuthorizationHeader, Bearer + authorizationHeaderValue);
            }

            var httpResult = await _client.SendAsync(request).ConfigureAwait(false);
            var content = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);
            if (!httpResult.IsSuccessStatusCode)
            {
                return new GenericResponse<GenericResult<ResourceSet>>()
                {
                    ContainsError = true,
                    Error = JsonConvert.DeserializeObject<ErrorDetails>(content),
                    HttpStatus = httpResult.StatusCode
                };
            }

            return new GenericResponse<GenericResult<ResourceSet>>
            {
                Content = JsonConvert.DeserializeObject<GenericResult<ResourceSet>>(content)
            };
        }
    }
}
