namespace SimpleAuth.Manager.Client
{
    using System;
    using System.Net.Http;
    using System.Threading.Tasks;
    using SimpleAuth.Shared;
    using SimpleAuth.Shared.Models;

    internal sealed class GetAllResourceOwnersOperation
    {
        private readonly HttpClient _httpClient;

        public GetAllResourceOwnersOperation(HttpClient httpClientFactory)
        {
            _httpClient = httpClientFactory;
        }

        public async Task<GenericResponse<ResourceOwner[]>> Execute(
            Uri resourceOwnerUri,
            string authorizationHeaderValue = null)
        {
            if (resourceOwnerUri == null)
            {
                throw new ArgumentNullException(nameof(resourceOwnerUri));
            }

            var request = new HttpRequestMessage {Method = HttpMethod.Get, RequestUri = resourceOwnerUri};
            if (!string.IsNullOrWhiteSpace(authorizationHeaderValue))
            {
                request.Headers.Add("Authorization", "JwtConstants.BearerScheme " + authorizationHeaderValue);
            }

            var httpResult = await _httpClient.SendAsync(request).ConfigureAwait(false);
            var content = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);
            if (!httpResult.IsSuccessStatusCode)
            {
                return new GenericResponse<ResourceOwner[]>
                {
                    ContainsError = true,
                    Error = Serializer.Default.Deserialize<ErrorDetails>(content),
                    HttpStatus = httpResult.StatusCode
                };
            }

            return new GenericResponse<ResourceOwner[]>
            {
                Content = Serializer.Default.Deserialize<ResourceOwner[]>(content)
            };
        }
    }
}
