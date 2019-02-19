namespace SimpleAuth.Manager.Client
{
    using System;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;
    using Newtonsoft.Json;
    using SimpleAuth.Shared;
    using SimpleAuth.Shared.Requests;
    using SimpleAuth.Shared.Responses;

    internal sealed class UpdateResourceOwnerClaimsOperation
    {
        private readonly HttpClient _httpClient;

        public UpdateResourceOwnerClaimsOperation(HttpClient httpClientFactory)
        {
            _httpClient = httpClientFactory;
        }

        public async Task<GenericResponse<object>> Execute(Uri resourceOwnerUri, UpdateResourceOwnerClaimsRequest updateResourceOwnerClaimsRequest, string authorizationHeaderValue = null)
        {
            if (resourceOwnerUri == null)
            {
                throw new ArgumentNullException(nameof(resourceOwnerUri));
            }

            if (updateResourceOwnerClaimsRequest == null)
            {
                throw new ArgumentNullException(nameof(updateResourceOwnerClaimsRequest));
            }

            var serializedJson = JsonConvert.SerializeObject(updateResourceOwnerClaimsRequest);
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

            var httpResult = await _httpClient.SendAsync(request).ConfigureAwait(false);
            var content = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);
            try
            {
                httpResult.EnsureSuccessStatusCode();
            }
            catch (Exception)
            {
                return new GenericResponse<object>
                {
                    ContainsError = true,
                    Error = JsonConvert.DeserializeObject<ErrorResponse>(content),
                    HttpStatus = httpResult.StatusCode
                };
            }

            return new GenericResponse<object>();
        }
    }
}
