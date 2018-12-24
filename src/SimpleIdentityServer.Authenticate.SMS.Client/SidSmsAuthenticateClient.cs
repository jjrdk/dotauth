namespace SimpleIdentityServer.Authenticate.SMS.Client
{
    using Newtonsoft.Json;
    using Common.Requests;
    using System;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;
    using SimpleAuth.Shared;
    using SimpleAuth.Shared.Responses;

    internal sealed class SidSmsAuthenticateClient : ISidSmsAuthenticateClient
    {
        private readonly HttpClient _client;

        public SidSmsAuthenticateClient(HttpClient client)
        {
            _client = client;
        }

        public Task<BaseResponse> Send(string requestUrl, ConfirmationCodeRequest request, string authorizationValue = null)
        {
            requestUrl += "/code";
            return SendSms(new Uri(requestUrl), request, authorizationValue);
        }

        private async Task<BaseResponse> SendSms(Uri requestUri, ConfirmationCodeRequest request, string authorizationValue = null)
        {
            if (requestUri == null)
            {
                throw new ArgumentNullException(nameof(requestUri));
            }

            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var json = JsonConvert.SerializeObject(request);
            var req = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                Content = new StringContent(json),
                RequestUri = requestUri
            };
            req.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            if (!string.IsNullOrWhiteSpace(authorizationValue))
            {
                req.Headers.Add("Authorization", "Basic " + authorizationValue);
            }

            var result = await _client.SendAsync(req).ConfigureAwait(false);
            var content = await result.Content.ReadAsStringAsync().ConfigureAwait(false);
            try
            {
                result.EnsureSuccessStatusCode();
            }
            catch
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
