namespace SimpleAuth.Manager.Client
{
    using System;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;
    using System.Threading.Tasks;
    using Newtonsoft.Json;
    using SimpleAuth.Shared;
    using SimpleAuth.Shared.Models;
    using SimpleAuth.Shared.Requests;
    using SimpleAuth.Shared.Responses;

    internal sealed class SearchClientOperation
    {
        private readonly HttpClient _httpClient;

        public SearchClientOperation(HttpClient httpClientFactory)
        {
            _httpClient = httpClientFactory;
        }

        public async Task<GenericResponse<PagedResponse<Client>>> Execute(
            Uri clientsUri,
            SearchClientsRequest parameter,
            string authorizationHeaderValue = null)
        {
            if (clientsUri == null)
            {
                throw new ArgumentNullException(nameof(clientsUri));
            }

            var serializedPostPermission = JsonConvert.SerializeObject(parameter);
            var body = new StringContent(serializedPostPermission, Encoding.UTF8, "application/json");
            var request = new HttpRequestMessage { Method = HttpMethod.Post, RequestUri = clientsUri, Content = body };
            if (!string.IsNullOrWhiteSpace(authorizationHeaderValue))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue(
                    JwtBearerConstants.BearerScheme,
                    authorizationHeaderValue);
            }

            var httpResult = await _httpClient.SendAsync(request).ConfigureAwait(false);
            var content = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);

            if (httpResult.IsSuccessStatusCode)
                return new GenericResponse<PagedResponse<Client>>
                {
                    Content = JsonConvert.DeserializeObject<PagedResponse<Client>>(content)
                };
            var result = new GenericResponse<PagedResponse<Client>>
            {
                ContainsError = true,
                HttpStatus = httpResult.StatusCode
            };
            if (!string.IsNullOrWhiteSpace(content))
            {
                result.Error = JsonConvert.DeserializeObject<ErrorDetails>(content);
            }

            return result;

        }
    }
}
