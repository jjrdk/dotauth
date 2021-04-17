namespace SimpleAuth.Sms
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;

    /// <summary>
    /// Defines the Twilio SMS client.
    /// </summary>
    /// <seealso cref="SimpleAuth.Sms.ISmsClient" />
    public class TwilioSmsClient : ISmsClient
    {
        private readonly IHttpClientFactory _client;
        private readonly TwilioSmsCredentials _credentials;
        private const string TwilioSmsEndpointFormat = "https://api.twilio.com/2010-04-01/Accounts/{0}/Messages.json";

        /// <summary>
        /// Initializes a new instance of the <see cref="TwilioSmsClient"/> class.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="credentials">The credentials.</param>
        public TwilioSmsClient(IHttpClientFactory client, TwilioSmsCredentials credentials)
        {
            _client = client;
            _credentials = credentials;
        }

        /// <summary>
        /// Sends the message.
        /// </summary>
        /// <param name="toPhoneNumber">To phone number.</param>
        /// <param name="message">The message.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">
        /// toPhoneNumber
        /// or
        /// message
        /// </exception>
        public async Task<(bool, string?)> SendMessage(string toPhoneNumber, string message)
        {
            var keyValues = new []
            {
                new KeyValuePair<string?, string?>("To", toPhoneNumber),
                new KeyValuePair<string?, string?>("From", _credentials.FromNumber),
                new KeyValuePair<string?, string?>("Body", message)
            };
            var content = new FormUrlEncodedContent(keyValues);
            var postUrl = string.Format(CultureInfo.InvariantCulture, TwilioSmsEndpointFormat, _credentials.AccountSid);
            var httpRequest = new HttpRequestMessage
            {
                Method = HttpMethod.Post, Content = content, RequestUri = new Uri(postUrl)
            };
            httpRequest.Headers.UserAgent.Add(
                new ProductInfoHeaderValue("twilio-csharp/5.13.4 (.NET Framework 4.5.1+)"));
            httpRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            httpRequest.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("utf-8"));
            httpRequest.Headers.Authorization = new AuthenticationHeaderValue(
                "Basic",
                CreateBasicAuthenticationHeader(_credentials.AccountSid, _credentials.AuthToken));
            var client = _client.CreateClient();
            var response = await client.SendAsync(httpRequest).ConfigureAwait(false);
            return !response.IsSuccessStatusCode
                ? (false, await response.Content.ReadAsStringAsync().ConfigureAwait(false))
                : (true, response.ReasonPhrase);
        }

        private static string CreateBasicAuthenticationHeader(string username, string password)
        {
            var credentials = username + ":" + password;
            var encoded = System.Text.Encoding.UTF8.GetBytes(credentials);
            return Convert.ToBase64String(encoded);
        }
    }
}
