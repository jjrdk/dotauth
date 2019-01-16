namespace SimpleAuth.Manager.Client.Scopes
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Results;
    using Shared.Requests;
    using Shared.Responses;
    using System;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;

    internal sealed class SearchScopesOperation
    {
        private readonly HttpClient _httpClientFactory;

        public SearchScopesOperation(HttpClient httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<PagedResult<ScopeResponse>> ExecuteAsync(Uri clientsUri, SearchScopesRequest parameter, string authorizationHeaderValue = null)
        {
            if (clientsUri == null)
            {
                throw new ArgumentNullException(nameof(clientsUri));
            }

            var serializedPostPermission = JsonConvert.SerializeObject(parameter);
            var body = new StringContent(serializedPostPermission, Encoding.UTF8, "application/json");
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = clientsUri,
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
                return new PagedResult<ScopeResponse>
                {
                    ContainsError = true,
                    Error = JsonConvert.DeserializeObject<ErrorResponse>(content),
                    HttpStatus = httpResult.StatusCode
                };
            }

            return new PagedResult<ScopeResponse>
            {
                Content = JsonConvert.DeserializeObject<PagedResponse<ScopeResponse>>(content)
            };
        }
    }
}
