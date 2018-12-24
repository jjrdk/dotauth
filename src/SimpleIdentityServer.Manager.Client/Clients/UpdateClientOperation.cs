namespace SimpleIdentityServer.Manager.Client.Clients
{
    using Newtonsoft.Json;
    using System;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;

    using Shared;
    using Shared.Models;
    using Shared.Responses;

    internal sealed class UpdateClientOperation : IUpdateClientOperation
    {
        private readonly HttpClient _httpClientFactory;

        public UpdateClientOperation(HttpClient httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<BaseResponse> ExecuteAsync(Uri clientsUri, Client client, string authorizationHeaderValue = null)
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
                Method = HttpMethod.Put,
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
