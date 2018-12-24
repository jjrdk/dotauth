namespace SimpleIdentityServer.Manager.Client.Clients
{
    using Newtonsoft.Json;
    using Results;
    using System;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Shared.Models;
    using Shared.Responses;

    internal sealed class GetClientOperation : IGetClientOperation
    {
        private readonly HttpClient _httpClientFactory;

        public GetClientOperation(HttpClient httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<GetClientResult> ExecuteAsync(Uri clientsUri, string authorizationHeaderValue = null)
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
            //var rec = JObject.Parse(content);
            try
            {
                httpResult.EnsureSuccessStatusCode();
            }
            catch(Exception)
            {
                return new GetClientResult
                {
                    ContainsError = true,
                    Error = JsonConvert.DeserializeObject<ErrorResponse>(content),
                    HttpStatus = httpResult.StatusCode
                };
            }

            return new GetClientResult
            {
                Content = JsonConvert.DeserializeObject<Client>(content)
            };
        }
    }
}
