namespace SimpleAuth.Twilio.Client
{
    using Newtonsoft.Json;
    using SimpleAuth.Shared;
    using SimpleAuth.Shared.Responses;
    using System;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;

    /// <summary>
    /// Defines the sid sms authentication client.
    /// </summary>
    public sealed class SidSmsAuthenticateClient
    {
        private readonly HttpClient _client;

        /// <summary>
        /// Initializes a new instance of the <see cref="SidSmsAuthenticateClient"/> class.
        /// </summary>
        /// <param name="client">The client.</param>
        public SidSmsAuthenticateClient(HttpClient client)
        {
            _client = client;
        }

        /// <summary>
        /// Sends the specified request URL.
        /// </summary>
        /// <param name="requestUrl">The request URL.</param>
        /// <param name="request">The request.</param>
        /// <param name="authorizationValue">The authorization value.</param>
        /// <returns></returns>
        public Task<GenericResponse<object>> Send(
            string requestUrl,
            ConfirmationCodeRequest request,
            string authorizationValue = null)
        {
            requestUrl += "/code";
            return SendSms(new Uri(requestUrl), request, authorizationValue);
        }

        /// <summary>
        /// Sends the SMS.
        /// </summary>
        /// <param name="requestUri">The request URI.</param>
        /// <param name="request">The request.</param>
        /// <param name="authorizationValue">The authorization value.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">
        /// requestUri
        /// or
        /// request
        /// </exception>
        private async Task<GenericResponse<object>> SendSms(
            Uri requestUri,
            ConfirmationCodeRequest request,
            string authorizationValue = null)
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
                return new GenericResponse<object>
                {
                    ContainsError = true,
                    Error = JsonConvert.DeserializeObject<ErrorResponse>(content),
                    HttpStatus = result.StatusCode
                };
            }

            return new GenericResponse<object>();
        }
    }
}
