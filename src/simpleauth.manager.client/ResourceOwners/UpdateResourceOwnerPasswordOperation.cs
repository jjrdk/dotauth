namespace SimpleAuth.Manager.Client.ResourceOwners
{
    using Newtonsoft.Json;
    using Shared;
    using Shared.Requests;
    using Shared.Responses;
    using System;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;

    internal sealed class UpdateResourceOwnerPasswordOperation
    {
        private readonly HttpClient _httpClientFactory;

        public UpdateResourceOwnerPasswordOperation(HttpClient httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<BaseResponse> ExecuteAsync(Uri resourceOwnerUri, UpdateResourceOwnerPasswordRequest updateResourceOwnerPasswordRequest, string authorizationHeaderValue = null)
        {
            if (resourceOwnerUri == null)
            {
                throw new ArgumentNullException(nameof(resourceOwnerUri));
            }

            if (updateResourceOwnerPasswordRequest == null)
            {
                throw new ArgumentNullException(nameof(updateResourceOwnerPasswordRequest));
            }

            var serializedJson = JsonConvert.SerializeObject(updateResourceOwnerPasswordRequest);
            var body = new StringContent(serializedJson, Encoding.UTF8, "application/json");
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Put,
                RequestUri = resourceOwnerUri,
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
