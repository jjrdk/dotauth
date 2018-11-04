using Newtonsoft.Json;
using SimpleIdentityServer.UserManagement.Client.Results;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace SimpleIdentityServer.UserManagement.Client.Operations
{
    using Common.Responses;
    using Shared.Responses;

    internal sealed class GetProfilesOperation : IGetProfilesOperation
    {
        private readonly HttpClient _httpClientFactory;

        public GetProfilesOperation(HttpClient httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public Task<GetProfilesResult> Execute(string requestUrl, string currentSubject, string authorizationHeaderValue = null)
        {
            if (string.IsNullOrWhiteSpace(requestUrl))
            {
                throw new ArgumentNullException(nameof(requestUrl));
            }

            if (string.IsNullOrWhiteSpace(currentSubject))
            {
                throw new ArgumentNullException(nameof(currentSubject));
            }

            requestUrl += $"/{currentSubject}";
            return GetAll(requestUrl, authorizationHeaderValue);
        }

        public Task<GetProfilesResult> Execute(string requestUrl, string authorizationHeaderValue = null)
        {
            if(string.IsNullOrWhiteSpace(requestUrl))
            {
                throw new ArgumentNullException(nameof(requestUrl));
            }

            requestUrl += "/.me";
            return GetAll(requestUrl, authorizationHeaderValue);
        }

        private async Task<GetProfilesResult> GetAll(string url, string authorizationValue = null)
        {
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(url)
            };
            if (!string.IsNullOrWhiteSpace(authorizationValue))
            {
                request.Headers.Add("Authorization", "Bearer " + authorizationValue);
            }

            var result = await _httpClientFactory.SendAsync(request).ConfigureAwait(false);
            var content = await result.Content.ReadAsStringAsync().ConfigureAwait(false);
            try
            {
                result.EnsureSuccessStatusCode();
            }
            catch (Exception)
            {
                return new GetProfilesResult
                {
                    ContainsError = true,
                    Error = JsonConvert.DeserializeObject<ErrorResponse>(content),
                    HttpStatus = result.StatusCode
                };
            }

            return new GetProfilesResult
            {
                Content = JsonConvert.DeserializeObject<IEnumerable<ProfileResponse>>(content)
            };
        }
    }
}
