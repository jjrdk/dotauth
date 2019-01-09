namespace SimpleAuth.Manager.Client.Clients
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Newtonsoft.Json;
    using Results;
    using Shared.Responses;

    internal sealed class GetAllClientsOperation : IGetAllClientsOperation
    {
        private readonly HttpClient _httpClientFactory;

        public GetAllClientsOperation(HttpClient httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<GetAllClientResult> ExecuteAsync(Uri clientsUri, string authorizationHeaderValue = null)
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
            catch(Exception)
            {
                return new GetAllClientResult
                {
                    ContainsError = true,
                    Error = JsonConvert.DeserializeObject<ErrorResponse>(content),
                    HttpStatus = httpResult.StatusCode
                };
            }

            return new GetAllClientResult
            {
                Content = JsonConvert.DeserializeObject<IEnumerable<ClientResponse>>(content)
            };
        }
    }
}
