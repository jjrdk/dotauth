namespace SimpleAuth.Manager.Client
{
    using System;
    using System.Net.Http;
    using System.Threading.Tasks;
    using SimpleAuth.Shared;
    using SimpleAuth.Shared.Models;
    using SimpleAuth.Shared.Responses;

    internal sealed class GetResourceOwnerOperation
    {
        private readonly HttpClient _httpClient;

        public GetResourceOwnerOperation(HttpClient httpClientFactory)
        {
            _httpClient = httpClientFactory;
        }

        public async Task<GenericResponse<ResourceOwner>> Execute(
            Uri clientsUri,
            string authorizationHeaderValue = null)
        {
            if (clientsUri == null)
            {
                throw new ArgumentNullException(nameof(clientsUri));
            }

            var request = new HttpRequestMessage {Method = HttpMethod.Get, RequestUri = clientsUri};
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
                return new GenericResponse<ResourceOwner>
                {
                    ContainsError = true,
                    Error = Serializer.Default.Deserialize<ErrorResponse>(content),
                    HttpStatus = httpResult.StatusCode
                };
            }

            return new GenericResponse<ResourceOwner>
            {
                Content = Serializer.Default.Deserialize<ResourceOwner>(content)
            };
        }
    }
}
