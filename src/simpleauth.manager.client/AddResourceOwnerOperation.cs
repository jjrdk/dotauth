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

    internal sealed class AddResourceOwnerOperation
    {
        private readonly HttpClient _httpClient;

        public AddResourceOwnerOperation(HttpClient httpClientFactory)
        {
            _httpClient = httpClientFactory;
        }

        public async Task<GenericResponse<string>> Execute(
            Uri resourceOwnerUri,
            AddResourceOwnerRequest resourceOwner,
            string authorizationHeaderValue = null)
        {
            if (resourceOwnerUri == null)
            {
                throw new ArgumentNullException(nameof(resourceOwnerUri));
            }

            if (resourceOwner == null)
            {
                throw new ArgumentNullException(nameof(resourceOwner));
            }

            var serializedJson = JsonConvert.SerializeObject(resourceOwner);
            var body = new StringContent(serializedJson, Encoding.UTF8, "application/json");
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = resourceOwnerUri,
                Content = body
            };
            if (!string.IsNullOrWhiteSpace(authorizationHeaderValue))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authorizationHeaderValue);
            }

            var httpResult = await _httpClient.SendAsync(request).ConfigureAwait(false);
            var content = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);
            if (!httpResult.IsSuccessStatusCode)
            {
                return new GenericResponse<string>
                {
                    ContainsError = true,
                    Error = JsonConvert.DeserializeObject<ErrorDetails>(content),
                    HttpStatus = httpResult.StatusCode
                };
            }

            var response = JsonConvert.DeserializeObject<AddResourceOwnerResponse>(content);
            return new GenericResponse<string> { Content = response.Subject };
        }

        private class AddResourceOwnerResponse
        {
            public string Subject { get; set; }
        }
    }
}
