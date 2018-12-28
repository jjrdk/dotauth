using Newtonsoft.Json;
using SimpleIdentityServer.Uma.Client.Results;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Uma.Client.Policy
{
    using SimpleAuth.Shared.Responses;
    using SimpleAuth.Uma.Shared.DTOs;

    internal sealed class SearchPoliciesOperation : ISearchPoliciesOperation
    {
        private readonly HttpClient _httpClientFactory;

        public SearchPoliciesOperation(HttpClient httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<SearchAuthPoliciesResult> ExecuteAsync(string url, SearchAuthPolicies parameter, string authorizationHeaderValue = null)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                throw new ArgumentNullException(nameof(url));
            }

            var serializedPostPermission = JsonConvert.SerializeObject(parameter);
            var body = new StringContent(serializedPostPermission, Encoding.UTF8, "application/json");
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(url),
                Content = body
            };
            if (!string.IsNullOrWhiteSpace(authorizationHeaderValue))
            {
                request.Headers.Add("Authorization", "Bearer " + authorizationHeaderValue);
            }

            var httpResult = await _httpClientFactory.SendAsync(request).ConfigureAwait(false);
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
    }
}
