namespace SimpleIdentityServer.UserManagement.Client.Operations
{
    using Newtonsoft.Json;
    using Shared;
    using Shared.Responses;
    using System;
    using System.Net.Http;
    using System.Threading.Tasks;

    internal sealed class UnlinkProfileOperation : IUnlinkProfileOperation
    {
        private readonly HttpClient _httpClientFactory;

        public UnlinkProfileOperation(HttpClient httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public Task<BaseResponse> Execute(string requestUrl, string externalSubject, string currentSubject, string authorizationHeaderValue = null)
        {
            if (string.IsNullOrWhiteSpace(requestUrl))
            {
                throw new ArgumentException(nameof(requestUrl));
            }

            if (string.IsNullOrWhiteSpace(externalSubject))
            {
                throw new ArgumentNullException(nameof(externalSubject));
            }

            if (string.IsNullOrWhiteSpace(currentSubject))
            {
                throw new ArgumentNullException(nameof(currentSubject));
            }

            var url = requestUrl + $"/{currentSubject}/{externalSubject}";
            return Delete(url, authorizationHeaderValue);
        }

        public Task<BaseResponse> Execute(string requestUrl, string externalSubject, string authorizationHeaderValue = null)
        {
            if (string.IsNullOrWhiteSpace(requestUrl))
            {
                throw new ArgumentException(nameof(requestUrl));
            }

            if (string.IsNullOrWhiteSpace(externalSubject))
            {
                throw new ArgumentNullException(nameof(externalSubject));
            }

            var url = requestUrl + $"/.me/{externalSubject}";
            return Delete(url, authorizationHeaderValue);
        }

        private async Task<BaseResponse> Delete(string url, string authorizationValue = null)
        {
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Delete,
                RequestUri = new Uri(url)
            };
            if (!string.IsNullOrWhiteSpace(authorizationValue))
            {
                request.Headers.Add("Authorization", "Bearer " + authorizationValue);
            }

            var result = await _httpClientFactory.SendAsync(request).ConfigureAwait(false);
            var content = await result.Content.ReadAsStringAsync().ConfigureAwait(false);
            try
            {
                result.EnsureSuccessStatusCode();
            }
            catch (Exception)
            {
                return new BaseResponse
                {
                    ContainsError = true,
                    Error = JsonConvert.DeserializeObject<ErrorResponse>(content),
                    HttpStatus = result.StatusCode
                };
            }

            return new BaseResponse();
        }
    }
}
