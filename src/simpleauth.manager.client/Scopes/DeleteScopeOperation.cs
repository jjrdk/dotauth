namespace SimpleAuth.Manager.Client.Scopes
{
    using Newtonsoft.Json;
    using Shared;
    using Shared.Responses;
    using System;
    using System.Net.Http;
    using System.Threading.Tasks;

    internal sealed class DeleteScopeOperation
    {
        private readonly HttpClient _httpClientFactory;

        public DeleteScopeOperation(HttpClient httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<BaseResponse> ExecuteAsync(Uri scopesUri, string authorizationHeaderValue = null)
        {
            if (scopesUri == null)
            {
                throw new ArgumentNullException(nameof(scopesUri));
            }

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Delete,
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
                return new BaseResponse
                {
                    ContainsError = true,
                    Error = JsonConvert.DeserializeObject<ErrorResponse>(content),
                    HttpStatus = httpResult.StatusCode
                };
            }

            return new BaseResponse();
        }
    }
}
