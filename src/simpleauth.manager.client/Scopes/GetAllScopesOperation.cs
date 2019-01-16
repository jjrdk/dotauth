namespace SimpleAuth.Manager.Client.Scopes
{
    using Newtonsoft.Json;
    using Results;
    using Shared.Responses;
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading.Tasks;

    internal sealed class GetAllScopesOperation
    {
        private readonly HttpClient _httpClientFactory;

        public GetAllScopesOperation(HttpClient httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<GetAllScopesResult> ExecuteAsync(Uri scopesUri, string authorizationHeaderValue = null)
        {
            if (scopesUri == null)
            {
                throw new ArgumentNullException(nameof(scopesUri));
            }

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = scopesUri
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
                return new GetAllScopesResult
                {
                    ContainsError = true,
                    Error = JsonConvert.DeserializeObject<ErrorResponse>(content),
                    HttpStatus = httpResult.StatusCode
                };
            }

            return new GetAllScopesResult
            {
                Content = JsonConvert.DeserializeObject<IEnumerable<ScopeResponse>>(content)
            };
        }
    }
}
