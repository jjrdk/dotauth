namespace SimpleAuth.Manager.Client
{
    using System;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;
    using System.Threading.Tasks;
    using Newtonsoft.Json;
    using SimpleAuth.Shared;
    using SimpleAuth.Shared.Models;
    using SimpleAuth.Shared.Requests;

    internal sealed class UpdateResourceOwnerPasswordOperation
    {
        private readonly HttpClient _httpClient;

        public UpdateResourceOwnerPasswordOperation(HttpClient httpClientFactory)
        {
            _httpClient = httpClientFactory;
        }

        public async Task<GenericResponse<object>> Execute(
            Uri resourceOwnerUri,
            UpdateResourceOwnerPasswordRequest updateResourceOwnerPasswordRequest,
            string authorizationHeaderValue = null)
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
                request.Headers.Authorization = new AuthenticationHeaderValue(
                    JwtBearerConstants.BearerScheme,
                    authorizationHeaderValue);
            }

            var httpResult = await _httpClient.SendAsync(request).ConfigureAwait(false);
            var content = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);
            return !httpResult.IsSuccessStatusCode
                ? new GenericResponse<object>
                {
                    ContainsError = true,
                    Error = JsonConvert.DeserializeObject<ErrorDetails>(content),
                    HttpStatus = httpResult.StatusCode
                }
                : new GenericResponse<object>();
        }
    }
}
