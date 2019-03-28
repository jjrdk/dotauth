namespace SimpleAuth.Manager.Client
{
    using System;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;
    using Newtonsoft.Json;
    using SimpleAuth.Shared;
    using SimpleAuth.Shared.Models;
    using SimpleAuth.Shared.Requests;
    using SimpleAuth.Shared.Responses;

    internal sealed class SearchResourceOwnersOperation
    {
        private readonly HttpClient _httpClient;

        public SearchResourceOwnersOperation(HttpClient httpClientFactory)
        {
            _httpClient = httpClientFactory;
        }

        public async Task<GenericResponse<PagedResponse<ResourceOwner>>> Execute(Uri resourceOwnerUri, SearchResourceOwnersRequest parameter, string authorizationHeaderValue = null)
        {
            if (resourceOwnerUri == null)
            {
                throw new ArgumentNullException(nameof(resourceOwnerUri));
            }

            var serializedPostPermission = JsonConvert.SerializeObject(parameter);
            var body = new StringContent(serializedPostPermission, Encoding.UTF8, "application/json");
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = resourceOwnerUri,
                Content = body
            };
            if (!string.IsNullOrWhiteSpace(authorizationHeaderValue))
            {
                request.Headers.Add("Authorization", "Bearer " + authorizationHeaderValue);
            }

            var httpResult = await _httpClient.SendAsync(request).ConfigureAwait(false);
            var content = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);

            if (httpResult.IsSuccessStatusCode)
            {
                return new GenericResponse<PagedResponse<ResourceOwner>>
                {
                    Content = Serializer.Default.Deserialize<PagedResponse<ResourceOwner>>(content)
                };
            }

            var result = new GenericResponse<PagedResponse<ResourceOwner>>
            {
                ContainsError = true,
                HttpStatus = httpResult.StatusCode
            };
            if (!string.IsNullOrWhiteSpace(content))
            {
                result.Error = Serializer.Default.Deserialize<ErrorDetails>(content);
            }

            return result;

        }
    }
}
