namespace SimpleAuth.Twilio
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Net.Http;
    using System.Threading.Tasks;

    internal class TwilioClient : ITwilioClient
    {
        private readonly HttpClient _client;
        private const string TwilioSmsEndpointFormat = "https://api.twilio.com/2010-04-01/Accounts/{0}/Messages.json";

        public TwilioClient(HttpClient client)
        {
            _client = client;
        }

        public async Task<bool> SendMessage(TwilioSmsCredentials credentials, string toPhoneNumber, string message)
        {
            if (string.IsNullOrWhiteSpace(toPhoneNumber))
            {
                throw new ArgumentException(nameof(toPhoneNumber));
            }

            if (string.IsNullOrWhiteSpace(message))
            {
                throw new ArgumentException(nameof(message));
            }

            var keyValues = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("To", toPhoneNumber),
                new KeyValuePair<string, string>("From", credentials.FromNumber),
                new KeyValuePair<string, string>("Body", message)
            };
            var content = new FormUrlEncodedContent(keyValues);
            var postUrl = string.Format(CultureInfo.InvariantCulture, TwilioSmsEndpointFormat, credentials.AccountSid);
            var httpRequest = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                Content = content,
                RequestUri = new Uri(postUrl)
            };
            httpRequest.Headers.Add("User-Agent", "twilio-csharp/5.13.4 (.NET Framework 4.5.1+)");
            httpRequest.Headers.Add("Accept", "application/json");
            httpRequest.Headers.Add("Accept-Encoding", "utf-8");
            httpRequest.Headers.Add("Authorization",
                "Basic " + CreateBasicAuthenticationHeader(credentials.AccountSid, credentials.AuthToken));
            var response = await _client.SendAsync(httpRequest).ConfigureAwait(false);
            try
            {
                response.EnsureSuccessStatusCode();

                return true;
            }
            catch (Exception ex)
            {
                var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                throw new TwilioException(json, ex);
            }
        }

        private string CreateBasicAuthenticationHeader(string username, string password)
        {
            var credentials = username + ":" + password;
            var encoded = System.Text.Encoding.UTF8.GetBytes(credentials);
            return Convert.ToBase64String(encoded);
        }
    }
}
