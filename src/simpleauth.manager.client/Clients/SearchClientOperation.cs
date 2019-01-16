namespace SimpleAuth.Manager.Client.Clients
{
    using Newtonsoft.Json;
    using Results;
    using Shared.Requests;
    using Shared.Responses;
    using System;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;

    internal sealed class SearchClientOperation
    {
        private readonly HttpClient _httpClientFactory;

        public SearchClientOperation(HttpClient httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<PagedResult<ClientResponse>> ExecuteAsync(Uri clientsUri, SearchClientsRequest parameter, string authorizationHeaderValue = null)
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
                var result = new PagedResult<ClientResponse>
                {
                    ContainsError = true,
                    HttpStatus = httpResult.StatusCode
                };
                if (!string.IsNullOrWhiteSpace(content))
                {
                    result.Error = JsonConvert.DeserializeObject<ErrorResponseWithState>(content);
                }

                return result;
            }

            return new PagedResult<ClientResponse>
            {
                Content = JsonConvert.DeserializeObject<PagedResponse<ClientResponse>>(content)
            };
        }
    }
}
