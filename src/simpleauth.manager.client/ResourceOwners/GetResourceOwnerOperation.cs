namespace SimpleAuth.Manager.Client.ResourceOwners
{
    using Newtonsoft.Json;
    using Results;
    using Shared.Responses;
    using System;
    using System.Net.Http;
    using System.Threading.Tasks;

    internal sealed class GetResourceOwnerOperation
    {
        private readonly HttpClient _httpClientFactory;

        public GetResourceOwnerOperation(HttpClient httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<GetResourceOwnerResult> ExecuteAsync(Uri clientsUri, string authorizationHeaderValue = null)
        {
            if (clientsUri == null)
            {
                throw new ArgumentNullException(nameof(clientsUri));
            }

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = clientsUri
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
                return new GetResourceOwnerResult
                {
                    ContainsError = true,
                    Error = JsonConvert.DeserializeObject<ErrorResponse>(content),
                    HttpStatus = httpResult.StatusCode
                };
            }

            return new GetResourceOwnerResult
            {
                Content = JsonConvert.DeserializeObject<ResourceOwnerResponse>(content)
            };
        }
    }
}
