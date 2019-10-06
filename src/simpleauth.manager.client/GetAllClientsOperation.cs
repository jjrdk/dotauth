namespace SimpleAuth.Manager.Client
{
    using System;
    using System.Net.Http;
    using System.Threading.Tasks;
    using SimpleAuth.Shared;
    using SimpleAuth.Shared.Models;

    internal sealed class GetAllClientsOperation
    {
        private readonly HttpClient _httpClient;

        public GetAllClientsOperation(HttpClient httpClientFactory)
        {
            _httpClient = httpClientFactory;
        }

        public async Task<GenericResponse<Client[]>> Execute(Uri clientsUri, string authorizationHeaderValue = null)
        {
            if (clientsUri == null)
            {
                throw new ArgumentNullException(nameof(clientsUri));
            }

            var request = new HttpRequestMessage {Method = HttpMethod.Get, RequestUri = clientsUri};
            if (!string.IsNullOrWhiteSpace(authorizationHeaderValue))
            {
                request.Headers.Add("Authorization", "JwtConstants.BearerScheme " + authorizationHeaderValue);
            }

            var httpResult = await _httpClient.SendAsync(request).ConfigureAwait(false);
            var content = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);
            if (!httpResult.IsSuccessStatusCode)
            {
                return new GenericResponse<Client[]>
                {
                    ContainsError = true,
                    Error = Serializer.Default.Deserialize<ErrorDetails>(content),
                    HttpStatus = httpResult.StatusCode
                };
            }

            return new GenericResponse<Client[]> {Content = Serializer.Default.Deserialize<Client[]>(content)};
        }
    }
}
