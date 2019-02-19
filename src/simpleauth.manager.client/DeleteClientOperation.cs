namespace SimpleAuth.Manager.Client
{
    using System;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Newtonsoft.Json;
    using SimpleAuth.Shared;
    using SimpleAuth.Shared.Models;
    using SimpleAuth.Shared.Responses;

    internal sealed class DeleteClientOperation
    {
        private readonly HttpClient _httpClient;

        public DeleteClientOperation(HttpClient httpClientFactory)
        {
            _httpClient = httpClientFactory;
        }

        public async Task<GenericResponse<Client>> Execute(Uri clientsUri, string authorizationHeaderValue = null)
        {
            var request = new HttpRequestMessage {Method = HttpMethod.Delete, RequestUri = clientsUri};
            if (!string.IsNullOrWhiteSpace(authorizationHeaderValue))
            {
                request.Headers.Add("Authorization", "Bearer " + authorizationHeaderValue);
            }

            var httpResult = await _httpClient.SendAsync(request).ConfigureAwait(false);
            var content = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);
            try
            {
                httpResult.EnsureSuccessStatusCode();
            }
            catch (Exception)
            {
                return new GenericResponse<Client>
                {
                    ContainsError = true, Error = JsonConvert.DeserializeObject<ErrorResponse>(content)
                };
            }

            return new GenericResponse<Client>();
        }
    }
}
