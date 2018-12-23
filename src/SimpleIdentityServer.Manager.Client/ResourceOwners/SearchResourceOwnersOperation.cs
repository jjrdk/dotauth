using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SimpleIdentityServer.Manager.Client.Results;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Manager.Client.ResourceOwners
{
    using Shared.Requests;
    using Shared.Responses;

    internal sealed class SearchResourceOwnersOperation : ISearchResourceOwnersOperation
    {
        private readonly HttpClient _httpClientFactory;

        public SearchResourceOwnersOperation(HttpClient httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<PagedResult<ResourceOwnerResponse>> ExecuteAsync(Uri resourceOwnerUri, SearchResourceOwnersRequest parameter, string authorizationHeaderValue = null)
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

            var httpResult = await _httpClientFactory.SendAsync(request).ConfigureAwait(false);
            var content = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);
            var rec = JObject.Parse(content);
            try
            {
                httpResult.EnsureSuccessStatusCode();
            }
            catch
            {
                var result = new PagedResult<ResourceOwnerResponse>
                {
                    ContainsError = true,
                    HttpStatus = httpResult.StatusCode
                };
                if (!string.IsNullOrWhiteSpace(content))
                {
                    result.Error = JsonConvert.DeserializeObject<ErrorResponseWithState>(content);
                }

                return result;
            }

            return new PagedResult<ResourceOwnerResponse>
            {
                Content = JsonConvert.DeserializeObject<PagedResponse<ResourceOwnerResponse>>(content)
            };
        }
    }
}
