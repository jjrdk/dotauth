namespace SimpleIdentityServer.Manager.Client.ResourceOwners
{
    using Newtonsoft.Json;
    using Results;
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading.Tasks;
    using SimpleAuth.Shared.Responses;

    internal sealed class GetAllResourceOwnersOperation : IGetAllResourceOwnersOperation
    {
        private readonly HttpClient _httpClientFactory;

        public GetAllResourceOwnersOperation(HttpClient httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<GetAllResourceOwnersResult> ExecuteAsync(Uri resourceOwnerUri, string authorizationHeaderValue = null)
        {
            if (resourceOwnerUri == null)
            {
                throw new ArgumentNullException(nameof(resourceOwnerUri));
            }

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = resourceOwnerUri
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
                return new GetAllResourceOwnersResult
                {
                    ContainsError = true,
                    Error = JsonConvert.DeserializeObject<ErrorResponse>(content),
                    HttpStatus = httpResult.StatusCode
                };
            }

            return new GetAllResourceOwnersResult
            {
                Content = JsonConvert.DeserializeObject<IEnumerable<ResourceOwnerResponse>>(content)
            };
        }
    }
}