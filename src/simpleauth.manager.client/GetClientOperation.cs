namespace SimpleAuth.Manager.Client
{
    using System;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;
    using Newtonsoft.Json;
    using SimpleAuth.Shared;
    using SimpleAuth.Shared.Models;

    internal sealed class GetClientOperation
    {
        private readonly HttpClient _httpClient;

        public GetClientOperation(HttpClient httpClientFactory)
        {
            _httpClient = httpClientFactory;
        }

        public async Task<GenericResponse<Client>> Execute(Uri clientsUri, string authorizationHeaderValue = null)
        {
            if (clientsUri == null)
            {
                throw new ArgumentNullException(nameof(clientsUri));
            }

            var request = new HttpRequestMessage { Method = HttpMethod.Get, RequestUri = clientsUri };
            if (!string.IsNullOrWhiteSpace(authorizationHeaderValue))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authorizationHeaderValue);
            }

            var httpResult = await _httpClient.SendAsync(request).ConfigureAwait(false);
            var content = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);
            if (!httpResult.IsSuccessStatusCode)
            {
                return new GenericResponse<Client>
                {
                    ContainsError = true,
                    Error = JsonConvert.DeserializeObject<ErrorDetails>(content),
                    HttpStatus = httpResult.StatusCode
                };
            }

            return new GenericResponse<Client> { Content = JsonConvert.DeserializeObject<Client>(content) };
        }
    }
}
