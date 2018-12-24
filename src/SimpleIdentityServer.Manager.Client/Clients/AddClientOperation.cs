namespace SimpleIdentityServer.Manager.Client.Clients
{
    using Newtonsoft.Json;
    using Results;
    using System;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;
    using Shared.Models;
    using Shared.Responses;

    internal sealed class AddClientOperation : IAddClientOperation
    {
        private readonly HttpClient _httpClientFactory;

        public AddClientOperation(HttpClient httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<AddClientResult> ExecuteAsync(Uri clientsUri, Client client, string authorizationHeaderValue = null)
        {
            if (clientsUri == null)
            {
                throw new ArgumentNullException(nameof(clientsUri));
            }

            if (client == null)
            {
                throw new ArgumentNullException(nameof(client));
            }

            var serializedJson = JsonConvert.SerializeObject(client);
            var body = new StringContent(serializedJson, Encoding.UTF8, "application/json");
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
                return new AddClientResult
                {
                    ContainsError = true,
                    Error = JsonConvert.DeserializeObject<ErrorResponse>(content),
                    HttpStatus = httpResult.StatusCode
                };
            }
            
            return new AddClientResult
            {
                Content = JsonConvert.DeserializeObject<Client>(content)
            };
        }
    }
}
